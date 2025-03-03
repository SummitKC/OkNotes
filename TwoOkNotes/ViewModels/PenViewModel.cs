using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using TwoOkNotes.Model;
using TwoOkNotes.Services;
using TwoOkNotes.Util;
using TwoOkNotes.Views;

namespace TwoOkNotes.ViewModels
{
    public class PenViewModel : ObservableObject
    {
        private readonly SettingsServices _settingsServices;
        private PenModel _penSettings;
        private string _currentPenKey; 
        public ObservableCollection<Color>? ColorOptions { get; set; }
        private Dictionary<string, PenModel> _availablePens = new();
        public ICommand SwitchColorCommand { get; }
        public ICommand DeletePenCommand { get; }

        private StrokeCollection _previewStrokes;

        public event EventHandler? PenDeleted;
        public event EventHandler? PenChanged;

        public PenViewModel()
        {
            _settingsServices = new SettingsServices();
            _penSettings = new PenModel();
            _penSettings.Name = "Default Pen";
            SwitchColorCommand = new RelayCommand(SwitchColor);
            DeletePenCommand = new RelayCommand(DeletePen);

            // Initialize with default pen
            _currentPenKey = _penSettings.Name;
            _availablePens[_currentPenKey] = _penSettings;

            InitializeColorOptions();
            _previewStrokes = new StrokeCollection();
            CreatePreviewStroke();
            InitializePenSettingsAsync();
        }

        private async void InitializePenSettingsAsync()
        {
            var loadedSettings = await _settingsServices.LoadPenSettings();
            if (loadedSettings != null)
            {
                // Load last used pen if available
                if (loadedSettings.LastUsedPen != null)
                {
                    _penSettings = loadedSettings.LastUsedPen;
                }

                // Load available pens
                if (loadedSettings.Pens != null && loadedSettings.Pens.Count > 0)
                {
                    _availablePens = new Dictionary<string, PenModel>(loadedSettings.Pens);
                    
                    // Set current pen key
                    if (_availablePens.ContainsKey(_penSettings.Name))
                    {
                        _currentPenKey = _availablePens.FirstOrDefault(x => x.Value == _penSettings).Key;
                    }
                    else if (_availablePens.Count > 0)
                    {
                        _currentPenKey = _availablePens.First().Key;
                        _penSettings = _availablePens[_currentPenKey];
                    }
                }
                else
                {
                    // If no pens were loaded, initialize with current pen
                    _availablePens = new Dictionary<string, PenModel>
                    {
                        { _penSettings.Name, _penSettings }
                    };
                    _currentPenKey = _penSettings.Name;
                }
                CreatePreviewStroke();
                OnPropertyChanged(nameof(FitToCurve));
                OnPropertyChanged(nameof(IgnorePreassure));
                OnPropertyChanged(nameof(ThickNess));
                OnPropertyChanged(nameof(Opacity)); 
            }
        }

        private void InitializeColorOptions()
        {
            ColorOptions = new ObservableCollection<Color>
            {
                Colors.Black,
                Colors.Red,
                Colors.Blue,
                Colors.Green,
                Colors.Yellow,
                Colors.Purple,
                Colors.Orange,
                Colors.Pink,
                Colors.Brown,
                Colors.Gray,
                Colors.White,
                Colors.LightBlue
            };
        }

        public PenModel PenSettings
        {
            get => _penSettings;
            set
            {
                _penSettings = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(PenSettings));
                OnPropertyChanged(nameof(FitToCurve));
                OnPropertyChanged(nameof(IgnorePreassure));
                OnPropertyChanged(nameof(ThickNess));
                OnPropertyChanged(nameof(Opacity));
            }
        }

        public Color PenColor

