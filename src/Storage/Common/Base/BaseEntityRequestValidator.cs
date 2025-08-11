using FluentValidation;
using Storage.Common.Requests;

namespace Storage.Common.Base;

public abstract class BaseEntityRequestValidator<T> : AbstractValidator<T> where T : EntityRequest
{
    public BaseEntityRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}