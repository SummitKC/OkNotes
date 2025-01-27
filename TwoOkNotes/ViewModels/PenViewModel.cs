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
using TwoOkNotes.Util;

namespace TwoOkNotes.ViewModels
{
    public class PenViewModel : ObservableObject
    {



        public readonly PenModel _penModel = new();


        //public ICommand ChangePenColorCommand { get; } 

        public PenViewModel()
        {

            PenModel penModel = new PenModel();

        }


        public Color PenColor

        {
            get => _penModel.PenColor;
            set
            {
                _penModel.PenColor = value;
                OnPropertyChanged(nameof(PenColor));
            }

        }
        public double ThickNess
        {
            get => _penModel.Thickness;
            set
            {
                _penModel.Thickness = value;
                OnPropertyChanged(nameof(ThickNess));
            }
        }

        public byte Red
        {
            get => _penModel.red;
            set
            {
                _penModel.red = (byte) value;
                _penModel.PenColor = Color.FromArgb(_penModel.Opacity, _penModel.red, _penModel.green, _penModel.blue);
                OnPropertyChanged(nameof(Red));
            }
        }
        public byte Green
        {
            get => _penModel.green;
            set
            {
                _penModel.green = (byte)value;
                _penModel.PenColor = Color.FromArgb(_penModel.Opacity, _penModel.red, _penModel.green, _penModel.blue);
                OnPropertyChanged(nameof(Green));
            }
        }
        public byte Blue
        {
            get => _penModel.blue;
            set
            {
                _penModel.blue = (byte)value;
                _penModel.PenColor = Color.FromArgb(_penModel.Opacity, _penModel.red, _penModel.green, _penModel.blue);
                OnPropertyChanged(nameof(Blue));
            }
        }
        public byte Opacity
        {
            get => _penModel.Opacity;
            set
            {
                
                _penModel.Opacity = (byte) value;
                _penModel.PenColor = Color.FromArgb(_penModel.Opacity, _penModel.red, _penModel.green, _penModel.blue);
                OnPropertyChanged(nameof(Opacity));
            }
        }


        public StylusTip Tip
        {
            get => _penModel.Tip;
            set
            {
                _penModel.Tip = value;
                OnPropertyChanged(nameof(Tip));
            }
        }

        public bool IsEraser
        {
            get => _penModel.IsEraser;
            set
            {
                _penModel.IsEraser = value;
                OnPropertyChanged(nameof(IsEraser));

            }
        }

        public bool IsHighlighter
        {
            get => _penModel.IsHighlighter;
            set
            {
                _penModel.IsHighlighter = value;
                OnPropertyChanged(nameof(IsHighlighter));
            }
        }

        public bool IgnorePreassure
        {
            get => _penModel.IgnorePressure;
            set
            {
                _penModel.IgnorePressure = value;
                OnPropertyChanged(nameof(IgnorePreassure));
            }
        }

        public bool FitToCurve
        {
            get => _penModel.FitToCurve;
            set
            {
                _penModel.FitToCurve = value;
                OnPropertyChanged(nameof(FitToCurve));
            }
        }
        public DrawingAttributes GetDrawingAttributes()
        {
            return new DrawingAttributes
            {
                Color = PenColor,
                Width = ThickNess,
                Height = ThickNess,
                StylusTip = Tip,
                IsHighlighter = IsHighlighter,
                IgnorePressure = IgnorePreassure,
                FitToCurve = FitToCurve,
            };
        }

    }
}