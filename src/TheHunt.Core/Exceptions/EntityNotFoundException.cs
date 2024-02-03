using System.Runtime.Serialization;

namespace TheHunt.Core.Exceptions;

[Serializable]
public class EntityNotFoundException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public EntityNotFoundException()
    {
    }

    public EntityNotFoundException(string message) : base(message)
    {
    }

    public EntityNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }

    public static EntityNotFoundException CompetitionNotFound =>
        new("There is no active competition associated with this channel.");
}