using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using survey_pro.Dtos;
using survey_pro.Interfaces;
using survey_pro.Models;
using survey_pro.Services;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace survey_pro.Controllers
{
    [ApiController]
    [Route("api/surveys")]
    public class SurveysController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveysController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await _surveyService.GetAllSurveysAsync();
            return Ok(surveys);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null)
            {
                return NotFound();
            }

            return Ok(survey);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(SurveyDto surveyDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var survey = await _surveyService.CreateSurveyAsync(surveyDto, userId);
            return CreatedAtAction(nameof(GetById), new { id = survey.Id }, survey);
        }


        [HttpPost("{id}/respond")]
        public async Task<IActionResult> RespondToSurvey(string id, [FromBody] List<QuestionResponse> responses)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var surveyResponse = await _surveyService.RespondToSurveyAsync(id, userId, responses);
                return CreatedAtAction(nameof(GetById), new { id = surveyResponse.Id }, surveyResponse);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/responses")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetSurveyResponses(string id)
        {
            var responses = await _surveyService.GetSurveyResponsesAsync(id);
            return Ok(responses);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(string id, [FromForm] SurveyUpdateDto surveyDto)
        {
            var success = await _surveyService.UpdateSurveyAsync(id, surveyDto);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _surveyService.DeleteSurveyAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }


        [HttpPost("{surveyId}/questions")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddQuestionsToSurvey(string surveyId, [FromBody] List<QuestionDto> questions)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var updatedSurvey = await _surveyService.AddQuestionsToSurveyAsync(surveyId, questions);
                return Ok(updatedSurvey);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Survey not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/response-count")]
        public async Task<IActionResult> GetResponseCount(string id)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null)
            {
                return NotFound(new { message = "Survey not found" });
            }

            return Ok(new { surveyId = id, responseCount = survey.NumberOfResponses });
        }


        [HttpGet("{id}/responses/export")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ExportResponses(string id)
        {
            try
            {
                var survey = await _surveyService.GetSurveyByIdAsync(id);
                if (survey == null)
                {
                    return NotFound(new { message = "Survey not found" });
                }

                var responses = await _surveyService.GetSurveyResponsesAsync(id);
                if (!responses.Any())
                {
                    return NotFound(new { message = "No responses found for this survey" });
                }

                var csvContent = new StringBuilder();

                var headers = new List<string> { "ResponseId", "RespondentId", "SubmittedAt" };
                foreach (var question in survey.Questions ?? [])
                {
                    headers.Add($"Q{question.Id}: {question.Title}");
                }
                csvContent.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

                foreach (var response in responses)
                {
                    var row = new List<string>
            {
                response.Id,
                response.RespondentId ?? "Anonymous",
                response.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };

                    foreach (var question in survey.Questions ?? [])
                    {
                        var answer = response.Responses
                            .FirstOrDefault(r => r.QuestionId == question.Id);

                        var answerText = answer != null
                            ? (question.Type == QuestionType.CHECKBOX
                                ? string.Join(";", answer.SelectedOptions)
                                : answer.Answer)
                            : "";

                        row.Add($"\"{answerText.Replace("\"", "\"\"")}\"");
                    }

                    csvContent.AppendLine(string.Join(",", row));
                }

                byte[] bytes = Encoding.UTF8.GetBytes(csvContent.ToString());
                string fileName = $"survey-responses-{id}-{DateTime.UtcNow:yyyyMMdd}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to export responses", error = ex.Message });
            }
        }        /*       [HttpPost("import-google-form")]
               [Authorize(Roles = "Admin")]
               public async Task<IActionResult> ImportFromGoogleForms([FromForm] ImportGoogleFormsDto importDto)
               {
                   var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                   var survey = await _surveyService.ImportFromGoogleFormsAsync(importDto, userId);
                   return CreatedAtAction(nameof(GetById), new { id = survey.Id }, survey);
               }*/
    }
}