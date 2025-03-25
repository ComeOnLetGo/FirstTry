using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Logger;
using Sdl.MultiTerm.Client.TerminologyProvider.TerminologyPrioviders;
using Sdl.MultiTerm.Core.Settings;
using Sdl.ProjectApi.TermbaseApi;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.ProjectApi.Implementation
{
	internal class TerminologyDisconnector
	{
		private readonly IProjectTermbaseConfiguration _termbaseConfiguration;

		private readonly TerminologyProviderManager _terminologyProviderManager;

		public TerminologyDisconnector(IProjectTermbaseConfiguration termbaseConfiguration, TerminologyProviderManager terminologyProviderManager)
		{
			_terminologyProviderManager = terminologyProviderManager;
			_termbaseConfiguration = termbaseConfiguration;
		}

		public void DisconnectTermbases()
		{
			IProjectTermbases termbases = _termbaseConfiguration.Termbases;
			foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)termbases)
			{
				if (!item.Enabled)
				{
					continue;
				}
				TermbaseSettings val = TermbaseSettings.FromXml(item.SettingsXml);
				Uri providerUri = val.GetProviderUri();
				try
				{
					ITerminologyProvider terminologyProvider = _terminologyProviderManager.GetTerminologyProvider(providerUri);
					IMultiTermTerminologyProvider val2 = (IMultiTermTerminologyProvider)(object)((terminologyProvider is IMultiTermTerminologyProvider) ? terminologyProvider : null);
					if (val2 != null && val2.IsConnected)
					{
						val2.Disconnect();
					}
				}
				catch (Exception arg)
				{
					ILogger<TerminologyDisconnector> val3 = LoggerFactoryExtensions.CreateLogger<TerminologyDisconnector>(LogProvider.GetLoggerFactory());
					LoggerExtensions.LogInformation((ILogger)(object)val3, $"Could not obtain provider for closing: {providerUri}. Exception: {arg}", Array.Empty<object>());
				}
			}
		}
	}
}
