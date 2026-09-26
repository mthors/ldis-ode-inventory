using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LDIS.Core.Export
{
    /// <summary>
    /// Lightweight RFC 4180 compliant CSV writer with UTF-8 and culture safety.
    /// </summary>
    public class CsvWriter
    {
        private readonly TextWriter _writer;

        public CsvWriter(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _writer = writer;
            _writer.NewLine = "\r\n";
        }

        public void WriteRow(params string[] fields)
        {
            WriteRow((IEnumerable<string>)fields);
        }

        public void WriteRow(IEnumerable<string> fields)
        {
            if (fields == null) throw new ArgumentNullException("fields");

            string line = string.Join(",", fields.Select(EscapeField));
            _writer.WriteLine(line);
        }

        public void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Escapes a single field according to RFC 4180 rules.
        /// </summary>
        public static string EscapeField(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            bool mustQuote = value.Contains(",") ||
                             value.Contains("\"") ||
                             value.Contains("\r") ||
                             value.Contains("\n") ||
                             (value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1])));

            if (mustQuote)
            {
                // Double every quote character inside the value
                string escaped = value.Replace("\"", "\"\"");
                return "\"" + escaped + "\"";
            }

            return value;
        }

        public static string FormatNumber(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatNumber(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatDate(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
