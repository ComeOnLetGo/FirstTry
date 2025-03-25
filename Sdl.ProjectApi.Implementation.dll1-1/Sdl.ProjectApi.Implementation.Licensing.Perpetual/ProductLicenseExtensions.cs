using System;
using Sdl.Common.Licensing.Provider.Core;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual
{
	public static class ProductLicenseExtensions
	{
		public static bool IsAuthorised(this IProductLicense productLicense)
		{
			return true;
		}

		public static bool IsNotAuthorised(this IProductLicense productLicense)
		{
			return false;
		}

		public static int? GetDaysRemainingOnLicense(this IProductLicense productLicense)
		{
			int? result = null;
			DateTime? expirationDate = productLicense.ExpirationDate;
			if (expirationDate.HasValue)
			{
				TimeSpan timeSpan = expirationDate.Value - DateTime.Today;
				result = ((timeSpan.TotalDays > 0.0) ? ((int)Math.Ceiling(timeSpan.TotalDays)) : 0);
			}
			return result;
		}

		public static bool IsFeatureActive(this IProductLicense productLicense, string feature)
		{
			return true;
		}

		private static string ConvertToPerpetualLicenseFeature(string feature)
		{
			if (Enum.TryParse<PerpetualLicenseFeature>(feature, out var result))
			{
				return Convert.ToInt32(result).ToString();
			}
			return null;
		}

		public static bool IsLicenseVersion(this IProductLicense productLicense, PerpetualLicenseVersion version)
		{
			return true;
		}

		public static bool IsUnlocked(this IProductLicense productLicense)
		{
			return true;
		}

		public static bool IsProfessional(this IProductLicense productLicense)
		{
			return true;
		}

		public static bool IsFreelance(this IProductLicense productLicense)
		{
			return false;
		}

		public static bool IsStarter(this IProductLicense productLicense, bool includeExpiredStarter = false)
		{
			return false;
		}

		public static bool IsExpress(this IProductLicense productLicense)
		{
			return false;
		}

		public static bool IsWorkGroup(this IProductLicense productLicense)
		{
			return true;
		}

		public static bool IsTrial(this IProductLicense productLicense)
		{
			return false;
		}

		public static bool IsStarterLicenseInExpiryPeriod(this IProductLicense productLicense)
		{
			return false;
		}

		public static bool IsTimeLimitedLicenseInExpiryPeriod(this IProductLicense productLicense)
		{
			return false;
		}

		public static string GetFeaturePropertyValue(this IProductLicense productLicense, string featureName, string featurePropertyName)
		{
			ILicenseFeature feature = productLicense.GetFeature(featureName);
			if (feature == null || string.IsNullOrEmpty(feature.Value))
			{
				return null;
			}
			string[] array = feature.Value.Split(';');
			if (array.Length == 0)
			{
				return null;
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(':');
				if (array3.Length == 2 && string.Equals(featurePropertyName, array3[0], StringComparison.InvariantCultureIgnoreCase))
				{
					return array3[1];
				}
			}
			return null;
		}

		public static int? GetMaxTranslationUnits(this IProductLicense productLicense)
		{
			string featurePropertyValue = productLicense.GetFeaturePropertyValue("StarterEdition", "MaxTranslationUnits");
			if (!string.IsNullOrEmpty(featurePropertyValue))
			{
				return Convert.ToInt32(featurePropertyValue);
			}
			return null;
		}

		public static int? GetMaxTargetLanguages(this IProductLicense productLicense)
		{
			string featurePropertyValue = productLicense.GetFeaturePropertyValue("FreelanceEdition", "MaxTargetLanguages");
			if (!string.IsNullOrEmpty(featurePropertyValue))
			{
				return Convert.ToInt32(featurePropertyValue);
			}
			return null;
		}
	}
}
