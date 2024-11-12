using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;


namespace Zello.Domain.Entities.Api.Message;

public class SimpleMessage {
    [Required]
    [JsonProperty("Message")]
    public required string Message { get; set; }

    [JsonProperty("Reason")]
    public string? Reason { get; set; }

}
