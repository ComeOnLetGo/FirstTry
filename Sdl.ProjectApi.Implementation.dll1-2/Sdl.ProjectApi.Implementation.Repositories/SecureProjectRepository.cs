using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.SecureProjects;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class SecureProjectRepository : ProjectRepository
	{
		private string _encryptionKey;

		private readonly Uri _server;

		private readonly string _instanceId;

		private const string SecureElementCascadeItem = "CascadeItem";

		private const string SecureElementTermbaseConfiguration = "TermbaseConfiguration";

		public string EncryptionKey
		{
			get
			{
				if (_encryptionKey == null)
				{
					_encryptionKey = Application.EncryptionKeyProvider.GetEncryptionKey(_server, _instanceId);
				}
				return _encryptionKey;
			}
		}

		public SecureProjectRepository(IApplication application, IProjectPathUtil pathUtil)
			: base(application, pathUtil)
		{
		}

		public SecureProjectRepository(IApplication application, IProjectPathUtil pathUtil, string projectName, IUser projectCreatedBy, DateTime projectCreatedAt, bool inPlace, Language sourceLanguage)
			: base(application, pathUtil, projectName, Guid.NewGuid(), projectCreatedBy, projectCreatedAt, inPlace, sourceLanguage)
		{
		}

		public SecureProjectRepository(IApplication application, IPackageProject packageProject, IProjectPathUtil pathUtil)
			: base(application, packageProject, pathUtil)
		{
		}

		public override void Load(string projectFilePath)
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (string.IsNullOrEmpty(EncryptionKey))
				{
					throw new InvalidEncryptionKeyException(StringResources.SecureProjects_NoEncryptionkey);
				}
				AesCryptoServiceProvider cryptoServiceProvider = GetCryptoServiceProvider();
				XmlDocument xmlDocument = new XmlDocument();
				using FileStream inStream = File.OpenRead(projectFilePath);
				xmlDocument.Load(inStream);
				SecureProjectUtil.Remove(xmlDocument, "CascadeItem");
				SecureProjectUtil.Remove(xmlDocument, "TermbaseConfiguration");
				SecureProjectUtil.Decrypt(xmlDocument, cryptoServiceProvider);
				DeserializeProject(xmlDocument);
			}
			catch (FileNotFoundException innerException)
			{
				throw new FileNotFoundException(string.Format(StringResources.ProjectFile_FileXNotFound, projectFilePath), innerException);
			}
			catch (InvalidVersionException)
			{
				throw;
			}
			catch (PackageLicenseCheckFailedException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.Project_ErrorDeserializing, string.Empty, projectFilePath, Util.GetXmlSerializationExceptionMessage(ex)), ex);
			}
			LoadCaches();
			MarkAsInitialized();
		}

		public override void Save(string projectFilePath)
		{
			if (string.IsNullOrEmpty(EncryptionKey))
			{
				return;
			}
			base.SettingsBundles.Save();
			AesCryptoServiceProvider cryptoServiceProvider = GetCryptoServiceProvider();
			XmlDocument xmlDocument = SerializeToXmlDocument();
			SecureProjectUtil.Encrypt(xmlDocument, "CascadeItem", cryptoServiceProvider);
			SecureProjectUtil.Encrypt(xmlDocument, "TermbaseConfiguration", cryptoServiceProvider);
			using FileStream outStream = new FileStream(projectFilePath, FileMode.Create);
			xmlDocument.Save(outStream);
		}

		private AesCryptoServiceProvider GetCryptoServiceProvider()
		{
			byte[] decodedKey = SecureProjectUtil.GetDecodedKey(EncryptionKey);
			return new AesCryptoServiceProvider
			{
				Mode = CipherMode.CBC,
				Padding = PaddingMode.PKCS7,
				Key = decodedKey
			};
		}
	}
}
