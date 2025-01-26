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
        public Color PenColor { get; set; } = Colors.Blue;
        public double Thickness { get; set; } = 20.0;
        public double Opacity { get; set; } = 255.0;
        public StylusTip Tip { get; set; } = StylusTip.Ellipse;
        public bool IsEraser { get; set; } = false;
        public bool IsHighlighter { get; set; } = false;
        public bool IgnorePressure { get; set; } = false;
        public bool FitToCurve { get; set; } = false;

        //public Color _penColor = Colors.Blue;
        //public double _thickness = 20.0;
        //public double _opacity = 1;
        //public StylusTip _penTip = StylusTip.Ellipse;
        //public bool _isEraser = false;
        //public bool _isHighlighter = false;
        //public bool _ignorePreassure = false;
        //public bool _fitToCurve = false;


        //TODo it support ARBG for Opacity
        //public Color PenColor
        //{
        //    get => _penColor;
        //    set
        //    {
        //        _penColor = value;
        //        OnPropertyChanged(nameof(PenColor));
        //    }
        //}

        //public double ThickNess
        //{
        //    get => _thickness;
        //    set
        //    {
        //        _thickness = value;
        //        OnPropertyChanged(nameof(ThickNess));
        //    }
        //}

        ////TODO: Use argb values to set opacity 
        //public double Opacity
        //{
        //    get => _opacity;
        //    set
        //    {
        //        _opacity = value;
        //        OnPropertyChanged(nameof(Opacity));
        //    }
        //}

        //public StylusTip Tip
        //{
        //    get => _penTip;
        //    set
        //    {
        //        _penTip = value;
        //        OnPropertyChanged(nameof(Tip));
        //    }
        //}

        //public bool IsEraser
        //{
        //    get => _isEraser;
        //    set
        //    {
        //        _isEraser = value;
        //        OnPropertyChanged(nameof(IsEraser));

        //    }
        //}

        //public bool IsHighlighter
        //{
        //    get => _isHighlighter;
        //    set
        //    {
        //        _isHighlighter = value;
        //        OnPropertyChanged(nameof(IsHighlighter));
        //    }
        //}

        //public bool IgnorePreassure
        //{
        //    get => _ignorePreassure;
        //    set
        //    {
        //        _ignorePreassure = value;
        //        OnPropertyChanged(nameof(IgnorePreassure));
        //    }
        //}

        //public bool FitToCurve
        //{
        //    get => _fitToCurve;
        //    set
        //    {
        //        _fitToCurve = value;
        //        OnPropertyChanged(nameof(FitToCurve));
        //    }
        //}

        public DrawingAttributes getdrawingattributes()
        {
            return new DrawingAttributes
            {
                Color = PenColor,
                Width = Thickness,
                Height = Thickness,
                StylusTip = Tip,
                IsHighlighter = IsHighlighter,
                IgnorePressure = IgnorePressure,
                FitToCurve = FitToCurve,
            };
        }
    }
}