using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace survey_pro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileStorageService _fileStorage;

        public FilesController(FileStorageService fileStorage)
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


