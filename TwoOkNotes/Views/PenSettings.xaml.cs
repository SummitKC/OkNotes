using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoOkNotes.ViewModels;

namespace TwoOkNotes.Views
{
    /// <summary>
    /// Interaction logic for PenSettings.xaml
    /// </summary>
    public partial class PenSettings : UserControl
    {
        public PenSettings()
        {
            InitializeComponent();
        }
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rectangle && rectangle.Fill is SolidColorBrush brush)
            {
                if (DataContext is PenViewModel viewModel)
                {
                    viewModel.SelectColorCommand.Execute(brush);
                }
            }
        }
    }

}
