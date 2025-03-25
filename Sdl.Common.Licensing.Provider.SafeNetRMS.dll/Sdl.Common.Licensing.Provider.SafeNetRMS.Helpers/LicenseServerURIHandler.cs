using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using Sdl.Common.Licensing.Provider.Core.Helpers;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	[ExcludeFromCodeCoverage]
	internal class LicenseServerURIHandler : ILicenseServerURIHandler
	{
		private const string ProductionEmsUrlLocator = "https://oos.sdl.com/emsurl.txt";

		public Uri UpdateActivationServerData(string providerName)
		{
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DevEMS"]))
			{
				return new Uri(ConfigurationManager.AppSettings["DevEMS"]);
			}
			if (!(WebRequest.Create("https://oos.sdl.com/emsurl.txt") is HttpWebRequest httpWebRequest))
			{
				SentinelProviderException ex = new SentinelProviderException(StringResources.SafeNet_FailedToConnectToOOS);
				TelemetryEventFactory.CreateTelemetryEventBuilder("Activation Deactivation").SetException((Exception)(object)ex).AddProperty("Provider Name", providerName ?? string.Empty)
					.Flush();
				throw ex;
			}
			int readWriteTimeout = (httpWebRequest.Timeout = 30000);
			httpWebRequest.ReadWriteTimeout = readWriteTimeout;
			using HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
			if (httpWebResponse == null || httpWebResponse.StatusCode != HttpStatusCode.OK)
			{
				SentinelProviderException ex2 = new SentinelProviderException(StringResources.SafeNet_InvalidResponseFromOOS);
				TelemetryEventFactory.CreateTelemetryEventBuilder("Activation Deactivation").SetException((Exception)(object)ex2).AddProperty("Provider Name", providerName ?? string.Empty)
					.AddProperty("Status Code", (httpWebResponse != null) ? httpWebResponse.StatusDescription : "")
					.Flush();
				throw ex2;
			}
			using (httpWebResponse)
			{
				Stream responseStream = httpWebResponse.GetResponseStream();
				if (responseStream == null)
				{
					SentinelProviderException ex3 = new SentinelProviderException(StringResources.SafeNet_InvalidResponseFromOOS);
					TelemetryEventFactory.CreateTelemetryEventBuilder("Activation Deactivation").SetException((Exception)(object)ex3).AddProperty("Provider Name", providerName ?? string.Empty)
						.Flush();
					throw ex3;
				}
				StreamReader streamReader = new StreamReader(responseStream);
				string text = streamReader.ReadToEnd();
				if (!text.EndsWith("/"))
				{
					text += "/";
				}
				return new Uri(text);
			}
		}
	}
}
