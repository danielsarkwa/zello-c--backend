namespace Zello.Domain.Entities;

public class AppErrorResponse {
    public string ErrorMessage { get; set; }
    public int? ErrorCode { get; set; }
    public string? Reason { get; set; }
    public string? Details { get; set; }

    public AppErrorResponse(string errorMessage, int? errorCode = null, string? reason = null, string? details = null) {
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Reason = reason;
        Details = details;
    }
}

// Example usage:
// return BadRequest(new AppErrorResponse("Invalid request", errorCode: 400, reason: "Missing parameters", details: "Username is required"));
