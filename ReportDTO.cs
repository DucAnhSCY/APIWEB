using System;

namespace diendan2;

public class ReportDTO
{
    public int ReportId { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateReportDTO
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; }
}

public class UpdateReportStatusDTO
{
    public string Status { get; set; }
} 