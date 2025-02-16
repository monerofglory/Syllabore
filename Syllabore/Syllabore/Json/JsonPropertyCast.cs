﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Syllabore.Json
{
    public class JsonPropertyCast<T> : JsonConverter<T>
    {
        private Type TargetType;

        public JsonPropertyCast(Type targetType)
        {
            this.TargetType = targetType;
        }
        
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (T)JsonSerializer.Deserialize(ref reader, this.TargetType, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, this.TargetType, options);
        }
    }
}
