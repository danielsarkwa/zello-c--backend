using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Features.Tasks.Models;

public class AddCommentRequest {
    [Required]
    [StringLength(500)]
    [JsonProperty("content")]
    public required string Content { get; set; }
}
