using System;

namespace survey_pro.Models;

public class AddQuestionRequest
{
    public string Text { get; set; }
    public QuestionType Type { get; set; }
    public IFormFile Image { get; set; }
    public List<string> Options { get; set; }
    public bool IsRequired { get; set; }
}
