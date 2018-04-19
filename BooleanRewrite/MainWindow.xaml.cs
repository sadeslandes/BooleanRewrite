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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            VariableTextBox.Focus();
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e) => Close();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(inputBox.SelectedText))
            {
                var temp = inputBox.SelectionStart;
                inputBox.Text = inputBox.Text.Remove(inputBox.SelectionStart,inputBox.SelectionLength);
                inputBox.CaretIndex = temp;
            }
            var index = inputBox.CaretIndex+1;
            inputBox.Text = inputBox.Text.Insert(inputBox.CaretIndex, (string)(sender as Button).Content);
            inputBox.Focus();
            inputBox.CaretIndex = index;
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(inputBox2.SelectedText))
            {
                var temp = inputBox2.SelectionStart;
                inputBox2.Text = inputBox2.Text.Remove(inputBox2.SelectionStart, inputBox2.SelectionLength);
                inputBox2.CaretIndex = temp;
            }
            var index = inputBox2.CaretIndex + 1;
            inputBox2.Text = inputBox2.Text.Insert(inputBox2.CaretIndex, (string)(sender as Button).Content);
            inputBox2.Focus();
            inputBox2.CaretIndex = index;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("1) Enter variable names into \"Variables\" text box, seperated by commas.\n" +
                            "2) Enter expressions into one of both of the text boxes below.\n" +
                            "3) Press <Enter> or click the \"Evaluate\" button\n\n" +
                            "Operator Keyboard Shortcuts:\n" +
                            "#: XOR\n" +
                            "$: Conditional\n" +
                            "%: Biconditional\n" +
                            "&: And\n" +
                            "|: Or\n" +
                            "! or ~: Negation", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
