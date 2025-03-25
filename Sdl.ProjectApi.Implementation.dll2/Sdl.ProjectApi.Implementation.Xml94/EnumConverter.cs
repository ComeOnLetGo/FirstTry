using System;
using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.Xml
{
	internal class EnumConverter<E1, E2>
	{
		private readonly Dictionary<int, E2> _convertToDictionary;

		private readonly Dictionary<int, E1> _convertFromDictionary;

		public EnumConverter()
		{
			_convertToDictionary = new Dictionary<int, E2>();
			_convertFromDictionary = new Dictionary<int, E1>();
			foreach (E1 value in Enum.GetValues(typeof(E1)))
			{
				E2 val2 = (E2)Enum.Parse(typeof(E2), value.ToString());
				_convertToDictionary[Convert.ToInt32(value)] = val2;
				_convertFromDictionary[Convert.ToInt32(val2)] = value;
			}
		}

		public E2 ConvertEnumValue(E1 value)
		{
			return _convertToDictionary[Convert.ToInt32(value)];
		}

		public E1 ConvertEnumValue(E2 value)
		{
			return _convertFromDictionary[Convert.ToInt32(value)];
		}
	}
}
