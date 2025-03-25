using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.MultiTerm.Client.TermAccess;
using Sdl.MultiTerm.Core.Common.Interfaces;
using Sdl.ProjectApi.TermbaseApi;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseConfiguration : IProjectTermbaseConfiguration, ICopyable<IProjectTermbaseConfiguration>
	{
		private IProjectTermbaseConfigurationFactory _factory;

		private readonly ITerminologyProviderCredentialStore _terminologyProviderCredentialStore;

		private readonly ProjectTermbaseProvider _termbasesProvider;

		public IProjectTermbaseProvider TermbasesProvider => (IProjectTermbaseProvider)(object)_termbasesProvider;

		public IProjectTermbaseServer TermbaseServer { get; set; }

		public IProjectTermbases Termbases { get; }

		public IProjectTermbaseLanguageIndexes LanguageIndexes { get; }

		public IProjectTermbaseRecognitionOptions RecognitionOptions { get; set; }

		public IProjectTermbaseConfigurationFactory Factory => _factory ?? (_factory = (IProjectTermbaseConfigurationFactory)(object)new ProjectTermbaseConfigurationFactory());

		public ProjectTermbaseConfiguration(IProjectTermbaseServer termbaseServer, IProjectTermbases termbases, IProjectTermbaseLanguageIndexes languageIndexes, IProjectTermbaseRecognitionOptions recognitionOptions, ITerminologyProviderCredentialStore terminologyProviderCredentialStore)
		{
			TermbaseServer = termbaseServer;
			Termbases = termbases;
			LanguageIndexes = languageIndexes;
			RecognitionOptions = recognitionOptions;
			_terminologyProviderCredentialStore = terminologyProviderCredentialStore;
			_termbasesProvider = new ProjectTermbaseProvider((IProjectTermbaseConfiguration)(object)this, terminologyProviderCredentialStore);
		}

		public TermAccess GetCachedTermAccess()
		{
			return new ProjectTermAccessFactory().GetCachedTermAccess((IProjectTermbaseConfiguration)(object)this);
		}

		public void UpdateLanguageIndexes(IList<Language> projectLanguages)
		{
			_termbasesProvider.UpdateProjectLanguageIndexes(projectLanguages);
		}

		public bool IsDefaultTermbaseSpecified()
		{
			if (Termbases != null && ((ICollection<IProjectTermbase>)Termbases).Count > 0 && Termbases.GetDefaultTermbase() != null)
			{
				return Termbases.GetDefaultTermbase().Enabled;
			}
			return false;
		}

		public bool IsDefaultTermbaseConnected(TermAccess termAccess)
		{
			if (((termAccess != null) ? termAccess.Termbases : null) == null)
			{
				return false;
			}
			if (((List<ITermbaseInfo>)(object)termAccess.Termbases).Count == 0)
			{
				return false;
			}
			if (((List<ITermbaseInfo>)(object)termAccess.Termbases)[0] == null)
			{
				return false;
			}
			string name = ((List<ITermbaseInfo>)(object)termAccess.Termbases)[0].Name;
			string name2 = Termbases.GetDefaultTermbase().Name;
			return object.Equals(name, name2);
		}

		public IProjectTermbaseConfiguration Copy()
		{
			IProjectTermbases termbases = ((ICopyable<IProjectTermbases>)(object)Termbases)?.Copy();
			IProjectTermbaseServer termbaseServer = ((TermbaseServer != null && Termbases != null && Termbases.HasServerTermbase()) ? ((ICopyable<IProjectTermbaseServer>)(object)TermbaseServer).Copy() : null);
			IProjectTermbaseLanguageIndexes languageIndexes = ((ICopyable<IProjectTermbaseLanguageIndexes>)(object)LanguageIndexes)?.Copy();
			IProjectTermbaseRecognitionOptions recognitionOptions = ((ICopyable<IProjectTermbaseRecognitionOptions>)(object)RecognitionOptions)?.Copy();
			return (IProjectTermbaseConfiguration)(object)new ProjectTermbaseConfiguration(termbaseServer, termbases, languageIndexes, recognitionOptions, _terminologyProviderCredentialStore);
		}

		public override bool Equals(object obj)
		{
			IProjectTermbaseConfiguration val = (IProjectTermbaseConfiguration)((obj is IProjectTermbaseConfiguration) ? obj : null);
			if (val == null)
			{
				return false;
			}
			return Equals(val);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public bool Equals(IProjectTermbaseConfiguration other)
		{
			if (other == null)
			{
				return false;
			}
			if (!IsEqual(Termbases, other.Termbases))
			{
				return false;
			}
			if (!IsEqual(TermbaseServer, other.TermbaseServer))
			{
				return false;
			}
			if (!IsEqual(LanguageIndexes, other.LanguageIndexes))
			{
				return false;
			}
			if (!IsEqual(RecognitionOptions, other.RecognitionOptions))
			{
				return false;
			}
			return true;
		}

		private bool IsEqual(object first, object second)
		{
			return first?.Equals(second) ?? (second == null);
		}
	}
}
