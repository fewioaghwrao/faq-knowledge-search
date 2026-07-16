using System;
using System.Configuration;

namespace FaqKnowledgeSearch.WebForms.Settings
{
    public class AiSettings
    {
        public string ApiKey { get; set; }

        public string Endpoint { get; set; }

        public string Model { get; set; }

        public int MaxContextFaqCount { get; set; }

        public static AiSettings Load()
        {
            int maxContextFaqCount;

            if (!int.TryParse(
                    ConfigurationManager.AppSettings["AiMaxContextFaqCount"],
                    out maxContextFaqCount))
            {
                maxContextFaqCount = 5;
            }

            var apiKey =
                Environment.GetEnvironmentVariable("FAQ_AI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey =
                    ConfigurationManager.AppSettings["AiApiKey"];
            }

            return new AiSettings
            {
                ApiKey = apiKey,
                Endpoint =
                    ConfigurationManager.AppSettings["AiApiEndpoint"],
                Model =
                    ConfigurationManager.AppSettings["AiModel"],
                MaxContextFaqCount =
                    maxContextFaqCount <= 0
                        ? 5
                        : maxContextFaqCount
            };
        }
    }
}