﻿using System;
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
            catch (InvalidInputException e)
            {
                MessageBox.Show("Illegal syntax detected.\nValid characters include:\n\tAlphanumeric characters\n\tUnderscores (\"_\")\n\tOperators: \"!\", \"&\", \"|\"\n\nInput cannot end with an operator.");
                return;
            }

            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();
            BoolExpr root = AST.Make(ref enumerator);

            Rewrite.DeM(ref root);

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
