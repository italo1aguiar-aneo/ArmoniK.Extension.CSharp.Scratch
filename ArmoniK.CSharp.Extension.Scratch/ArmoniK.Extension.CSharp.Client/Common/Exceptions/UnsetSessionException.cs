using System;

namespace ArmoniK.Extension.CSharp.Client.Common.Exceptions;

public class UnsetSessionException : Exception
{
    public UnsetSessionException()
        : base("The session has not been set.")
    {
    }

    public UnsetSessionException(string message)
        : base(message)
    {
    }

    public UnsetSessionException(string message, Exception inner)
        : base(message, inner)
    {
    }
}