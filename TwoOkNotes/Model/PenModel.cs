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
    public class PenModel : ObservableObject
    {
        private Color _penColor = Colors.Blue;
        private double _thickness = 2.0;
        private double _opacity = 1; 
        private StylusTip _penTip = StylusTip.Ellipse;
        private bool _isEraser = false;
        private bool _isHighlighter = false;
        private bool _ignorePreassure = false;
        private bool _fitToCurve = false;

        public Color Color
        {
            get => _penColor;
            set
            {
                _penColor = value;
                OnPropertyChanged(nameof(Color));
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

        public DrawingAttributes GetDrawingAttributes()
        {
            return new DrawingAttributes
            {
                Color = _penColor,
                Width = _thickness,
                Height = _thickness,
                StylusTip = _penTip,
                IsHighlighter = _isHighlighter,
                IgnorePressure = _ignorePreassure,
                FitToCurve = _fitToCurve,
            };
        }
    }
}
