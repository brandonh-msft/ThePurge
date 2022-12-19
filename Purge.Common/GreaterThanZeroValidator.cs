using PowerArgs;

namespace Purge.Common
{
    public class GreaterThanZeroValidator : ArgValidator
    {
        public override void Validate(string name, ref string arg)
        {
            if (uint.TryParse(arg, out var value))
            {
                if (value <= 0)
                {
                    throw new ValidationArgException($"ERROR: '{name}' must be greater than 0");
                }
            }
            else
            {
                throw new ValidationArgException($"ERROR: '{name}' must be a number greater than 0");
            }

            base.Validate(name, ref arg);
        }
    }
}
