using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    public static class FileWriter
    {
        public static void WriteCSV(string filePath, IEnumerable<ConversionStep> first, IEnumerable<ConversionStep> second)
        {
            System.IO.File.WriteAllText(filePath, WriteCSVString(first, second), Encoding.UTF8);
        }

        private static string WriteCSVString(IEnumerable<ConversionStep> first, IEnumerable<ConversionStep> second)
        {
            var stringBuilder = new StringBuilder();

            var expressions = first.Select(x => x.Expression).Concat(second.Reverse().Select(x => x.Expression).Skip(1));
            var justtifications = first.Select(x => x.Justification).Concat(second.Reverse().Select(x => x.Justification).TakeWhile(x => x != "Input"));

            foreach (var step in expressions.Zip(justtifications, (exp, jst) => exp + "," + jst))
            {
                stringBuilder.Append(step + "\n");
            }

            return stringBuilder.ToString();
        }
    }
}
