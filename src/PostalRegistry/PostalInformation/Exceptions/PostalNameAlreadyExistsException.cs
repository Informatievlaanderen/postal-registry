namespace PostalRegistry.PostalInformation.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class PostalNameAlreadyExistsException : PostalRegistryException
    {
        public PostalName PostalName { get; }

        public PostalNameAlreadyExistsException(PostalName postalName)
        {
            PostalName = postalName;
        }

        private PostalNameAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
