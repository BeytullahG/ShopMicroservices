using Microsoft.AspNetCore.Mvc;

namespace Shop.Catalog.Api.Common;

public static class ControllerExtentions
{
    public static ObjectResult ToProblem(this ControllerBase controller, Error error)
    {
        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        var problem = controller.Problem(statusCode: status, title: error.Message);
        if (problem.Value is ProblemDetails details)
        {
            details.Extensions["code"] = error.Code;
        }
        return problem;
    }
}