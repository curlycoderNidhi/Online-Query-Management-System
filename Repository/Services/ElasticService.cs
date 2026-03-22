using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Repository.Models;

public class ElasticService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticService(IConfiguration config)
    {
        var cloudId = config["Elasticsearch:CloudId"];
        var apiKey = config["Elasticsearch:ApiKey"];
        _indexName = config["Elasticsearch:IndexName"];

        if (string.IsNullOrEmpty(cloudId))
            throw new Exception("CloudId missing");

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("ApiKey missing");

        if (string.IsNullOrEmpty(_indexName))
            throw new Exception("IndexName missing");

        var settings = new ElasticsearchClientSettings(cloudId, new ApiKey(apiKey))
            .DefaultIndex(_indexName);

        _client = new ElasticsearchClient(settings);
    }

   public async Task UpdateQuery(Query model)
{
    try
    {
        // 🔥 SAFETY CHECK
        if (model.QueryId == 0)
        {
            Console.WriteLine("❌ QueryId is 0, skipping elastic");
            return;
        }

        // ✅ DEBUG LINE ADD HERE 👇
        Console.WriteLine($"🔍 Elastic Sync ID: {model.QueryId}");

        Console.WriteLine($"📤 Sending to Elastic: QueryId = {model.QueryId}");

        var response = await _client.UpdateAsync<Query, Query>(model.QueryId, u => u
            .Index(_indexName)
            .Doc(model)
            .DocAsUpsert(true)
            .Refresh(Refresh.WaitFor)
        );

        if (!response.IsValidResponse)
        {
            Console.WriteLine("❌ Elastic failed:");
            Console.WriteLine(response.DebugInformation);
        }
        else
        {
            Console.WriteLine("✅ Elastic insert/update success");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Elastic exception: " + ex.Message);
    }
}
}