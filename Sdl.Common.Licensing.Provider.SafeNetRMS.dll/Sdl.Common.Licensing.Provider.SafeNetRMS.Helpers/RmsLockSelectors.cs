using System;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Helpers
{
	[Flags]
	internal enum RmsLockSelectors
	{
		VLS_LOCK_ID_PROM = 1,
		VLS_LOCK_IP_ADDR = 2,
		VLS_LOCK_DISK_ID = 4,
		VLS_LOCK_HOSTNAME = 8,
		VLS_LOCK_ETHERNET = 0x10,
		VLS_LOCK_NW_SERIAL = 0x40,
		VLS_LOCK_PORTABLE_SERV = 0x80,
		VLS_LOCK_CUSTOM = 0x100,
		VLS_LOCK_PROCESSOR_ID = 0x200,
		VLS_LOCK_CUSTOM_EX = 0x400,
		VLS_LOCK_HARD_DISK_SERIAL = 0x800,
		VLS_LOCK_CPU_INFO = 0x1000,
		VLS_LOCK_UUID = 0x3000
	}
}
