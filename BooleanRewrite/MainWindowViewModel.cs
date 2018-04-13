using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BooleanRewrite
{
    class MainWindowViewModel : ViewModelBase
    {
        string inputText;
        public string InputText
        {
            get { return inputText; }
            set
            {
                inputText = value;
                OnPropertyChanged();
            }
        }

        public IList<ConversionStep> Steps { get; private set; }

        private void Evaluate()
        {
#if DEBUG
            Console.WriteLine("Re-running");
#endif
            List<Token> tokens = null;
            var stripped = InputText.Replace(" ", String.Empty);
            try
            {
                tokens = Token.Tokenize(stripped);
            }
            catch (IllegalCharacterException)
            {
                MessageBox.Show("Illegal character detected.\nValid characters include:\n\tAlphanumeric characters and parentheses\n\tUnderscores (\"_\")\n\tOperators: \"!\", \"~\", \"&\", \"|\"\n\nInput cannot end with an operator.");
                return;
            }
            catch (ParenthesisMismatchExeption)
            {
                MessageBox.Show("Number of parentheses do not match.");
                return;
            }


            AST tree;
            try
            {
                tree = new AST(tokens);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not parse expression.");
                return;
            }

            Steps = tree.Evaluate();
            OnPropertyChanged(nameof(Steps));
        }

        public ICommand EvaluateCommand
        {
            get
            {
                return new RelayCommand(o=>Evaluate(),o=>!String.IsNullOrEmpty(InputText));
            }
        }
    }

    public class ConversionStep
    {
        public ConversionStep(string exp, string justification)
        {
            Expression = exp;
            Justification = justification;
        }
        public string Expression { get; }
        public string Justification { get; }
    }
}
