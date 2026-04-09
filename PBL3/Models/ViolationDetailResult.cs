namespace PBL3.Models;

public class ViolationDetailResult
{
    public int RecordId { get; set; }
    public string HeaderTitle { get; set; } = string.Empty;
    public string HeaderSubtitle { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string ViolationDate { get; set; } = string.Empty;
    public string ViolationTime { get; set; } = string.Empty;
    public string ViolationLocation { get; set; } = string.Empty;
    public string ViolationDescription { get; set; } = string.Empty;
    public string FineRange { get; set; } = string.Empty;
    public string PaymentLocation { get; set; } = string.Empty;
    public string PointsDeducted { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public string EvidenceImagePath { get; set; } = string.Empty;
    public string EvidenceCaption { get; set; } = string.Empty;
    public string LastUpdated { get; set; } = string.Empty;
}
