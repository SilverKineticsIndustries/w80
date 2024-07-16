namespace SilverKinetics.w80.Application.DTOs;

public record StatisticsDto
{
    public List<ApplicationStateCountsDto> ApplicationRejectionStateCounts { get; set; } = [];

    public decimal AverageApplicationLifetimeInSeconds { get; set; }
}