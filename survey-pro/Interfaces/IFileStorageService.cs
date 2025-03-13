using System;

namespace survey_pro.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subfolder);
    void DeleteFile(string filePath);
}
