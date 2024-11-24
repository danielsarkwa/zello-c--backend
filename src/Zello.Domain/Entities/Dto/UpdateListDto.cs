using Newtonsoft.Json;

namespace Zello.Domain.Entities.Dto;

public class UpdateListDto {
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("position")]
    public int Position { get; set; }
}
