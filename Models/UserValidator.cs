using Inv_M_Sys.Models;
using System.Collections.Generic;

public static class UserValidator
{
    public static List<string> Validate(User user, bool validatePassword = false, string confirmPassword = "")
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(user.FirstName))
            errors.Add("First name is required.");

        if (string.IsNullOrWhiteSpace(user.LastName))
            errors.Add("Last name is required.");

        if (string.IsNullOrWhiteSpace(user.Email))
            errors.Add("Email is required.");

        if (string.IsNullOrWhiteSpace(user.Phone))
            errors.Add("Phone number is required.");

        if (string.IsNullOrWhiteSpace(user.Address))
            errors.Add("Address is required.");

        if (string.IsNullOrWhiteSpace(user.Username))
            errors.Add("Username is required.");

        if (validatePassword)
        {
            if (string.IsNullOrWhiteSpace(user.HashedPassword))
                errors.Add("Password is required.");

            if (string.IsNullOrWhiteSpace(confirmPassword))
                errors.Add("Confirm password is required.");

            if (user.HashedPassword != confirmPassword)
                errors.Add("Passwords do not match.");
        }

        return errors;
    }
}
