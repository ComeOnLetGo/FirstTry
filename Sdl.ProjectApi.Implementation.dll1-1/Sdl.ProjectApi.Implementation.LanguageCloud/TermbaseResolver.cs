using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Sdl.ProjectApi.Implementation.LanguageCloud
{
	public class TermbaseResolver : DefaultContractResolver
	{
		private readonly string _anonymousTypeName = "AnonymousType";

		private readonly string _termbasesPropertyName = "termbases";

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			JsonProperty val = ((DefaultContractResolver)this).CreateProperty(member, memberSerialization);
			if (val.DeclaringType.Name.Contains(_anonymousTypeName))
			{
				val.PropertyName = _termbasesPropertyName;
			}
			return val;
		}
	}
}
