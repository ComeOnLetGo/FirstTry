using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using com.sntl.licensing;
using Microsoft.Extensions.Logging;
using Sdl.Common.Licensing.Provider.Core;
using Sdl.Common.Licensing.Provider.Core.Exceptions;
using Sdl.Common.Licensing.Provider.Core.Helpers;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal class ProductLicense : IProductLicense
	{
		private readonly Dictionary<string, dynamic> _licProperties = new Dictionary<string, object>();

		private readonly IReadOnlyCollection<LicenseFeature> _features;

		private readonly ILogger<ProductLicense> _logger;

		private readonly IRMSApi _safeNetRmsProvider;

		private int? _licenseType;

		private bool _useServerLicense;

		[CompilerGenerated]
		private LicenseMode _003CMode_003Ek__BackingField;

		[CompilerGenerated]
		private LicenseModeDetails _003CModeDetail_003Ek__BackingField;

		[CompilerGenerated]
		private LicenseStatus _003CStatus_003Ek__BackingField;

		public string Id => Provider.Configuration.ProductName;

		public int LicenseType
		{
			get
			{
				if (!_licenseType.HasValue)
				{
					ILicenseTypeMapper licenseTypeMapper = LicensingProviderConfiguration.LicenseTypeMapper;
					_licenseType = ((licenseTypeMapper != null) ? licenseTypeMapper.GetLicenseType((IProductLicense)(object)this) : 0);
				}
				return _licenseType.Value;
			}
			set
			{
				_licenseType = value;
			}
		}

		public ILicensingProvider Provider { get; private set; }

		public LicenseMode Mode
		{
			[CompilerGenerated]
			get
			{
				return (LicenseMode)2;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CMode_003Ek__BackingField = value;
			}
		}

		public LicenseModeDetails ModeDetail
		{
			[CompilerGenerated]
			get
			{
				return (LicenseModeDetails)2;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CModeDetail_003Ek__BackingField = value;
			}
		}

		public LicenseStatus Status
		{
			[CompilerGenerated]
			get
			{
				return (LicenseStatus)0;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CStatus_003Ek__BackingField = value;
			}
		}

		public DateTime? ExpirationDate { get; private set; }

		private SafeNetRMSProviderConfiguration LicensingProviderConfiguration => Provider.Configuration as SafeNetRMSProviderConfiguration;

		public bool IsLoggedIn => true;

		public bool IsLocal => true;

		public bool IsBorrowed => false;

		public ProductLicense(ILicensingProvider licProvider, IRMSApi rmsAPI, ILoggerFactory loggerFactory, IReadOnlyCollection<LicenseFeature> features)
		{
			Provider = licProvider;
			_safeNetRmsProvider = rmsAPI;
			_logger = LoggerFactoryExtensions.CreateLogger<ProductLicense>(loggerFactory);
			_features = features;
			InitializeProductLicense();
		}

		public bool HasFeature(string id)
		{
			return true;
		}

		public bool IsFeatureCheckedOut(string id)
		{
			return _features.Any((LicenseFeature f) => f.Id == id && f.IsLoggedIn());
		}

		public ILicenseFeature GetFeature(string id)
		{
			return (ILicenseFeature)(object)_features.SingleOrDefault((LicenseFeature f) => f.Id == id || f.Name == id);
		}

		public IList<ILicenseFeature> GetFeatures()
		{
			return ((IEnumerable<ILicenseFeature>)_features).ToList();
		}

		public void Authorize()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)Status == 1 && ValidateLicenseAgainstExpiration() && _features.Count > 0)
			{
				Status = (LicenseStatus)0;
			}
		}

		public void CheckOut()
		{
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			SetProperty("CheckOutFailReason", null);
			Status = (LicenseStatus)1;
			try
			{
				int num = 0;
				foreach (LicenseFeature feature in _features)
				{
					if (feature.IsLoggedIn())
					{
						continue;
					}
					try
					{
						feature.LoginSession = _safeNetRmsProvider.CheckOutFeature(feature);
						LoggerExtensions.LogDebug((ILogger)(object)_logger, "CheckOut(" + feature.Name + " " + feature.Version, Array.Empty<object>());
					}
					catch (Exception ex)
					{
						if (IsEdition(feature))
						{
							if (ex is SentinelProviderException)
							{
								throw;
							}
							throw new SentinelProviderException(string.Format(StringResources.SafeNet_FailedLogin, feature.Name), ex);
						}
					}
					SetLicenseType(feature.Name);
					num++;
				}
				if (_features.Count >= num && _features.Count > 0 && (!LicensingProviderConfiguration.UseLicenseServer || num != 0))
				{
					Status = (LicenseStatus)0;
				}
			}
			catch (Exception ex2)
			{
				LoggerExtensions.LogError((ILogger)(object)_logger, ex2, "Error at License CheckOut", Array.Empty<object>());
				try
				{
					CheckIn();
				}
				catch (Exception ex3)
				{
					LoggerExtensions.LogError((ILogger)(object)_logger, ex3, "Error while reverting CheckOut", Array.Empty<object>());
					TelemetryEventFactory.CreateTelemetryEventBuilder("Activation").SetException(ex3).Flush();
				}
				TelemetryEventBuilder val = TelemetryEventFactory.CreateTelemetryEventBuilder("Activation").SetException(ex2);
				if (ex2 is SentinelProviderException ex4)
				{
					LicenseFeature controlFeature = GetControlFeature();
					if (controlFeature != null && (int)controlFeature.Mode == 0)
					{
						ModeDetail = (LicenseModeDetails)9;
						LicenseMode mode = controlFeature.Mode;
						TelemetryEventBuilder obj = val.AddProperty("License Mode", ((object)(LicenseMode)(ref mode)).ToString());
						LicenseModeDetails modeDetail = ModeDetail;
						obj.AddProperty("License Mode Details", ((object)(LicenseModeDetails)(ref modeDetail)).ToString());
					}
					if (((LicensingProviderException)ex4).ProviderErrorCode.HasValue)
					{
						val.AddMetric("Provider Error Code", (double)((LicensingProviderException)ex4).ProviderErrorCode.Value);
					}
					SetProperty("CheckOutFailReason", ((LicensingProviderException)ex4).ProviderErrorCode);
				}
				val.Flush();
				throw;
			}
		}

		private void SetLicenseType(string checkedOutFeature)
		{
			if (!(LicensingProviderConfiguration.LicenseTypeMapper is SingleFeatureLicenseTypeMapper<string> val))
			{
				return;
			}
			foreach (int key in ((Dictionary<int, string>)(object)val.LicenseTypeToFeatureMap).Keys)
			{
				if (((Dictionary<int, string>)(object)val.LicenseTypeToFeatureMap)[key].Contains(checkedOutFeature))
				{
					LicenseType = key;
				}
			}
		}

		public void CheckIn()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_007c: Expected O, but got Unknown
			if ((int)ModeDetail == 18)
			{
				return;
			}
			foreach (LicenseFeature feature in _features)
			{
				if (feature.IsLoggedIn())
				{
					try
					{
						feature.Logout();
						LoggerExtensions.LogDebug((ILogger)(object)_logger, "CheckIn(" + feature.Name + " " + feature.Version + ")", Array.Empty<object>());
					}
					catch (LicensingException val)
					{
						LicensingException val2 = val;
						SentinelProviderException ex = new SentinelProviderException(string.Format(StringResources.SafeNet_FailedtoCheckInFeature, feature.Name), (Exception)(object)val2);
						TelemetryEventBuilder val3 = TelemetryEventFactory.CreateTelemetryEventBuilder("Activation Deactivation").SetException((Exception)(object)val2).AddProperty("Feature Name", feature.Name)
							.AddProperty("Feature Version", feature.Version);
						val3.Flush();
						throw ex;
					}
				}
			}
			if (ValidateLicenseAgainstExpiration())
			{
				Status = (LicenseStatus)1;
			}
		}

		public void CommuterCheckOut(int duration)
		{
			if (!ValidateLicenseAgainstExpiration())
			{
				return;
			}
			try
			{
				CheckIn();
				int num = 0;
				foreach (LicenseFeature feature in _features)
				{
					_safeNetRmsProvider.CheckoutCommuterLicense(feature.Name, feature.Version, duration, feature.LockingCriteria);
					LoggerExtensions.LogDebug((ILogger)(object)_logger, "CommuterCheckOut(" + feature.Name + " " + feature.Version + ")", Array.Empty<object>());
					num++;
				}
				if (_features.Count >= num && _features.Count > 0)
				{
					if (!LicensingProviderConfiguration.UseLicenseServer || num != 0)
					{
						Status = (LicenseStatus)0;
						Mode = (LicenseMode)1;
					}
				}
				else
				{
					Status = (LicenseStatus)1;
					Mode = (LicenseMode)2;
				}
			}
			catch
			{
				try
				{
					CheckIn();
				}
				catch (Exception ex)
				{
					LoggerExtensions.LogDebug((ILogger)(object)_logger, ex, "Error while rolling back partial CommuterCheckOut", Array.Empty<object>());
				}
				throw;
			}
		}

		public void CommuterCheckIn()
		{
			CheckIn();
			foreach (LicenseFeature feature in _features)
			{
				_safeNetRmsProvider.ReturnCommuterLicense(feature.Name, feature.Version, feature.LockingCriteria);
				LoggerExtensions.LogDebug((ILogger)(object)_logger, "CommuterCheckIn(" + feature.Name + " " + feature.Version + ")", Array.Empty<object>());
			}
			Mode = (LicenseMode)2;
		}

		public dynamic GetProperty(string name)
		{
			if (!_licProperties.ContainsKey(name))
			{
				return null;
			}
			return _licProperties[name];
		}

		public bool IsServerRunning()
		{
			if (IsLocal)
			{
				return false;
			}
			LicenseServerStatus licenseServerStatus = _safeNetRmsProvider.IsServerRunning();
			switch (licenseServerStatus)
			{
			case LicenseServerStatus.LicenseServerNotRunning:
				ModeDetail = (LicenseModeDetails)18;
				Mode = (LicenseMode)2;
				throw new SentinelProviderException(210005L, string.Empty);
			case LicenseServerStatus.LicenseServerHasNoFeatures:
				ModeDetail = (LicenseModeDetails)19;
				Mode = (LicenseMode)2;
				throw new SentinelProviderException(210010L, string.Empty);
			default:
				return licenseServerStatus == LicenseServerStatus.LicenseServerRunning;
			}
		}

		public override int GetHashCode()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			foreach (LicenseFeature feature in _features)
			{
				if (feature.IsLoggedIn())
				{
					num ^= feature.GetHashCode();
				}
			}
			if (num == 0)
			{
				return 0;
			}
			int num2 = num;
			LicenseStatus status = Status;
			num = num2 ^ ((object)(LicenseStatus)(ref status)).GetHashCode();
			int num3 = num;
			LicenseMode mode = Mode;
			return num3 ^ ((object)(LicenseMode)(ref mode)).GetHashCode();
		}

		private void InitializeProductLicense()
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			LoggerExtensions.LogDebug((ILogger)(object)_logger, "Initializing SafeNet product license", Array.Empty<object>());
			_useServerLicense = LicensingProviderConfiguration.UseLicenseServer;
			LoggerExtensions.LogDebug((ILogger)(object)_logger, _useServerLicense ? $"Using network license from {LicensingProviderConfiguration.LicenseServer}" : "Using standalone license", Array.Empty<object>());
			Status = (LicenseStatus)1;
			ExpirationDate = null;
			if (_features.Count == 0)
			{
				ModeDetail = (LicenseModeDetails)1;
				Mode = (LicenseMode)2;
				return;
			}
			LicenseFeature controlFeature = GetControlFeature();
			ModeDetail = controlFeature.ModeDetail;
			ExpirationDate = controlFeature.ExpirationDate;
			Mode = controlFeature.Mode;
			SetProperty("CommuterMaxCheckOutDays", controlFeature.CommuterMaxCheckOutDays);
		}

		private bool IsEdition(LicenseFeature feature)
		{
			return !((Dictionary<ProductFeature, string>)(object)LicensingProviderConfiguration.ProductFeatureMapper).Values.Contains(feature.Name);
		}

		private LicenseFeature GetControlFeature()
		{
			LicenseFeature licenseFeature = null;
			if (LicensingProviderConfiguration.LicenseTypeMapper != null)
			{
				licenseFeature = LicensingProviderConfiguration.LicenseTypeMapper.GetLicenseTypeFeature((IProductLicense)(object)this) as LicenseFeature;
			}
			return licenseFeature ?? _features.FirstOrDefault();
		}

		internal void SetProperty(string name, dynamic value)
		{
			_licProperties[name] = (object)value;
		}

		private bool ValidateLicenseAgainstExpiration()
		{
			return false;
		}
	}
}
