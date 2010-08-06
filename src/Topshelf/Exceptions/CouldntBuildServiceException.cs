namespace Topshelf.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CouldntBuildServiceException : 
        Exception
    {
        public CouldntBuildServiceException(string name) : base(string.Format("Couldn't build '{0}'.", name))
        {
            
        }

        public CouldntBuildServiceException(string name, Type serviceType)
            : base(string.Format("Couldn't build service '{0}' named '{1}'.", serviceType.Name, name))
        {

        }

		public CouldntBuildServiceException(string name, Type serviceType, Exception innerException)
			: base(string.Format("Couldn't build service '{0}' named '{1}'.", serviceType.Name, name), innerException)
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