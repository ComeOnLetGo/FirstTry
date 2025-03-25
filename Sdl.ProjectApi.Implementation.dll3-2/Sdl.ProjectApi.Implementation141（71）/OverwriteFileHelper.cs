namespace Sdl.ProjectApi.Implementation
{
	internal class OverwriteFileHelper
	{
		private OverwriteFileEventResult _permanentOverwriteFileEventResult;

		public bool ShouldOverwriteFile(IOverwriteFileQuestion op)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Invalid comparison between Unknown and I4
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Invalid comparison between Unknown and I4
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			OverwriteFileEventResult val;
			if ((int)_permanentOverwriteFileEventResult == 0)
			{
				val = op.ShouldOverwriteFile();
				if ((int)val == 3)
				{
					OverwriteFileEventResult val2 = (OverwriteFileEventResult)1;
					_permanentOverwriteFileEventResult = (OverwriteFileEventResult)1;
					val = val2;
				}
				else if ((int)val == 4)
				{
					OverwriteFileEventResult val2 = (OverwriteFileEventResult)2;
					_permanentOverwriteFileEventResult = (OverwriteFileEventResult)2;
					val = val2;
				}
			}
			else
			{
				val = _permanentOverwriteFileEventResult;
			}
			if ((int)val == 5)
			{
				throw new ProjectApiException(ErrorMessages.ReturnPackageImport_Cancelled);
			}
			return (int)val == 1;
		}
	}
}
