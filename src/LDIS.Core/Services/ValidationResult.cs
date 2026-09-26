using System;
using System.Collections.Generic;

namespace LDIS.Core.Services
{
    public class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        public bool IsValid
        {
            get { return _errors.Count == 0; }
        }

        public IList<string> Errors
        {
            get { return _errors.AsReadOnly(); }
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                _errors.Add(error.Trim());
            }
        }

        public string ErrorMessage
        {
            get { return string.Join(Environment.NewLine, _errors); }
        }
    }
}
