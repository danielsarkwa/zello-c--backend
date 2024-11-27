using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Features.Tasks.Models;

public class AssignUserRequest {
    [JsonProperty("user_id")]
    [Required]
    public Guid UserId { get; set; }
}
