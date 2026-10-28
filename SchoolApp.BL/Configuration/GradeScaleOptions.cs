namespace SchoolApp.BL.Configuration;

public sealed class GradeScaleOptions
{
    public const string SectionName = "GradeScale";

    public decimal? Minimum { get; set; }
    public decimal? Maximum { get; set; }
}
