using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harvest.Core.Utilities
{
    public static class JsonOptions
    {
        public static JsonSerializerOptions Standard => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        public static JsonSerializerOptions WithGeoJson(this JsonSerializerOptions options)
        {
            options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
            return options;
        }
    }
}
