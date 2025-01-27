using System;
using System.Collections.Generic;
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



        //public ICommand ChangePenColorCommand { get; } 

        public PenViewModel()
        {
            _settingsServices =  new SettingsServices();
            PenSettings = _settingsServices.LoadPenSettings();
        }


        public Color PenColor

        {
            get => PenSettings.PenColor;
            set
            {
                PenSettings.PenColor = value;
                OnPropertyChanged(nameof(PenColor));
            }

        }
        public double ThickNess
        {
            get => PenSettings.Thickness;
            set
            {
                PenSettings.Thickness = value;
                OnPropertyChanged(nameof(ThickNess));
            }
        }

        public byte Red
        {
            get => PenSettings.red;
            set
            {
                PenSettings.red = (byte) value;
                PenSettings.PenColor = Color.FromArgb(PenSettings.Opacity, PenSettings.red, PenSettings.green, PenSettings.blue);
                OnPropertyChanged(nameof(Red));
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
            }
        }


        public StylusTip Tip
        {
            get => PenSettings.Tip;
            set
            {
                PenSettings.Tip = value;
                OnPropertyChanged(nameof(Tip));
            }
        }

        public bool IsEraser
        {
            get => PenSettings.IsEraser;
            set
            {
                PenSettings.IsEraser = value;
                OnPropertyChanged(nameof(IsEraser));

            }
        }

        public bool IsHighlighter
        {
            get => PenSettings.IsHighlighter;
            set
            {
                PenSettings.IsHighlighter = value;
                OnPropertyChanged(nameof(IsHighlighter));
            }
        }

        public bool IgnorePreassure
        {
            get => PenSettings.IgnorePressure;
            set
            {
                PenSettings.IgnorePressure = value;
                OnPropertyChanged(nameof(IgnorePreassure));
            }
        }

        public bool FitToCurve
        {
            get => PenSettings.FitToCurve;
            set
            {
                PenSettings.FitToCurve = value;
                OnPropertyChanged(nameof(FitToCurve));
            }
        }
        public DrawingAttributes GetDrawingAttributes()
        {
            return PenSettings.getdrawingattributes();
        }

        public void SavePenSettings()
        {
            _settingsServices.SavePenSettings(PenSettings);
        }   

    }
}