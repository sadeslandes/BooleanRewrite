using System;
using System.Collections.Generic;
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
        public MainWindowViewModel()
        {
            OutputText = "Initialized";
        }

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

        string outputText;
        public string OutputText
        {
            get { return outputText; }
            set
            {
                outputText = value;
                OnPropertyChanged();
            }
        }

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

            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();

            BoolExpr root; 
            try
            {
                root = AST.Make(ref enumerator);
            }
            catch (Exception)
            {

                MessageBox.Show("Could not parse expression.");
                return;
            }

            //Rewrite.DeM(ref root);

            OutputText = AST.PrettyPrint(root);
        }

        public ICommand EvaluateCommand
        {
            get
            {
                return new RelayCommand(o=>Evaluate(),o=>!String.IsNullOrEmpty(InputText));
            }
        }
    }
}
