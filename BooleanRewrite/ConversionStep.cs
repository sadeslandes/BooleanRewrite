using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
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
