using System;

namespace survey_pro.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public string? Detail { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
