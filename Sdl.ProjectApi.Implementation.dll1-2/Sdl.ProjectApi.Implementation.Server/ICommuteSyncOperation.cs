namespace Sdl.ProjectApi.Implementation.Server
{
	public interface ICommuteSyncOperation
	{
		string Description { get; }

		bool IsFullProjectUpdate { get; }

		bool ShouldExecute();

		void Execute();
	}
}
