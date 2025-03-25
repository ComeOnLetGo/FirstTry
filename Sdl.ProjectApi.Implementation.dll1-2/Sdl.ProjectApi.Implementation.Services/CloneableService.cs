using Newtonsoft.Json;

namespace Sdl.ProjectApi.Implementation.Services
{
	internal static class CloneableService
	{
		public static T Clone<T>(T source)
		{
			string text = JsonConvert.SerializeObject((object)source);
			return JsonConvert.DeserializeObject<T>(text);
		}
	}
}
