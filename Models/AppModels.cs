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
    // Note: The prompt implies a simple object, but APIs often return arrays.
    // We will handle the deserialization logic in the service.
    public class Poem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("poemText")]
        // Note: Adjust property name based on actual API JSON key if different
        // Some poem APIs use "content" or "lines".
        public string Content { get; set; }
    }
}
