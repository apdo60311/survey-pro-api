using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using survey_pro.Dtos;
using survey_pro.Interfaces;
using survey_pro.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace survey_pro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        /*       [HttpPost("import-google-form")]
               [Authorize(Roles = "Admin")]
               public async Task<IActionResult> ImportFromGoogleForms([FromForm] ImportGoogleFormsDto importDto)
               {
                   var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                   var survey = await _surveyService.ImportFromGoogleFormsAsync(importDto, userId);
                   return CreatedAtAction(nameof(GetById), new { id = survey.Id }, survey);
               }*/
    }
}