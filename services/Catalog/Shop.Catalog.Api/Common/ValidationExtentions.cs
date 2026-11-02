using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Shop.Catalog.Api.Common;

public static class ValidationExtentions
{
    public static void AddToModelState(
        this ValidationResult result,
        ModelStateDictionary modelState)
    {
        foreach(var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}