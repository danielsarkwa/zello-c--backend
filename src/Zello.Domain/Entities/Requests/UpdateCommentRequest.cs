using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Domain.Entities.Requests;

public class UpdateCommentRequest {
    [JsonProperty("content")]
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;
}
