namespace DiscordStatusChecker
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Text.Json;

    public static class Program
    {
        public const string WEBHOOK_USERNAME = "DC Status";
        public const string WEBHOOK_AVATAR_URL = "https://assets-global.website-files.com/6257adef93867e50d84d30e2/636e0a6a49cf127bf92de1e2_icon_clyde_blurple_RGB.png";
        public const string CONTENT_FORMAT_STRING = "**Discord Incident** (Updated: <t:{0}:F>) - **{1}**:\n{2}";

        public static async Task Main(string[] args)
        {
            DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
            using (HttpClient client = new HttpClient())
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(await client.GetStringAsync("https://discordstatus.com/history.atom"));
                string updated = doc.GetElementsByTagName("updated")[0]!.InnerText;
                DateTime updatedDate = DateTime.Parse(updated);
                long timestamp = ((DateTimeOffset)updatedDate).ToUnixTimeSeconds();
                bool fileNeedsUpdate = true;
                if (File.Exists("lastUpdatedValue"))
                {
                    string lastUpdatedValue = File.ReadAllText("lastUpdatedValue");
                    if (lastUpdatedValue.Trim().Equals(updated.Trim()))
                    {
                        Console.WriteLine("Already latest version");
                        fileNeedsUpdate = false;
                    }
                    else
                    {
                        Console.WriteLine("Needs update");
                        XmlNode entry = doc.GetElementsByTagName("entry")[0]!;
                        string title = "N/A";
                        string content = "N/A";
                        foreach (XmlNode node in entry.ChildNodes)
                        {
                            if (node.Name == "title") title = node.InnerText.Trim();
                            if (node.Name == "content") content = node.InnerText.Trim();
                        }
                        var config = new ReverseMarkdown.Config {
                        	UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.Bypass
                        };
                        var converter = new ReverseMarkdown.Converter(config);
                        content = converter.Convert(content);
                        Console.WriteLine((await PostDiscordMessage(client, timestamp, title, content)).StatusCode);
                    }
                }
                else
                {
                    Console.WriteLine("File does not exist");
                }
                if (fileNeedsUpdate) File.WriteAllText("lastUpdatedValue", updated);
            }
        }
        
        private static async Task<HttpResponseMessage> PostDiscordMessage(HttpClient client, long timestamp, string title, string incidentContent)
        {
            string url = $"{Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL")}?wait=true";
            WebhookData webhookData = new WebhookData
            {
                Username = WEBHOOK_USERNAME,
                AvatarUrl = WEBHOOK_AVATAR_URL,
                AllowedMentions = new Dictionary<string, string[]>{
                    { "parse", new string[0] }
                },
                Content = String.Format(CONTENT_FORMAT_STRING, timestamp, title, incidentContent)
            };
            Console.WriteLine(webhookData.Content);
            string webhookJson = JsonSerializer.Serialize<WebhookData>(webhookData);
            StringContent content = new StringContent(webhookJson, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }
    }
}
