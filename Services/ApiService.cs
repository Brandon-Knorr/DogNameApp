using DogPoemApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DogPoemApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string DogApiUrl = "https://dog.ceo/api/breeds/image/random";
        private const string PoemApiUrl = "https://www.beanpoems.com/api/poems/random";

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiStudentApp");
        }

        public async Task<DogResponse> GetRandomDogAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DogResponse>(DogApiUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching dog: {ex.Message}");
                return null;
            }
        }

        public async Task<Poem> GetRandomPoemAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(PoemApiUrl);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json)) return null;

                // Log raw JSON so you can inspect actual shape in Output (Debug).
                Debug.WriteLine("BeanPoems JSON:");
                Debug.WriteLine(json);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // If API returns an array, use first element
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                    root = root[0];

                // Helpers
                static string CleanHtmlAndDecode(string raw)
                {
                    if (string.IsNullOrWhiteSpace(raw)) return raw;
                    // convert common <br> to newline first so splitting preserves breaks
                    raw = raw.Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
                             .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase);
                    // remove any remaining tags
                    raw = Regex.Replace(raw, "<.*?>", string.Empty);
                    // decode HTML entities
                    return WebUtility.HtmlDecode(raw).Trim();
                }

                static List<string> ExtractLinesFromJsonElement(JsonElement el)
                {
                    var lines = new List<string>();
                    if (el.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in el.EnumerateArray())
                            if (item.ValueKind == JsonValueKind.String)
                                lines.Add(item.GetString());
                    }
                    else if (el.ValueKind == JsonValueKind.String)
                    {
                        var raw = CleanHtmlAndDecode(el.GetString());
                        if (!string.IsNullOrEmpty(raw))
                            lines.AddRange(raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
                    }
                    return lines;
                }

                // Try typical keys
                string TryGetString(params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (root.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.String)
                            return CleanHtmlAndDecode(p.GetString());
                    }
                    return null;
                }

                List<string> TryGetLines(params string[] keys)
                {
                    foreach (var key in keys)
                    {
                        if (root.TryGetProperty(key, out var p))
                        {
                            var extracted = ExtractLinesFromJsonElement(p);
                            if (extracted.Count > 0) return extracted;
                        }
                    }
                    return new List<string>();
                }

                var title = TryGetString("title", "name");
                var author = TryGetString("author", "poet", "writer", "by", "creator");

                var contentLines = TryGetLines("poem", "poemText", "content", "lines", "text", "body");

                // Deep search fallback: scan properties for a long string or an array of strings
                if ((string.IsNullOrWhiteSpace(author) || contentLines.Count == 0) && root.ValueKind == JsonValueKind.Object)
                {
                    void DeepSearch(JsonElement element, ref string foundAuthor, ref List<string> foundContent)
                    {
                        if (element.ValueKind != JsonValueKind.Object) return;

                        foreach (var prop in element.EnumerateObject())
                        {
                            var nameLower = prop.Name.ToLowerInvariant();

                            // Author heuristics
                            if (foundAuthor == null && (nameLower.Contains("author") || nameLower.Contains("poet") || nameLower.Contains("by")))
                            {
                                if (prop.Value.ValueKind == JsonValueKind.String)
                                    foundAuthor = CleanHtmlAndDecode(prop.Value.GetString());
                            }

                            // Content heuristics
                            if (foundContent == null || foundContent.Count == 0)
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Array)
                                {
                                    var arr = ExtractLinesFromJsonElement(prop.Value);
                                    if (arr.Count > 0) foundContent = arr;
                                }
                                else if (prop.Value.ValueKind == JsonValueKind.String)
                                {
                                    var s = CleanHtmlAndDecode(prop.Value.GetString());
                                    if (!string.IsNullOrWhiteSpace(s) && (s.Contains("\n") || s.Length > 50))
                                        foundContent = s.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
                                }
                            }

                            if (prop.Value.ValueKind == JsonValueKind.Object)
                                DeepSearch(prop.Value, ref foundAuthor, ref foundContent);

                            if (foundAuthor != null && foundContent != null && foundContent.Count > 0)
                                return;
                        }
                    }

                    string foundA = author;
                    var foundC = contentLines.Count > 0 ? contentLines : new List<string>();
                    DeepSearch(root, ref foundA, ref foundC);
                    author = author ?? foundA;
                    if (contentLines.Count == 0 && foundC != null && foundC.Count > 0)
                        contentLines = foundC;
                }

                // Final safe defaults
                if (string.IsNullOrWhiteSpace(title)) title = "Untitled";
                if (string.IsNullOrWhiteSpace(author)) author = "Unknown";
                if (contentLines == null || contentLines.Count == 0)
                    contentLines = new List<string> { "(No poem body returned by API)" };

                return new Poem
                {
                    Title = title,
                    Author = author,
                    Content = contentLines
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching poem: {ex.Message}");
            }
            return null;
        }
    }
}
