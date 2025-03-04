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
using System.Windows.Ink;
using System.Collections.ObjectModel;
//using System.Windows.Forms;

namespace TwoOkNotes.Services
{
    public class FileSavingServices
    {
        private readonly string _defaultNotesDirectoryLocation; //Path of the JSON that points to the default notes directory
        private readonly string _globalMetaDataLocation;
        private readonly string _notesDirectory;
        public string _currFilePath;

        public FileSavingServices()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "TwoOkNotes");

            createDir(appFolder);

            _defaultNotesDirectoryLocation = Path.Combine(appFolder, "DefaultNotesStorage.json");
            ValidateDirLocationExists(_defaultNotesDirectoryLocation);

            DefaultNotesStorage noteDir = JsonSerializer.Deserialize<DefaultNotesStorage>(File.ReadAllText(_defaultNotesDirectoryLocation));
            _notesDirectory = noteDir.DefaultFilePath;
            


            _globalMetaDataLocation = Path.Combine(_notesDirectory, "GlobalMetaData.json");
            ValidateFilePaths(_globalMetaDataLocation);
        }

        private void createDir(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private async void ValidateDirLocationExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                DefaultNotesStorage defDirectoryPath = new()
                {
                    DefaultFilePath = GetDirectoryPathFromUser() 
                };
                await SaveMetadataAsync(defDirectoryPath, filePath);
            }
        }

        private void ValidateFilePaths(string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateFile(filePath).Wait();
            }
        }

        public async Task<bool> CreateNotebook(string directoryName)
        {
            string notebookPath = Path.Combine(_notesDirectory, directoryName);
            if (!Directory.Exists(notebookPath))
            {
                Directory.CreateDirectory(notebookPath);
                var metadata = await LoadMetadataAsync<GlobalMetaData>(_globalMetaDataLocation);
                if (!metadata.NoteBooks.Contains(directoryName))
                {
                    metadata.NoteBooks.Add(directoryName);
                }
                await SaveMetadataAsync(metadata, _globalMetaDataLocation);
                bool sectionCreated = await CreateSection("Section1", directoryName);
                return sectionCreated;
            }
            return false;
        }


        public async Task<bool> CreateSection(string sectionName, string notebookName)
        {
            string sectionPath = Path.Combine(_notesDirectory, notebookName, sectionName);
            if (!Directory.Exists(sectionPath))
            {
                Directory.CreateDirectory(sectionPath);
                string metadataFilePath = Path.Combine(_notesDirectory, notebookName, "NoteBookMetaData.json");
                var metadata = await LoadMetadataAsync<NoteBookMetaData>(metadataFilePath);
                if (!metadata.Sections.Any(s => s.Name == sectionName))
                {
                    metadata.Sections.Add(new NoteBookSection { Name = sectionName });
                }
                await SaveMetadataAsync(metadata, metadataFilePath);
                bool pageCreated = await CreatePage(notebookName, sectionName, "Page1.isf");
                return pageCreated;
            }
            return false; // Section already exists
        }

        public async Task<bool> CreatePage(string? notebookName, string? sectionName, string pageName)
        {
            string pagePath = Path.Combine(_notesDirectory, notebookName ?? "", sectionName ?? "", pageName);
            if (!File.Exists(pagePath))
            {
                await CreateFile(pagePath);
                if (notebookName != null && sectionName != null)
                {
                    string metadataFilePath = Path.Combine(_notesDirectory, notebookName, sectionName, "SectionMetaData.json");
                    var metadata = await LoadMetadataAsync<SectionMetaData>(metadataFilePath);
                    if (!metadata.Pages.Any(p => p.Name == pageName))
                    {
                        metadata.Pages.Add(new NoteBookPage { Name = pageName });
                    }
                    await SaveMetadataAsync(metadata, metadataFilePath);
                    return true;
                }
                else
                {
                    var metadata = await LoadMetadataAsync<GlobalMetaData>(_globalMetaDataLocation);
                    if (!metadata.OrphanPages.Contains(pageName))
                    {
                        metadata.OrphanPages.Add(pageName);
                    }
                    await SaveMetadataAsync(metadata, _globalMetaDataLocation);
                    return true;
                }
            }
            return false;
        }

        public async Task<ObservableCollection<NoteBookSection>> GetNotebookMetadata(string notebookName)
        {
            string metadataFilePath = Path.Combine(_notesDirectory, notebookName, "NoteBookMetaData.json");
            var metadata = await LoadMetadataAsync<NoteBookMetaData>(metadataFilePath);
            return new ObservableCollection<NoteBookSection>(metadata.Sections);
        }

        public async Task<ObservableCollection<NoteBookPage>> GetSectionMetadata(string notebookName, string sectionName)
        {
            string metadataFilePath = Path.Combine(_notesDirectory, notebookName, sectionName, "SectionMetaData.json");
            var metadata = await LoadMetadataAsync<SectionMetaData>(metadataFilePath);
            return new ObservableCollection<NoteBookPage>(metadata.Pages);
        }

        public string GetCurrFilePath(string? noteBookName, string? sectionName, string pageName)
        {
            return Path.Combine(_notesDirectory, noteBookName ?? "", sectionName ?? "", pageName);
        }

        //TODO: when maning is implemented change return type to bool for name repete check 
        public static async Task<bool> CreateFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    await File.WriteAllTextAsync(filePath, string.Empty);
                    return true;
                }
                else
                {
                    //TODO: Add a message box to ask to overwrite the file when file naming is implemented 
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static async Task<bool> SaveFileAsync(string filePath, byte[] fileContent)
        {
            try
            {
                await File.WriteAllBytesAsync(filePath, fileContent);
                return true;
            }
            catch (FileNotFoundException ) 
            {
                
                return false;
            }
        }
        private static async Task<T> LoadMetadataAsync<T>(string filePath) where T : new()
        {
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<T>(json) ?? new T();
                }
            }
            return new T();
        }

        //TODO: Looks like this save and the 5 second timer are clashing 
        private async Task SaveMetadataAsync<T>(T metaData, string filePath)
        {
            string json = JsonSerializer.Serialize(metaData, new JsonSerializerOptions { WriteIndented = true });
            int retryCount = 3;

            while (retryCount > 0)
            {
                try
                {
                    await File.WriteAllTextAsync(filePath, json);
                    break;
                }
                catch (IOException)
                {
                    retryCount--;
                    if (retryCount == 0) throw;
                    await Task.Delay(100);
                }
            }
        }

        public async Task<Dictionary<string, (string type, string Fileapth, DateTime LastAccessTime)>> GetMetadataNameAndFilePathAsync()
        {
            if (File.Exists(_globalMetaDataLocation))
            {
                Dictionary<string, (string Name, string FilePath, DateTime LastAccessTime)> result = new();
                GlobalMetaData metadata = await LoadMetadataAsync<GlobalMetaData>(_globalMetaDataLocation);

                if (metadata != null)
                {
                    foreach (var item in metadata.NoteBooks)
                    {
                        string itemFilePath = Path.Combine(_notesDirectory, item);
                        result[item] = ("NoteBook", itemFilePath, File.GetLastAccessTime(itemFilePath));
                    }

                    foreach (var item in metadata.OrphanPages)
                    {
                        string itemFilePath = Path.Combine(_notesDirectory, item);
                        result[item] = ("OrphanPage", itemFilePath, File.GetLastAccessTime(itemFilePath));
                    }
                }

                return result;
            }
            return new Dictionary<string, (string Name, string FilePath, DateTime LastAccessTime)>();
        }

        public static async Task<StrokeCollection> GetFileContents(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] fileContent = await File.ReadAllBytesAsync(filePath);

                if (fileContent.Length > 0)
                {
                using (var memoryStream = new MemoryStream(fileContent))
                {
                    return new StrokeCollection(memoryStream);
                }
            }
            }
            return new StrokeCollection();
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

            MessageBox.Show("No folder selected, please select a folder");
            return GetDirectoryPathFromUser(); //Unsure the best way to handle this 
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
