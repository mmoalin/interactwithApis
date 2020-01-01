using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArtistStats_web.Models;

namespace ArtistStats_web.Helpers
{
    public class DeserializeJson<T>
    {
        public DeserializeJson()
        {

        }
        public T Deserialize(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
