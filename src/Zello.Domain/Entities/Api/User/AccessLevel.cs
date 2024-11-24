using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zello.Domain.Entities.Api.User;

[JsonConverter(typeof(StringEnumConverter))]
public enum AccessLevel {
    [JsonProperty("Guest User")]
    Guest = 0,

    [JsonProperty("Team Member")]
    Member = 10,

    [JsonProperty("Workspace Owner")]
    Owner = 20,

    [JsonProperty("Administrator")]
    Admin = 30
}
