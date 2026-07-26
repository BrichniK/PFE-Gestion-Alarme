using System.Text.Json;
using System.Text.Json.Serialization;

namespace CollectManagement.Infrastructure.Common;

public class NullableUlidJsonConverter : JsonConverter<Ulid>
{
    public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Ulid.Empty;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
            {
                return Ulid.Empty;
            }

            if (Ulid.TryParse(value, out var ulid))
            {
                return ulid;
            }
        }

        // Fallback to default parsing for other token types or if parsing fails
        // The default behavior might throw an exception, which is what we want to avoid for null/empty string
        // but might be desired for malformed strings.
        // For this specific case, we are returning empty.
        return Ulid.Empty;
    }

    public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
} 