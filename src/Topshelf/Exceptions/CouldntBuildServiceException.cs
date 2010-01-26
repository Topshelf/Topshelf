namespace Topshelf.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CouldntBuildServiceException : 
        Exception
    {
        public CouldntBuildServiceException(string name) : base(string.Format("Couldn't find '{0}' in the ServiceLocator", name))
        {
            
        }

        public CouldntBuildServiceException(string name, Type serviceType)
            : base(string.Format("Couldn't find service '{0}' named '{1}' in the ServiceLocator", serviceType.Name, name))
        {

        }

        public CouldntBuildServiceException()
        {
        }

        public CouldntBuildServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CouldntBuildServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}