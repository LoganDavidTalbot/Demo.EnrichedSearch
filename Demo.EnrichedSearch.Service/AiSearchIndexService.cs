using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Demo.EnrichedSearch.Shared.Models.AiSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public class AiSearchIndexService : IAiSearchIndexService
    {
        private readonly SearchIndexClient _indexClient;
        private readonly SearchIndexerClient _indexerClient;
        private readonly string _storageConnectionString;
        private readonly string _containerName;
        private readonly string _indexName;
        private readonly string _indexerName;
        private readonly string _cognitiveServicesKey;


        public AiSearchIndexService(SearchIndexClient indexClient,
            SearchIndexerClient searchIndexerClient,
            string storageConnectionString, 
            string containerName, 
            string indexName, 
            string indexerName,
            string cognitiveServicesKey)
        {
            _indexClient = indexClient;
            _indexerClient = searchIndexerClient;
            _storageConnectionString = storageConnectionString;
            _containerName = containerName;
            _indexName = indexName;
            _indexerName = indexerName;
            _cognitiveServicesKey = cognitiveServicesKey;
        }
        public async Task<string> CreateIndexAndIndexerAsync()
        {
            // Create or Update the data source
            SearchIndexerDataSourceConnection dataSource = CreateOrUpdateDataSource(_indexerClient);

            // Create the skills
            OcrSkill ocrSkill = CreateOcrSkill();
            MergeSkill mergeSkill = CreateMergeSkill();
            EntityRecognitionSkill entityRecognitionSkill = CreateEntityRecognitionSkill();
            LanguageDetectionSkill languageDetectionSkill = CreateLanguageDetectionSkill();
            SplitSkill splitSkill = CreateSplitSkill();
            KeyPhraseExtractionSkill keyPhraseExtractionSkill = CreateKeyPhraseExtractionSkill();

            // Create the skillset
            List<SearchIndexerSkill> skills = new List<SearchIndexerSkill>();
            skills.Add(ocrSkill);
            skills.Add(mergeSkill);
            skills.Add(languageDetectionSkill);
            skills.Add(splitSkill);
            skills.Add(entityRecognitionSkill);
            skills.Add(keyPhraseExtractionSkill);

            SearchIndexerSkillset skillset = CreateOrUpdateDemoSkillSet(_indexerClient, skills, _cognitiveServicesKey);

            // Create the index
            SearchIndex demoIndex = await CreateDemoIndexAsync(_indexClient);

            // Create the indexer, map fields, and execute transformations
            SearchIndexer demoIndexer = await CreateDemoIndexerAsync(_indexerClient, dataSource, skillset, demoIndex);

            // Check indexer overall status
            return await CheckIndexerOverallStatusAsync(_indexerClient, demoIndexer.Name);
        }

        public async Task<bool> DeleteIndexAsync()
        {
            var index = await _indexClient.DeleteIndexAsync(_indexName);

            return index.Status == (int)HttpStatusCode.OK;
        }

        public async Task<SearchIndexStatistics> GetIndexStatisticsAsync()
        {
            try
            {
                return await _indexClient.GetIndexStatisticsAsync(_indexName);
            }
            catch (RequestFailedException e) when (e.Status == (int)HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<string> GetIndexerOverallStatusAsync()
        {
            return await CheckIndexerOverallStatusAsync(_indexerClient, _indexerName);
        }

        #region Private Methods

        private SearchIndexerDataSourceConnection CreateOrUpdateDataSource(SearchIndexerClient indexerClient)
        {
            SearchIndexerDataSourceConnection dataSource = new SearchIndexerDataSourceConnection(
                name: "demodata",
                type: SearchIndexerDataSourceType.AzureBlob,
                connectionString: _storageConnectionString,
                container: new SearchIndexerDataContainer(_containerName))
            {
                Description = "Demo files to demonstrate cognitive search capabilities."
            };

            // The data source does not need to be deleted if it was already created
            // since we are using the CreateOrUpdate method
            try
            {
                indexerClient.CreateOrUpdateDataSourceConnection(dataSource);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create or update the data source", ex);
            }

            return dataSource;
        }

        private OcrSkill CreateOcrSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("image")
            {
                Source = "/document/normalized_images/*"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("text")
            {
                TargetName = "text"
            });

            OcrSkill ocrSkill = new OcrSkill(inputMappings, outputMappings)
            {
                Description = "Extract text (plain and structured) from image",
                Context = "/document/normalized_images/*",
                DefaultLanguageCode = OcrSkillLanguage.En,
                ShouldDetectOrientation = true
            };

            return ocrSkill;
        }

        private MergeSkill CreateMergeSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/content"
            });
            inputMappings.Add(new InputFieldMappingEntry("itemsToInsert")
            {
                Source = "/document/normalized_images/*/text"
            });
            inputMappings.Add(new InputFieldMappingEntry("offsets")
            {
                Source = "/document/normalized_images/*/contentOffset"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("mergedText")
            {
                TargetName = "merged_text"
            });

            MergeSkill mergeSkill = new MergeSkill(inputMappings, outputMappings)
            {
                Description = "Create merged_text which includes all the textual representation of each image inserted at the right location in the content field.",
                Context = "/document",
                InsertPreTag = " ",
                InsertPostTag = " "
            };

            return mergeSkill;
        }

        private LanguageDetectionSkill CreateLanguageDetectionSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/merged_text"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("languageCode")
            {
                TargetName = "languageCode"
            });

            LanguageDetectionSkill languageDetectionSkill = new LanguageDetectionSkill(inputMappings, outputMappings)
            {
                Description = "Detect the language used in the document",
                Context = "/document"
            };

            return languageDetectionSkill;
        }

        private SplitSkill CreateSplitSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/merged_text"
            });
            inputMappings.Add(new InputFieldMappingEntry("languageCode")
            {
                Source = "/document/languageCode"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("textItems")
            {
                TargetName = "pages",
            });

            SplitSkill splitSkill = new SplitSkill(inputMappings, outputMappings)
            {
                Description = "Split content into pages",
                Context = "/document",
                TextSplitMode = TextSplitMode.Pages,
                MaximumPageLength = 4000,
                DefaultLanguageCode = SplitSkillLanguage.En
            };

            return splitSkill;
        }

        private EntityRecognitionSkill CreateEntityRecognitionSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/pages/*"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("organizations")
            {
                TargetName = "organizations"
            });

            EntityRecognitionSkill entityRecognitionSkill = new EntityRecognitionSkill(inputMappings, outputMappings)
            {
                Description = "Recognize organizations",
                Context = "/document/pages/*",
                DefaultLanguageCode = EntityRecognitionSkillLanguage.En
            };
            entityRecognitionSkill.Categories.Add(EntityCategory.Organization);

            return entityRecognitionSkill;
        }

        private KeyPhraseExtractionSkill CreateKeyPhraseExtractionSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/pages/*"
            });
            inputMappings.Add(new InputFieldMappingEntry("languageCode")
            {
                Source = "/document/languageCode"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("keyPhrases")
            {
                TargetName = "keyPhrases"
            });

            KeyPhraseExtractionSkill keyPhraseExtractionSkill = new KeyPhraseExtractionSkill(inputMappings, outputMappings)
            {
                Description = "Extract the key phrases",
                Context = "/document/pages/*",
                DefaultLanguageCode = KeyPhraseExtractionSkillLanguage.En
            };

            return keyPhraseExtractionSkill;
        }

        private SearchIndexerSkillset CreateOrUpdateDemoSkillSet(SearchIndexerClient indexerClient, IList<SearchIndexerSkill> skills, string cognitiveServicesKey)
        {
            SearchIndexerSkillset skillset = new SearchIndexerSkillset("demoskillset", skills)
            {
                Description = "Demo skillset",
                CognitiveServicesAccount = new CognitiveServicesAccountKey(cognitiveServicesKey)
            };

            // Create the skillset in your search service.
            // The skillset does not need to be deleted if it was already created
            // since we are using the CreateOrUpdate method
            try
            {
                indexerClient.CreateOrUpdateSkillset(skillset);
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Failed to create the skillset", ex);
            }

            return skillset;
        }

        private async Task<SearchIndex> CreateDemoIndexAsync(SearchIndexClient indexClient)
        {
            FieldBuilder builder = new FieldBuilder();
            var index = new SearchIndex(_indexName)
            {
                Fields = builder.Build(typeof(DemoIndex))
            };

            try
            {
                indexClient.GetIndex(index.Name);
                indexClient.DeleteIndex(index.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                //if the specified index not exist, 404 will be thrown.
            }

            try
            {
                await indexClient.CreateIndexAsync(index);
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Failed to create the index", ex);
            }

            return index;
        }

        private async Task<SearchIndexer> CreateDemoIndexerAsync(SearchIndexerClient indexerClient, SearchIndexerDataSourceConnection dataSource, SearchIndexerSkillset skillSet, SearchIndex index)
        {
            IndexingParameters indexingParameters = new IndexingParameters()
            {
                MaxFailedItems = -1,
                MaxFailedItemsPerBatch = -1,
            };
            indexingParameters.Configuration.Add("dataToExtract", "contentAndMetadata");
            indexingParameters.Configuration.Add("imageAction", "generateNormalizedImages");

            SearchIndexer indexer = new SearchIndexer(_indexerName, dataSource.Name, index.Name)
            {
                Description = "Demo Indexer",
                SkillsetName = skillSet.Name,
                Parameters = indexingParameters
            };

            FieldMappingFunction mappingFunction = new FieldMappingFunction("base64Encode");
            mappingFunction.Parameters.Add("useHttpServerUtilityUrlTokenEncode", true);
            indexer.FieldMappings.Add(new FieldMapping("metadata_storage_path")
            {
                TargetFieldName = "id",
                MappingFunction = mappingFunction

            });

            indexer.FieldMappings.Add(new FieldMapping("metadata_storage_name")
            {
                TargetFieldName = "fileName"
            });

            indexer.FieldMappings.Add(new FieldMapping("metadata_storage_path")
            {
                TargetFieldName = "fileLocation"
            });

            indexer.FieldMappings.Add(new FieldMapping("content")
            {
                TargetFieldName = "content"
            });

            indexer.OutputFieldMappings.Add(new FieldMapping("/document/pages/*/organizations/*")
            {
                TargetFieldName = "organizations"
            });
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/pages/*/keyPhrases/*")
            {
                TargetFieldName = "keyPhrases"
            });
            indexer.OutputFieldMappings.Add(new FieldMapping("/document/languageCode")
            {
                TargetFieldName = "languageCode"
            });

            try
            {
                indexerClient.GetIndexer(indexer.Name);
                indexerClient.DeleteIndexer(indexer.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                //if the specified indexer not exist, 404 will be thrown.
            }

            try
            {
                await indexerClient.CreateIndexerAsync(indexer);
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Failed to create the indexer", ex);
            }

            return indexer;
        }

        private static async Task<string> CheckIndexerOverallStatusAsync(SearchIndexerClient indexerClient, string indexerName)
        {
            try
            {
                var demoIndexerExecutionInfo = await indexerClient.GetIndexerStatusAsync(indexerName);

                if(demoIndexerExecutionInfo.Value?.Status != null)
                {
                    return demoIndexerExecutionInfo.Value.Status.ToString();
                }
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Failed to get indexer overall status", ex);
            }
        }

        #endregion Private Methods
    }
}
