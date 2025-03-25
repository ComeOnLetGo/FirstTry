namespace Sdl.ProjectApi.Implementation
{
	internal interface ICopyableTranslationProviderItem
	{
		bool CanBeCopied { get; }

		void CopyTranslationProvider();
	}
}
