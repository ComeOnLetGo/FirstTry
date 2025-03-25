using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using com.sntl.licensing;
using Microsoft.Extensions.Logging;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	[ExcludeFromCodeCoverage]
	internal class LoginSessionWrapper : ILoginSession
	{
		private static readonly object RmsMutex = new object();

		private LoginSession _loginSession;

		private Timer _refreshTimer;

		private readonly ILogger<LoginSessionWrapper> _logger;

		public LoginSessionWrapper(LoginSession loginSession, int refreshInterval, ILogger<LoginSessionWrapper> logger)
		{
			_loginSession = loginSession;
			_logger = logger;
			if (refreshInterval != -1)
			{
				_refreshTimer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(refreshInterval), TimeSpan.FromSeconds(refreshInterval));
			}
		}

		public void Logout()
		{
			lock (RmsMutex)
			{
				LoginSession loginSession = _loginSession;
				if (loginSession != null)
				{
					loginSession.logout();
				}
				_loginSession = null;
				_refreshTimer?.Change(-1, -1);
			}
			_refreshTimer?.Dispose();
			_refreshTimer = null;
		}

		private void TimerCallback(object state)
		{
			try
			{
				lock (RmsMutex)
				{
					LoginSession loginSession = _loginSession;
					if (loginSession != null)
					{
						loginSession.refresh();
					}
				}
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError((ILogger)(object)_logger, ex, "Error refreshing LoginSession", Array.Empty<object>());
			}
		}
	}
}
