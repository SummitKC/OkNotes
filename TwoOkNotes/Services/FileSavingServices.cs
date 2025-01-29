using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TwoOkNotes.Services
{
    public class FileSavingServices
    {
        private readonly string _metaDataFilePath; 
    

    public FileSavingServices()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "TwoOkNotes");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            _metaDataFilePath = Path.Combine(appFolder, "currFilesMetadata");
        }

    public async Task SaveFileAsync(string filePath, byte[] fileContent)
        {
            await File.WriteAllBytesAsync(filePath, fileContent);
            await UpdateMetadataAsync(filePath);
        }

        private async Task UpdateMetadataAsync(string filePath)
        {
            var metadata = await LoadMetadataAsync();
            var fileInfo = new FileInfo(filePath);

            var fileMetadata = new FileMetadata
            {
                FileName = fileInfo.Name,
                Id = metadata.Count,
                CreationDate = fileInfo.CreationTime,
                LastModifiedDate = fileInfo.LastWriteTime,
            };

            metadata[fileInfo.Name] = fileMetadata;
            await SaveMetadataAsync(metadata);
        }
        private async Task<Dictionary<string, FileMetadata>> LoadMetadataAsync()
        {
            if (File.Exists(_metaDataFilePath))
            {
                string json = await File.ReadAllTextAsync(_metaDataFilePath);
                return JsonSerializer.Deserialize<Dictionary<string, FileMetadata>>(json) ?? new Dictionary<string, FileMetadata>();
            }
            return new Dictionary<string, FileMetadata>();
        }

        private async Task SaveMetadataAsync(Dictionary<string, FileMetadata> metadata)
        {
            string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_metaDataFilePath, json);
        }

    }
}
