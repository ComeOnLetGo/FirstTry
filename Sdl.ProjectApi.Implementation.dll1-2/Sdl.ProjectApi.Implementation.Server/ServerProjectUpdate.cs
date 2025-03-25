using System;
using System.IO;
using Sdl.ProjectApi.Server;
using Sdl.StudioServer.ProjectServer.Package;

namespace Sdl.ProjectApi.Implementation.Server
{
	public class ServerProjectUpdate
	{
		private readonly Project _project;

		private readonly bool _metaDataOnly;

		public ServerProjectUpdate(Project project, bool metaDataOnly = true)
		{
			if (project == null)
			{
				throw new ArgumentNullException("project");
			}
			_project = project;
			_metaDataOnly = metaDataOnly;
		}

		public void Execute()
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			string text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				_project.Save();
				ICommuteClient val = _project.CreateCommuteClient();
				using (FileStream fileStream = File.Create(text))
				{
					ProjectPackage val2 = new ProjectPackage((Stream)fileStream, (PackageAccessMode)0);
					try
					{
						val2.ProjectName = _project.Name;
						val2.ManifestXml = PackageTransforms.TransformProjectToPackage(_project.ProjectFilePath, Guid.NewGuid(), _metaDataOnly);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				val.UpdateProjectSettings(_project.Guid, text);
			}
			finally
			{
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
		}
	}
}
