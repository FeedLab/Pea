using Pea.Meter.Helper;

namespace Pea.Meter.Models;

public class UserLoggedInMessage(IAuthData authData)
{
    public IAuthData AuthData { get; } = authData;
}