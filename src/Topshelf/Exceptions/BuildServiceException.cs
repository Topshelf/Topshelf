namespace Topshelf.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BuildServiceException : 
        Exception
    {
        public BuildServiceException(string name) : base(string.Format("Couldn't build '{0}'.", name))
        {
            
        }

        public BuildServiceException(string name, Type serviceType)
            : base(string.Format("Couldn't build service '{0}' named '{1}'.", serviceType.Name, name))
        {

        }

		public BuildServiceException(string name, Type serviceType, Exception innerException)
			: base(string.Format("Couldn't build service '{0}' named '{1}'.", serviceType.Name, name), innerException)
		{

		}

        public BuildServiceException()
        {
        }

        public BuildServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BuildServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}