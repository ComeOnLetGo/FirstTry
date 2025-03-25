using System;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseLanguageIndex : IProjectTermbaseLanguageIndex, ICopyable<IProjectTermbaseLanguageIndex>
	{
		private IProjectTermbaseIndex _termbaseIndex;

		public Language Language { get; }

		public IProjectTermbaseIndex TermbaseIndex
		{
			get
			{
				return _termbaseIndex;
			}
			set
			{
				IProjectTermbaseIndex termbaseIndex = _termbaseIndex;
				_termbaseIndex = value;
				OnTermbaseIndexChanged(termbaseIndex, value);
			}
		}

		public event EventHandler<ProjectTermbaseIndexChangedEventArgs> TermbaseIndexChanged;

		public ProjectTermbaseLanguageIndex(Language language, IProjectTermbaseIndex termbaseIndex)
		{
			Language = language;
			_termbaseIndex = termbaseIndex;
		}

		public IProjectTermbaseLanguageIndex Copy()
		{
			Language language = Language;
			IProjectTermbaseIndex termbaseIndex = ((ICopyable<IProjectTermbaseIndex>)(object)_termbaseIndex)?.Copy();
			return (IProjectTermbaseLanguageIndex)(object)new ProjectTermbaseLanguageIndex(language, termbaseIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is ProjectTermbaseLanguageIndex projectTermbaseLanguageIndex && object.Equals(Language, projectTermbaseLanguageIndex.Language))
			{
				return object.Equals(_termbaseIndex, projectTermbaseLanguageIndex._termbaseIndex);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = 97;
			num += ((Language != null) ? (171 * ((object)Language).GetHashCode()) : 0);
			return num + ((_termbaseIndex != null) ? (203 * ((object)_termbaseIndex).GetHashCode()) : 0);
		}

		private void OnTermbaseIndexChanged(IProjectTermbaseIndex oldTermbaseIndex, IProjectTermbaseIndex newTermbaseIndex)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (this.TermbaseIndexChanged != null)
			{
				this.TermbaseIndexChanged(this, new ProjectTermbaseIndexChangedEventArgs(oldTermbaseIndex, newTermbaseIndex));
			}
		}
	}
}
