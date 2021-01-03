# Azure Cognitive Search Demo

## Introduction
This repository is used to demo and investigate the capabilities of the Azure Cognitive Search Service. The repository is split into two parts. One part which demos the a enriched search experience and the second part that demos searching files using Azure Cognitive Services to index (collect) the data. With this repository you can easily build and deploy a working solution of Azure Cognitive Search using Azure DevOps (see Setup).

### Useful Links
- [What is Azure Cognitive Search](https://docs.microsoft.com/en-gb/azure/search/search-what-is-azure-search)
- [Azure Cogitive Search Documentation](https://docs.microsoft.com/en-gb/azure/search/)
- [JFK Demo](https://github.com/microsoft/AzureSearch_JFK_Files)

## Demo Desciprition

### Part 1: Enriched Search
The enriched search part of the demo, demonstrates Azure Cognitive Searches capabilities as a search engine. The features it includes are:
- Creating an index.
- Populating the indexer with static dummy data.
- Deleting the index.
- Getting statistics about the index.
- Searching the index.
- Filtering through the use of facets.
- Providing autocomplete and autosuggest functionally as the user is typing.
- Paging the returned search results.

### Part 2: Enriched Search with AI indexed
The enriched search with AI indexed part of the demo is more about focusing on connecting and processing files to be indexed using an indexer and AI based skillsets. This will then allow the files to be searchable. The features it includes are:
- Creating an index.
- Creating a skillset.
  - OCR Skill.
  - Language detection skill.
  - Entity Recognition skill.
  - Key Phrase Extraction skill.
  - Merge Skill.
  - Split skill.
- Creates a Indexer Data Source connection to an container.
- Creates an indexer which uses skillsets and connects to an Indexer Data Source connection.
- Search the index.
- Paging the returned search results.
- Downloading the a selected file from blob storage using a SAS Token.

## Repository Sturture

### Demo.EnrichedSearch.IaC
The project/ folder where all of the yaml files, powershell and ARM templetes are to build, deploy and clean up resource after use.

YAML files:
- azure-pipelines.yml - builds and deploys the code and the ARM templates to a Azure Resource Group.
- delete-azure-pipelines.yml - deletes/ cleans up the Azure Resource Group and all of its internal resources.

ARM Templates:
- cognitiveService.json - Creates a Azure Cognitive Service.
- searchService.json = Creates an Azure Cognitive Search Service.
- storageAccount.json - Creates an Azure Storage Account and a blob container.
- webApp.json - Creates an Azure App Service plan and a Azure App Service.

Powershell files:
- Add-AppSettings.ps1 - get key and connectionstrings from Azure services and add them to the App Settings in the Azure App Service.

### Demo.EnrichedSearch.Client
This project is a Blazor client webassembly application with 3 pages:
- Home page - Application root handing page.
- Enriched Search page - Allow users to create, delete and search an Azure Cognitive Search Index.
- Enriched Search with AI page - Allows user to create an index, indexer, and skillsets. It also allows you to search the created index.

### Demo.EnrichedSearch.Server
This project is a .NET 5.0 ASP.net Web API project which services the client application.

Endpoints:
- Enriched Searches
  - POST /EnrichedSearches/Create
  - DELETE /EnrichedSearches
  - POST /EnrichedSearches/Search
  - GET /EnrichedSearches/AutoSuggest
- Enriched Search with AI indexed
  - POST /AiSearches/Create
  - POST /AiSearches/Search
  - GET /AiSearches

### Demo.EnrichedSearch.Service
This project intereacts with the Azure Cognitive service API.

### Demo.EnrichedSearch.Shared
Common models which are used across the client, server and service layers.

## Setup
The repository is designed in a way so that you up and running without any code changes or creating Azure Services yourself. You just need an Azure DevOps Project with Access to Pipelines, an Azure Subscription, and 2 service connections which are connected to your Azure Subscprition and Github account.

To start off please fork the repository.

### Setup Service connection
Inorder to access the repository in Github and create resource in your subscription we require two service connections to do this.

The first one is to access the GitHub Repository. To do this:
- Go to the Project Settings in your Azure DevOps Project.
- Then Service Connections and click New service connection.
- Select GitHub and then next.
- Leave as Grant Authorization.
- Then Azure Pipelines and click Authorize.
- This will now to through the steps of authorizing the connection to your GitHub account.
- After you have done this click save.

The 2nd service connection is to connect an Azure scription.
- Go to the Project Settings in your Azure DevOps Project.
- Then Service Connections and click New service connection.
- Select Azure Resource Manager and then next.
- Select Service principal (automatic), then next.
- Select Subcription and in the scription dropdown select the Azure scription you want to deploy to.
- Leave Resource Group dropdown blank.
- Enter "Enriched Search Service Connection" into the Service connection name and ensure Grant access to permission to all pipelines is ticked.
- Click Save.

### Variable Group setup
We require a variable group to be setup so that the YAML pipelines can access it. The steps for this are:
- Pipelines > Libarary > click "+ Variable Group"
- Name the variable group "EnrichedSearch".
- Now add the below list of variables which the correct content.
  - azureServiceConnectionName - Name of the Azure service connection in the project settings.
  - cognitiveServiceName - Name of the Azure Cognitive Service the will be created (must be unqiue, no other resource of this type can have this name).
  - containerName - name of the container in side of the Account storage.
  - demoLocation - where the application will be hosted on azure e.g. West Europe.
  - demoResourceGroup - the resource group name where all of your resources will be created in.
  - demoSubscriptionId - the scription id of where the resource group will be created in.
  - searchServiceName - the name of the Azure Cognitive Search (must be unqiue, no other resource of this type can have this name).
  - storageAccountName - name of the storage account (must be unqiue, no other resource of this type can have this name).
  - webAppName - Azure App Service name (must be unqiue, no other resource of this type can have this name).
  - webAppPlanName -  Azure App Service Plan name (must be unqiue, no other resource of this type can have this name).
- Now click save at the top.

### YAML Pipeline Setup
In order to build and deploy the demo require to import Azure Pipeline YAML files. There are two YAML files one to building and deploying code and resources and one to clean up/ delete the resource once they are not needed.

Steps to import a YAML Pipelines file:
- In your Azure DevOps project, Pipelines,
- Click New pipeline.
- Select Github, when your forked repository.
- Select "Existing Azure Pipelines YAML file".
- Copy and paste `/Demo.EnrichedSearch.IaC/azure-pipelines.yml` for the YAML to building and deploying code and resources or `/Demo.EnrichedSearch.IaC/delete-azure-pipelines.yml` for the YAML to delete the resources.
- Now you can click run or save to complete.

### Adding files to the Azure Blob container
The final step is to add files to the Azure blob container after the build and deploy pipeline has released. These files can be any type of file e.g. pdf, png, ppt, word, etc.

# Using the application
After doing the setup process should now have an application which is hosted on your App Service. Go the App Services URL and an Blazor .NET application should upload.
