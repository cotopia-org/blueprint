using System.ComponentModel.DataAnnotations;

namespace blueprint.core.DataAnotations
{
    public class Password : ValidationAttribute
    {

        public Password()
        {
        }

        public override bool IsValid(object value)
        {
            string strValue = value as string;
            return strValue.Length > 5;
        }
        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }
}
