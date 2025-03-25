using System;
using System.Runtime.Serialization;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual.Helpers
{
	[Serializable]
	public class TrialLicenseException : Exception
	{
		public TrialLicenseException()
		{
		}

		public TrialLicenseException(string message)
			: base(message)
		{
		}

		public TrialLicenseException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected TrialLicenseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
