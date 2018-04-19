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
                if (String.IsNullOrEmpty(value))
                {
                    inputText = "";
                }
                else
                {
                    value = value.Replace('&', LogicalSymbols.And);
                    value = value.Replace('|', LogicalSymbols.Or);
                    value = value.Replace('!', LogicalSymbols.Not);
                    value = value.Replace('~', LogicalSymbols.Not);
                    inputText = value;
                }
                OnPropertyChanged();
            }
        }

        string inputText2;
        public string InputText2
        {
            get { return inputText2; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    inputText2 = "";
                }
                else
                {
                    value = value.Replace('&', LogicalSymbols.And);
                    value = value.Replace('|', LogicalSymbols.Or);
                    value = value.Replace('!', LogicalSymbols.Not);
                    value = value.Replace('~', LogicalSymbols.Not);
                    inputText2 = value;
                }
                OnPropertyChanged();
            }
        }


        private string variables;
        public string Variables
        {
            get
            {
                return variables;
            }
            set
            {
                variables = string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
                OnPropertyChanged();
            }
        }

        public bool ReverseOrder { get; set; } = false;


        public IList<ConversionStep> Steps { get; private set; }

        private void Evaluate()
        {
#if DEBUG
            Console.WriteLine("Re-running");
#endif
            var variablesList = Variables.Split(',').Where(s => !String.IsNullOrEmpty(s));
            if(variablesList.Count() == 0)
            {
                MessageBox.Show("No variable names given.\n\nType all desired variable names (seperated by commas) into the variables textbox.","Error");
                return;
            }

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

            Steps = tree.Evaluate(variablesList, ReverseOrder);
            OnPropertyChanged(nameof(Steps));
        }

        public ICommand EvaluateCommand
        {
            get
            {
                return new RelayCommand(o=>Evaluate(),o=>!String.IsNullOrEmpty(InputText));
            }
        }

        public ICommand AppendTextCommand
        {
            get
            {
                return new RelayCommand(o=>AppendText(o as string, nameof(InputText)));
            }
        }

        public ICommand AppendTextCommand2
        {
            get
            {
                return new RelayCommand(o => AppendText(o as string, nameof(InputText2)));
            }
        }

        private void AppendText(string text, string propName)
        {
            if (propName == nameof(InputText))
                InputText += text;
            else
                InputText2 += text;
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
