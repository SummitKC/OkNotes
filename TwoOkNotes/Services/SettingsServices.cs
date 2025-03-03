﻿using System;
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
            //initilize file paths
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "TwoOkNotes");
            //Create the  file if it doesn't exist
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            _settingsFilePath = Path.Combine(appFolder, "PenSettings.json");
            _windowSettingsPath = Path.Combine(appFolder, "WindowSettings.json");
        }

        public async Task SavePenSettings(PenSettingsModel settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            string json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }

        public async Task<PenSettingsModel> LoadPenSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                return JsonSerializer.Deserialize<PenSettingsModel>(json) ?? new PenSettingsModel();
            }
            else
            {
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
            try
            {
                await File.WriteAllTextAsync(_windowSettingsPath, json);
                Debug.WriteLine("Window settings saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save window settings: {ex.Message}");
            }
        }

        public async Task<WindowSettings> LoadEditingWindowSettings()
        {
            if (File.Exists(_windowSettingsPath))
            {
                string json = await File.ReadAllTextAsync(_windowSettingsPath);
                return JsonSerializer.Deserialize<WindowSettings>(json) ?? new WindowSettings();
            }
            else
            {
                return new WindowSettings();
            }
        }
    }
}
