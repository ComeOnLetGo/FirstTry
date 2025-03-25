using System;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.TermbaseApi
{
	public class ProjectTermbaseRecognitionOptions : IProjectTermbaseRecognitionOptions, ICopyable<IProjectTermbaseRecognitionOptions>
	{
		private bool _showWithNoAvailableTranslation;

		private int _minimumMatchValue;

		private int _searchDepth;

		private TermbaseSearchOrder _searchOrder;

		private bool _allowOverlappingTerms;

		private bool _enableTwoLetterTermRecognition;

		public bool ShowWithNoAvailableTranslation
		{
			get
			{
				return _showWithNoAvailableTranslation;
			}
			set
			{
				_showWithNoAvailableTranslation = value;
			}
		}

		public int MinimumMatchValue
		{
			get
			{
				return _minimumMatchValue;
			}
			set
			{
				CheckMinimumMatchValue(value);
				_minimumMatchValue = value;
			}
		}

		public int SearchDepth
		{
			get
			{
				return _searchDepth;
			}
			set
			{
				CheckSearchDepth(value);
				_searchDepth = value;
			}
		}

		public TermbaseSearchOrder SearchOrder
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _searchOrder;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_searchOrder = value;
			}
		}

		public bool DefaultShowWithNoAvailableTranslation => false;

		public int DefaultMinimumMatchValue => 70;

		public int DefaultSearchDepth => 200;

		public TermbaseSearchOrder DefaultSearchOrder => (TermbaseSearchOrder)2;

		public bool AllowOverlappingTerms
		{
			get
			{
				return _allowOverlappingTerms;
			}
			set
			{
				_allowOverlappingTerms = value;
			}
		}

		public bool EnableTwoLetterTermRecognition
		{
			get
			{
				return _enableTwoLetterTermRecognition;
			}
			set
			{
				_enableTwoLetterTermRecognition = value;
			}
		}

		public ProjectTermbaseRecognitionOptions()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			_showWithNoAvailableTranslation = DefaultShowWithNoAvailableTranslation;
			_minimumMatchValue = DefaultMinimumMatchValue;
			_searchDepth = DefaultSearchDepth;
			_searchOrder = DefaultSearchOrder;
			_allowOverlappingTerms = false;
			_enableTwoLetterTermRecognition = false;
		}

		public ProjectTermbaseRecognitionOptions(bool showWithNoAvailableTranslation, int minimumMatchValue, int searchDepth, TermbaseSearchOrder searchOrder)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			CheckMinimumMatchValue(minimumMatchValue);
			CheckSearchDepth(searchDepth);
			_showWithNoAvailableTranslation = showWithNoAvailableTranslation;
			_minimumMatchValue = minimumMatchValue;
			_searchDepth = searchDepth;
			_searchOrder = searchOrder;
		}

		public IProjectTermbaseRecognitionOptions Copy()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			return (IProjectTermbaseRecognitionOptions)(object)new ProjectTermbaseRecognitionOptions(_showWithNoAvailableTranslation, _minimumMatchValue, _searchDepth, _searchOrder)
			{
				AllowOverlappingTerms = _allowOverlappingTerms,
				EnableTwoLetterTermRecognition = _enableTwoLetterTermRecognition
			};
		}

		private void CheckMinimumMatchValue(int minimumMatchValue)
		{
			if (minimumMatchValue < 0 || minimumMatchValue > 100)
			{
				throw new ArgumentOutOfRangeException("minimumMatchValue", minimumMatchValue, "minimumMatchValue must be within the range 0 - 100");
			}
		}

		private void CheckSearchDepth(int searchDepth)
		{
			if (searchDepth < 10 || searchDepth > 999)
			{
				throw new ArgumentOutOfRangeException("searchDepth", searchDepth, "searchDepth must be within the range 10 - 999");
			}
		}

		public override bool Equals(object obj)
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			if (obj is ProjectTermbaseRecognitionOptions projectTermbaseRecognitionOptions && object.Equals(_showWithNoAvailableTranslation, projectTermbaseRecognitionOptions._showWithNoAvailableTranslation) && object.Equals(_minimumMatchValue, projectTermbaseRecognitionOptions._minimumMatchValue) && object.Equals(_searchDepth, projectTermbaseRecognitionOptions._searchDepth) && object.Equals(_searchOrder, projectTermbaseRecognitionOptions._searchOrder) && object.Equals(_allowOverlappingTerms, projectTermbaseRecognitionOptions._allowOverlappingTerms))
			{
				return object.Equals(_enableTwoLetterTermRecognition, projectTermbaseRecognitionOptions._enableTwoLetterTermRecognition);
			}
			return false;
		}

		public override int GetHashCode()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			return new { _showWithNoAvailableTranslation, _minimumMatchValue, _searchDepth, _searchOrder, _allowOverlappingTerms, _enableTwoLetterTermRecognition }.GetHashCode();
		}
	}
}
