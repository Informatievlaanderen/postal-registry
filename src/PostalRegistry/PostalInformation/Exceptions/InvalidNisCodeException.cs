namespace PostalRegistry.PostalInformation.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class InvalidNisCodeException : PostalRegistryException
    {
        public NisCode? NewNisCode { get; }

        public InvalidNisCodeException(NisCode? newNisCode)
        {
            NewNisCode = newNisCode;
        }

        private InvalidNisCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
