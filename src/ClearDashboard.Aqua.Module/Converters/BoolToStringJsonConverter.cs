using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClearDashboard.Aqua.Module.Converters
{
    public class BoolToStringJsonConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
