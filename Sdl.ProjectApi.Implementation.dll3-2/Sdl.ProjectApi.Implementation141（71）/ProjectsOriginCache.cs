using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Sdl.Desktop.Platform.Services;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectsOriginCache : IProjectsOriginCache
	{
		private const string LCProject = "LC project";

		private readonly ConcurrentDictionary<string, uint> _projectOrigins;

		private readonly IEventAggregator _eventAggregator;

		public ProjectsOriginCache(IEventAggregator eventAggregator)
		{
			_projectOrigins = new ConcurrentDictionary<string, uint>();
			_eventAggregator = eventAggregator;
		}

		public uint GetOrigin(string key)
		{
			if (!_projectOrigins.TryGetValue(key, out var value))
			{
				return 0u;
			}
			return value;
		}

		public void AddOrUpdateOrigin(string key)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			if (!string.IsNullOrEmpty(key) && !key.Equals("LC project"))
			{
				if (_projectOrigins.ContainsKey(key))
				{
					_projectOrigins[key]++;
					return;
				}
				_projectOrigins.TryAdd(key, 1u);
				_eventAggregator.Publish<ProjectOriginFilterChangedEvent>(new ProjectOriginFilterChangedEvent(key, (ProjectOriginFilterStatus)0));
			}
		}

		public List<string> RetrieveOrigins()
		{
			return _projectOrigins.Keys.ToList();
		}

		public void Remove(string key)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			if (_projectOrigins.TryGetValue(key, out var value))
			{
				value = _projectOrigins[key]--;
				if (_projectOrigins[key] == 0)
				{
					_projectOrigins.TryRemove(key, out value);
					_eventAggregator.Publish<ProjectOriginFilterChangedEvent>(new ProjectOriginFilterChangedEvent(key, (ProjectOriginFilterStatus)1));
				}
			}
		}

		public void Clear()
		{
			_projectOrigins.Clear();
		}
	}
}
