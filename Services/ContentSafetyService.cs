using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ContentSafetyDemo.Services
{
    public class ContentSafetyService : IContentSafety
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ContentSafetyService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<ContentSafetyResponse> AnalyzeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("text must be provided", nameof(text));

            var endpoint = _config["AzureContentSafety:Endpoint"];
            var apiKey = _config["AzureContentSafety:ApiKey"];

            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("AzureContentSafety:Endpoint and AzureContentSafety:ApiKey must be configured.");

            if (!endpoint.EndsWith('/'))
                endpoint += '/';

            var url = $"{endpoint}contentsafety/text:analyze?api-version=2023-10-01";

            var payload = new { text = text };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Content Safety API failed: {responseContent}");
            }

            // Defensive parsing of the response
            using var doc = JsonDocument.Parse(responseContent);
            var categories = new List<CategoryResult>();

            if (doc.RootElement.TryGetProperty("categoriesAnalysis", out var categoriesElement) &&
                categoriesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in categoriesElement.EnumerateArray())
                {
                    var category = item.TryGetProperty("category", out var c) && c.ValueKind == JsonValueKind.String
                        ? c.GetString()
                        : null;

                    var severity = item.TryGetProperty("severity", out var s) && s.ValueKind == JsonValueKind.Number
                        ? s.GetInt32()
                        : 0;

                    if (!string.IsNullOrEmpty(category))
                    {
                        categories.Add(new CategoryResult
                        {
                            Category = category,
                            Severity = severity
                        });
                    }
                }
            }

            return new ContentSafetyResponse
            {
                CategoriesAnalysis = categories
            };
        }
    }
}