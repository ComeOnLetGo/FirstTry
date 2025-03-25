using System;

namespace Sdl.ProjectApi.Implementation.Licensing.Perpetual
{
	[Flags]
	public enum PerpetualLicenseVersion
	{
		Express = 1,
		Starter = 2,
		Freelance = 4,
		Professional = 8,
		WorkGroup = 0x10,
		FullVersion = 0x1C,
		NotStarter = 0x1D,
		NotExpress = 0x1E
	}
}
