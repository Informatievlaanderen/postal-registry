namespace PostalRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public abstract class PostalRegistryException : DomainException
    {
        protected PostalRegistryException() { }

        protected PostalRegistryException(string message) : base(message) { }

        protected PostalRegistryException(string message, Exception inner) : base(message, inner) { }
    }
}
