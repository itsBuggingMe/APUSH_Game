using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal class Vector2Converter : JsonConverter<Vector2>
    {
        public static readonly Vector2Converter Instance = new Vector2Converter();

        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string s = reader.GetString()!;
            int index = s.IndexOf(',');
            ReadOnlySpan<char> asSpan = s.AsSpan();
            return new Vector2(float.Parse(asSpan[..index]), float.Parse(asSpan[(index + 1)..]));
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.X},{value.Y}");
        }
    }

    internal class Vector2ArrConverter : JsonConverter<Vector2[]>
    {
        public override Vector2[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<Vector2>();

            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var vectors = new Vector2[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                int index = parts[i].IndexOf(',');
                ReadOnlySpan<char> asSpan = parts[i];
                vectors[i] = new Vector2(float.Parse(asSpan[..index]), float.Parse(asSpan[(index + 1)..]));
            }

            return vectors;
        }

        public override void Write(Utf8JsonWriter writer, Vector2[] value, JsonSerializerOptions options)
        {
            StringBuilder sb = new();
            for(int i = 0; i < value.Length; i++)
            {
                sb  .Append(value[i].X)
                    .Append(',')
                    .Append(value[i].Y)
                    .Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            writer.WriteStringValue(sb.ToString());
        }
    }
}
