using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerAI.Core.DTOs;

namespace CustomerAI.Services.Validators
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto> 
    {
        public CreateCustomerValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Müşteri Adı Boş Olamaz.")
                .Length(2, 100).WithMessage("İsim 2 ile 100 karakter arasında olmalıdır.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email adresi zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.Sector)
                .NotEmpty().WithMessage("Sektör bilgisi boş bırakılamaz.");

            RuleFor(x => x.City)
                .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.City))
                .WithMessage("Şehir ismi en az 3 karakter olmalıdır.");
        }
    }
}
