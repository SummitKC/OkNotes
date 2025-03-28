﻿using System;
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
            if (JsonHelper.IsJsonEmpty(File.ReadAllText(_defaultNotesDirectoryLocation)))
            {
                string getPath = GetDirectoryPathFromUser();
                DefaultNotesStorage noteDirr = new()
                {
                    DefaultFilePath = getPath
                };
                File.WriteAllText(_defaultNotesDirectoryLocation, JsonSerializer.Serialize(noteDirr));
            }

            DefaultNotesStorage noteDir = JsonSerializer.Deserialize<DefaultNotesStorage>(File.ReadAllText(_defaultNotesDirectoryLocation));
            if (noteDir.DefaultFilePath == null || JsonHelper.isAttributeEmtpy(noteDir.DefaultFilePath))
            {
                noteDir.DefaultFilePath = GetDirectoryPathFromUser();
                File.WriteAllText(_defaultNotesDirectoryLocation, JsonSerializer.Serialize(noteDir));
            }
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
                bool sectionCreated = await CreateSection("Section 1", directoryName);
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
                bool pageCreated = await CreatePage(notebookName, sectionName, "Page 1.isf");
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

        public async Task<(ObservableCollection<NoteBookSection> sections, int activeIndex)> GetNotebookMetadata(string notebookName)
        {
            string metadataFilePath = Path.Combine(_notesDirectory, notebookName, "NoteBookMetaData.json");
            var metadata = await LoadMetadataAsync<NoteBookMetaData>(metadataFilePath);
            return (new ObservableCollection<NoteBookSection>(metadata.Sections), metadata.ActiveSectionIndex);
        }

        public async Task<(ObservableCollection<NoteBookPage> pages, int activeIndex)> GetSectionMetadata(string notebookName, string sectionName)
        {
            string metadataFilePath = Path.Combine(_notesDirectory, notebookName, sectionName, "SectionMetaData.json");
            var metadata = await LoadMetadataAsync<SectionMetaData>(metadataFilePath);
            return (new ObservableCollection<NoteBookPage>(metadata.Pages), metadata.ActivePageIndex);
        }

        public string GetCurrFilePath(string? noteBookName, string? sectionName, string pageName)
        {
            string path = Path.Combine(_notesDirectory, noteBookName ?? "", sectionName ?? "", pageName);
            return path;
        }

        public static async Task<bool> CreateFile(string filePath)
        {
            int maxRetries = 3;
            int retryDelayMs = 100;
            int currentRetry = 0;

            while (currentRetry <= maxRetries)
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
                catch (IOException ex)
                {
                    MessageBox.Show($"IO error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (currentRetry == maxRetries)
                        return false;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (currentRetry == maxRetries)
                        return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (currentRetry == maxRetries)
                        return false;
                }

                // Increment retry counter and wait before retrying
                currentRetry++;
                if (currentRetry <= maxRetries)
                {
                    await Task.Delay(retryDelayMs * currentRetry);
                }
            }

            return false;
        }
        public static async Task<bool> SaveFileAsync(string filePath, byte[] fileContent)
        {
            int maxRetries = 3;
            int retryDelayMs = 100;
            int currentRetry = 0;

            while (currentRetry <= maxRetries)
            {
                try
                {
                    await File.WriteAllBytesAsync(filePath, fileContent);
                    return true;
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show($"File not found: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (currentRetry == maxRetries)
                        return false;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"IO error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (currentRetry == maxRetries)
                        return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (currentRetry == maxRetries)
                        return false;
                }

                // Increment retry counter and wait before retrying
                currentRetry++;
                if (currentRetry <= maxRetries)
                {
                    await Task.Delay(retryDelayMs * currentRetry);
                }
            }

            return false;
        }
        private static async Task<T> LoadMetadataAsync<T>(string filePath) where T : new()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            return JsonSerializer.Deserialize<T>(json) ?? new T();
                        }
                        catch (JsonException ex)
                        {
                            MessageBox.Show($"JSON error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            // Return a new instance if deserialization fails
                            return new T();
                        }
                    }
                }
                return new T();
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IO error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new T();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new T();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new T();
            }
        }

        //TODO: Looks like this save and the 5 second timer are clashing 
        private static async Task SaveMetadataAsync<T>(T metaData, string filePath)
        {
            string json = JsonSerializer.Serialize(metaData, new JsonSerializerOptions { WriteIndented = true });
            int maxRetries = 3;
            int retryDelayMs = 100;
            int currentRetry = 0;

            while (currentRetry <= maxRetries)
            {
                try
                {
                    await File.WriteAllTextAsync(filePath, json);
                    return;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"IO error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If it's the last retry, throw the exception
                    if (currentRetry == maxRetries)
                        throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If it's the last retry, throw the exception
                    if (currentRetry == maxRetries)
                        throw;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If it's the last retry, throw the exception
                    if (currentRetry == maxRetries)
                        throw;
                }

                // Increment retry counter and wait before retrying
                currentRetry++;
                if (currentRetry <= maxRetries)
                {
                    await Task.Delay(retryDelayMs * currentRetry);
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
            StrokeCollection strokes = new StrokeCollection();
            int maxRetries = 3;
            int retryDelayMs = 100;
            int currentRetry = 0;

            while (currentRetry <= maxRetries)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        byte[] fileContent = await File.ReadAllBytesAsync(filePath);
                        if (fileContent.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(fileContent))
                            {
                                strokes = new StrokeCollection(ms);
                            }
                        }
                    }
                    return strokes;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"IO error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If it's the last retry, return empty strokes
                    if (currentRetry == maxRetries)
                        return strokes;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If it's the last retry, return empty strokes
                    if (currentRetry == maxRetries)
                        return strokes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If it's the last retry, return empty strokes
                    if (currentRetry == maxRetries)
                        return strokes;
                }

                // Increment retry counter and wait before retrying
                currentRetry++;
                if (currentRetry <= maxRetries)
                {
                    await Task.Delay(retryDelayMs * currentRetry);
                }
            }

            return strokes;
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
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IO error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Handle IO error silently
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Handle access denied silently
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Handle unexpected error silently
            }
        }

        public async Task UpdateSectionMetadata(string notebookName, ObservableCollection<NoteBookSection> sections, int activeSectionIndex)
        {
            string metadataFilePath = Path.Combine(_notesDirectory, notebookName, "NoteBookMetaData.json");
            var metadata = await LoadMetadataAsync<NoteBookMetaData>(metadataFilePath);
            metadata.Sections = sections.ToList();
            metadata.ActiveSectionIndex = activeSectionIndex;
            await SaveMetadataAsync(metadata, metadataFilePath);
        }

        public async Task UpdatePageMetadata(string notebookName, string sectionName, ObservableCollection<NoteBookPage> pages, int activePageIndex)
        {
            string metadataFilePath = Path.Combine(_notesDirectory, notebookName, sectionName, "SectionMetaData.json");
            var metadata = await LoadMetadataAsync<SectionMetaData>(metadataFilePath);
            metadata.Pages = pages.ToList();
            metadata.ActivePageIndex = activePageIndex;
            await SaveMetadataAsync(metadata, metadataFilePath);
        }

        public async Task<bool> DeleteSection(string notebookName, string sectionName)
        {
            try
            {
                string sectionPath = Path.Combine(_notesDirectory, notebookName, sectionName);
                if (!Directory.Exists(sectionPath))
                {
                    return false;
                }

                string metadataFilePath = Path.Combine(_notesDirectory, notebookName, "NoteBookMetaData.json");
                if (!File.Exists(metadataFilePath))
                {
                    return false;
                }

                // Load notebook metadata
                var metadata = await LoadMetadataAsync<NoteBookMetaData>(metadataFilePath);

                // Remove the section from the metadata
                var sectionToRemove = metadata.Sections.FirstOrDefault(s => s.Name == sectionName);
                if (sectionToRemove != null)
                {
                    metadata.Sections.Remove(sectionToRemove);
                    // Adjust active section index if needed
                    if (metadata.ActiveSectionIndex >= metadata.Sections.Count && metadata.Sections.Count > 0)
                        metadata.ActiveSectionIndex = metadata.Sections.Count - 1;
                    else if (metadata.Sections.Count == 0)
                        metadata.ActiveSectionIndex = -1;

                    await SaveMetadataAsync(metadata, metadataFilePath);
                }

                // Delete the directory and its contents
                Directory.Delete(sectionPath, true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeletePage(string notebookName, string sectionName, string pageName)
        {
            try
            {
                string pagePath = Path.Combine(_notesDirectory, notebookName, sectionName, pageName);
                if (!File.Exists(pagePath))
                {
                    return false;
                }

                string metadataFilePath = Path.Combine(_notesDirectory, notebookName, sectionName, "SectionMetaData.json");
                if (!File.Exists(metadataFilePath))
                {
                    return false;
                }

                // Load section metadata
                var metadata = await LoadMetadataAsync<SectionMetaData>(metadataFilePath);

                // Remove the page from the metadata
                var pageToRemove = metadata.Pages.FirstOrDefault(p => p.Name == pageName);
                if (pageToRemove != null)
                {
                    metadata.Pages.Remove(pageToRemove);
                    // Adjust active page index if needed
                    if (metadata.ActivePageIndex >= metadata.Pages.Count && metadata.Pages.Count > 0)
                        metadata.ActivePageIndex = metadata.Pages.Count - 1;
                    else if (metadata.Pages.Count == 0)
                        metadata.ActivePageIndex = -1;

                    await SaveMetadataAsync(metadata, metadataFilePath);
                }

                // Delete the file
                File.Delete(pagePath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RenameSection(string notebookName, string oldSectionName, string newSectionName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newSectionName))
                    return false;

                string oldPath = Path.Combine(_notesDirectory, notebookName, oldSectionName);
                if (!Directory.Exists(oldPath))
                {
                    return false;
                }

                string newPath = Path.Combine(_notesDirectory, notebookName, newSectionName);
                if (Directory.Exists(newPath))
                {
                    return false;
                }

                string metadataFilePath = Path.Combine(_notesDirectory, notebookName, "NoteBookMetaData.json");
                if (!File.Exists(metadataFilePath))
                {
                    return false;
                }

                // Load notebook metadata
                var metadata = await LoadMetadataAsync<NoteBookMetaData>(metadataFilePath);

                // Update section name in metadata
                var sectionToUpdate = metadata.Sections.FirstOrDefault(s => s.Name == oldSectionName);
                if (sectionToUpdate != null)
                {
                    sectionToUpdate.Name = newSectionName;
                    await SaveMetadataAsync(metadata, metadataFilePath);
                }

                // Rename the directory
                Directory.Move(oldPath, newPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RenamePage(string notebookName, string sectionName, string oldPageName, string newPageName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPageName))
                    return false;

                // Ensure the new name has the .isf extension
                if (!newPageName.EndsWith(".isf", StringComparison.OrdinalIgnoreCase))
                    newPageName += ".isf";

                string oldPath = Path.Combine(_notesDirectory, notebookName, sectionName, oldPageName);
                if (!File.Exists(oldPath))
                {
                    return false;
                }

                string newPath = Path.Combine(_notesDirectory, notebookName, sectionName, newPageName);
                if (File.Exists(newPath))
                {
                    return false;
                }

                string metadataFilePath = Path.Combine(_notesDirectory, notebookName, sectionName, "SectionMetaData.json");
                if (!File.Exists(metadataFilePath))
                {
                    return false;
                }

                // Load section metadata
                var metadata = await LoadMetadataAsync<SectionMetaData>(metadataFilePath);

                // Update page name in metadata
                var pageToUpdate = metadata.Pages.FirstOrDefault(p => p.Name == oldPageName);
                if (pageToUpdate != null)
                {
                    pageToUpdate.Name = newPageName;
                    await SaveMetadataAsync(metadata, metadataFilePath);
                }

                // Rename the file
                File.Move(oldPath, newPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteNotebook(string notebookName)
        {
            try
            {
                string notebookPath = Path.Combine(_notesDirectory, notebookName);
                if (!Directory.Exists(notebookPath))
                {
                    return false;
                }

                // Load global metadata
                var metadata = await LoadMetadataAsync<GlobalMetaData>(_globalMetaDataLocation);
                
                // Remove the notebook from the metadata
                metadata.NoteBooks.Remove(notebookName);
                await SaveMetadataAsync(metadata, _globalMetaDataLocation);

                // Delete the directory and its contents
                Directory.Delete(notebookPath, true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RenameNotebook(string oldName, string newName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newName))
                    return false;

                string oldPath = Path.Combine(_notesDirectory, oldName);
                if (!Directory.Exists(oldPath))
                {
                    return false;
                }

                string newPath = Path.Combine(_notesDirectory, newName);
                if (Directory.Exists(newPath))
                {
                    return false;
                }

                // Load global metadata
                var metadata = await LoadMetadataAsync<GlobalMetaData>(_globalMetaDataLocation);
                
                // Update notebook name in metadata
                if (metadata.NoteBooks.Contains(oldName))
                {
                    metadata.NoteBooks.Remove(oldName);
                    metadata.NoteBooks.Add(newName);
                    await SaveMetadataAsync(metadata, _globalMetaDataLocation);
                }

                // Rename the directory
                Directory.Move(oldPath, newPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteOrphanPage(string pageName)
        {
            try
            {
                string pagePath = Path.Combine(_notesDirectory, pageName);
                if (!File.Exists(pagePath))
                {
                    return false;
                }

                // Update global metadata
                var metadata = await LoadMetadataAsync<GlobalMetaData>(_globalMetaDataLocation);
                
                if (metadata.OrphanPages.Contains(pageName))
                {
                    metadata.OrphanPages.Remove(pageName);
                    await SaveMetadataAsync(metadata, _globalMetaDataLocation);
                }

                // Delete the file
                File.Delete(pagePath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RenameOrphanPage(string oldName, string newName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newName))
                    return false;

                // Ensure the new name has the .isf extension
                if (!newName.EndsWith(".isf", StringComparison.OrdinalIgnoreCase))
                    newName += ".isf";

                string oldPath = Path.Combine(_notesDirectory, oldName);
                if (!File.Exists(oldPath))
                {
                    return false;
                }

                string newPath = Path.Combine(_notesDirectory, newName);
                if (File.Exists(newPath))
                {
                    return false;
                }

                // Update global metadata
                var metadata = await LoadMetadataAsync<GlobalMetaData>(_globalMetaDataLocation);
                
                if (metadata.OrphanPages.Contains(oldName))
                {
                    metadata.OrphanPages.Remove(oldName);
                    metadata.OrphanPages.Add(newName);
                    await SaveMetadataAsync(metadata, _globalMetaDataLocation);
                }

                // Rename the file
                File.Move(oldPath, newPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}