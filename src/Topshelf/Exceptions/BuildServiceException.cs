namespace Topshelf.Exceptions
{
	using System;
	using System.Runtime.Serialization;
	using Internal;


	[Serializable]
	public class BuildServiceException :
		Exception
	{
		public BuildServiceException([NotNull] string name)
			: base(string.Format("Couldn't build '{0}'.", name))
		{
		}

		public BuildServiceException([NotNull] string name, [NotNull] Type serviceType)
			: base(string.Format("Couldn't build service '{0}' named '{1}'.", serviceType, name))
		{
		}

		public BuildServiceException([NotNull] string name, [NotNull] Type serviceType, Exception innerException)
			: base(string.Format("Couldn't build service '{0}' named '{1}'.", serviceType, name), innerException)
		{
		}

		public BuildServiceException()
		{
		}

		public BuildServiceException(string name, Exception innerException)
			: base(string.Format("Couldn't build '{0}'.", name), innerException)
		{
		}

		protected BuildServiceException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}