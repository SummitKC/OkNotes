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
        private int _currPenIndex;
        public ObservableCollection<Color> ColorOptions { get; set; }
        private ObservableCollection<PenModel> _availablePens = new ();
        public ICommand SwitchColorCommand { get; }

        private StrokeCollection _previewStrokes;

        //public ICommand ChangePenColorCommand { get; } 
        public PenViewModel()
        {
            _settingsServices = new SettingsServices();
            _penSettings = new PenModel();
            SwitchColorCommand = new RelayCommand(SwitchColor);
            _availablePens.Add(_penSettings);
            InitializePenSettingsAsync();
            InitializeColorOptions();
            CreatePreviewStroke();

        }

        private async void InitializePenSettingsAsync()
        {
            var loadedSettings = await _settingsServices.LoadPenSettings();
            if (loadedSettings != null)
            {
                _penSettings = loadedSettings.LastUsedPen ?? new PenModel();
                OnPropertyChanged(nameof(PenSettings));
                _availablePens = new ObservableCollection<PenModel>(loadedSettings.Pens);

                if (loadedSettings.Pens.IndexOf(_penSettings) > 0)
                {
                    _currPenIndex = loadedSettings.Pens.IndexOf(_penSettings);
                } else _currPenIndex = 0;
                CreatePreviewStroke();

            }
        }

        public void InitializeColorOptions()
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
                Colors.Gray
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
                if (value)
                {
                    _penSettings.PenColor = Color.FromArgb(128, 255, 255, 0);
                }
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
                _penSettings.PenColor = color;
                SavePenSettings();
                CreatePreviewStroke();
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

        public ObservableCollection<PenModel> GetAvailablePens()
        {
            return _availablePens;
        }

        public void AddNewPen()
        {
            PenModel newPen = new PenModel();
            newPen.Name = "Pen " + (_availablePens.Count + 1);
            _availablePens.Add(newPen);
            _currPenIndex = _availablePens.Count - 1;
            PenSettings = newPen;
            SavePenSettings();
            CreatePreviewStroke();
        }

        public void SwitchPen(int index)
        {
            PenSettings = _availablePens[index];
            SavePenSettings();
            CreatePreviewStroke();
        }

        public void DeletePen()
        {
            if (_availablePens.Count > 1)
            {
                _availablePens.RemoveAt(_currPenIndex);
                _currPenIndex = 0;
                PenSettings = _availablePens[_currPenIndex];
                SavePenSettings();
                CreatePreviewStroke();
            }
        }

        public async void SavePenSettings()
        {
            PenSettingsModel curPenSettings = await _settingsServices.LoadPenSettings();
            curPenSettings.LastUsedPen = _penSettings;
            if (curPenSettings.Pens.Count <= _currPenIndex)
            {
                curPenSettings.Pens.Add(PenSettings);
            }
             curPenSettings.Pens[_currPenIndex] = _penSettings;
            await _settingsServices.SavePenSettings(curPenSettings);
        }
    }
}