using Pea.Meter.Helpers;

namespace Pea.Meter.Models;

public class UserLoggedInMessage(IAuthData authData)
{
    public IAuthData AuthData { get; } = authData;
}