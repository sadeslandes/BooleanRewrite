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
            Steps1 = new ObservableCollection<ConversionStep>();
            Steps2 = new ObservableCollection<ConversionStep>();
        }

        #region Properties
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
                    inputText = ReplaceLogicalSymbols(value);
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
                    inputText2 = ReplaceLogicalSymbols(value);
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

        #endregion

        #region Methods
        /// <summary>
        /// replaces keyboard shortcuts with logical symbols
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ReplaceLogicalSymbols(string input)
        {
            input = input.Replace('&', LogicalSymbols.And);
            input = input.Replace('|', LogicalSymbols.Or);
            input = input.Replace('!', LogicalSymbols.Not);
            input = input.Replace('~', LogicalSymbols.Not);
            input = input.Replace('$', LogicalSymbols.Conditional);
            input = input.Replace('%', LogicalSymbols.Biconditional);
            input = input.Replace('#', LogicalSymbols.XOr);
            return input;
        }

        private void Evaluate()
        {
            IEnumerable<string> variablesList = Variables.Split(',').Where(s => !String.IsNullOrEmpty(s));
            if(variablesList.Count() == 0)
            {
                MessageBox.Show("No variable names given.\n\nType all desired variable names (seperated by commas) into the variables textbox.","Error");
                return;
            }

            Steps1.Clear();
            Steps2.Clear();
            OnPropertyChanged(nameof(Steps1));
            OnPropertyChanged(nameof(Steps2));

            if(!String.IsNullOrWhiteSpace(InputText))
            {
                Steps1 = EvaluateExpression(variablesList, InputText);
                OnPropertyChanged(nameof(Steps1));
            }
            if(!String.IsNullOrWhiteSpace(InputText2))
            {
                Steps2 = EvaluateExpression(variablesList, InputText2);
                OnPropertyChanged(nameof(Steps2));
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
                    ResultColor = Brushes.LightGreen;
                }
                else
                {
                    ResultText = "Not equivalent";
                    ResultColor = Brushes.LightCoral;
                }
            }
        }

        private void ResetResults()
        {
            ResultColor = Brushes.Transparent;
            ResultText = String.Empty;
        }

        private IList<ConversionStep> EvaluateExpression(IEnumerable<string> variables, string input)
        {
            List<Token> tokens = null;
            var stripped = input.Replace(" ", String.Empty);
            try
            {
                tokens = Token.Tokenize(stripped,variables);
            }
            catch (IllegalCharacterException)
            {
                MessageBox.Show("Illegal character detected.\nValid characters include:\n\tAlphanumeric characters and parentheses\n\tUnderscores (\"_\")\n\tOperators: \"!\", \"~\", \"&\", \"|\"\n\nInput cannot end with an operator.");
                return new ObservableCollection<ConversionStep>();
            }
            catch (ParenthesisMismatchExeption)
            {
                MessageBox.Show("Number of parentheses do not match.");
                return new ObservableCollection<ConversionStep>();
            }
            catch (IllegalVariableException e)
            {
                MessageBox.Show(e.Message);
                return new ObservableCollection<ConversionStep>();
            }


            AST tree;
            try
            {
                tree = new AST(tokens);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not parse expression.");
                return new ObservableCollection<ConversionStep>();
            }

            return tree.Evaluate(variables, ReverseOrder);
        }

        public void Export(IEnumerable<ConversionStep> first, IEnumerable<ConversionStep> second)
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
                    FileWriter.WriteCSV(fileDialog.FileName, first, second);
                }
            }
        }
        #endregion

        #region Commands
        public ICommand EvaluateCommand
        {
            get
            {
                return new RelayCommand(o=>Evaluate()/*, o=>!String.IsNullOrEmpty(Variables)*/);
            }
        }

        public ICommand ExportLeftRightCommand
        {
            get { return new RelayCommand(o => Export(Steps1, Steps2)); }
        }

        public ICommand ExportRightLeftCommand
        {
            get { return new RelayCommand(o => Export(Steps2, Steps1)); }
        }
        #endregion

    }
}
