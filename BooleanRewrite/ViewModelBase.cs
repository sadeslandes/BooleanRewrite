using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BooleanRewrite
{

    public class ViewModelBase : INotifyPropertyChanged
    {
        protected void updateTextBoxOnEnter(Object obj)
        {
            var tBox = obj as System.Windows.Controls.TextBox;
            if (tBox != null)
            {
                System.Windows.DependencyProperty prop = System.Windows.Controls.TextBox.TextProperty;
                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                    binding.UpdateSource();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberNameAttribute] string name = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
