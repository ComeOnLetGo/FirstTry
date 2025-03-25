using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public static class PackageProjectHelpers
	{
		public const string PackageLicenseInfoSettings = "PackageLicenseInfo";

		public const string GrantFeatureSetting = "Grant";

		public const string TQASettingsGroupName = "TranslationQualityAssessmentSettings";

		public const string GrantHashName = "Hash";

		public const string GrantAllowTQA = "AllowTQA";

		public static bool HasValidDigitalSignature(string packageProjectPath)
		{
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			RSAParameters rSAParameters = default(RSAParameters);
			rSAParameters.D = null;
			rSAParameters.DP = null;
			rSAParameters.DQ = null;
			rSAParameters.Exponent = new byte[3] { 1, 0, 1 };
			rSAParameters.InverseQ = null;
			rSAParameters.Modulus = new byte[128]
			{
				212, 41, 240, 11, 240, 231, 175, 213, 248, 181,
				26, 223, 183, 127, 251, 77, 138, 245, 238, 113,
				123, 202, 157, 47, 80, 89, 201, 185, 209, 166,
				139, 222, 199, 200, 89, 40, 91, 10, 94, 45,
				89, 215, 207, 59, 67, 86, 147, 11, 45, 91,
				3, 181, 62, 53, 8, 181, 186, 235, 69, 191,
				103, 126, 179, 57, 217, 210, 229, 188, 69, 8,
				161, 114, 165, 188, 14, 223, 204, 180, 143, 107,
				237, 164, 105, 115, 177, 59, 67, 203, 37, 191,
				4, 245, 187, 58, 50, 0, 197, 121, 145, 69,
				184, 52, 135, 76, 185, 147, 39, 95, 130, 126,
				82, 236, 128, 107, 207, 109, 21, 8, 163, 187,
				227, 254, 131, 158, 81, 211, 88, 37
			};
			rSAParameters.P = null;
			rSAParameters.Q = null;
			RSAParameters parameters = rSAParameters;
			rSACryptoServiceProvider.ImportParameters(parameters);
			try
			{
				return VerifyXmlDocument(packageProjectPath, rSACryptoServiceProvider);
			}
			catch
			{
			}
			finally
			{
				rSACryptoServiceProvider.Clear();
			}
			return false;
		}

		private static bool VerifyXmlDocument(string packageProjectFile, RSACryptoServiceProvider publicKey)
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.PreserveWhitespace = true;
				xmlDocument.Load(packageProjectFile);
				SignedXml signedXml = new SignedXml(xmlDocument);
				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Signature");
				if (elementsByTagName == null || elementsByTagName.Count == 0)
				{
					return false;
				}
				signedXml.LoadXml((XmlElement)elementsByTagName[0]);
				return signedXml.CheckSignature(publicKey);
			}
			catch
			{
				return false;
			}
		}
	}
}
