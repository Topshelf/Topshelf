namespace Topshelf.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CouldntFindServiceException : 
        Exception
    {
        public CouldntFindServiceException(string name) : base(string.Format("Couldn't find '{0}' in the ServiceLocator", name))
        {
            
        }

        public CouldntFindServiceException(string name, Type serviceType)
            : base(string.Format("Couldn't find service '{0}' named '{1}' in the ServiceLocator", serviceType.Name, name))
        {

        }

        public CouldntFindServiceException()
        {
        }

        public CouldntFindServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CouldntFindServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}