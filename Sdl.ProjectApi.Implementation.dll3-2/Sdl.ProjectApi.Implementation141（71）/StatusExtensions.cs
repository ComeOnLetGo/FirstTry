using System;

namespace Sdl.ProjectApi.Implementation
{
	public static class StatusExtensions
	{
		public static ValueStatus CombineValueStatus(this ValueStatus totalStatus, ValueStatus status)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected I4, but got Unknown
			//IL_0013: Expected I4, but got Unknown
			if ((int)status != 0)
			{
				if (status - 1 <= 2)
				{
					if ((int)totalStatus == 0)
					{
						return (ValueStatus)1;
					}
					return (ValueStatus)Math.Min((int)totalStatus, (int)status);
				}
				throw new ArgumentException("Unexpected ValueStatus: " + ((object)(ValueStatus)(ref status)).ToString());
			}
			if ((int)totalStatus == 0)
			{
				return (ValueStatus)0;
			}
			return (ValueStatus)1;
		}
	}
}
