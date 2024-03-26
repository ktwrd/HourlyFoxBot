using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
/*
 *   Copyright 2024 Kate Ward <kate@dariox.club>
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */

#nullable enable
namespace HourlyFoxBot
{
    public class Program
    {
        public HttpClient HttpClient = new HttpClient();
        public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
        IncludeFields = true
        };

        private static void Main(string[] args) => new Program().AsyncMain(args).Wait();

        public async Task AsyncMain(string[] args)
        {
            string webhookUrl = Environment.GetEnvironmentVariable("HFX_DiscordWebhook");
            if (webhookUrl == null)
            {
                Console.WriteLine("Missing Environment Variable \"HFX_DiscordWebhook\"");
                Environment.Exit(1);
            }
            else
            {
                string str1 = await Boilerplate("fox");
                if (str1 != null)
                {
                    var msgData = new DiscordMessage()
                    {
                        Content = "",
                        Username = "HourlyFox",
                        AvatarUrl = "https://res.kate.pet/upload/e9fffe52-ebc4-4f44-bff0-927846126ba4/ezgif-5-141677e75c.jpg",
                        Embeds = new[]
                        {
                            new DiscordEmbed()
                            {
                                Color = 5814783,
                                Footer = new DiscordEmbed.DiscordEmbedFooter()
                                {
                                    Text = "made by @kate.pet"
                                },
                                Image = new DiscordEmbed.DiscordEmbedImage()
                                {
                                    Url = str1
                                }
                            }
                        }
                    };
                    string str2 = JsonSerializer.Serialize(msgData, SerializerOptions);
                    HttpResponseMessage httpResponseMessage = await this.HttpClient.PostAsync(webhookUrl, new StringContent(str2, null, "application/json"));
                    string result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(httpResponseMessage.StatusCode.ToString() + "\n\n" + result);
                }
            }
        }

        public class DiscordMessage
        {
            [JsonPropertyName("username")]
            public string? Username { get; set; }
            [JsonPropertyName("avatar_url")]
            public string? AvatarUrl { get; set; }
            [JsonPropertyName("content")]
            public string Content { get; set; }
            [JsonPropertyName("embeds")]
            public DiscordEmbed[]? Embeds { get; set; }
        }

        public class DiscordEmbed
        {
            [JsonPropertyName("color")]
            public int Color { get; set; }
            [JsonPropertyName("footer")]
            public DiscordEmbedFooter Footer { get; set; }
            [JsonPropertyName("image")]
            public DiscordEmbedImage Image { get; set; }

            public class DiscordEmbedFooter
            {
                [JsonPropertyName("text")]
                public string Text { get; set; }
            }

            public class DiscordEmbedImage
            {
                [JsonPropertyName("url")]
                public string Url { get; set; }
            }
        }

        public async Task<string?> Boilerplate(string animalType)
        {
            try
            {
                var result = await this.HttpClient.GetAsync("https://api.tinyfox.dev/img?animal=" + animalType + "&json");
                if ((int)result.StatusCode != 200)
                {
                    Console.WriteLine("Failed to fetch fox\n" + result.Content.ReadAsStringAsync().Result);
                    return null;
                }
                return (JsonSerializer.Deserialize<TinyFoxImageModel>(result.Content.ReadAsStringAsync().Result, Program.SerializerOptions) ?? throw new Exception("Deserialized content to null")).ImageLocation;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
        }
    }
}