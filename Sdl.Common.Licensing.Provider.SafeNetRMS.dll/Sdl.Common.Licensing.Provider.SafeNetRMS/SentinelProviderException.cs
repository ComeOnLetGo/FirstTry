using System;
using com.sntl.licensing;
using Sdl.Common.Licensing.Provider.Core.Exceptions;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	public sealed class SentinelProviderException : LicensingProviderException
	{
		public override long? ProviderErrorCode
		{
			get
			{
				if (!((LicensingProviderException)this).ProviderErrorCode.HasValue)
				{
					Exception innerException = ((Exception)this).InnerException;
					LicensingException val = (LicensingException)(object)((innerException is LicensingException) ? innerException : null);
					if (val != null)
					{
						return val.getStatusCode();
					}
				}
				return ((LicensingProviderException)this).ProviderErrorCode;
			}
		}

		public override string ProviderErrorMessage
		{
			get
			{
				if (string.IsNullOrWhiteSpace(((LicensingProviderException)this).ProviderErrorMessage))
				{
					Exception innerException = ((Exception)this).InnerException;
					LicensingException val = (LicensingException)(object)((innerException is LicensingException) ? innerException : null);
					if (val != null)
					{
						return val.getMessage();
					}
				}
				return ((LicensingProviderException)this).ProviderErrorMessage;
			}
		}

		public override long? ErrorCode
		{
			get
			{
				if (!((LicensingProviderException)this).ProviderErrorCode.HasValue)
				{
					return null;
				}
				switch (((LicensingProviderException)this).ProviderErrorCode)
				{
				case 214102L:
				case 214108L:
					return 2L;
				case -939519987L:
				case 214109L:
					return 16L;
				case 210003L:
				case 210005L:
					return 1L;
				case 210081L:
					return 8L;
				case 210026L:
					return 32L;
				case 210076L:
				case 210077L:
				case 210235L:
					return 4L;
				case 210092L:
					return 128L;
				case 210187L:
					return 64L;
				case 831L:
					return 256L;
				case 220L:
					return 512L;
				case 4040L:
					return 16L;
				default:
					return 2147483647L;
				}
			}
		}

		public SentinelProviderException(string message)
			: base(message)
		{
		}

		public SentinelProviderException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public SentinelProviderException(long providerErrorCode, string providerMessage)
			: base(string.Empty)
		{
			((LicensingProviderException)this).ProviderErrorCode = providerErrorCode;
			((LicensingProviderException)this).ProviderErrorMessage = providerMessage;
		}
	}
}
