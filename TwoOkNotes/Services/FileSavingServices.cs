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
//using System.Windows.Forms;

namespace TwoOkNotes.Services
{
    public class FileSavingServices
    {
        
        private readonly string _metaDataFilePath;
        private readonly string _defaultFileSettings;

        public FileSavingServices()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "TwoOkNotes");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            _metaDataFilePath = Path.Combine(appFolder, "currFilesMetadata.json");
            _defaultFileSettings = Path.Combine(appFolder, "FileSettings.json");
        }

        //TODO: Refactor, just for testing rn 
        public string GetDefaultFilePath()
        {
            try
            {
                if (File.Exists(_defaultFileSettings))
                {
                    string json = File.ReadAllText(_defaultFileSettings);
                    FileSettings fileSettings = JsonSerializer.Deserialize<FileSettings>(json);

                    if (fileSettings.DefaultFilePath != null)
                    {
                        return fileSettings.DefaultFilePath;
                    }
                    else
                    {
                        setNoteFilePath(GetDirectoryPathFromUser());
                        return GetDefaultFilePath();
                    }               
                }
                throw new FileNotFoundException("FileSettings.json not found");
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e.Message);
                return "C:/"; 
            }
        }

        public async void createFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, null);
                //await SaveFileAsync(filePath, new byte[0]);
            }
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
                FilePath = filePath,
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

        public async Task<Dictionary<string, string>> GetMetadataNameAndFilePathAsync()
        {
            if (File.Exists(_metaDataFilePath))
            {
                Debug.WriteLine("Here?");
                var result = new Dictionary<string, string>();
                var metadata = await LoadMetadataAsync();

                if (metadata != null)
                {
                    foreach (var item in metadata)
                    {
                        result[item.Key] = item.Value.FilePath;
                    }
                }

                return result;
            }
            return new Dictionary<string, string>();
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
