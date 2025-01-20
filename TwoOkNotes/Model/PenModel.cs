using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;

namespace TwoOkNotes.Model
{
    public class PenModel
    {
        public Color PenColor { get; set; } 
        public double Thickness { get; set; } 
        public double Opacity { get; set; }
        public StylusTip PenTip { get; set; }
        public bool IsEraser { get; set; }

        public PenModel(String color, double thickness, double opacity, StylusTip tip, bool isEraser)
        {
            PenColor = (Color)ColorConverter.ConvertFromString(color);
            Thickness = thickness;
            Opacity = opacity;
            PenTip = tip;
            IsEraser = isEraser;
        }
    }
}
