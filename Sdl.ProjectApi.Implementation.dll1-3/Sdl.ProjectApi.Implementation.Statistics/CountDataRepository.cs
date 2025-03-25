using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	internal class CountDataRepository : ICountData
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.CountData _xmlCountData;

		public int Words
		{
			get
			{
				return _xmlCountData.Words;
			}
			set
			{
				_xmlCountData.Words = value;
			}
		}

		public int Segments
		{
			get
			{
				return _xmlCountData.Segments;
			}
			set
			{
				_xmlCountData.Segments = value;
			}
		}

		public int Characters
		{
			get
			{
				return _xmlCountData.Characters;
			}
			set
			{
				_xmlCountData.Characters = value;
			}
		}

		public int Placeables
		{
			get
			{
				return _xmlCountData.Placeables;
			}
			set
			{
				_xmlCountData.Placeables = value;
			}
		}

		public int Tags
		{
			get
			{
				return _xmlCountData.Tags;
			}
			set
			{
				_xmlCountData.Tags = value;
			}
		}

		public bool IsZero
		{
			get
			{
				if (Words == 0 && Characters == 0 && Placeables == 0)
				{
					return Segments == 0;
				}
				return false;
			}
		}

		protected Sdl.ProjectApi.Implementation.Xml.CountData XmlCountData => _xmlCountData;

		public CountDataRepository(Sdl.ProjectApi.Implementation.Xml.CountData xmlCountData)
		{
			_xmlCountData = xmlCountData ?? new Sdl.ProjectApi.Implementation.Xml.CountData();
		}

		public void Increment(int characters, int words, int segments, int placeables, int tags)
		{
			_xmlCountData.Characters += characters;
			_xmlCountData.Words += words;
			_xmlCountData.Segments += segments;
			_xmlCountData.Placeables += placeables;
			_xmlCountData.Tags += tags;
		}

		public void Decrement(int characters, int words, int segments, int placeables, int tags)
		{
			_xmlCountData.Characters -= characters;
			_xmlCountData.Words -= words;
			_xmlCountData.Segments -= segments;
			_xmlCountData.Placeables -= placeables;
			_xmlCountData.Tags -= tags;
		}

		public void Increment(ICountData data)
		{
			_xmlCountData.Characters += data.Characters;
			_xmlCountData.Words += data.Words;
			_xmlCountData.Segments += data.Segments;
			_xmlCountData.Placeables += data.Placeables;
			_xmlCountData.Tags += data.Tags;
		}

		public void Decrement(ICountData data)
		{
			_xmlCountData.Characters -= data.Characters;
			_xmlCountData.Words -= data.Words;
			_xmlCountData.Segments -= data.Segments;
			_xmlCountData.Placeables -= data.Placeables;
			_xmlCountData.Tags -= data.Tags;
		}

		public void Reset()
		{
			_xmlCountData.Words = 0;
			_xmlCountData.Segments = 0;
			_xmlCountData.Characters = 0;
			_xmlCountData.Placeables = 0;
			_xmlCountData.Tags = 0;
		}

		public void Assign(ICountData other)
		{
			_xmlCountData.Words = other.Words;
			_xmlCountData.Segments = other.Segments;
			_xmlCountData.Characters = other.Characters;
			_xmlCountData.Placeables = other.Placeables;
			_xmlCountData.Tags = other.Tags;
		}

		public void Assign(WordCounts wordCounts)
		{
			_xmlCountData.Words = wordCounts.Words;
			_xmlCountData.Segments = wordCounts.Segments;
			_xmlCountData.Characters = wordCounts.Characters;
			_xmlCountData.Placeables = wordCounts.Placeables;
			_xmlCountData.Tags = wordCounts.Tags;
		}

		public override int GetHashCode()
		{
			return (Characters + Words + Placeables + Segments + Tags).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			ICountData val = (ICountData)((obj is ICountData) ? obj : null);
			if (val != null && val.Characters == Characters && val.Placeables == Placeables && val.Segments == Segments && val.Tags == Tags)
			{
				return val.Words == Words;
			}
			return false;
		}
	}
}
