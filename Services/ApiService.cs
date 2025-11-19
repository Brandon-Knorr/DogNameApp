using DogPoemApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DogPoemApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string DogApiUrl = "https://dog.ceo/api/breeds/image/random";
        // Note: If BeanPoems is unavailable, this serves as the placeholder structure
        private const string PoemApiUrl = "https://www.beanpoems.com/api/poems/random";

        public ApiService()
        {
            _httpClient = new HttpClient();
            // Many APIs reject requests without a User-Agent
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MauiStudentApp");
        }

        public async Task<DogResponse> GetRandomDogAsync()
        {
            try
            {
                // Fetch and deserialize
                return await _httpClient.GetFromJsonAsync<DogResponse>(DogApiUrl);
            }
            catch (Exception ex)
            {
                // Return a fallback or null to be handled by ViewModel
                System.Diagnostics.Debug.WriteLine($"Error fetching dog: {ex.Message}");
                return null;
            }
        }

        public async Task<Poem> GetRandomPoemAsync()
        {
            try
            {
                // Depending on the API, it might return a single object or a list.
                // We handle a generic response here.
                var response = await _httpClient.GetAsync(PoemApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // Check if it's an array or single object
                    if (json.TrimStart().StartsWith("["))
                    {
                        var poems = JsonSerializer.Deserialize<List<Poem>>(json);
                        return poems?.FirstOrDefault();
                    }
                    else
                    {
                        return JsonSerializer.Deserialize<Poem>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching poem: {ex.Message}");
            }
            return null;
        }
    }
}
