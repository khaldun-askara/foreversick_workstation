using System.Globalization;
using System.Windows.Controls;

namespace foreversick_workstationWPF.ViewModel
{
    public class DoubleValueValidator : ValidationRule
    {
        string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            double res;
            if (!double.TryParse((value ?? string.Empty).ToString(), out res))
                result = new ValidationResult(false, this.ErrorMessage);
            return result;
        }
    }

    public class IntPositiveValueValidator : ValidationRule
    {
        string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            int res;
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return result;
            if (!int.TryParse(value.ToString(), out res) || res < 0 || res > 8)
                result = new ValidationResult(false, this.ErrorMessage);
            return result;
        }
    }
}
