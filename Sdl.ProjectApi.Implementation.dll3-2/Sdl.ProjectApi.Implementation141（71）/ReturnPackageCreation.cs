using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Logging;
using Sdl.Desktop.Logger;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.Implementation.Builders;
using Sdl.ProjectApi.Implementation.LanguageCloud;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ReturnPackageCreation : PackageCreation, IReturnPackageCreation, IPackageCreation, IPackageOperation
	{
		private class MyMessageReporter : IPackageOperationMessageReporter
		{
			private readonly ReturnPackageCreation _creation;

			public MyMessageReporter(ReturnPackageCreation creation)
			{
				_creation = creation;
			}

			public void ReportMessage(string source, string message, MessageLevel level)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, message, level);
			}

			public void ReportMessage(string source, string message, MessageLevel level, Exception exception)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, message, level, exception);
			}

			public void ReportMessage(string source, string message, MessageLevel level, IProjectFile projectFile)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, GetFileMessage(projectFile, message), level);
			}

			public void ReportMessage(string source, string message, MessageLevel level, Exception exception, IProjectFile projectFile)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, GetFileMessage(projectFile, message), level, exception);
			}

			public void ReportMessage(string source, string message, MessageLevel level, ITranslatableFile translatableFile, IMessageLocation fromLocation, IMessageLocation uptoLocation)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				_creation.ReportMessage(source, GetFileMessage((IProjectFile)(object)translatableFile, message), level, fromLocation, uptoLocation);
			}

			public void ReportCurrentOperation(string currentOperationDescription)
			{
				_creation.SetCurrentOperationDescription(currentOperationDescription);
			}

			private string GetFileMessage(IProjectFile projectFile, string message)
			{
				return string.Format(StringResources.ProjectPackageCreation_FileMessage, Path.GetFileName(projectFile.LocalFilePath), message);
			}
		}

		private readonly IProjectPackageInitializer _packageInitializer;

		internal Sdl.ProjectApi.Implementation.Xml.ReturnPackageCreation XmlReturnPackageCreation => (Sdl.ProjectApi.Implementation.Xml.ReturnPackageCreation)base.XmlPackageCreation;

		internal ReturnPackageCreation(IProject project, ITaskFile[] taskFiles, string packageName, string comment, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(project, new XmlProjectPackageCreationBuilder(projectPathUtil).CreateXmlReturnPackageCreation(project, taskFiles, packageName, comment), projectPathUtil)
		{
			_packageInitializer = packageInitializer;
		}

		internal ReturnPackageCreation(IProject project, Sdl.ProjectApi.Implementation.Xml.ReturnPackageCreation returnPackageCreation, IProjectPackageInitializer packageInitializer, IProjectPathUtil projectPathUtil)
			: base(project, returnPackageCreation, projectPathUtil)
		{
			_packageInitializer = packageInitializer;
		}

		protected override void StartImpl()
		{
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Invalid comparison between Unknown and I4
			SetCurrentOperationDescription(StringResources.ReturnPackageCreation_InitPackage, 0);
			string path = CreateTemporaryPackageFolder();
			string path2 = Path.GetFileNameWithoutExtension(GetAbsolutePackagePath()) + FileTypes.ProjectFileExtension;
			path = Path.Combine(path, path2);
			IPackageProject val = _packageInitializer.Create(base.Project.Name, (PackageType)1, base.Project.SourceLanguage, path, base.Project.Guid);
			XmlReturnPackageCreation.PackageGuid = val.PackageGuid;
			val.Comment = base.Comment;
			CopyProjectProperties(base.Project, (IProject)(object)val);
			val.CopyTranslationProviderCascade(((IProjectConfiguration)base.Project).CascadeItem, ((IProjectConfiguration)val).CascadeItem, false, false, true, (ILanguageDirection)null);
			ILanguageDirection[] languageDirections = ((IProjectConfiguration)base.Project).LanguageDirections;
			foreach (ILanguageDirection val2 in languageDirections)
			{
				((IProject)val).AddLanguageDirection(val2, false, false, false, true);
			}
			SetCurrentOperationDescription(StringResources.ReturnPackageCreation_AddingFiles, 20);
			IEnumerable<TaskFileGroup> enumerable = TaskFileGroup.GroupTaskFilesByTask(Files);
			foreach (TaskFileGroup item in enumerable)
			{
				val.AddManualTask(item.Task, item.Files.ToArray(), (IPackageOperationMessageReporter)(object)new MyMessageReporter(this));
			}
			SetCurrentOperationDescription(StringResources.ReturnPackageCreation_AddingFiles, 50);
			ITaskFile[] files = Files;
			foreach (ITaskFile taskFile in files)
			{
				if (XliffUtil.IsSdlXliffFile(taskFile.ProjectFile.LocalFilePath))
				{
					continue;
				}
				string text = taskFile.ProjectFile.LocalFilePath + ".sdlxliff";
				if (File.Exists(text))
				{
					IProjectFile val3 = ((IProject)val).GetAllProjectFiles().FirstOrDefault((IProjectFile f) => f.Guid == taskFile.ProjectFile.Guid);
					if (val3 != null)
					{
						Util.CopyFile(text, val3.LocalFilePath + ".sdlxliff");
					}
				}
			}
			SetCurrentOperationDescription(StringResources.ReturnPackageCreation_CompressingPackage, 70);
			((IProjectConfiguration)val).Save();
			if ((int)base.Project.ProjectType == 12)
			{
				XmlProjectIdsReplacer xmlProjectIdsReplacer = new XmlProjectIdsReplacer(new XmlDocument(), (ILogger)(object)LoggerFactoryExtensions.CreateLogger<XmlProjectIdsReplacer>(LogProvider.GetLoggerFactory()));
				xmlProjectIdsReplacer.ReplaceProjectIdForReturnPackage(((IProject)val).ProjectFilePath, base.Project.MigrationData);
			}
			((IProject)val).ForceReload();
			val.CreatePackage(GetAbsolutePackagePath(), false);
			SetPercentComplete(100);
			base.Project.AddReturnPackageCreationOperation((IReturnPackageCreation)(object)this);
			((IProjectConfiguration)base.Project).Save();
		}

		public override string ToString()
		{
			return StringResources.ReturnPackageCreation_Source;
		}
	}
}
