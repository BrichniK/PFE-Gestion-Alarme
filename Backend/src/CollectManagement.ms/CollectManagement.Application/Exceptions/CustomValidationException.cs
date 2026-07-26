
using FluentValidation.Results;

namespace CollectManagement.Application.Exceptions
{
    public class CustomValidationException : ApplicationException
    {
        public List<string> ValdationErrors { get; }

        public CustomValidationException(ValidationResult? validationResult)
        {
            ValdationErrors = [];
            
            if (validationResult != null)
                ValdationErrors.AddRange(
                    validationResult.Errors.ConvertAll(validationFailure => 
                        validationFailure.ErrorMessage));
        }
    }
}
