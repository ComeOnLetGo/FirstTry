using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sdl.Common.Licensing.Provider.SafeNetRMS.Model;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS.Communication
{
	[ExcludeFromCodeCoverage]
	internal class ThalesRestClient : IThalesRestClient, IDisposable
	{
		private static class RestEndpoints
		{
			public const string GetProductKey = "/productKeys/";

			public const string ActivateLicense = "/activations/bulkActivate";

			public const string GeneratePermissionTicket = "/activations/aId={0}/generatePermissionTickets";

			public const string SubmitRevocationProof = "/activations/aId={0}/submitRevokeProofs";
		}

		private const string API = "api/v5";

		private readonly string _baseUrl;

		private readonly string _productKey;

		private readonly ILogger _logger;

		private readonly HttpClient _httpClient;

		private readonly JsonSerializerSettings _settings;

		public ThalesRestClient(Uri url, string productKey, ILogger logger)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			_baseUrl = url?.ToString() + "api/v5";
			_productKey = productKey;
			_logger = logger;
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64Encode("pkId:" + productKey));
			_settings = new JsonSerializerSettings
			{
				DefaultValueHandling = (DefaultValueHandling)1,
				NullValueHandling = (NullValueHandling)1
			};
		}

		public async Task<ProductKey> GetProductKeyInfoAsync()
		{
			string url = _baseUrl + "/productKeys/" + _productKey;
			GetProductKeyResponse getProductKeyResponse = JsonConvert.DeserializeObject<GetProductKeyResponse>(await (await SendRequestAsync(url, HttpMethod.Get, string.Empty, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)).Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), _settings);
			return getProductKeyResponse.ProductKey;
		}

		public async Task<LicenseActivationResponse> ActivateLicenseAsync(LicenseActivationRequest activationRequest)
		{
			string url = _baseUrl + "/activations/bulkActivate";
			string requestContent = JsonConvert.SerializeObject((object)activationRequest, (Formatting)1);
			return JsonConvert.DeserializeObject<LicenseActivationResponse>(await (await SendRequestAsync(url, HttpMethod.Post, requestContent, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)).Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), _settings);
		}

		public async Task<PermissionTicketResponse> GeneratePermissionTicketAsync(string aId)
		{
			string url = _baseUrl + $"/activations/aId={aId}/generatePermissionTickets";
			return JsonConvert.DeserializeObject<PermissionTicketResponse>(await (await SendRequestAsync(url, HttpMethod.Post, null, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)).Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), _settings);
		}

		public async Task<bool> SubmitRevocationProofAsync(string aId, SubmitRevocationRequest revocationRequest)
		{
			string url = _baseUrl + $"/activations/aId={aId}/submitRevokeProofs";
			string requestContent = JsonConvert.SerializeObject((object)revocationRequest, (Formatting)1);
			return (await SendRequestAsync(url, HttpMethod.Post, requestContent, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false)).IsSuccessStatusCode;
		}

		public void Dispose()
		{
			((HttpMessageInvoker)_httpClient).Dispose();
		}

		private async Task<HttpResponseMessage> SendRequestAsync(string url, HttpMethod method, string requestContent, CancellationToken cancellationToken)
		{
			LoggerExtensions.LogDebug(_logger, $"Calling {method} {url}", Array.Empty<object>());
			UriBuilder uriBuilder = new UriBuilder(url);
			HttpRequestMessage requestMessage = new HttpRequestMessage(method, uriBuilder.Uri);
			try
			{
				if (!string.IsNullOrWhiteSpace(requestContent))
				{
					requestMessage.Content = (HttpContent)new StringContent(requestContent);
				}
				HttpResponseMessage result = null;
				try
				{
					result = await ((HttpMessageInvoker)_httpClient).SendAsync(requestMessage, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (HttpRequestException val)
				{
					HttpRequestException val2 = val;
					LoggerExtensions.LogError(_logger, (Exception)(object)val2, "HTTP request error : ", Array.Empty<object>());
					throw new SentinelProviderException(StringResources.SafeNet_ServerConnectionError, (Exception)(object)val2);
				}
				catch (Exception ex)
				{
					LoggerExtensions.LogError(_logger, ex, "Error sending request : ", Array.Empty<object>());
				}
				if (result.IsSuccessStatusCode)
				{
					LoggerExtensions.LogDebug(_logger, $"Received successfull result from {method} {url}", Array.Empty<object>());
				}
				else
				{
					string text = await result.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
					RestApiError restApiError;
					try
					{
						restApiError = JsonConvert.DeserializeObject<RestApiError>(text, _settings);
					}
					catch (Exception ex2)
					{
						restApiError = null;
						LoggerExtensions.LogError(_logger, ex2, "Failed to deserialize error response. Response is not JSON.", Array.Empty<object>());
					}
					if (restApiError?.Response != null)
					{
						throw new SentinelProviderException(restApiError.Response.ErrorCode, restApiError.Response.Description);
					}
				}
				return result;
			}
			finally
			{
				((IDisposable)requestMessage)?.Dispose();
			}
		}

		private string Base64Encode(string textToEncode)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(textToEncode);
			return Convert.ToBase64String(bytes);
		}
	}
}
