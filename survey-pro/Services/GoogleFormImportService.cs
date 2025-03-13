// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Forms.v1;
// using Google.Apis.Forms.v1.Data;
// using Google.Apis.Services;
// using Microsoft.AspNetCore.Cors.Infrastructure;
// using survey_pro.Models;
// using SurveyApi.Models;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Threading.Tasks;

// namespace survey_pro.Services
// {
//     public interface IGoogleFormsImportService
//     {
//         Task<Survey> ImportQuestionsFromGoogleForm(string formId, string title, string description, string coverImageUrl, string userId);
//     }

//     public class GoogleFormsImportService : IGoogleFormsImportService
//     {
//         private readonly string _credentialsPath = "credentials.json";

//         public async Task<Survey> ImportQuestionsFromGoogleForm(string formId, string title, string description, string coverImageUrl, string userId)
//         {
//             try
//             {
//                 // Authenticate with Google
//                 GoogleCredential credential;
//                 using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
//                 {
//                     credential = GoogleCredential.FromStream(stream)
//                         .CreateScoped(CorsService.Scope.FormsReadonly);
//                 }

//                 // Create the Forms API service
//                 var formsService = new FormsService(new BaseClientService.Initializer()
//                 {
//                     HttpClientInitializer = credential,
//                     ApplicationName = "Survey API Google Forms Import"
//                 });

//                 // Get the form
//                 var googleForm = await formsService.Forms.Get(formId).ExecuteAsync();

//                 // Create a new survey
//                 var survey = new Survey
//                 {
//                     Title = !string.IsNullOrEmpty(title) ? title : googleForm.Info.Title,
//                     Description = !string.IsNullOrEmpty(description) ? description : googleForm.Info.Description,
//                     CoverImageUrl = coverImageUrl,
//                     CreatedAt = DateTime.UtcNow,
//                     CreatedBy = userId,
//                     IsActive = false,
//                     Questions = new List<SurveyQuestion>()
//                 };

//                 // Extract questions from the Google Form
//                 if (googleForm.Items != null)
//                 {
//                     foreach (var item in googleForm.Items)
//                     {
//                         // Skip sections and other non-question items
//                         if (item.QuestionItem == null)
//                             continue;

//                         var question = new SurveyQuestion
//                         {
//                             Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
//                             Title = item.Title,
//                             Description = item.Description ?? "",
//                             IsRequired = item.QuestionItem.Required ?? false
//                         };

//                         // Map Google Forms question types to our types
//                         if (item.QuestionItem.Question.ChoiceQuestion != null)
//                         {
//                             if (item.QuestionItem.Question.ChoiceQuestion.Type == "RADIO")
//                             {
//                                 question.Type = QuestionType.MultipleChoice;
//                                 question.Options = item.QuestionItem.Question.ChoiceQuestion.Options
//                                     .Select(o => o.Value).ToList();
//                             }
//                             else if (item.QuestionItem.Question.ChoiceQuestion.Type == "CHECKBOX")
//                             {
//                                 question.Type = QuestionType.Checkboxes;
//                                 question.Options = item.QuestionItem.Question.ChoiceQuestion.Options
//                                     .Select(o => o.Value).ToList();
//                             }
//                             else if (item.QuestionItem.Question.ChoiceQuestion.Type == "DROP_DOWN")
//                             {
//                                 question.Type = QuestionType.Dropdown;
//                                 question.Options = item.QuestionItem.Question.ChoiceQuestion.Options
//                                     .Select(o => o.Value).ToList();
//                             }
//                         }
//                         else if (item.QuestionItem.Question.TextQuestion != null)
//                         {
//                             question.Type = item.QuestionItem.Question.TextQuestion.Paragraph == true
//                                 ? QuestionType.Paragraph
//                                 : QuestionType.ShortAnswer;
//                         }
//                         else if (item.QuestionItem.Question.DateQuestion != null)
//                         {
//                             question.Type = QuestionType.Date;
//                         }
//                         else if (item.QuestionItem.Question.TimeQuestion != null)
//                         {
//                             question.Type = QuestionType.Time;
//                         }
//                         else if (item.QuestionItem.Question.ScaleQuestion != null)
//                         {
//                             question.Type = QuestionType.LinearScale;
//                             // Add scale options
//                             int low = item.QuestionItem.Question.ScaleQuestion.Low ?? 1;
//                             int high = item.QuestionItem.Question.ScaleQuestion.High ?? 5;
//                             for (int i = low; i <= high; i++)
//                             {
//                                 question.Options.Add(i.ToString());
//                             }
//                         }

//                         survey.Questions.Add(question);
//                     }
//                 }

//                 return survey;
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception($"Error importing Google Form: {ex.Message}", ex);
//             }
//         }
//     }
// }