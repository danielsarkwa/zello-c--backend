using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace Zello.Domain.Entities.Api.User;

public static class ApiUserJsonConverter {
    public static ApiUser FromJson(string json) {
        var user = JsonConvert.DeserializeObject<ApiUser>(json, Converter.Settings);
        if (user == null) {
            throw new JsonSerializationException("Failed to deserialize ApiUser from JSON");
        }
        return user;
    }

    public static string ToJson(ApiUser user) =>
        JsonConvert.SerializeObject(user, Converter.Settings);
}

internal static class Converter {
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
    };
}
