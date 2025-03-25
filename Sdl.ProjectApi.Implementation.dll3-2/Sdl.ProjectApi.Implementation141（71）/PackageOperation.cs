using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Sdl.Core.Settings;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.MultiTerm.Core.Settings;
using Sdl.ProjectApi.Implementation.ProjectSettings;
using Sdl.ProjectApi.Implementation.Server;
using Sdl.ProjectApi.Implementation.TermbaseApi;
using Sdl.ProjectApi.Implementation.Xml;
using Sdl.ProjectApi.Settings;
using Sdl.ProjectApi.Settings.SettingTypes;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation
{
	public abstract class PackageOperation : IPackageOperation
	{
		private IProject _project;

		private ITaskFile[] _lazyTaskFiles;

		private IManualTask[] _lazyManualTasks;

		private ExecutionResult _lazyResult;

		private Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings _oldLanguageCloudProjectSettings;

		private LanguageCloudFilesSettings _oldLanguageCloudFileSettings;

		[CompilerGenerated]
		private PackageOperationStatusChangedEventHandler m_StatusChanged;

		public string CurrentOperationDescription { get; private set; }

		public Guid Id => XmlPackageOperation.Guid;

		public Guid PackageId => XmlPackageOperation.PackageGuid;

		public string Name => XmlPackageOperation.PackageName;

		public string Comment => XmlPackageOperation.Comment;

		public PackageStatus Status => EnumConvert.ConvertPackageStatus(XmlPackageOperation.Status);

		public int PercentComplete => XmlPackageOperation.PercentComplete;

		public IExecutionResult Result => (IExecutionResult)(object)ResultImpl;

		internal ExecutionResult ResultImpl => _lazyResult ?? (_lazyResult = new ExecutionResult(XmlPackageOperation.Result, null));

		public IProject Project => _project;

		public virtual ITaskFile[] Files
		{
			get
			{
				if (_project == null)
				{
					throw new InvalidOperationException("This property is only available once the package has been imported.");
				}
				if (_lazyTaskFiles == null)
				{
					_lazyTaskFiles = (ITaskFile[])(object)new ITaskFile[XmlPackageOperation.Files.Count];
					for (int i = 0; i < _lazyTaskFiles.Length; i++)
					{
						_lazyTaskFiles[i] = _project.GetTaskFile(XmlPackageOperation.Files[i].TaskFileGuid);
					}
				}
				return _lazyTaskFiles;
			}
			set
			{
				_lazyTaskFiles = value;
				XmlPackageOperation.Files.Clear();
				ITaskFile[] lazyTaskFiles = _lazyTaskFiles;
				foreach (ITaskFile val in lazyTaskFiles)
				{
					TaskFileRef item = new TaskFileRef
					{
						TaskFileGuid = val.Id
					};
					XmlPackageOperation.Files.Add(item);
				}
			}
		}

		public virtual IManualTask[] Tasks
		{
			get
			{
				if (_project == null)
				{
					throw new InvalidOperationException("This property is only available once the package has been imported.");
				}
				if (_lazyManualTasks == null)
				{
					_lazyManualTasks = (IManualTask[])(object)new IManualTask[XmlPackageOperation.Tasks.Count];
					for (int i = 0; i < _lazyManualTasks.Length; i++)
					{
						_lazyManualTasks[i] = (IManualTask)(object)ProjectImpl.GetManualTask(XmlPackageOperation.Tasks[i].TaskGuid);
					}
				}
				return _lazyManualTasks;
			}
			set
			{
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				_lazyManualTasks = value;
				XmlPackageOperation.Tasks.Clear();
				IManualTask[] lazyManualTasks = _lazyManualTasks;
				foreach (IManualTask val in lazyManualTasks)
				{
					TaskRef taskRef = new TaskRef();
					TaskId id = ((ITaskBase)val).Id;
					taskRef.TaskGuid = ((TaskId)(ref id)).ToGuidArray()[0];
					TaskRef item = taskRef;
					XmlPackageOperation.Tasks.Add(item);
				}
			}
		}

		public bool PackageExists => File.Exists(GetAbsolutePackagePath());

		internal Sdl.ProjectApi.Implementation.Xml.PackageOperation XmlPackageOperation { get; }

		public Project ProjectImpl
		{
			get
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				if (!(_project is Project result))
				{
					throw new ProjectApiException(ErrorMessages.PackageOperation_PropertyNotAvailable);
				}
				return result;
			}
		}

		public event PackageOperationStatusChangedEventHandler StatusChanged
		{
			[CompilerGenerated]
			add
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Expected O, but got Unknown
				PackageOperationStatusChangedEventHandler val = this.m_StatusChanged;
				PackageOperationStatusChangedEventHandler val2;
				do
				{
					val2 = val;
					PackageOperationStatusChangedEventHandler value2 = (PackageOperationStatusChangedEventHandler)Delegate.Combine((Delegate)(object)val2, (Delegate)(object)value);
					val = Interlocked.CompareExchange(ref this.m_StatusChanged, value2, val2);
				}
				while (val != val2);
			}
			[CompilerGenerated]
			remove
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Expected O, but got Unknown
				PackageOperationStatusChangedEventHandler val = this.m_StatusChanged;
				PackageOperationStatusChangedEventHandler val2;
				do
				{
					val2 = val;
					PackageOperationStatusChangedEventHandler value2 = (PackageOperationStatusChangedEventHandler)Delegate.Remove((Delegate)(object)val2, (Delegate)(object)value);
					val = Interlocked.CompareExchange(ref this.m_StatusChanged, value2, val2);
				}
				while (val != val2);
			}
		}

		internal PackageOperation(IProject project, Sdl.ProjectApi.Implementation.Xml.PackageOperation xmlPackageOperation)
			: this(xmlPackageOperation)
		{
			_project = project;
		}

		internal PackageOperation(Sdl.ProjectApi.Implementation.Xml.PackageOperation xmlPackageOperation)
		{
			XmlPackageOperation = xmlPackageOperation;
			if (XmlPackageOperation.Result == null)
			{
				XmlPackageOperation.Result = new Sdl.ProjectApi.Implementation.Xml.ExecutionResult();
			}
		}

		protected void ReportMessage(string source, string message, MessageLevel level, Exception exception)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			ResultImpl.ReportMessage(source, message, level, exception);
		}

		protected void ReportMessage(string source, string message, MessageLevel level)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			ResultImpl.ReportMessage(source, message, level);
		}

		protected void ReportMessage(string source, string message, MessageLevel level, IMessageLocation fromLocation, IMessageLocation uptoLocation)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			ResultImpl.ReportMessage(source, message, level, fromLocation, uptoLocation);
		}

		public void Start()
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Invalid comparison between Unknown and I4
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Invalid comparison between Unknown and I4
			SetStatus((PackageStatus)3);
			bool flag = false;
			try
			{
				StartImpl();
			}
			catch (Exception ex)
			{
				flag = ex.Message == ErrorMessages.ProjectPackageImport_FileAlreadyExistsLocally;
				MessageLevel level = (MessageLevel)((!flag) ? 2 : 0);
				ReportMessage(ToString(), ex.Message, level, ex);
			}
			if ((int)Status == 6)
			{
				SetStatus((PackageStatus)7);
			}
			if ((int)Status != 7)
			{
				SetStatus((PackageStatus)((Result.HasErrors || flag) ? 4 : 5));
			}
		}

		protected abstract void StartImpl();

		public void Cancel()
		{
			if (!IsComplete() && !IsCancelling())
			{
				SetStatus((PackageStatus)6);
			}
		}

		private bool IsCancelling()
		{
			return XmlPackageOperation.Status == PackageStatus.Cancelling;
		}

		public bool IsComplete()
		{
			PackageStatus status = XmlPackageOperation.Status;
			if (status == PackageStatus.Invalid || (uint)(status - 4) <= 1u || status == PackageStatus.Cancelled)
			{
				return true;
			}
			return false;
		}

		internal abstract string GetAbsolutePackagePath();

		protected void SetProject(IProject project)
		{
			_project = project;
		}

		protected void SetStatus(PackageStatus status)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Invalid comparison between Unknown and I4
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			PackageStatus packageStatus = EnumConvert.ConvertPackageStatus(status);
			if (packageStatus != XmlPackageOperation.Status)
			{
				XmlPackageOperation.Status = packageStatus;
				if ((int)status != 3)
				{
					CurrentOperationDescription = "";
				}
				OnStatusChanged(status);
			}
		}

		protected void SetPercentComplete(int percentComplete)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			XmlPackageOperation.PercentComplete = percentComplete;
			OnStatusChanged(Status);
		}

		protected virtual void OnStatusChanged(PackageStatus status)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			PackageOperationStatusChangedEventHandler statusChanged = this.StatusChanged;
			if (statusChanged != null)
			{
				statusChanged.Invoke((IPackageOperation)(object)this, status);
			}
		}

		protected void SetCurrentOperationDescription(string currentOperationDescription)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			CurrentOperationDescription = currentOperationDescription;
			OnStatusChanged(Status);
		}

		protected void SetCurrentOperationDescription(string currentOperationDescription, int percentComplete)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			CurrentOperationDescription = currentOperationDescription;
			XmlPackageOperation.PercentComplete = percentComplete;
			OnStatusChanged(Status);
		}

		protected void CopyProjectProperties(IProject fromProject, IProject toProject)
		{
			toProject.Description = fromProject.Description;
			toProject.DueDate = fromProject.DueDate;
			toProject.ChangeCustomer(fromProject.Customer);
			CopySettings(fromProject, toProject);
			((IProjectConfiguration)toProject).LanguageResources = ((IProjectConfiguration)fromProject).LanguageResources;
			int[] analysisBands = Array.ConvertAll(((IProjectConfiguration)fromProject).AnalysisBands, (IAnalysisBand band) => band.MinimumMatchValue);
			((IProjectConfiguration)toProject).SetAnalysisBands(analysisBands);
		}

		protected void CopySettings(IProject fromProject, IProject toProject)
		{
			if (toProject.IsLCProject)
			{
				_oldLanguageCloudProjectSettings = ((IObjectWithSettings)toProject).Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>();
				_oldLanguageCloudFileSettings = ((IObjectWithSettings)toProject).Settings.GetSettingsGroup<LanguageCloudFilesSettings>();
			}
			((IObjectWithSettings)toProject).Settings = ((IObjectWithSettings)fromProject).Settings;
			((AbstractSettingsGroupBase)((IObjectWithSettings)toProject).Settings.GetSettingsGroup<Sdl.ProjectApi.Implementation.ProjectSettings.ProjectSettings>()).ImportSettings((ISettingsGroup)(object)_oldLanguageCloudProjectSettings);
			((AbstractSettingsGroupBase)((IObjectWithSettings)toProject).Settings.GetSettingsGroup<LanguageCloudFilesSettings>()).ImportSettings((ISettingsGroup)(object)_oldLanguageCloudFileSettings);
			PublishProjectOperationSettings settingsGroup = ((IObjectWithSettings)toProject).Settings.GetSettingsGroup<PublishProjectOperationSettings>();
			((AbstractSettingsGroupBase)settingsGroup).Reset();
			ProjectSyncSettings settingsGroup2 = ((IObjectWithSettings)toProject).Settings.GetSettingsGroup<ProjectSyncSettings>();
			((AbstractSettingsGroupBase)settingsGroup2).Reset();
			bool flag = ((IObjectWithSettings)fromProject).Settings.ContainsSettingsGroup("TranslationQualityAssessmentSettings");
			if (flag)
			{
				TranslationQualityAssessmentSettings settingsGroup3 = ((IObjectWithSettings)fromProject).Settings.GetSettingsGroup<TranslationQualityAssessmentSettings>();
				settingsGroup3.LoadFeedbackCategories(((IObjectWithSettings)fromProject).Settings);
				settingsGroup3.LoadFeedbackSeverities(((IObjectWithSettings)fromProject).Settings);
				flag = settingsGroup3.Categories != null && ((List<AssessmentCategoryConfigurationItem>)(object)settingsGroup3.Categories).Count > 0 && settingsGroup3.Severities != null && ((List<AssessmentSeverityConfigurationItem>)(object)settingsGroup3.Severities).Count > 0;
			}
			if (flag)
			{
				ISettingsGroup settingsGroup4 = ((IObjectWithSettings)toProject).Settings.GetSettingsGroup("PackageLicenseInfo");
				Setting<string> setting = settingsGroup4.GetSetting<string>("Grant");
				string value = setting.Value;
				if (string.IsNullOrEmpty(value))
				{
					setting.Value = "AllowTQA";
				}
				else if (!value.Contains("AllowTQA"))
				{
					setting.Value = string.Format("{0},{1}", value, "AllowTQA");
				}
				Setting<string> setting2 = settingsGroup4.GetSetting<string>("Hash");
				if (string.IsNullOrWhiteSpace(setting2.Value))
				{
					setting2.Value = AllowTQAValidator.HashProject(fromProject.Guid.ToString());
				}
			}
		}

		protected void CopyProjectTermbases(IProject sourceProject, IProject targetProject, bool includeFileBasedTermbases)
		{
			IProjectTermbaseConfiguration val = ((ICopyable<IProjectTermbaseConfiguration>)(object)((IProjectConfiguration)sourceProject).TermbaseConfiguration).Copy();
			((ICollection<IProjectTermbase>)val.Termbases).Clear();
			string localTermbaseDirectory = GetLocalTermbaseDirectory(targetProject);
			foreach (IProjectTermbase item3 in (IEnumerable<IProjectTermbase>)((IProjectConfiguration)sourceProject).TermbaseConfiguration.Termbases)
			{
				TermbaseSettings val2 = TermbaseSettings.FromXml(item3.SettingsXml);
				if (IsLocalTermbase(val2))
				{
					if (includeFileBasedTermbases)
					{
						IProjectTermbase item = CopyLocalTermbase(item3, val2, localTermbaseDirectory);
						((ICollection<IProjectTermbase>)val.Termbases).Add(item);
					}
				}
				else
				{
					IProjectTermbase item2 = CopyServerTermbase(item3);
					((ICollection<IProjectTermbase>)val.Termbases).Add(item2);
				}
			}
			((IProjectConfiguration)targetProject).TermbaseConfiguration = val;
		}

		protected string GetReadOnlyFileBasedTermbase(IProject sourceProject)
		{
			foreach (IProjectTermbase item in (IEnumerable<IProjectTermbase>)((IProjectConfiguration)sourceProject).TermbaseConfiguration.Termbases)
			{
				TermbaseSettings val = TermbaseSettings.FromXml(item.SettingsXml);
				if (IsLocalTermbase(val))
				{
					FileInfo fileInfo = new FileInfo(val.Path);
					if (fileInfo.IsReadOnly)
					{
						return val.Path;
					}
				}
			}
			return string.Empty;
		}

		protected void CopyProjectReports(IEnumerable<Sdl.ProjectApi.Implementation.Xml.AutomaticTask> xmlAutomaticTasks, string projectPath, string targetPath)
		{
			foreach (TaskReport item in xmlAutomaticTasks.SelectMany((Sdl.ProjectApi.Implementation.Xml.AutomaticTask xmlAutomaticTask) => xmlAutomaticTask.Reports))
			{
				CopyReport(item.PhysicalPath, projectPath, targetPath);
			}
		}

		private void CopyReport(string reportPhysicalPath, string projectPath, string packageTempPath)
		{
			string sourceFilePath = Path.Combine(projectPath, reportPhysicalPath);
			string targetFilePath = Path.Combine(packageTempPath, reportPhysicalPath);
			Util.CopyFile(sourceFilePath, targetFilePath);
		}

		private static bool IsLocalTermbase(TermbaseSettings settings)
		{
			if (!settings.IsCustom)
			{
				return settings.Local;
			}
			return false;
		}

		private static string GetLocalTermbaseDirectory(IProject project)
		{
			return Path.Combine(Path.GetDirectoryName(project.ProjectFilePath), "Termbases");
		}

		private IProjectTermbase CopyLocalTermbase(IProjectTermbase sourceLocalTermbase, TermbaseSettings sourceLocalTermbaseSettings, string targetLocalTermbaseDirectory)
		{
			string targetLocalTermbasePath = CopyLocalTermbaseFile(sourceLocalTermbaseSettings, targetLocalTermbaseDirectory);
			return CopyLocalTermbaseObject(sourceLocalTermbase, sourceLocalTermbaseSettings, targetLocalTermbasePath);
		}

		private string CopyLocalTermbaseFile(TermbaseSettings sourceLocalTermbaseSettings, string targetLocalTermbaseDirectory)
		{
			string path = sourceLocalTermbaseSettings.Path;
			string fileName = Path.GetFileName(path);
			string text = Path.Combine(targetLocalTermbaseDirectory, fileName);
			Util.CopyFile(path, text);
			return text;
		}

		private IProjectTermbase CopyLocalTermbaseObject(IProjectTermbase sourceLocalTermbase, TermbaseSettings sourceLocalTermbaseSettings, string targetLocalTermbasePath)
		{
			sourceLocalTermbaseSettings.Path = targetLocalTermbasePath;
			string settingsXml = sourceLocalTermbaseSettings.ToXml();
			return (IProjectTermbase)(object)new ProjectTermbase(sourceLocalTermbase.Name, settingsXml, sourceLocalTermbase.Filter, sourceLocalTermbase.Enabled);
		}

		private IProjectTermbase CopyServerTermbase(IProjectTermbase sourceServerTermbase)
		{
			return ((ICopyable<IProjectTermbase>)(object)sourceServerTermbase).Copy();
		}
	}
}
