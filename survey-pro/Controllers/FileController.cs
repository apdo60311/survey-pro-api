using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using survey_pro.Interfaces;

namespace survey_pro.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorage;

        public FilesController(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(string id)
        {
            var (fileData, contentType) = await _fileStorage.GetFileAsync(id);
            return File(fileData, contentType);
        }
    }
}


