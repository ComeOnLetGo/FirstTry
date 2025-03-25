using System;
using System.Xml.Linq;

namespace Sdl.ProjectApi.Implementation.Migration
{
	internal interface IMigration
	{
		void Migrate(XDocument document, Version documentVersion);
	}
}
