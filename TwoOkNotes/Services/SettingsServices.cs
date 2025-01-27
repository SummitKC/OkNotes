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
namespace TwoOkNotes.Services
{
    public class SettingsServices
    {
        private readonly string _settingsFilePath;
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
        }

        public void SavePenSettings(PenModel settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsFilePath, json);
        }

        public PenModel LoadPenSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                //return the settings if they exist, else return a new instance of the settings
                return JsonSerializer.Deserialize<PenModel>(json) ?? new PenModel();
            }
            else
            {
                return new PenModel();
            }
        }

        public void SaveEditingWindowSettings(CanvasModel settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsFilePath, json);
        }

        public CanvasModel LoadCanvasSettings()
        {
            return null;
        }
    }
}
