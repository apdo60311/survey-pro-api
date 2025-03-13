using System;

namespace survey_pro.Models;

public class FilterSurveysRequest
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
