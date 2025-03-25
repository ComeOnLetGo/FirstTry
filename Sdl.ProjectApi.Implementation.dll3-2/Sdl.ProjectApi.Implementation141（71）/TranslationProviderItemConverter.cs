using System;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal static class TranslationProviderItemConverter
	{
		public static ITranslationProviderItem ToObject(this TranslationProviderItem xmlTranslationProviderItem, IRelativePathManager relativePathManager)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			Uri absoluteUri = GetAbsoluteUri(xmlTranslationProviderItem.Uri, relativePathManager);
			string state = xmlTranslationProviderItem.State;
			bool enabled = xmlTranslationProviderItem.Enabled;
			return (ITranslationProviderItem)new TranslationProviderItem(absoluteUri, state, enabled);
		}

		public static TranslationProviderItem ToXml(this ITranslationProviderItem objTranslationProviderItem, IRelativePathManager relativePathManager)
		{
			TranslationProviderItem translationProviderItem = new TranslationProviderItem();
			translationProviderItem.Uri = GetRelativeUri(objTranslationProviderItem.Uri, relativePathManager);
			translationProviderItem.State = objTranslationProviderItem.State;
			translationProviderItem.Enabled = objTranslationProviderItem.Enabled;
			return translationProviderItem;
		}

		internal static Uri GetAbsoluteUri(string relativeUri, IRelativePathManager relativePathManager)
		{
			if (relativeUri == null)
			{
				return null;
			}
			if (!HasFileBasedTranslationMemoryPrefix(relativeUri))
			{
				return new Uri(relativeUri);
			}
			relativeUri = relativeUri.Replace('\\', '/');
			string text = relativeUri.Substring(GetFileBasedTranslationMemoryPrefix().Length);
			string text2 = ((!text.StartsWith("/")) ? ("//" + text) : text.Substring(1));
			text2 = text2.Replace('/', '\\');
			string text3 = ((relativePathManager != null) ? relativePathManager.MakeAbsolutePath(text2) : text2);
			return FileBasedTranslationMemory.GetFileBasedTranslationMemoryUri(text3);
		}

		internal static string GetRelativeUri(Uri absoluteUri, IRelativePathManager relativePathManager)
		{
			if (absoluteUri == null)
			{
				return null;
			}
			if (!FileBasedTranslationMemory.IsFileBasedTranslationMemory(absoluteUri))
			{
				return absoluteUri.AbsoluteUri;
			}
			string fileBasedTranslationMemoryFilePath = FileBasedTranslationMemory.GetFileBasedTranslationMemoryFilePath(absoluteUri);
			string text = ((relativePathManager != null) ? relativePathManager.MakeRelativePath(fileBasedTranslationMemoryFilePath) : fileBasedTranslationMemoryFilePath);
			text = text.Replace('\\', '/');
			if (!text.StartsWith("//"))
			{
				return GetFileBasedTranslationMemoryPrefix() + "/" + text;
			}
			return GetFileBasedTranslationMemoryPrefix() + text.Substring(2);
		}

		internal static bool HasFileBasedTranslationMemoryPrefix(string uri)
		{
			string fileBasedTranslationMemoryPrefix = GetFileBasedTranslationMemoryPrefix();
			return uri.StartsWith(fileBasedTranslationMemoryPrefix);
		}

		internal static string GetFileBasedTranslationMemoryPrefix()
		{
			return FileBasedTranslationMemory.GetFileBasedTranslationMemoryScheme() + "://";
		}
	}
}
