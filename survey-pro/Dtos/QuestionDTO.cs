using System;
using survey_pro.Models;

namespace survey_pro.Dtos;

public class QuestionDto
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public QuestionType Type { get; set; }

    public List<string> Options { get; set; } = new List<string>();

    public bool IsRequired { get; set; }

    public IFormFile Image { get; set; }
}

public class SurveyDto
{
    public string Title { get; set; }

    public string Description { get; set; }

    public IFormFile CoverImage { get; set; }

    public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    public bool IsActive { get; set; }
}

public class SurveyUpdateDto
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public IFormFile CoverImage { get; set; }

    public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    public bool IsActive { get; set; }
}

public class ImportGoogleFormsDto
{
    public string FormId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public IFormFile CoverImage { get; set; }
}