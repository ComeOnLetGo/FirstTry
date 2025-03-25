using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Model
{
	public class LicenseActivationRequest
	{
		[JsonProperty("bulkActivation")]
		public BulkActivation BulkActivation { get; set; }

		public LicenseActivationRequest()
		{
		}

		public LicenseActivationRequest(string pkId, int activationQuantity, ActivationAttributes activationattributes)
		{
			BulkActivation = new BulkActivation
			{
				ActivationProductKeys = new ActivationProductKeys
				{
					ActivationProductKey = new List<BulkActivationProductKey>
					{
						new BulkActivationProductKey
						{
							PkId = pkId,
							ActivationQuantity = activationQuantity
						}
					}
				},
				ActivationAttributes = activationattributes
			};
		}
	}
}
