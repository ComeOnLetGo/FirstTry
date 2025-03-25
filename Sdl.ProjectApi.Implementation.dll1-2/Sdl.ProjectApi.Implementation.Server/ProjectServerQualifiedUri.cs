using System;
using System.Globalization;

namespace Sdl.ProjectApi.Implementation.Server
{
	public static class ProjectServerQualifiedUri
	{
		public static Uri GetProjectServerQualifiedUri(this Uri uri)
		{
			return new Uri(string.Format(CultureInfo.InvariantCulture, "ps.{0}", uri.AbsoluteUri));
		}
	}
}