        {
            get => _penSettings.PenColor;
            set
            {
                Debug.WriteLine(value);
                _penSettings.PenColor = value;
                SavePenSettings();
                CreatePreviewStroke();
                PenChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged(nameof(PenColor));

            }

        }
        public double ThickNess
        {
            get => _penSettings.Thickness;
            set
            {
                _penSettings.Thickness = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(ThickNess));


            }
        }
        public byte Opacity
        {
            get => _penSettings.Opacity;
            set
            {

                _penSettings.Opacity = value;
                _penSettings.PenColor = Color.FromArgb(_penSettings.Opacity, _penSettings.PenColor.R, _penSettings.PenColor.G, _penSettings.PenColor.B);
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(Opacity));
            }
        }

        public StylusTip Tip
        {
            get => _penSettings.Tip;
            set
            {
                _penSettings.Tip = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(Tip));


            }
        }
        public bool IsHighlighter
        {
            get => _penSettings.IsHighlighter;
            set
            {
                _penSettings.IsHighlighter = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(IsHighlighter));
            }
        }

        public bool IgnorePreassure
        {
            get => _penSettings.IgnorePressure;
            set
            {
                _penSettings.IgnorePressure = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(IgnorePreassure));
            }
        }

        public bool FitToCurve
        {
            get => _penSettings.FitToCurve;
            set
            {
                _penSettings.FitToCurve = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(FitToCurve));


            }
        }

        public StrokeCollection PreviewStrokes
        {
            get => _previewStrokes;
            set
            {
                _previewStrokes = value;
                OnPropertyChanged(nameof(PreviewStrokes));

            }
        }

        private bool _pickColorVisiability;
        public bool PickColorVisiability
        {
            get => _pickColorVisiability;
            set
            {
                _pickColorVisiability = value;
                OnPropertyChanged(nameof(PickColorVisiability));
            }
        }


        public void SwitchColor(object? obj)
        {
            if (obj is Color color)
            {
                PenSettings.PenColor = color;
                SavePenSettings();
                CreatePreviewStroke();
                PenChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CreatePreviewStroke()
        {
            _previewStrokes = new StrokeCollection();

            double startX = 40;
            double endX = 334;
            double centerY = 50;

            StylusPointCollection points = new StylusPointCollection();

            for (int i = 0; i <= 100; i++)
            {
                double t = i / 100.0;
                double x = startX + (endX - startX) * t;
                double y = centerY + Math.Sin(t * Math.PI) * 30;
                float pressure = 0.7f - ((float)t * 0.7f);

                points.Add(new StylusPoint(x, y, pressure));
            }

            var previewStroke = new Stroke(points)
            {
                DrawingAttributes = GetDrawingAttributes()
            };

            _previewStrokes.Add(previewStroke);

            OnPropertyChanged(nameof(PreviewStrokes));
        }

        public DrawingAttributes GetDrawingAttributes()
        {
            return _penSettings.getdrawingattributes();
        }

        public ObservableCollection<KeyValuePair<string, PenModel>> GetAvailablePens()
        {
            return new ObservableCollection<KeyValuePair<string, PenModel>>(_availablePens);
        }

        public void AddNewPen()
        {
            PenModel newPen = new PenModel();
            string baseName = "Pen";
            string penName = baseName + " " + (_availablePens.Count + 1);
            
            // Ensure unique name
            int counter = 1;
            while (_availablePens.ContainsKey(penName))
            {
                counter++;
                penName = baseName + " " + counter;
            }
            
            newPen.Name = penName;
            _availablePens[penName] = newPen;
            _currentPenKey = penName;
            PenSettings = newPen;
            
            SavePenSettings();
            CreatePreviewStroke();
        }

        public void SwitchPen(string penName)
        {
            if (_availablePens.ContainsKey(penName))
            {
                _currentPenKey = penName;
                PenSettings = _availablePens[penName];
                SavePenSettings();
                CreatePreviewStroke();
            }
        }

        public void DeletePen(object? obj)
        {
            if (_availablePens.Count > 1)
            {
                _availablePens.Remove(_currentPenKey);
                
                // Switch to first available pen
                _currentPenKey = _availablePens.First().Key;
                PenSettings = _availablePens[_currentPenKey];
                
                SavePenSettings();
                CreatePreviewStroke();

                PenDeleted?.Invoke(this, EventArgs.Empty);

            }
        }

        public async void SavePenSettings()
        {
            PenSettingsModel penSettings = new PenSettingsModel
            {
                LastUsedPen = _penSettings,
                Pens = _availablePens
            };
            
            await _settingsServices.SavePenSettings(penSettings);
        }
    }
}