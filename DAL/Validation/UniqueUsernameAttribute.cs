using DAL.Models;
using DAL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Validation
{
    public class UniqueUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            IUserService userService = validationContext.GetService(typeof(IUserService)) as IUserService;
            User user = validationContext.ObjectInstance as User;

            if (!userService.IsUsernameUnique(user))
            {
                return new ValidationResult("This username is already taken.", new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
