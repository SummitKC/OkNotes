using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        public string Name { get; set; } = "Pen 1";
        public Color PenColor { get; set; } = Colors.White;
        public double Thickness { get; set; } = 20.0;
        public byte Opacity { get; set; } = (byte) 255;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StylusTip Tip { get; set; } = StylusTip.Ellipse;
        public bool IsHighlighter { get; set; } = false;
        public bool IgnorePressure { get; set; } = false;
        public bool FitToCurve { get; set; } = false;

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