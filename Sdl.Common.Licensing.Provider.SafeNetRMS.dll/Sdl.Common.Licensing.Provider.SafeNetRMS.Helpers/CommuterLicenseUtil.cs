using System.IO;
using System.Xml.Serialization;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	public class CommuterLicenseUtil
	{
		public static LicenseDefinition GetLicenseDefinition(SafeNetRMSProviderConfiguration config)
		{
			string licenseCommuterFilePath = config.LicenseCommuterFilePath;
			if (!File.Exists(licenseCommuterFilePath))
			{
				return DeserializeFromString<LicenseDefinition>(config.LicenseCommuterDefinition);
			}
			return DeserializeObjectFromFile<LicenseDefinition>(licenseCommuterFilePath);
		}

		private static T DeserializeObjectFromFile<T>(string path)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using FileStream stream = File.OpenRead(path);
			return (T)xmlSerializer.Deserialize(stream);
		}

		private static T DeserializeFromString<T>(string input) where T : class
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using StringReader textReader = new StringReader(input);
			return (T)xmlSerializer.Deserialize(textReader);
		}
	}
}
