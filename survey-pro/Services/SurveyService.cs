using System;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using survey_pro.Dtos;
using survey_pro.Interfaces;
using survey_pro.Models;
using survey_pro.Settings;

namespace survey_pro.Services;

public class SurveyService : ISurveyService
{

    private readonly IMongoCollection<Survey> _surveys;
    private readonly IMongoCollection<SurveyResponse> _surveyResponses;

    private readonly IFileStorageService _fileStorageService;



    public SurveyService(
          IOptions<MongoDbSettings> mongoSettings,
          IFileStorageService fileStorageService
          )
    {
        var client = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoSettings.Value.AppDatabaseName);
        _surveys = database.GetCollection<Survey>(mongoSettings.Value.SurveysCollection);
        _surveyResponses = database.GetCollection<SurveyResponse>(mongoSettings.Value.ResponsesCollection);

        _fileStorageService = fileStorageService;
    }

    public async Task<List<Survey>> GetAllSurveysAsync()
    {
        return await _surveys.Find(_ => true).ToListAsync();
    }

    public async Task<Survey> GetSurveyByIdAsync(string id)
    {
        return await _surveys.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Survey> CreateSurveyAsync(SurveyDto surveyDto, string userId)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)
            }
        };


        var questions = JsonSerializer.Deserialize<List<QuestionDto>>(surveyDto.QuestionsJson, options);
        var survey = new Survey
        {
            Title = surveyDto.Title,
            Description = surveyDto.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsActive = surveyDto.IsActive,
            Questions = new List<Question>()
        };

        if (surveyDto.CoverImage != null)
        {
            survey.CoverImageUrl = await _fileStorageService.SaveFileAsync(surveyDto.CoverImage, "survey-covers");
        }

        // Process questions
        foreach (var questionDto in questions)
        {
            var question = new Question
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Title = questionDto.Title,
                Description = questionDto.Description,
                Type = questionDto.Type,
                Options = questionDto.Options,
                IsRequired = questionDto.IsRequired
            };

            if (questionDto.Image != null)
            {
                question.ImageUrl = await _fileStorageService.SaveFileAsync(questionDto.Image, "question-images");
            }

            survey.Questions.Add(question);
        }

        survey.CreatedAt = DateTime.UtcNow;
        await _surveys.InsertOneAsync(survey);
        return survey;
    }


    public async Task<SurveyResponse> RespondToSurveyAsync(string surveyId, string? userId, List<QuestionResponse> responses)
    {
        var survey = await _surveys.Find(s => s.Id == surveyId && s.IsActive).FirstOrDefaultAsync();
        if (survey == null)
        {
            throw new KeyNotFoundException("Survey not found or is inactive");
        }

        var requiredQuestionIds = survey.Questions!
            .Where(q => q.IsRequired)
            .Select(q => q.Id)
            .ToHashSet();

        var answeredQuestionIds = responses
            .Select(r => r.QuestionId)
            .ToHashSet();

        if (!requiredQuestionIds.All(id => answeredQuestionIds.Contains(id)))
        {
            throw new ArgumentException("Not all required questions were answered");
        }

        var surveyResponse = new SurveyResponse
        {
            SurveyId = surveyId,
            RespondentId = userId,
            SubmittedAt = DateTime.UtcNow,
            Responses = responses.Select(r => new QuestionResponse
            {
                QuestionId = r.QuestionId,
                Answer = r.Answer,
                SelectedOptions = r.SelectedOptions
            }).ToList()
        };

        await _surveyResponses.InsertOneAsync(surveyResponse);
        return surveyResponse;
    }

    public async Task<List<SurveyResponse>> GetSurveyResponsesAsync(string surveyId)
    {
        var responses = await _surveyResponses
            .Find(r => r.SurveyId == surveyId)
            .ToListAsync();

        if (!responses.Any())
        {
            throw new KeyNotFoundException("No responses found for this survey");
        }

        return responses;
    }


    public async Task<bool> UpdateSurveyAsync(string id, SurveyUpdateDto surveyDto)
    {
        var existingSurvey = await _surveys.Find(s => s.Id == id).FirstOrDefaultAsync();

        if (existingSurvey == null)
        {
            return false;
        }

        existingSurvey.Title = surveyDto.Title;
        existingSurvey.Description = surveyDto.Description;
        existingSurvey.IsActive = surveyDto.IsActive;
        existingSurvey.UpdatedAt = DateTime.UtcNow;

        if (surveyDto.CoverImage != null)
        {
            if (!string.IsNullOrEmpty(existingSurvey.CoverImageUrl))
            {
                _fileStorageService.DeleteFile(existingSurvey.CoverImageUrl);
            }

            existingSurvey.CoverImageUrl = await _fileStorageService.SaveFileAsync(surveyDto.CoverImage, "survey-covers");
        }

        var existingQuestions = new Dictionary<string, Question>();
        foreach (var question in existingSurvey.Questions ?? [])
        {
            if (!string.IsNullOrEmpty(question.Id))
            {
                existingQuestions[question.Id] = question;
            }
        }

        var updatedQuestions = new List<Question>();
        foreach (var questionDto in surveyDto.Questions)
        {
            Question question;

            if (!string.IsNullOrEmpty(questionDto.Id) && existingQuestions.ContainsKey(questionDto.Id))
            {
                question = existingQuestions[questionDto.Id];
                question.Title = questionDto.Title;
                question.Description = questionDto.Description;
                question.Type = questionDto.Type;
                question.Options = questionDto.Options;
                question.IsRequired = questionDto.IsRequired;

                existingQuestions.Remove(questionDto.Id);
            }
            else
            {
                question = new Question
                {
                    Title = questionDto.Title,
                    Description = questionDto.Description,
                    Type = questionDto.Type,
                    Options = questionDto.Options,
                    IsRequired = questionDto.IsRequired
                };
            }

            if (questionDto.Image != null)
            {
                if (!string.IsNullOrEmpty(question.ImageUrl))
                {
                    _fileStorageService.DeleteFile(question.ImageUrl);
                }

                question.ImageUrl = await _fileStorageService.SaveFileAsync(questionDto.Image, "question-images");
            }

            updatedQuestions.Add(question);
        }

        foreach (var removedQuestion in existingQuestions.Values)
        {
            if (!string.IsNullOrEmpty(removedQuestion.ImageUrl))
            {
                _fileStorageService.DeleteFile(removedQuestion.ImageUrl);
            }
        }

        existingSurvey.Questions = updatedQuestions;

        existingSurvey.UpdatedAt = DateTime.UtcNow;
        var result = await _surveys.ReplaceOneAsync(s => s.Id == id, existingSurvey);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteSurveyAsync(string id)
    {
        var survey = await _surveys.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (survey == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(survey.CoverImageUrl))
        {
            _fileStorageService.DeleteFile(survey.CoverImageUrl);
        }

        foreach (var question in survey.Questions ?? [])
        {
            if (!string.IsNullOrEmpty(question.ImageUrl))
            {
                _fileStorageService.DeleteFile(question.ImageUrl);
            }
        }
        var result = await _surveys.DeleteOneAsync(s => s.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    // public async Task<Survey> ImportFromGoogleFormsAsync(ImportGoogleFormsDto importDto, string userId)
    // {
    //     string coverImageUrl = null;

    //     if (importDto.CoverImage != null)
    //     {
    //         coverImageUrl = await _fileStorageService.SaveFileAsync(importDto.CoverImage, "survey-covers");
    //     }

    //     var survey = await _googleFormsImportService.ImportQuestionsFromGoogleForm(
    //         importDto.FormId,
    //         importDto.Title,
    //         importDto.Description,
    //         coverImageUrl,
    //         userId
    //     );
    //     survey.CreatedAt = DateTime.UtcNow;
    //     await _surveys.InsertOneAsync(survey);
    //     return survey;
    // }
}
