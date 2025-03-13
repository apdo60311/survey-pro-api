using System;

namespace survey_pro.Models;

public class CreateSurveyRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IFormFile CoverImage { get; set; }
    public List<Question> Questions { get; set; }
}
