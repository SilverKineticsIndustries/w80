using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Security;

public class RequestSecurityContext
    : ISecurityContext
{
    public Role Role { get; private set; }
    public ObjectId UserId { get; private set; }
    public string Language { get; } = null!;
    public string Region { get; } = null!;

    public RequestSecurityContext(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor?.HttpContext?.User;
        if (user is not null && user.Identity != null && user.Identity.IsAuthenticated)
        {
            UserId = ObjectId.Parse(user.Claims.First(x => x.Type == "ID").Value);
            Role = (Role)Enum.Parse(typeof(Role), user.Claims.First(x => x.Type == "Role").Value);

            var culture = user.Claims.FirstOrDefault(x => x.Type == "Culture");
            var cultureParts = (culture != null ? culture.Value : SupportedCultures.DefaultCulture).Split("-");
            Language = cultureParts[0];
            Region = cultureParts[1];
        }
    }

    public bool CanAccess(ObjectId userId)
    {
        return
            UserId == userId || Role == Role.Administrator;
    }
}