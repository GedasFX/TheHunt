using System;

namespace TheHunt.Application.Helpers;

public static class GuidUtils
{
    public static string ToString(Guid guid)
    {
        return Convert.ToBase64String(guid.ToByteArray());
    }

    public static Guid ToGuid(string str)
    {
        return new Guid(Convert.FromBase64String(str));
    }
}