using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    static class LogicalSymbols
    {
        public const char And = '\u2227';
        public const char Not = '\u00ac';
        public const char Or = '\u2228';
        public const char Falsum = '\u22a5';
        public const char Conditional = '\u2192';
        public const char Biconditional = '\u2194';
        public const char XOr = '\u22bb';

        public static readonly string Operators = "" + And + Not + Or + Falsum + Conditional + Biconditional + XOr;
    }
}
