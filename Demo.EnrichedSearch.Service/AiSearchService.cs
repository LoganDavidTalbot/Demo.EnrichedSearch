using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Demo.EnrichedSearch.Shared.Models;
using Demo.EnrichedSearch.Shared.Models.AiSearch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.EnrichedSearch.Service
{
    public class AiSearchService : IAiSearchService
    {
        private readonly SearchClient _searchclient;
        private readonly string _accountName;
        private readonly string _accountKey;
        private readonly string _containerName;
        public AiSearchService(SearchClient searchclient,
            string accountName,
            string accountKey,
            string containerName)
        {
            _searchclient = searchclient;
            _accountName = accountName;
            _accountKey = accountKey;
            _containerName = containerName;
        }
        public async Task<AiSearchResponse> RunQueriesAsync(string searchQuery, int page)
        {
            SearchResults<DemoIndex> response;
            var options = new SearchOptions
            {
                Filter = "",
                SearchMode = SearchMode.All,
                // Skip past results that have already been returned.
                Skip = (page - 1) * GlobalVariables.ResultsPerPage,
                // Take only the next page worth of results.
                Size = GlobalVariables.ResultsPerPage,
                // Include the total number of results.
                IncludeTotalCount = true
            };

            // Enter Hotel property names into this list, so only these values will be returned.
            options.Select.Add("id");
            options.Select.Add("fileName");
            options.Select.Add("fileLocation");

            response = await _searchclient.SearchAsync<DemoIndex>(searchQuery, options);

            var demIndexShorts = new List<DemoIndexShort>();
            await foreach (var demo in response.GetResultsAsync())
            {
                var fileUri = await GetUserDelegationSasBlob(demo.Document.FileName);
                demIndexShorts.Add(new DemoIndexShort()
                {
                    Id = demo.Document.Id,
                    FileName = demo.Document.FileName,
                    FileLocation = fileUri
                });
            }

            var model = new AiSearchResponse()
            {
                ResultList = demIndexShorts,
                TotalCount = response.TotalCount.Value
            };
            model.pageCount = ((int)response.TotalCount + GlobalVariables.ResultsPerPage - 1) / GlobalVariables.ResultsPerPage;

            // This variable communicates the page number being displayed to the view.
            model.currentPage = page;
            if (model.currentPage - 1 < 1)
            {
                model.previousPage = null;
            }
            else
            {
                model.previousPage = model.currentPage - 1;
            }

            if (model.currentPage + 1 > model.pageCount)
            {
                model.nextPage = null;
            }
            else
            {
                model.nextPage = model.currentPage + 1;
            }

            return model;
        }

        private async Task<Uri> GetUserDelegationSasBlob(string blobName)
        {
            Uri serviceUri = new Uri($"https://{_accountName}.blob.core.windows.net");
            StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(_accountName, _accountKey);
            BlobServiceClient blobServiceClient = new BlobServiceClient(serviceUri, sharedKeyCredential);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            PageBlobClient pageBlobClient = containerClient.GetPageBlobClient(blobName);
            // Create a SAS token that's valid for 7 days.
            BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
            };
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            BlobUriBuilder sasUriBuilder = new BlobUriBuilder(containerClient.Uri)
            {
                Query = blobSasBuilder.ToSasQueryParameters(sharedKeyCredential).ToString()
            };


            Uri containerSasUri = sasUriBuilder.ToUri();

            BlobUriBuilder blobUriBuilder = new BlobUriBuilder(containerSasUri)
            {
                BlobName = pageBlobClient.Name
            };

            return blobUriBuilder.ToUri();
        }
    }
}
