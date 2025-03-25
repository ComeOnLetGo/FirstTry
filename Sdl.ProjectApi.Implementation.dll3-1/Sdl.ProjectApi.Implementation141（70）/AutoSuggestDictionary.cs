namespace Sdl.ProjectApi.Implementation
{
	public class AutoSuggestDictionary : IAutoSuggestDictionary
	{
		private readonly string _filePath;

		public string FilePath => _filePath;

		public AutoSuggestDictionary(string filePath)
		{
			_filePath = filePath;
		}
	}
}
