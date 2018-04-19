using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BooleanRewrite
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Steps1 = new List<ConversionStep>();
            Steps2 = new List<ConversionStep>();
        }

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
                ResetResults();
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
                ResetResults();
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
                ResetResults();
                OnPropertyChanged();
            }
        }

        private string resultText;
        public string ResultText
        {
            get
            {
                return resultText;
            }
            set
            {
                resultText = value;
                OnPropertyChanged();
            }
        }

        private Brush resultColor = Brushes.Transparent;
        public Brush ResultColor
        {
            get
            {
                return resultColor;
            }
            set
            {
                resultColor = value;
                OnPropertyChanged();
            }
        }

        public bool ReverseOrder { get; set; } = false;

        public IList<ConversionStep> Steps1 { get; private set; }
        public IList<ConversionStep> Steps2 { get; private set; }

        private void Evaluate()
        {
            IEnumerable<string> variablesList = Variables.Split(',').Where(s => !String.IsNullOrEmpty(s));
            if(variablesList.Count() == 0)
            {
                MessageBox.Show("No variable names given.\n\nType all desired variable names (seperated by commas) into the variables textbox.","Error");
                return;
            }

            if(!String.IsNullOrWhiteSpace(InputText))
            {
                EvaluateExpression1(variablesList);
            }
            if(!String.IsNullOrWhiteSpace(InputText2))
            {
                EvaluateExpression2(variablesList);
            }

            checkEquivalence();
        }

        private void checkEquivalence()
        {
            if(Steps1 !=  null && Steps1.Count > 0 && Steps2 != null && Steps2.Count > 0)
            {
                if (Steps1.Last().Expression == Steps2.Last().Expression)
                {
                    ResultText = "Equivalent!";
                    ResultColor = Brushes.Green;
                }
                else
                {
                    ResultText = "Not equivalent";
                    ResultColor = Brushes.Red;
                }
            }
        }

        private void ResetResults()
        {
            ResultColor = Brushes.Transparent;
            ResultText = String.Empty;
        }

        private void EvaluateExpression1(IEnumerable<string> variables)
        {
            List<Token> tokens = null;
            var stripped = InputText.Replace(" ", String.Empty);
            try
            {
                tokens = Token.Tokenize(stripped, variables);
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
            catch (IllegalVariableException e)
            {
                MessageBox.Show(e.Message);
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

            Steps1 = tree.Evaluate(variables, ReverseOrder);
            OnPropertyChanged(nameof(Steps1));
        }

        private void EvaluateExpression2(IEnumerable<string> variables)
        {
            List<Token> tokens = null;
            var stripped = InputText2.Replace(" ", String.Empty);
            try
            {
                tokens = Token.Tokenize(stripped,variables);
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
            catch (IllegalVariableException e)
            {
                MessageBox.Show(e.Message);
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

            Steps2 = tree.Evaluate(variables, ReverseOrder);
            OnPropertyChanged(nameof(Steps2));
        }

        public ICommand EvaluateCommand
        {
            get
            {
                return new RelayCommand(o=>Evaluate()/*, o=>!String.IsNullOrEmpty(Variables)*/);
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

        public ICommand ExportCommand
        {
            get { return new RelayCommand(o => Export()); }
        }

        public void Export()
        {
            var fileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString(),
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*",
            };

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // do work to save file
                if (!String.IsNullOrEmpty(fileDialog.FileName))
                {
                    System.IO.File.WriteAllText(fileDialog.FileName, WriteCSV());
                }
            }
        }

        private string WriteCSV()
        {
            var stringBuilder = new StringBuilder();

            var enum_steps1 = Steps1.GetEnumerator();
            var enum_steps2 = Steps2.GetEnumerator();

            bool hasElements1 = true;
            bool hasElements2 = true;
            while(hasElements1 || hasElements2)
            {
                hasElements1 = enum_steps1.MoveNext();
                hasElements2 = enum_steps2.MoveNext();
                stringBuilder.Append($"{(hasElements1 ? enum_steps1.Current.Expression : " ")}," +
                    $"{(hasElements1 ? enum_steps1.Current.Justification : " ")}," +
                    $"{(hasElements2 ? enum_steps2.Current.Expression : " ")}," +
                    $"{(hasElements2 ? enum_steps2.Current.Justification : " ")}\n");
            }

            return stringBuilder.ToString();
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
