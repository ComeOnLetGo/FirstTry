using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sdl.Common.Licensing.Provider.Core;
using Sdl.Common.Licensing.Provider.Core.Helpers;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Communication;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Model;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	internal class LicensingProvider : ILicensingProvider, IDisposable
	{
		private readonly List<string> LockingCodeAttributes = new List<string> { "CLIENT_1_CRITERIA", "PRIMARY_1_CRITERIA" };

		private readonly ILogger<LicensingProvider> _logger;

		private readonly ILoggerFactory _loggerFactory;

		private readonly IApiClientFactory _apiClientFactory;

		private readonly IFileWrapper _fileWrapper;

		protected SafeNetRMSProviderConfiguration Config { get; private set; }

		internal IRMSApi SafeNetRmsProvider { get; set; }

		public string Id => SafeNetRMSProviderConfiguration.DefaultLicensingProviderId;

		public string ProviderName => PluginResources.Plugin_Description;

		public ILicensingProviderConfiguration Configuration => (ILicensingProviderConfiguration)(object)Config;

		public LicensingProvider(SafeNetRMSProviderConfiguration spc, IApiClientFactory apiClientFactory, IRMSApi rmsApi, IFileWrapper fileWrapper, ILoggerFactory loggerFactory)
		{
			Config = spc;
			_loggerFactory = loggerFactory;
			_logger = LoggerFactoryExtensions.CreateLogger<LicensingProvider>(loggerFactory);
			_apiClientFactory = apiClientFactory;
			SafeNetRmsProvider = rmsApi;
			_fileWrapper = fileWrapper;
		}

		public IProductLicense GetProductLicense()
		{
			IReadOnlyCollection<LicenseFeature> features;
			try
			{
				features = GetAvailableFeatures();
			}
			catch (SentinelProviderException ex)
			{
				features = new List<LicenseFeature>();
				LoggerExtensions.LogError((ILogger)(object)_logger, "Failed to get features from server", new object[1] { ex });
			}
			return (IProductLicense)(object)new ProductLicense((ILicensingProvider)(object)this, SafeNetRmsProvider, _loggerFactory, features);
		}

		public IProductLicense TryGetServerLicense(string serverName)
		{
			IReadOnlyCollection<LicenseFeature> availableFeatures = GetAvailableFeatures(serverName);
			IEnumerable<string> second = availableFeatures.Select((LicenseFeature f) => f.Name);
			string checkedOutEdition = Configuration.CheckedOutEdition;
			List<string> featuresToCheckout = GetFeaturesToCheckout(checkedOutEdition);
			if (!featuresToCheckout.Any())
			{
				IEnumerable<string> first = Configuration.AvailableEditions.Select((LicenseEdition e) => e.Id);
				IEnumerable<string> source = first.Intersect(second);
				if (source.Count() == 1)
				{
					featuresToCheckout = GetFeaturesToCheckout(source.FirstOrDefault());
				}
			}
			List<LicenseFeature> list = availableFeatures.Where((LicenseFeature sf) => featuresToCheckout.Contains(sf.Name)).ToList();
			return (IProductLicense)(object)new ProductLicense((ILicensingProvider)(object)this, SafeNetRmsProvider, _loggerFactory, new ReadOnlyCollection<LicenseFeature>(list));
		}

		public IReadOnlyCollection<ILicenseFeature> GetFeaturesFromServer(string serverName)
		{
			IReadOnlyCollection<LicenseFeature> availableFeatures = GetAvailableFeatures(serverName);
			return ((IEnumerable<ILicenseFeature>)availableFeatures).ToList().AsReadOnly();
		}

		public async Task<IProductLicense> ActivateAsync(string code)
		{
			ActivationResult activationResult = await ActivateLicenseAsync(code).ConfigureAwait(continueOnCapturedContext: false);
			string licenseString = string.Join(Environment.NewLine + Environment.NewLine, activationResult.ActivationResponse.Activations.Activation[0].LicenseKeys.LicenseKey.Select((LicenseKey l) => l.Key));
			SafeNetRmsProvider.InstallLicense(licenseString);
			return GetProductLicense();
		}

		public List<string> GetFeaturesToCheckout(string editionName = null)
		{
			List<string> list = new List<string>();
			LicenseDefinition licenseDefinition;
			try
			{
				licenseDefinition = CommuterLicenseUtil.GetLicenseDefinition(Config);
			}
			catch
			{
				licenseDefinition = null;
			}
			if (licenseDefinition != null)
			{
				LicenseEdition licenseEdition = (string.IsNullOrEmpty(editionName) ? licenseDefinition.LicenseEdition.FirstOrDefault((LicenseEdition edition) => edition.IsCurrent) : licenseDefinition.LicenseEdition.FirstOrDefault((LicenseEdition edition) => edition.Name == editionName));
				if (licenseEdition != null)
				{
					list.Add(licenseEdition.Name);
					list.AddRange(from feature in licenseEdition.Features
						where feature.IsDefault
						select feature.Name);
				}
			}
			return list;
		}

		internal async Task<ActivationResult> ActivateLicenseAsync(string code, int numberOfSeats = 1)
		{
			using IThalesRestClient apiClient = _apiClientFactory.GetRestClient(ProviderName, code);
			ProductKey pkInfo = await apiClient.GetProductKeyInfoAsync().ConfigureAwait(continueOnCapturedContext: false);
			string value = ValidateLicense(pkInfo);
			if (!string.IsNullOrWhiteSpace(value))
			{
				SentinelProviderException ex = new SentinelProviderException(StringResources.SafeNet_KeyNotValidForProduct);
				TelemetryEventFactory.CreateTelemetryEventBuilder("Online Activation").SetException((Exception)(object)ex).AddProperty("Provider Name", ProviderName ?? string.Empty)
					.Flush();
				throw ex;
			}
			try
			{
				LicenseActivationRequest activationRequest = CreateLicenseActivationRequest(pkInfo, code, numberOfSeats);
				LicenseActivationResponse licenseActivationResponse = await apiClient.ActivateLicenseAsync(activationRequest).ConfigureAwait(continueOnCapturedContext: false);
				if (licenseActivationResponse?.Activations == null)
				{
					throw new NullReferenceException(StringResources.Error_InvalidActivationResponse);
				}
				pkInfo.AvailableQuantity -= numberOfSeats;
				return new ActivationResult
				{
					ActivationResponse = licenseActivationResponse,
					ProductKeyInfo = pkInfo
				};
			}
			catch (Exception innerException)
			{
				SentinelProviderException ex2 = new SentinelProviderException(StringResources.SafeNet_FailedActivation, innerException);
				TelemetryEventFactory.CreateTelemetryEventBuilder("Online Activation").SetException((Exception)(object)ex2).AddProperty("Provider Name", ProviderName ?? string.Empty)
					.Flush();
				throw ex2;
			}
		}

		private LicenseActivationRequest CreateLicenseActivationRequest(ProductKey pkInfo, string code, int numberOfSeats)
		{
			ActivationAttributes activationAttributes = pkInfo.ActivationAttributes;
			foreach (ActivationAttribute item in activationAttributes.ActivationAttribute)
			{
				if (LockingCodeAttributes.Contains(item.Name))
				{
					RmsLockSelectors entLockSelector = (RmsLockSelectors)int.Parse(item.Value, NumberStyles.HexNumber);
					string lockingCode = SafeNetRmsProvider.GetLockingCode(entLockSelector);
					item.AssociatedAttribute.Value = lockingCode;
				}
			}
			switch (pkInfo.ActivationMethod)
			{
			case "FIXED":
				numberOfSeats = pkInfo.FixedQuantity;
				break;
			case "FULL":
				numberOfSeats = pkInfo.TotalQuantity;
				break;
			}
			return new LicenseActivationRequest(code, numberOfSeats, activationAttributes);
		}

		public IProductLicense OfflineActivate(string certificate)
		{
			try
			{
				SafeNetRmsProvider.InstallLicense(certificate);
			}
			catch (SentinelProviderException exception)
			{
				TelemetryEventFactory.CreateTelemetryEventBuilder("Offline Activation").SetException((Exception)(object)exception).AddProperty("Provider Name", ProviderName ?? string.Empty)
					.Flush();
				throw;
			}
			catch (Exception ex)
			{
				TelemetryEventFactory.CreateTelemetryEventBuilder("Offline Activation").SetException(ex).AddProperty("Provider Name", ProviderName ?? string.Empty)
					.Flush();
				throw new SentinelProviderException(StringResources.SafeNet_FailedActivation, ex);
			}
			return GetProductLicense();
		}

		public async Task DeactivateAsync(string code)
		{
			string activationId = GetActivationId();
			await DeactivateLicenseAsync(code, activationId).ConfigureAwait(continueOnCapturedContext: false);
		}

		private string GetActivationId()
		{
			if (string.IsNullOrWhiteSpace(Config.TrialActivationId))
			{
				SentinelProviderException ex = new SentinelProviderException(StringResources.Error_TrialAidMissing);
				TelemetryEventFactory.CreateTelemetryEventBuilder("Online Deactivation").SetException((Exception)(object)ex).Flush();
				throw ex;
			}
			using (StreamReader streamReader = _fileWrapper.OpenText(Config.LicenseFilePath))
			{
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					string[] array = text.Split(new string[1] { "AID=" }, StringSplitOptions.RemoveEmptyEntries);
					if (array.Length > 1 && !string.Equals(array[1], Config.TrialActivationId))
					{
						return array[1];
					}
				}
			}
			SentinelProviderException ex2 = new SentinelProviderException(StringResources.SafeNet_FailedToGetActivationId);
			TelemetryEventFactory.CreateTelemetryEventBuilder("Online Deactivation").SetException((Exception)(object)ex2).Flush();
			throw ex2;
		}

		internal async Task DeactivateLicenseAsync(string code, string aId)
		{
			string revocationTicket = null;
			PermissionTicketResponse permissionTicketResponse = null;
			using IThalesRestClient apiClient = _apiClientFactory.GetRestClient(ProviderName, code);
			_ = 1;
			try
			{
				LoggerExtensions.LogDebug((ILogger)(object)_logger, "Creating revocation request.", Array.Empty<object>());
				permissionTicketResponse = await apiClient.GeneratePermissionTicketAsync(aId).ConfigureAwait(continueOnCapturedContext: false);
				if (permissionTicketResponse == null)
				{
					throw new SentinelProviderException(StringResources.Error_CreatePermissionTicketFailed);
				}
				revocationTicket = RevokeLicense(permissionTicketResponse.Revocation.PermissionTickets.PermissionTicket);
				LoggerExtensions.LogDebug((ILogger)(object)_logger, "Uploading revocation to EMS.", Array.Empty<object>());
				if (!(await apiClient.SubmitRevocationProofAsync(aId, new SubmitRevocationRequest(revocationTicket)).ConfigureAwait(continueOnCapturedContext: false)))
				{
					throw new SentinelProviderException(StringResources.Error_SubmitRevocationRequestFailed);
				}
			}
			catch (SentinelProviderException ex)
			{
				TelemetryEventFactory.CreateTelemetryEventBuilder("Online Deactivation").SetException((Exception)(object)ex).AddProperty("Provider Name", ProviderName ?? string.Empty)
					.AddProperty("Activation ID", aId)
					.AddProperty("Permission Ticket", permissionTicketResponse?.Revocation?.PermissionTickets?.PermissionTicket ?? string.Empty)
					.AddProperty("Revocation Ticket", revocationTicket ?? string.Empty)
					.Flush();
				LoggerExtensions.LogError((ILogger)(object)_logger, (Exception)(object)ex, "Revocation error", Array.Empty<object>());
				throw;
			}
			catch (Exception ex2)
			{
				TelemetryEventFactory.CreateTelemetryEventBuilder("Online Deactivation").SetException(ex2).AddProperty("Activation ID", aId)
					.AddProperty("Provider Name", ProviderName ?? string.Empty)
					.Flush();
				LoggerExtensions.LogError((ILogger)(object)_logger, ex2, "Revocation error", Array.Empty<object>());
				throw new SentinelProviderException(StringResources.SafeNet_RevokeFailed, ex2);
			}
		}

		private string RevokeLicense(string permissionTicket)
		{
			LoggerExtensions.LogInformation((ILogger)(object)_logger, "Revoking license online", Array.Empty<object>());
			string permissionTicket2 = ConvertEmsToRms(permissionTicket);
			string s = SafeNetRmsProvider.InstallRevocationRequest(permissionTicket2);
			byte[] array = Convert.FromBase64String(s);
			StringBuilder stringBuilder = new StringBuilder();
			int num = array.Count();
			for (int i = 0; i < num; i++)
			{
				stringBuilder.Append(((sbyte)array[i]).ToString());
				if (i != num - 1)
				{
					stringBuilder.Append(",");
				}
			}
			return stringBuilder.ToString();
		}

		private static string ConvertEmsToRms(string toConvert)
		{
			toConvert = toConvert.Replace("[", "");
			toConvert = toConvert.Replace("]", "");
			string[] array = toConvert.Split(',');
			byte[] array2 = new byte[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = (byte)short.Parse(array[i]);
			}
			return Convert.ToBase64String(array2);
		}

		public string GetInstallationId()
		{
			return SafeNetRmsProvider.GetLockingCode();
		}

		private string ValidateLicense(ProductKey license)
		{
			if (license == null)
			{
				return StringResources.SafeNet_KeyNotValidForProduct;
			}
			if (string.IsNullOrEmpty(Config.ProductVersion))
			{
				return null;
			}
			try
			{
				NameVersion nameVersion = license.Item.ItemProduct.Product.NameVersion;
				if (Helper.GetFirstWord(Config.Name) != Helper.GetFirstWord(nameVersion.Name))
				{
					return StringResources.SafeNet_KeyNotValidForProduct;
				}
				if (Config.ProductVersion != nameVersion.Version)
				{
					return StringResources.SafeNet_KeyNotValidForProduct;
				}
				if (!IsStandaloneLicense(license))
				{
					return StringResources.Error_NetworkLicenseNotSupportedInStandaloneProduct;
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		private bool IsStandaloneLicense(ProductKey license)
		{
			IEnumerable<Sdl.Common.Licensing.Provider.SafeNetRMS.Model.Attribute> source = license.Item.ItemProduct.ItemProductFeatures.ItemProductFeature.Select((ItemProductFeature f) => f.ItemFeatureLicenseModel.Attributes.Attribute.FirstOrDefault((Sdl.Common.Licensing.Provider.SafeNetRMS.Model.Attribute attr) => attr.Name.Equals("LICENSE_TYPE")));
			return source.All((Sdl.Common.Licensing.Provider.SafeNetRMS.Model.Attribute ft) => ft.Value.Equals("1"));
		}

		private IReadOnlyCollection<LicenseFeature> GetAvailableFeatures(string serverName = "")
		{
			IReadOnlyCollection<FeatureInfo> readOnlyCollection = (string.IsNullOrWhiteSpace(serverName) ? SafeNetRmsProvider.GetFeatures() : SafeNetRmsProvider.GetFeaturesFromServer(serverName));
			SingleFeatureLicenseTypeMapper<string> val = Config.LicenseTypeMapper as SingleFeatureLicenseTypeMapper<string>;
			List<LicenseFeature> list = new List<LicenseFeature>();
			List<LicenseFeature> list2 = new List<LicenseFeature>();
			bool flag = Config.UseLicenseServer && !Config.UseBorrowedLicense;
			foreach (FeatureInfo feature in readOnlyCollection)
			{
				KeyValuePair<ProductFeature, string> keyValuePair = ((IEnumerable<KeyValuePair<ProductFeature, string>>)Config.ProductFeatureMapper).FirstOrDefault((KeyValuePair<ProductFeature, string> entry) => entry.Value == feature.FeatureName);
				if (!IsValidFeatureVersion(feature, keyValuePair.Key, Config.ProductVersion))
				{
					continue;
				}
				LicenseFeature licenseFeature = new LicenseFeature(feature);
				if (licenseFeature.HasExpired && !licenseFeature.IsCommuted && !licenseFeature.IsInstalledTrial)
				{
					continue;
				}
				licenseFeature.Id = ((keyValuePair.Key != null) ? keyValuePair.Key.Id.ToString() : feature.FeatureName);
				bool flag2 = ((Dictionary<int, string>)(object)val?.LicenseTypeToFeatureMap).ContainsValue(licenseFeature.Name) ?? false;
				if ((!(flag2 && flag) || Config.CheckedOutEdition == null || Config.CheckedOutEdition.Equals(licenseFeature.Name)) && (flag2 || keyValuePair.Key != null))
				{
					if (licenseFeature.IsInstalledTrial)
					{
						list.Add(licenseFeature);
					}
					else
					{
						list2.Add(licenseFeature);
					}
				}
			}
			if (list.Any() && !list2.Any())
			{
				list2.AddRange(list);
			}
			return list2;
		}

		private bool IsValidFeatureVersion(FeatureInfo licensedFeature, ProductFeature productFeature, string defaultVersion)
		{
			string text = ((!string.IsNullOrEmpty((productFeature != null) ? productFeature.Version : null)) ? productFeature.Version : defaultVersion);
			if (!string.IsNullOrEmpty(licensedFeature.Version) || !string.IsNullOrEmpty(text))
			{
				return string.Equals(licensedFeature.Version, text, StringComparison.InvariantCultureIgnoreCase);
			}
			return true;
		}

		public string GetDiagnosticInfo(IProductLicense license)
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				stringBuilder.AppendLine($"Using license server: {Config.UseLicenseServer}");
				if (Config.UseLicenseServer)
				{
					stringBuilder.AppendLine($"Licensing server name: {Config.LicenseServer}");
				}
				stringBuilder.AppendLine($"License status: {license.Status}").AppendLine($"License mode: {license.Mode}").AppendLine($"License mode details: {license.ModeDetail}");
				if (!Config.UseLicenseServer)
				{
					stringBuilder.AppendLine($"AID: {GetActivationId()}");
				}
				IList<ILicenseFeature> features = license.GetFeatures();
				if (features == null || features.Count == 0)
				{
					stringBuilder.AppendLine("** No Features **");
				}
				if (license.ExpirationDate.HasValue)
				{
					stringBuilder.AppendLine($"Expiration date: {license.ExpirationDate}");
				}
				foreach (ILicenseFeature item in features)
				{
					if (item is LicenseFeature licenseFeature)
					{
						stringBuilder.AppendLine("------------------------------------");
						licenseFeature.ToString(stringBuilder);
					}
				}
			}
			catch (Exception arg)
			{
				stringBuilder.AppendLine($"An exception occurred while running the diagnosis - {arg}");
			}
			stringBuilder.AppendLine("------------------------------------");
			return stringBuilder.ToString();
		}

		public string OfflineDeactivate(string permissionTicket)
		{
			LoggerExtensions.LogInformation((ILogger)(object)_logger, "Revoking offline the license with ticket : " + permissionTicket, Array.Empty<object>());
			return SafeNetRmsProvider.InstallRevocationRequest(permissionTicket);
		}

		public void Dispose()
		{
			SafeNetRmsProvider.CleanupLicensingResources();
		}
	}
}
