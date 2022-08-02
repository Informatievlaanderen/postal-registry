namespace PostalRegistry
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public abstract class PostalRegistryException : DomainException
    {
        protected PostalRegistryException()
        { }

        protected PostalRegistryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        protected PostalRegistryException(string message)
            : base(message)
        { }

        protected PostalRegistryException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
