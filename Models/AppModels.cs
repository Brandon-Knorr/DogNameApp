using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DogPoemApp.Models
{
    public class DogResponse
    {
        [JsonPropertyName("message")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    // 2. BeanPoems Model
    // https://www.beanpoems.com/api/poems/random
    // Note: The API may return the poem text as an array of lines or a single string.
    public class Poem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        // Changed to a list so ViewModel can join lines reliably.
        // ApiService will normalize whichever shape the API returns (string or array).
        public List<string> Content { get; set; }
    }
}
