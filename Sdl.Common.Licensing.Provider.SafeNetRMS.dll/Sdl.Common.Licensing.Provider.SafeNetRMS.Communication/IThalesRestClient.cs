using System;
using System.Threading.Tasks;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Model;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Communication
{
	public interface IThalesRestClient : IDisposable
	{
		Task<LicenseActivationResponse> ActivateLicenseAsync(LicenseActivationRequest activationRequest);

		Task<PermissionTicketResponse> GeneratePermissionTicketAsync(string aId);

		Task<ProductKey> GetProductKeyInfoAsync();

		Task<bool> SubmitRevocationProofAsync(string aId, SubmitRevocationRequest revocationRequest);
	}
}
