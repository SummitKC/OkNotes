using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using TwoOkNotes.Model;
using TwoOkNotes.Services;
using TwoOkNotes.Util;
using TwoOkNotes.Views;
using System.Collections.ObjectModel;


namespace TwoOkNotes.ViewModels
{
    public class PenViewModel : ObservableObject
    {

        public SolidColorBrush SelectedColor = new SolidColorBrush(Colors.White);
        private readonly SettingsServices _settingsServices;

        public PenModel PenSettings { get; set; }

        public ICommand SavePenSettingsCommand { get; }

        //public ICommand ChangePenColorCommand { get; } 
        public ICommand OpenColorPickerCommand { get; }
        public ICommand SelectColorCommand { get; }
        public ObservableCollection<SolidColorBrush> AvailableColors { get; set; }
        public PenViewModel()
        {
            _settingsServices = new SettingsServices();

            AvailableColors = new ObservableCollection<SolidColorBrush>
            {
                new SolidColorBrush(Colors.Red),
                new SolidColorBrush(Colors.Green),
                new SolidColorBrush(Colors.Blue),
                new SolidColorBrush(Colors.Yellow),
                new SolidColorBrush(Colors.Black),
                new SolidColorBrush(Colors.White)
            };
            OpenColorPickerCommand = new RelayCommand(_ => OpenColorPicker());
            SelectColorCommand = new RelayCommand(SelectColor);
            InitializePenSettingsAsync();
        }
       
        public bool IsColorPickerOpen
        {
            get => PenSettings._isColorPickerOpen;
            set
            {
                PenSettings._isColorPickerOpen = value;
                OnPropertyChanged(nameof(IsColorPickerOpen));
                SavePenSettings();
            }
        }

        private void OpenColorPicker()
        {
            IsColorPickerOpen = true;
        }

        private void SelectColor(object selectedColor)
        {
            Debug.WriteLine("does it get here? 33");
            if (selectedColor is SolidColorBrush brush)
            {
                PenColor = brush.Color;
            }
            IsColorPickerOpen = false;
        }


        private async void InitializePenSettingsAsync()
        {
            PenSettings = await _settingsServices.LoadPenSettings();
        }

        
        public Color PenColor

        {
            get => PenSettings.PenColor;
            set
            {
                PenSettings.PenColor = value;
                OnPropertyChanged(nameof(PenColor));
                SavePenSettings();
            }

        }
        public double ThickNess
        {
            get => PenSettings.Thickness;
            set
            {
                PenSettings.Thickness = value;
                OnPropertyChanged(nameof(ThickNess));
                SavePenSettings();
            }
        }

        public byte Red
        {
            get => PenSettings.red;
            set
            {
                PenSettings.red = (byte)value;
                PenSettings.PenColor = Color.FromArgb(PenSettings.Opacity, PenSettings.red, PenSettings.green, PenSettings.blue);
                OnPropertyChanged(nameof(Red));
                SavePenSettings();
            }
        }
        public byte Green
        {
            get => PenSettings.green;
            set
            {
                PenSettings.green = (byte)value;
                PenSettings.PenColor = Color.FromArgb(PenSettings.Opacity, PenSettings.red, PenSettings.green, PenSettings.blue);
                OnPropertyChanged(nameof(Green));
                SavePenSettings();
            }
        }
        public byte Blue
        {
            get => PenSettings.blue;
            set
            {
                PenSettings.blue = (byte)value;
                PenSettings.PenColor = Color.FromArgb(PenSettings.Opacity, PenSettings.red, PenSettings.green, PenSettings.blue);
                OnPropertyChanged(nameof(Blue));
                SavePenSettings();
            }
        }
        public byte Opacity
        {
            get => PenSettings.Opacity;
            set
            {

                PenSettings.Opacity = value;
                PenSettings.PenColor = Color.FromArgb((byte)PenSettings.Opacity, PenSettings.PenColor.R, PenSettings.PenColor.G, PenSettings.PenColor.B);
                OnPropertyChanged(nameof(Opacity));
                SavePenSettings();
            }
        }

        public StylusTip Tip
        {
            get => PenSettings.Tip;
            set
            {
                PenSettings.Tip = value;
                OnPropertyChanged(nameof(Tip));
                SavePenSettings();
            }
        }

        public bool IsEraser
        {
            get => PenSettings.IsEraser;
            set
            {
                PenSettings.IsEraser = value;
                OnPropertyChanged(nameof(IsEraser));
                SavePenSettings();

            }
        }

        public bool IsHighlighter
        {
            get => PenSettings.IsHighlighter;
            set
            {
                PenSettings.IsHighlighter = value;
                OnPropertyChanged(nameof(IsHighlighter));
                SavePenSettings();
            }
        }

        public bool IgnorePreassure
        {
            get => PenSettings.IgnorePressure;
            set
            {
                PenSettings.IgnorePressure = value;
                OnPropertyChanged(nameof(IgnorePreassure));
                SavePenSettings();
            }
        }

        public bool FitToCurve
        {
            get => PenSettings.FitToCurve;
            set
            {
                PenSettings.FitToCurve = value;
                OnPropertyChanged(nameof(FitToCurve));
                SavePenSettings();
            }
        }
        public DrawingAttributes GetDrawingAttributes()
        {
            return PenSettings.getdrawingattributes();
        }

        public async void SavePenSettings()
        {
            Debug.WriteLine("Saving Pen Settings, gets to here?");
            await _settingsServices.SavePenSettings(PenSettings);
        }
    }
}