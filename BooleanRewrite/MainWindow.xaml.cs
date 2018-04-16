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

namespace BooleanRewrite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool needMoveCursor = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            inputBox.Focus();
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e) => Close();

        private void inputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(needMoveCursor)
            {
                inputBox.Focus();
                inputBox.CaretIndex = inputBox.Text.Length;
                needMoveCursor = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            needMoveCursor = true;
        }
    }
}
