using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TwoOkNotes.Views;
using System.Text.Json;
using System.Text.Json.Serialization;
using TwoOkNotes.Model;
using System.Diagnostics;
namespace TwoOkNotes.Services
{
    public class SettingsServices
    {
        private readonly string _settingsFilePath;
        private readonly string _windowSettingsPath;
        public SettingsServices()
        {
            try
            {
                //initilize file paths
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataFolder, "TwoOkNotes");
                //Create the  file if it doesn't exist
                if (!Directory.Exists(appFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(appFolder);
                    }
                    catch (IOException ex)
                    {
                        // Handle IO error silently
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Handle access denied silently
                    }
                    catch (Exception ex)
                    {
                        // Handle unexpected error silently
                    }
                }
                _settingsFilePath = Path.Combine(appFolder, "PenSettings.json");
                _windowSettingsPath = Path.Combine(appFolder, "WindowSettings.json");
            }
            catch (Exception ex)
            {
                // Fallback to default paths in case of error
                _settingsFilePath = "PenSettings.json";
                _windowSettingsPath = "WindowSettings.json";
            }
        }

        public async Task SavePenSettings(PenSettingsModel settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            string json = JsonSerializer.Serialize(settings, options);
            
            int maxRetries = 3;
            int retryDelayMs = 100;
            int currentRetry = 0;

            while (currentRetry <= maxRetries)
            {
                try
                {
                    await File.WriteAllTextAsync(_settingsFilePath, json);
                    return;
                }
                catch (IOException ex)
                {
                    // If it's the last retry, just log the error
                    if (currentRetry == maxRetries)
                    {
                        // Failed to save pen settings after multiple attempts
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // If it's the last retry, just log the error
                    if (currentRetry == maxRetries)
                    {
                        // Failed to save pen settings after multiple attempts
                    }
                }
                catch (Exception ex)
                {
                    // If it's the last retry, just log the error
                    if (currentRetry == maxRetries)
                    {
                        // Failed to save pen settings after multiple attempts
                    }
                }

                // Increment retry counter and wait before retrying
                currentRetry++;
                if (currentRetry <= maxRetries)
                {
                    await Task.Delay(retryDelayMs * currentRetry);
                }
            }
        }

        public async Task<PenSettingsModel> LoadPenSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(_settingsFilePath);
                        try
                        {
                            return JsonSerializer.Deserialize<PenSettingsModel>(json) ?? new PenSettingsModel();
                        }
                        catch (JsonException ex)
                        {
                            // Error deserializing pen settings JSON
                            return new PenSettingsModel();
                        }
                    }
                    catch (IOException ex)
                    {
                        // IO error while reading pen settings file
                        return new PenSettingsModel();
                    }
                }
                else
                {
                    // Pen settings file not found, creating default settings
                    return new PenSettingsModel();
                }
            }
            catch (Exception ex)
            {
                // Unexpected error while loading pen settings
                return new PenSettingsModel();
            }
        }

        public async Task SaveEditingWindowSettings(WindowSettings settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string json = JsonSerializer.Serialize(settings, options);
            
            int maxRetries = 3;
            int retryDelayMs = 100;
            int currentRetry = 0;

            while (currentRetry <= maxRetries)
            {
                try
                {
                    await File.WriteAllTextAsync(_windowSettingsPath, json);
                    return;
                }
                catch (IOException ex)
                {
                    // If it's the last retry, just log the error
                    if (currentRetry == maxRetries)
                    {
                        // Failed to save window settings after multiple attempts
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // If it's the last retry, just log the error
                    if (currentRetry == maxRetries)
                    {
                        // Failed to save window settings after multiple attempts
                    }
                }
                catch (Exception ex)
                {
                    // If it's the last retry, just log the error
                    if (currentRetry == maxRetries)
                    {
                        // Failed to save window settings after multiple attempts
                    }
                }

                // Increment retry counter and wait before retrying
                currentRetry++;
                if (currentRetry <= maxRetries)
                {
                    await Task.Delay(retryDelayMs * currentRetry);
                }
            }
        }

        public async Task<WindowSettings> LoadEditingWindowSettings()
        {
            try
            {
                if (File.Exists(_windowSettingsPath))
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(_windowSettingsPath);
                        try
                        {
                            return JsonSerializer.Deserialize<WindowSettings>(json) ?? new WindowSettings();
                        }
                        catch (JsonException ex)
                        {
                            // Error deserializing window settings JSON
                            return new WindowSettings();
                        }
                    }
                    catch (IOException ex)
                    {
                        // IO error while reading window settings file
                        return new WindowSettings();
                    }
                }
                else
                {
                    // Window settings file not found, creating default settings
                    return new WindowSettings();
                }
            }
            catch (Exception ex)
            {
                // Unexpected error while loading window settings
                return new WindowSettings();
            }
        }
    }
}
