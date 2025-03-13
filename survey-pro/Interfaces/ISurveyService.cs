using System;
using survey_pro.Dtos;
using survey_pro.Models;

namespace survey_pro.Interfaces;

public interface ISurveyService
{
    Task<List<Survey>> GetAllSurveysAsync();
    Task<Survey> GetSurveyByIdAsync(string id);
    Task<Survey> CreateSurveyAsync(SurveyDto surveyDto, string? userId);
    Task<bool> UpdateSurveyAsync(string id, SurveyUpdateDto surveyDto);
    Task<bool> DeleteSurveyAsync(string id);
    // Task<Survey> ImportFromGoogleFormsAsync(ImportGoogleFormsDto importDto, string? userId);
}
