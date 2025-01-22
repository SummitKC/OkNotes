//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Media;
//using TwoOkNotes.Model;
//using TwoOkNotes.Util;

//namespace TwoOkNotes.ViewModels
//{
//    public class PenViewModel : EditingWIndowViewModel
//    {

//        public PenModel CurrentPenModel { get; set; }
//        public ICommand ChangePenColorCommand { get; }

//        public PenViewModel()
//        {

//            ChangePenColorCommand = new RelayCommand(ChangeColor);
//        }

//        private void ChangeColor(object? obj)
//        {
//            Debug.WriteLine(CurrentPenModel.GetType());
//            if (CurrentPenModel != null)
//            {
//                if (obj is Color newColor)
//                {
//                    CurrentPenModel.PenColor = newColor;
//                }
//            }
//            else
//            {
//                Debug.WriteLine("Not current pen");
//            }
//        }
//    }
//}

