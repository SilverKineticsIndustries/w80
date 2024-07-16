using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;
using SilverKinetics.w80.Application;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Controller;

public static class Extensions
{
    public static bool CanCurrentUserPerformActionOnTargetUser(
        this ControllerBase controllerBase,
        ObjectId targetUserId)
    {
        var securityContext = controllerBase.Request.HttpContext.RequestServices.GetRequiredService<ISecurityContext>();
        return securityContext.CanAccess(targetUserId);
    }

    public static IActionResult OkOrNotFound<T>(this ControllerBase controller, T obj)
    {
        if (obj is not null)
            return controller.Ok(obj);
        else
            return controller.NotFound();
    }

    public static IActionResult OkOrValidationErrors<T>(this ControllerBase controller, ComplexResponseDto<T> obj)
    {
        if (obj.Errors is null || obj.Errors.Count == 0)
            return controller.Ok(obj.Result);
        else
            return controller.BadRequest(
                new ProblemDetails() {
                    Type = ProblemDetailsTypes.HasValidationErrors.ToString(),
                    Extensions = new Dictionary<string, object?>() {
                        { "validationErrors", obj.Errors.ToList() }
                    }
                });
    }
}