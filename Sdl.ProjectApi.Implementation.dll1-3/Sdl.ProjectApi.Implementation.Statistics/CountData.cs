using Sdl.LanguagePlatform.TranslationMemory;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	public class CountData : ICountData
	{
		public int Words { get; set; }

		public int Segments { get; set; }

		public int Characters { get; set; }

		public int Placeables { get; set; }

		public int Tags { get; set; }

		public bool IsZero
		{
			get
			{
				if (Words == 0 && Characters == 0 && Placeables == 0 && Segments == 0)
				{
					return Tags == 0;
				}
				return false;
			}
		}

		public CountData()
		{
		}

		public CountData(int segments, int words, int characters, int placeables, int tags)
		{
			Segments = segments;
			Words = words;
			Characters = characters;
			Placeables = placeables;
			Tags = tags;
		}

		public CountData(ICountData other)
		{
			Segments = other.Segments;
			Words = other.Words;
			Characters = other.Characters;
			Placeables = other.Placeables;
			Tags = 0;
		}

		public void Reset()
		{
			Characters = 0;
			Words = 0;
			Segments = 0;
			Placeables = 0;
			Tags = 0;
		}

		public void Increment(int characters, int words, int segments, int placeables, int tags)
		{
			Characters += characters;
			Words += words;
			Segments += segments;
			Placeables += placeables;
			Tags += tags;
		}

		public void Decrement(int characters, int words, int segments, int placeables, int tags)
		{
			Characters -= characters;
			Words -= words;
			Segments -= segments;
			Placeables -= placeables;
			Tags -= tags;
		}

		public void Increment(ICountData data)
		{
			Characters += data.Characters;
			Words += data.Words;
			Segments += data.Segments;
			Placeables += data.Placeables;
			Tags += data.Tags;
		}

		public void Increment(WordCounts wordCounts)
		{
			Characters += wordCounts.Characters;
			Words += wordCounts.Words;
			Segments += wordCounts.Segments;
			Placeables += wordCounts.Placeables;
			Tags += wordCounts.Tags;
		}

		public void Decrement(ICountData data)
		{
			Characters -= data.Characters;
			Words -= data.Words;
			Segments -= data.Segments;
			Placeables -= data.Placeables;
			Tags -= data.Tags;
		}

		public void Assign(ICountData other)
		{
			Characters = other.Characters;
			Words = other.Words;
			Segments = other.Segments;
			Placeables = other.Placeables;
			Tags = other.Tags;
		}

		public void Assign(WordCounts wordCounts)
		{
			Characters = wordCounts.Characters;
			Words = wordCounts.Words;
			Segments = wordCounts.Segments;
			Placeables = wordCounts.Placeables;
			Tags = wordCounts.Tags;
		}
	}
}
