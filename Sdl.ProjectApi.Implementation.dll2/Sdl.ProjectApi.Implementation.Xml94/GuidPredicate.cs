using System;

namespace Sdl.ProjectApi.Implementation.Xml
{
	internal class GuidPredicate
	{
		private Guid _guid;

		public GuidPredicate(Guid guid)
		{
			_guid = guid;
		}

		public bool MatchTargetLanguageSettingsBundleGuid(GenericItemWithSettings genericItemWithSettings)
		{
			_ = genericItemWithSettings.SettingsBundleGuid;
			return genericItemWithSettings.SettingsBundleGuid.Equals(_guid);
		}

		public bool MatchGuid(GenericItem item)
		{
			if (item.HasGuid())
			{
				return item.Guid.Equals(_guid);
			}
			return false;
		}
	}
}
