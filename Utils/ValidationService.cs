using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Utils
{
    internal class ValidationService
    {
        public string? ValidateId(string value)
        {
            int result;
            bool isSuccesful = int.TryParse(value, out result);
            if (isSuccesful)
                return result.ToString();

            return null;
        }
        public string? ValidateName(string value)
        {
            if(Regex.IsMatch(value, @"^[a-zA-Z]+$"))
                return value;

            return null;
        }

        public string? ValidateAmount(string value)
        {
            int result;
            bool isSuccesful = int.TryParse(value, out result);
            if (isSuccesful)
                if (result >= 0 && result <= 100)
                return result.ToString();

            return null;
        }
    }
}
