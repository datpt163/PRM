using Capstone.Application.Module.Projects.Command;
using FluentValidation;
using System.ComponentModel;

namespace Capstone.Api.Module.Projects.Validators
{
    public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectValidator() { 
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.StartDate)
           .Must(BeValidDateWithoutTimeZone)
           .WithMessage("Start date must not include time zone information.");

            RuleFor(x => x.EndDate)
         .Must(BeValidDateWithoutTimeZone)
         .WithMessage("End date of birth must not include time zone information.");
        }

        private bool BeValidDateWithoutTimeZone(DateTime? dob)
        {
            if(dob != null)
                return dob.Value.Kind == DateTimeKind.Unspecified;
            else
                return true;
        }
    }
}


