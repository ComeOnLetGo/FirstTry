using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using com.sntl.licensing;
using Microsoft.Extensions.Logging;
using Sdl.Common.Licensing.Provider.Core.Helpers;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	[ExcludeFromCodeCoverage]
	internal class UnifiedAPIProvider : IRMSApi
	{
		private const string LicenseVersion850 = "139460608";

		private const string LicenseVersion920 = "153092096";

		private const string LicenseVersion950 = "156237824";

		private const string LicenseVersion1000 = "268435456";

		private Attribute _localAttrAppContext;

		private ApplicationContext _localAppContext;

		private ApplicationContext _serverAppContext;

		private Attribute _serverAppContextAttribute;

		private readonly ILogger<UnifiedAPIProvider> _logger;

		private readonly string _licenseFilePath;

		private readonly string _licenseServerManager;

		private readonly ILoggerFactory _loggerFactory;

		private readonly IAppConfigWrapper _appConfig;

		private readonly SafeNetRMSProviderConfiguration _config;

		private static bool Initialized;

		private string _licenseServer;

		private ApplicationContext LocalAppContext
		{
			get
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Expected O, but got Unknown
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Expected O, but got Unknown
				EnsureApiInitialized();
				if (_localAppContext == null)
				{
					try
					{
						_localAttrAppContext = new Attribute();
						_localAttrAppContext.set("appcontext_contact_server", string.IsNullOrEmpty(_licenseServerManager) ? "no-net" : _licenseServerManager);
						_localAttrAppContext.set("appcontext_control_remote_session", "allow-terminal-allow-rdp");
						_localAppContext = new ApplicationContext(_localAttrAppContext);
					}
					catch (Exception ex)
					{
						TelemetryEventFactory.CreateTelemetryEventBuilder("Activation Deactivation").SetException(ex).Flush();
						throw new SentinelProviderException(StringResources.SafeNet_FailedRMSCreation, ex);
					}
				}
				return _localAppContext;
			}
		}

		private ApplicationContext ServerAppContext
		{
			get
			{
				if (!string.IsNullOrEmpty(_licenseServer) && _licenseServer != _config.LicenseServer)
				{
					ApplicationContext serverAppContext = _serverAppContext;
					if (serverAppContext != null)
					{
						serverAppContext.Dispose();
					}
					_serverAppContext = null;
					Attribute serverAppContextAttribute = _serverAppContextAttribute;
					if (serverAppContextAttribute != null)
					{
						serverAppContextAttribute.Dispose();
					}
					_serverAppContextAttribute = null;
				}
				if (_serverAppContext == null)
				{
					try
					{
						CreateServerContext(_config.LicenseServer).Deconstruct(out var item, out var item2);
						_serverAppContext = item;
						_serverAppContextAttribute = item2;
						_licenseServer = _config.LicenseServer;
					}
					catch (Exception ex)
					{
						TelemetryEventFactory.CreateTelemetryEventBuilder("Activation Deactivation").SetException(ex).Flush();
						throw new SentinelProviderException(StringResources.SafeNet_FailedRMSCreation, ex);
					}
				}
				return _serverAppContext;
			}
		}

		public UnifiedAPIProvider(string licenseFilePath, string licenseServerManager, ILoggerFactory loggerFactory, IAppConfigWrapper appConfig, SafeNetRMSProviderConfiguration config)
		{
			_licenseFilePath = licenseFilePath;
			_licenseServerManager = licenseServerManager;
			_loggerFactory = loggerFactory;
			_appConfig = appConfig;
			_config = config;
			_logger = LoggerFactoryExtensions.CreateLogger<UnifiedAPIProvider>(loggerFactory);
		}

		private Tuple<ApplicationContext, Attribute> CreateServerContext(string serverName)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			EnsureApiInitialized();
			Attribute val = new Attribute();
			val.set("appcontext_contact_server", serverName);
			val.set("appcontext_enable_local_renewal", "no");
			return new Tuple<ApplicationContext, Attribute>(new ApplicationContext(val), val);
		}

		private void ClearZombieLicensesFromServer()
		{
			try
			{
				List<string> featureListFromDefinition = GetFeatureListFromDefinition();
				foreach (string item in featureListFromDefinition)
				{
					ClearZombieLicensesForFeature(item, _config.ProductVersion);
				}
			}
			catch (Exception ex)
			{
				LoggerExtensions.LogError((ILogger)(object)_logger, ex, "Clear zombie licenses failed!", Array.Empty<object>());
			}
		}

		private List<string> GetFeatureListFromDefinition()
		{
			List<string> definedFeatures = new List<string>();
			LicenseDefinition licenseDefinition = CommuterLicenseUtil.GetLicenseDefinition(_config);
			LicenseEdition[] licenseEdition = licenseDefinition.LicenseEdition;
			foreach (LicenseEdition licenseEdition2 in licenseEdition)
			{
				definedFeatures.Add(licenseEdition2.Name);
				List<string> collection = (from f in licenseEdition2.Features
					select f.Name into name
					where !definedFeatures.Contains(name)
					select name).ToList();
				definedFeatures.AddRange(collection);
			}
			return definedFeatures;
		}

		private void ClearZombieLicensesForFeature(string feature, string version)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			//IL_005f: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			try
			{
				Attribute val = new Attribute();
				try
				{
					val.set("login_feature_version", version);
					val.set("login_zombie_session", "yes");
					val.set("login_zombie_session_identifier_mask", 7.ToString());
					LoginSession val2 = new LoginSession();
					val2.login(_serverAppContext, feature, val);
					val2.logout();
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (LicensingException val3)
			{
				LicensingException val4 = val3;
				if (val4.getStatusCode() != 210007 && val4.getStatusCode() != 210018 && val4.getStatusCode() != 210091)
				{
					throw;
				}
			}
		}

		public void InstallLicense(string licenseString)
		{
			//IL_0074: Expected O, but got Unknown
			if (string.IsNullOrEmpty(licenseString))
			{
				throw new SentinelProviderException(StringResources.SafeNet_FailedLicenseInstallation, new ArgumentNullException("licenseString"));
			}
			string[] array = licenseString.Split('*');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text))
				{
					string text2 = text.TrimEnd('\n', '\r');
					try
					{
						LocalAppContext.install("*" + text2);
					}
					catch (LicensingException val)
					{
						LicensingException innerException = val;
						SentinelProviderException ex = new SentinelProviderException(StringResources.SafeNet_FailedRMSLicenseStringInstallation, (Exception)(object)innerException);
						TelemetryEventFactory.CreateTelemetryEventBuilder("Activation").SetException((Exception)(object)ex).Flush();
						throw ex;
					}
				}
			}
		}

		public string InstallRevocationRequest(string permissionTicket)
		{
			//IL_0011: Expected O, but got Unknown
			try
			{
				return LocalAppContext.install(permissionTicket, (Attribute)null);
			}
			catch (LicensingException val)
			{
				LicensingException innerException = val;
				SentinelProviderException ex = new SentinelProviderException(StringResources.SafeNet_FailedRMSLicenseStringInstallation, (Exception)(object)innerException);
				TelemetryEventFactory.CreateTelemetryEventBuilder("Activation").SetException((Exception)(object)ex).Flush();
				throw ex;
			}
		}

		public string GetLockingCode(RmsLockSelectors entLockSelector = RmsLockSelectors.VLS_LOCK_DISK_ID | RmsLockSelectors.VLS_LOCK_HOSTNAME)
		{
			//IL_0054: Expected O, but got Unknown
			string text = "<sentinelScope><lockSelector>0x" + entLockSelector.ToString("X") + "</lockSelector></sentinelScope>";
			string innerText;
			try
			{
				string info = LocalAppContext.getInfo(text, LicensingConstants.SNTL_QUERY_LOCK_CODE_INFO_LATEST);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(info);
				innerText = xmlDocument.SelectSingleNode("/sentinelInfo/lockCode/value").InnerText;
			}
			catch (LicensingException val)
			{
				LicensingException val2 = val;
				TelemetryEventFactory.CreateTelemetryEventBuilder("Online Activation").SetException((Exception)(object)val2).Flush();
				throw new SentinelProviderException(StringResources.SafeNet_CannotRetrieveLockingCriteria + entLockSelector, (Exception)(object)val2);
			}
			return innerText.Insert(4, " ").Insert(9, " ").Insert(14, " ");
		}

		public void CleanupLicensingResources()
		{
			try
			{
				Attribute localAttrAppContext = _localAttrAppContext;
				if (localAttrAppContext != null)
				{
					localAttrAppContext.Dispose();
				}
				ApplicationContext localAppContext = _localAppContext;
				if (localAppContext != null)
				{
					localAppContext.Dispose();
				}
				_localAttrAppContext = null;
				_localAppContext = null;
				Attribute serverAppContextAttribute = _serverAppContextAttribute;
				if (serverAppContextAttribute != null)
				{
					serverAppContextAttribute.Dispose();
				}
				ApplicationContext serverAppContext = _serverAppContext;
				if (serverAppContext != null)
				{
					serverAppContext.Dispose();
				}
				_serverAppContextAttribute = null;
				_serverAppContext = null;
				LicensingAPI.cleanup();
			}
			catch (Exception ex)
			{
				TelemetryEventFactory.CreateTelemetryEventBuilder("Online Deactivation").SetException(ex).Flush();
				throw new SentinelProviderException(StringResources.SafeNet_FailedLicensingResourcesCleanup, ex);
			}
		}

		public IReadOnlyCollection<FeatureInfo> GetFeaturesFromServer(string serverName)
		{
			CreateServerContext(serverName).Deconstruct(out var item, out var item2);
			ApplicationContext val = item;
			Attribute val2 = item2;
			Attribute val3 = val2;
			try
			{
				ApplicationContext val4 = val;
				try
				{
					return GetFeatures(val);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}

		public IReadOnlyCollection<FeatureInfo> GetFeatures()
		{
			return GetFeatures(GetContext());
		}

		private IReadOnlyCollection<FeatureInfo> GetFeatures(ApplicationContext appContext)
		{
			//IL_022d: Expected O, but got Unknown
			if (_serverAppContext != null && appContext == _serverAppContext)
			{
				ClearZombieLicensesFromServer();
			}
			string format = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><sentinelScope><feature index=\"{0}\"><name></name><version></version></feature></sentinelScope>";
			List<FeatureInfo> list = new List<FeatureInfo>();
			int num = 0;
			while (true)
			{
				try
				{
					string text = string.Format(format, num);
					num++;
					string info = appContext.getInfo(text, LicensingConstants.SNTL_QUERY_FEATURE_INFO_LATEST);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(info);
					if (IsValidRmsVersion(xmlDocument.SelectSingleNode("/sentinelInfo/feature/licenseVersion").InnerText))
					{
						FeatureInfo featureInfo = new FeatureInfo(info);
						featureInfo.CommuterMaxCheckOutDays = int.Parse(xmlDocument.SelectSingleNode("/sentinelInfo/feature/commuterMaxCheckoutDays").InnerText);
						featureInfo.DeathDay = int.Parse(xmlDocument.SelectSingleNode("/sentinelInfo/feature/deathTime").InnerText);
						featureInfo.FeatureName = xmlDocument.SelectSingleNode("/sentinelInfo/feature/featureName").InnerText;
						featureInfo.IsAllowedOnVm = xmlDocument.SelectSingleNode("/sentinelInfo/feature/vmDetection").InnerText == "0";
						featureInfo.IsCommuted = xmlDocument.SelectSingleNode("/sentinelInfo/feature/isCommuter").InnerText == "2";
						featureInfo.IsNodeLocked = xmlDocument.SelectSingleNode("/sentinelInfo/feature/isNodeLocked").InnerText != "2";
						featureInfo.LicenseType = int.Parse(xmlDocument.SelectSingleNode("/sentinelInfo/feature/licType").InnerText);
						if (!int.TryParse(xmlDocument.SelectSingleNode("/sentinelInfo/feature/numLicenses").InnerText, out var result))
						{
							result = -1;
						}
						featureInfo.NumLicenses = result;
						featureInfo.TrialCalendarPeriodLeft = int.Parse(xmlDocument.SelectSingleNode("/sentinelInfo/feature/trialDaysLeft").InnerText);
						featureInfo.VendorInfo = xmlDocument.SelectSingleNode("/sentinelInfo/feature/vendorInfo").InnerText;
						featureInfo.Version = xmlDocument.SelectSingleNode("/sentinelInfo/feature/featureVersion").InnerText;
						featureInfo.LockingCrit = int.Parse(xmlDocument.SelectSingleNode("/sentinelInfo/feature/lockingCrit").InnerText);
						featureInfo.KeyLifetimeSec = int.Parse(xmlDocument.SelectSingleNode("/sentinelInfo/feature/keyLifeTime").InnerText);
						featureInfo.IsNetwork = xmlDocument.SelectSingleNode("/sentinelInfo/feature/isStandalone").InnerText == "0";
						list.Add(featureInfo);
					}
				}
				catch (LicensingException val)
				{
					LicensingException val2 = val;
					if (val2.getStatusCode() != 210010)
					{
						LoggerExtensions.LogError((ILogger)(object)_logger, (Exception)(object)val2, "GetFeatures failed", Array.Empty<object>());
						throw new SentinelProviderException(StringResources.Error_GetFeaturesFailed, (Exception)(object)val2);
					}
					return list;
				}
			}
		}

		public LicenseServerStatus IsServerRunning()
		{
			//IL_0032: Expected O, but got Unknown
			try
			{
				string info = ServerAppContext.getInfo("<sentinelScope/>", LicensingConstants.SNTL_QUERY_SERVER_INFO_VERSION("1.0"));
				if (info != null && Regex.IsMatch(info, "<hostName>([\\S].+?)<\\/hostName>"))
				{
					return LicenseServerStatus.LicenseServerRunning;
				}
			}
			catch (LicensingException val)
			{
				LicensingException val2 = val;
				TelemetryEventBuilder val3 = TelemetryEventFactory.CreateTelemetryEventBuilder("Activation Deactivation").SetException((Exception)(object)val2);
				int statusCode = val2.getStatusCode();
				if (statusCode == 210005)
				{
					return LicenseServerStatus.LicenseServerNotRunning;
				}
				val3.AddProperty("Error Code", statusCode.ToString());
				val3.Flush();
				throw new SentinelProviderException(StringResources.Error_FailedToContactLicenseServer, (Exception)(object)val2);
			}
			return LicenseServerStatus.LicenseServerRunning;
		}

		public ILoginSession CheckOutFeature(LicenseFeature feature)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_006e: Expected O, but got Unknown
			try
			{
				LoginSession val = new LoginSession();
				ApplicationContext context = GetContext();
				Attribute val2 = new Attribute();
				try
				{
					val2.set("login_feature_version", feature.Version);
					val.login(context, feature.Name, val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				int refreshInterval = (feature.IsNetwork ? GetRefreshInterval(feature.KeyLifetimeSec) : (-1));
				return new LoginSessionWrapper(val, refreshInterval, LoggerFactoryExtensions.CreateLogger<LoginSessionWrapper>(_loggerFactory));
			}
			catch (LicensingException val3)
			{
				LicensingException val4 = val3;
				if (val4.getStatusCode() != 214102)
				{
					TelemetryEventFactory.CreateTelemetryEventBuilder("Activation").SetException((Exception)(object)val4).AddProperty("Feature Name", feature.Name)
						.AddProperty("Feature Version", feature.Version)
						.Flush();
				}
				throw new SentinelProviderException(string.Format(StringResources.SafeNet_FailedToCheckOutFeature, feature.Name), (Exception)(object)val4);
			}
		}

		public void ReturnCommuterLicense(string name, string version, int lockCriteria)
		{
			//IL_007e: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			string text = "<returnCommuter><featureName>" + name + "</featureName><featureVersion>" + version + "</featureVersion></returnCommuter>";
			try
			{
				Attribute val = new Attribute();
				try
				{
					val.set("transfer_units_required", "1");
					val.set("transfer_lock_mask", Convert.ToString(lockCriteria));
					string text2 = ServerAppContext.transfer(text, (string)null, val, (string)null);
					ServerAppContext.install(text2);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (LicensingException val2)
			{
				LicensingException innerException = val2;
				throw new SentinelProviderException(string.Format(StringResources.SafeNet_CheckOutCommuterFeatureFailed, name), (Exception)(object)innerException);
			}
		}

		public void CheckoutCommuterLicense(string name, string version, int duration, int lockCriteria)
		{
			//IL_0091: Expected O, but got Unknown
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			string text = "<commute><featureName>" + name + "</featureName><featureVersion>" + version + "</featureVersion><duration>" + duration + "</duration></commute>";
			try
			{
				Attribute val = new Attribute();
				try
				{
					val.set("transfer_units_required", "1");
					val.set("transfer_lock_mask", Convert.ToString(lockCriteria));
					string text2 = ServerAppContext.transfer(text, (string)null, val, (string)null);
					LocalAppContext.install(text2);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (LicensingException val2)
			{
				LicensingException innerException = val2;
				throw new SentinelProviderException(string.Format(StringResources.SafeNet_CheckOutCommuterFeatureFailed, name), (Exception)(object)innerException);
			}
		}

		private void EnsureApiInitialized()
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			if (!Initialized)
			{
				string directoryName = Path.GetDirectoryName(_licenseFilePath);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				Attribute val = new Attribute();
				val.set("config_lservrc_file", _licenseFilePath);
				if (_appConfig.TraceEnabled)
				{
					val.set("config_trace_writer_file", _appConfig.TraceFilePath);
					val.set("config_trace_level", "trace_error");
				}
				LicensingAPI.configure(val);
				Initialized = true;
			}
		}

		private ApplicationContext GetContext()
		{
			string licenseServer = _config.LicenseServer;
			bool useLicenseServer = _config.UseLicenseServer;
			bool useBorrowedLicense = _config.UseBorrowedLicense;
			if (!(string.IsNullOrEmpty(licenseServer) || !useLicenseServer || useBorrowedLicense))
			{
				return ServerAppContext;
			}
			return LocalAppContext;
		}

		private bool IsValidRmsVersion(string licenseVersion)
		{
			switch (licenseVersion)
			{
			default:
				return licenseVersion == "268435456";
			case "139460608":
			case "153092096":
			case "156237824":
				return true;
			}
		}

		private int GetRefreshInterval(int keyLifetime)
		{
			int num = 120;
			int num2 = num;
			int num3 = keyLifetime - num;
			if (num3 <= 0)
			{
				num3 = num2;
			}
			return num3;
		}
	}
}
