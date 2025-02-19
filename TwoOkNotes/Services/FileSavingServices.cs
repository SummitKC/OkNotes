using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwoOkNotes.Model;
using Microsoft.Win32;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Documents;
//using System.Windows.Forms;

namespace TwoOkNotes.Services
{
    public class FileSavingServices
    {
        private readonly string _notebookFilePath;
        private readonly string _sectionFilePath;
        private readonly string _metaDataFilePath;
        private readonly string _defaultFileSettings;

        public FileSavingServices()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "TwoOkNotes");

            ValidateDirectory(appFolder);

            _defaultFileSettings = Path.Combine(appFolder, "FileSettings.json");
            _notebookFilePath = Path.Combine(appFolder, "notebookFilePath.json");
            _sectionFilePath = Path.Combine(appFolder, "sectionFilePath.json");
            _metaDataFilePath = Path.Combine(appFolder, "currFilesMetadata.json");

            ValidateFilePaths();
        }

        private void ValidateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void ValidateFilePaths()
        {
            var filePaths = new Dictionary<string, string>
            {{ nameof(_defaultFileSettings), _defaultFileSettings },
            { nameof(_notebookFilePath), _notebookFilePath },
            { nameof(_sectionFilePath), _sectionFilePath },
            { nameof(_metaDataFilePath), _metaDataFilePath }};

            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath.Value))
                {
                    Debug.WriteLine($"File not found: {filePath.Key} - {filePath.Value}");
                    CreateMetaDataFiles(filePath.Value);
                }
            }
        }

        //TODO: Refactor, just for testing rn 
        public async Task<string> GetDefaultFilePath()
        {
            try
            {
                if (File.Exists(_defaultFileSettings))
                {
                    string json = await File.ReadAllTextAsync(_defaultFileSettings);
                    FileSettings? fileSettings = JsonSerializer.Deserialize<FileSettings>(json);
                    //Empty check for Json 
                    if (fileSettings != null && !JsonHelper.isAttributeEmtpy(fileSettings.DefaultFilePath))
                    {
                        return fileSettings.DefaultFilePath; // Could return an invalid filepath 
                    }

                    await setNoteFilePath(GetDirectoryPathFromUser());
                    return await GetDefaultFilePath();
                }
                throw new FileNotFoundException(_defaultFileSettings);
            }
            catch (FileNotFoundException e)
            {
                createFile(e.Message);
                await setNoteFilePath(GetDirectoryPathFromUser());
                return await GetDefaultFilePath();
            }
        }

        private async void CreateMetaDataFiles(string filePath)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, null);
            }
            catch (Exception)
            {
                Debug.WriteLine("here?");
            }
        }

        //TODO: when maning is implemented change return type to bool for name repete check 
        public async void createFile(string fileName)
        {
            try
            {
                string filePath = await GetDefaultFilePath() + fileName;
                if (!File.Exists(filePath))
                {
                    await File.WriteAllTextAsync(filePath, null);
                    //return true;
                }
                else
                {
                    //TODO: Add a message box to ask to overwrite the file when file naming is implemented 
                    //return false; 
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("here?");
                //return false; 
            }
        }
        public async Task<bool> SaveFileAsync(string filePath, byte[] fileContent)
        {
            try
            {
                await File.WriteAllBytesAsync(filePath, fileContent);
                await UpdateMetadataAsync(filePath);
                return true;
            }
            catch (FileNotFoundException ) 
            {
                
                return false;
            }
        }

        private async Task UpdateMetadataAsync(string filePath)
        {
            try
            {
                var metadata = await LoadMetadataAsync();
                var fileInfo = new FileInfo(filePath);

                var fileMetadata = new FileMetadata
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    Id = metadata.Count,
                    LastModifiedDate = fileInfo.LastWriteTime,
                };

                metadata[fileInfo.Name] = fileMetadata;
                await SaveMetadataAsync(metadata);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while updating metadata: {ex.Message}");
                throw;
            }
        }
        private async Task<Dictionary<string, FileMetadata>> LoadMetadataAsync()
        {

            if (File.Exists(_metaDataFilePath) && !JsonHelper.IsJsonEmpty(_metaDataFilePath))
            {
                string json = await File.ReadAllTextAsync(_metaDataFilePath);
                return JsonSerializer.Deserialize<Dictionary<string, FileMetadata>>(json);
            }

            var dict = new Dictionary<string, FileMetadata>();
            var fileMetadata = new FileMetadata
            {
                FilePath = _defaultFileSettings,
            };

            dict[fileMetadata.FileName] = fileMetadata;
            await SaveMetadataAsync(dict);
            return dict;
        }

        //TODO: Looks like this save and the 5 second timer are clashing 
        private async Task SaveMetadataAsync(Dictionary<string, FileMetadata> metadata)
        {
            string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            int retryCount = 3;

            //Not sure why but when reasing the program crashes 
            //added a retry count to try and fix it 
            //TODO: It's working for now, Look into this further
            while (retryCount > 0)
            {
                try
                {
                    await File.WriteAllTextAsync(_metaDataFilePath, json);
                    break;
                }
                catch (IOException)
                {
                    retryCount--;
                    if (retryCount == 0) throw;
                    await Task.Delay(100); // Wait before retrying
                }
            }
        }

        public async Task<Dictionary<string, (string FilePath, DateTime LastModifiedDate)>> GetMetadataNameAndFilePathAsync()
        {
            if (File.Exists(_metaDataFilePath))
            {
                Debug.WriteLine("Here?");
                var result = new Dictionary<string, (string FilePath, DateTime LastModifiedDate)>();
                var metadata = await LoadMetadataAsync();

                if (metadata != null)
                {
                    foreach (var item in metadata)
                    {
                        result[item.Key] = (item.Value.FilePath, item.Value.LastModifiedDate);
                    }
                }

                return result;
            }
            return new Dictionary<string, (string FilePath, DateTime LastModifiedDate)>();
        }

        public async Task<bool> setNoteFilePath(string filePath)
        {
            FileSettings fileSettings = new FileSettings
            {
                DefaultFilePath = filePath
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
       
            };
            string json = JsonSerializer.Serialize(fileSettings, options);
            await File.WriteAllTextAsync(_defaultFileSettings, json);
            return true;
        }

        public static string GetDirectoryPathFromUser()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select a folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return null;
        }
        public static string GetFilePathFromUser()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "InkCanvas Files (*.isf)|*.isf|All Files (*.*)|*.*",
                Title = "Select a File"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return string.Empty;
        }

        //change it to bool for confirmation later 
        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);

            }
        }
    }
}
