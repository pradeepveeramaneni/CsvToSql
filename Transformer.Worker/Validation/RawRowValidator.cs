using Common.Contracts;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformer.Worker.Validation
{
    public sealed class RawRowValidator: AbstractValidator<RawRowEvent>
    {
        public RawRowValidator() {

            RuleFor(x => x.Columns).NotNull().Must(c => c.Length == 13)
                .WithMessage("Expected 13 columns per row.");
            RuleFor(x => x.Columns[3]).Must(c=>decimal.TryParse(c, out _))
                .WithMessage("Units Sold must be numeric.");
            RuleFor(x => x.Columns[4]).Must(c => decimal.TryParse(c, out _))
                        .WithMessage("Revenue must be numeric.");
            RuleFor(x => x.Columns[5]).Must(c => decimal.TryParse(c, out _))
                .WithMessage("COGS must be numeric.");
            RuleFor(x => x.Columns[6]).Must(c => decimal.TryParse(c, out _))
                .WithMessage("Profit must be numeric.");
            RuleFor(x => x.Columns[7]).Must(c => DateOnly.TryParse(c, out _))
                .WithMessage("Purchase Date must be a date.");


        }
    }
}
