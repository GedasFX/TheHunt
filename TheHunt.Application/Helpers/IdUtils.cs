using HashidsNet;

namespace TheHunt.Application.Helpers;

public static class IdUtils
{
    public static IHashids HashIds { get; set; } = new Hashids();
    
    public static string ToUserId(this long id) => HashIds.EncodeLong(id);
    public static long ToInternalId(this string id) => HashIds.DecodeSingleLong(id);
}