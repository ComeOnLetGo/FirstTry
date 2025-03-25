using System;
using Sdl.ProjectApi.Implementation.Statistics;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	internal class FuzzyCountData : CountDataRepository, IFuzzyCountData, ICountData
	{
		public IAnalysisBand Band { get; }

		public FuzzyCountData(IAnalysisBand band, Sdl.ProjectApi.Implementation.Xml.CountData xmlFuzzyCountData)
			: base(xmlFuzzyCountData)
		{
			Band = band ?? throw new ArgumentNullException("Band");
		}
	}
}
