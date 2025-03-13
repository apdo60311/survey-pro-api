using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;
using survey_pro.Interfaces;
using survey_pro.Settings;
using Microsoft.Extensions.Options;

public class FileStorageService : IFileStorageService
{
    private readonly IGridFSBucket _gridFS;
    private readonly string _baseUrl;

    public FileStorageService(
                IOptions<MongoDbSettings> mongoSettings, IConfiguration configuration)
    {
        var database = new MongoClient().GetDatabase(mongoSettings.Value.DatabaseName);
        _gridFS = new GridFSBucket(database);
        _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5098";
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var fileName = $"{subfolder}/{Guid.NewGuid()}_{file.FileName}";
        var options = new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "contentType", file.ContentType },
                { "subfolder", subfolder }
            }
        };

        using (var stream = file.OpenReadStream())
        {
            var fileId = await _gridFS.UploadFromStreamAsync(fileName, stream, options);
            return $"{_baseUrl}/api/files/{fileId}";
        }
    }

    public async Task<(byte[] FileData, string ContentType)> GetFileAsync(string fileId)
    {
        var objectId = ObjectId.Parse(fileId);
        var file = await _gridFS.DownloadAsBytesAsync(objectId);
        var fileInfo = await _gridFS.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
        var contentType = fileInfo.Metadata["contentType"].AsString;

        return (file, contentType);
    }

    public async void DeleteFile(string fileId)
    {
        if (string.IsNullOrEmpty(fileId))
        {
            return;
        }

        await _gridFS.DeleteAsync(ObjectId.Parse(fileId));
    }

}
