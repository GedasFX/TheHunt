using System.Runtime.Serialization;

namespace TheHunt.Core.Exceptions;

[Serializable]
public class EntityValidationException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public EntityValidationException()
    {
    }

    public EntityValidationException(string message) : base(message)
    {
    }

    public EntityValidationException(string message, Exception inner) : base(message, inner)
    {
    }

    protected EntityValidationException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}