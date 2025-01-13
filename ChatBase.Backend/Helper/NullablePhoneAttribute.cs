using System.ComponentModel.DataAnnotations;

namespace ChatBase.Backend.Helper;

public class NullablePhoneAttribute : ValidationAttribute
{
    /// <summary>
    /// Returns true if phone is empty or valid.
    /// </summary>
    /// <param name="value">The value of the object to validate.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified value is valid; otherwise, <see langword="false" />.
    /// </returns>
    public override bool IsValid(object value)
    {
        if (value == null)
        {
            return true;
        }
        if (string.IsNullOrEmpty(value.ToString()))
        {
            return true;
        }
        PhoneAttribute phone = new PhoneAttribute();
        return phone.IsValid(value);
    }

}
