using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using TwoOkNotes.Util;

namespace TwoOkNotes.Model
{
    //TODO: Change this logic to be in the viewmodel insted of the model, most functionality is working now 
    public class PenModel : ObservableObject
    {
        private Color _penColor = Colors.Blue;
        private double _thickness = 20.0;
        private double _opacity = 1; 
        private StylusTip _penTip = StylusTip.Ellipse;
        private bool _isEraser = false;
        private bool _isHighlighter = false;
        private bool _ignorePreassure = false;
        private bool _fitToCurve = false;


        //TODo it support ARBG for Opacity
        public Color PenColor
        {
            get => _penColor;
            set
            {
                _penColor = value;
                OnPropertyChanged(nameof(PenColor));
            }
        }

        public double ThickNess
        {
            get => _thickness;
            set
            {
                _thickness = value;
                OnPropertyChanged(nameof(ThickNess));
            }
        }

        //TODO: Use argb values to set opacity 
        public double Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                OnPropertyChanged(nameof(Opacity));
            }
        }

        public StylusTip Tip
        {
            get => _penTip;
            set
            {
                _penTip = value;
                OnPropertyChanged(nameof(Tip));
            }
        }

        public bool IsEraser
        {
            get => _isEraser;
            set
            {
                _isEraser = value;
                OnPropertyChanged(nameof(IsEraser));

            }
        }

        public bool IsHighlighter
        {
            get => _isHighlighter;
            set
            {
                _isHighlighter = value;
                OnPropertyChanged(nameof(IsHighlighter));
            }
        }

        public bool IgnorePreassure
        {
            get => _ignorePreassure;
            set
            {
                _ignorePreassure = value;
                OnPropertyChanged(nameof(IgnorePreassure));
            }
        }

        public bool FitToCurve
        {
            get => _fitToCurve;
            set
            {
                _fitToCurve = value;
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
