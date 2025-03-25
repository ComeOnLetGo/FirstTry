using System;

namespace Sdl.ProjectApi.Implementation
{
	public static class LevenshteinDistance
	{
		public static int Compute(string s, string t)
		{
			if (string.IsNullOrEmpty(s))
			{
				if (!string.IsNullOrEmpty(t))
				{
					return t.Length;
				}
				return 0;
			}
			if (string.IsNullOrEmpty(t))
			{
				return s.Length;
			}
			int length = s.Length;
			int length2 = t.Length;
			int[,] array = new int[length + 1, length2 + 1];
			int num = 0;
			while (num <= length)
			{
				array[num, 0] = num++;
			}
			int num2 = 1;
			while (num2 <= length2)
			{
				array[0, num2] = num2++;
			}
			for (int i = 1; i <= length; i++)
			{
				for (int j = 1; j <= length2; j++)
				{
					int num3 = ((t[j - 1] != s[i - 1]) ? 1 : 0);
					int val = array[i - 1, j] + 1;
					int val2 = array[i, j - 1] + 1;
					int val3 = array[i - 1, j - 1] + num3;
					array[i, j] = Math.Min(Math.Min(val, val2), val3);
				}
			}
			return array[length, length2];
		}
	}
}
