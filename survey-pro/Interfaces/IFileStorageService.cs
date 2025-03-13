using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using MongoDB.Driver;
using System;

namespace survey_pro.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subfolder);
    Task<(byte[] FileData, string ContentType)> GetFileAsync(string fileId);
    void DeleteFile(string filePath);
}
