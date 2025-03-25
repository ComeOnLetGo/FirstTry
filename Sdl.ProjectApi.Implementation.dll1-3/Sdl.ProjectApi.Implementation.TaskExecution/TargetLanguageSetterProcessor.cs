using Sdl.Core.Globalization;
using Sdl.FileTypeSupport.Framework.BilingualApi;

namespace Sdl.ProjectApi.Implementation.TaskExecution
{
	internal class TargetLanguageSetterProcessor : IBilingualContentHandler
	{
		private readonly Language _targetLanguage;

		public TargetLanguageSetterProcessor(Language targetLanguage)
		{
			_targetLanguage = targetLanguage;
		}

		public void Initialize(IDocumentProperties documentInfo)
		{
			if (documentInfo.TargetLanguage == null || documentInfo.TargetLanguage.CultureInfo == null)
			{
				documentInfo.TargetLanguage = _targetLanguage;
			}
		}

		public void Complete()
		{
		}

		public void SetFileProperties(IFileProperties fileInfo)
		{
		}

		public void FileComplete()
		{
		}

		public void ProcessParagraphUnit(IParagraphUnit paragraphUnit)
		{
		}
	}
}
