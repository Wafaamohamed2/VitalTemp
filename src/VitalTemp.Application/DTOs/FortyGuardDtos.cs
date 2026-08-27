namespace VitalTemp.Application.DTOs;

public class FortyGuardHeatmapRequest
{
    public string City { get; set; } = "Phoenix";
    public double MinLat { get; set; } = 33.4000;
    public double MaxLat { get; set; } = 33.5000;
    public double MinLng { get; set; } = -112.1300;
    public double MaxLng { get; set; } = -112.0300;
    public string Date { get; set; } = "2023-08-18";
}

public class FortyGuardSubmitResponse
{
    public string ActivityId { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING"; // PENDING, PROCESSING, COMPLETED, FAILED
    public string Message { get; set; } = string.Empty;
}

public class HeatmapPointDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TemperatureF { get; set; }
    public double TemperatureC { get; set; }
}

public class FortyGuardHeatmapResponse
{
    public string ActivityId { get; set; } = string.Empty;
    public string Status { get; set; } = "COMPLETED"; // PENDING, PROCESSING, COMPLETED, FAILED
    public List<HeatmapPointDto> HeatPoints { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool FromCache { get; set; }
    public bool IsLiveApiCall { get; set; }
}
