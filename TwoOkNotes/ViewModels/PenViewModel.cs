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
        public PenModel PenSettings { get; set; }
        public ObservableCollection<Color> ColorOptions { get; set; }

        public ICommand SwitchColorCommand { get; }

        private StrokeCollection _previewStrokes;

        //public ICommand ChangePenColorCommand { get; } 
        public PenViewModel()
        {
            _settingsServices = new SettingsServices();
            PenSettings = new PenModel();
            SwitchColorCommand = new RelayCommand(SwitchColor);
            InitializePenSettingsAsync();
            InitializeColorOptions();
        }

        private async void InitializePenSettingsAsync()
        {
            var loadedSettings = await _settingsServices.LoadPenSettings();
            if (loadedSettings != null)
            {
                PenSettings = loadedSettings;
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
        public Color PenColor

        {
            get => PenSettings.PenColor;
            set
            {
                PenSettings.PenColor = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(PenColor));


            }

        }
        public double ThickNess
        {
            get => PenSettings.Thickness;
            set
            {
                PenSettings.Thickness = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(ThickNess));


            }
        }
        public byte Opacity
        {
            get => PenSettings.Opacity;
            set
            {

                PenSettings.Opacity = value;
                PenSettings.PenColor = Color.FromArgb(PenSettings.Opacity, PenSettings.PenColor.R, PenSettings.PenColor.G, PenSettings.PenColor.B);
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(Opacity));


            }
        }

        public StylusTip Tip
        {
            get => PenSettings.Tip;
            set
            {
                PenSettings.Tip = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(Tip));


            }
        }

        public bool IsHighlighter
        {
            get => PenSettings.IsHighlighter;
            set
            {
                PenSettings.IsHighlighter = value;
                if (value)
                {
                    PenSettings.PenColor = Color.FromArgb(128, 255, 255, 0);
                }
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(IsHighlighter));
            }
        }

        public bool IgnorePreassure
        {
            get => PenSettings.IgnorePressure;
            set
            {
                PenSettings.IgnorePressure = value;
                SavePenSettings();
                CreatePreviewStroke();
                OnPropertyChanged(nameof(IgnorePreassure));


            }
        }

        public bool FitToCurve
        {
            get => PenSettings.FitToCurve;
            set
            {
                PenSettings.FitToCurve = value;
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

            // Notify that PreviewStrokes has changed
            OnPropertyChanged(nameof(PreviewStrokes));
        }

        public DrawingAttributes GetDrawingAttributes()
        {
            return PenSettings.getdrawingattributes();
        }

        public async void SavePenSettings()
        {
            await _settingsServices.SavePenSettings(PenSettings);
        }
    }
}