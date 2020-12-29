using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Demo.EnrichedSearch.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Demo.EnrichedSearch.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            string serviceName = Configuration["SearchService_ServiceName"];
            string apiKey = Configuration["SearchService_ApiKey"];
            string indexName = Configuration["SearchService_HotelIndexName"];
            string storageConnectionString = Configuration["AzureBlobConnectionString"];
            string containerName = Configuration["AzureBlobContainerName"];
            string indexAiName = Configuration["AiSearchIndexName"];
            string indexerAiName = Configuration["AiSearchIndexerName"];
            string cognitiveServiceKey = Configuration["CognitiveServiceKey"];

            // Create a SearchIndexClient to send create/delete index commands
            Uri serviceEndpoint = new Uri($"https://{serviceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            SearchIndexClient adminClient = new SearchIndexClient(serviceEndpoint, credential);
            SearchIndexerClient indexerClient = new SearchIndexerClient(serviceEndpoint, credential);

            // Create a SearchClient to load and query documents
            SearchClient srchclient = new SearchClient(serviceEndpoint, indexName, credential);
            SearchClient aiSrchclient = new SearchClient(serviceEndpoint, indexAiName, credential);

            services.AddTransient<ISearchIndexService, SearchIndexService>(s => new SearchIndexService(srchclient, adminClient));
            services.AddTransient<ISearchService, SearchService>(s => new SearchService(srchclient));
            services.AddTransient<IAiSearchIndexService, AiSearchIndexService>(s => new AiSearchIndexService(adminClient, indexerClient,
                storageConnectionString, containerName, indexAiName, indexerAiName, cognitiveServiceKey));
            services.AddTransient<IAiSearchService, AiSearchService>(s => new AiSearchService(aiSrchclient));
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
