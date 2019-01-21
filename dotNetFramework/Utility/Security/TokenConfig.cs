using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.Security {
	using System.Threading;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http;
	using System.IdentityModel.Tokens;
	using System.Security.Cryptography.X509Certificates;

	using QueZed.Utility.Configuration;

	public class TokenConfig {
		public static void Register(HttpConfiguration config) {
			config.MessageHandlers.Add(new TokenValidationHandler());
		}
	}

	internal class TokenValidationHandler : DelegatingHandler {
		private static string allowedAudience = string.Empty;
		private static string tenant = string.Empty;
		private static string trustedIssuer = string.Empty;
		private static string federationMetadataEndpoint = string.Empty;
		private static string thumbprint = string.Empty;
		static TokenValidationHandler() {
			allowedAudience = ConfigurationManager.GetSetting("AllowedAudience");
			tenant = ConfigurationManager.GetSetting("Tenant");
			trustedIssuer = ConfigurationManager.GetSetting("TrustedIssuer");
			federationMetadataEndpoint = ConfigurationManager.GetSetting("FedMetadataEndpoint");
			thumbprint = ConfigurationManager.GetSetting("Thumbprint");
		}
		private static bool tryRetrieveToken(HttpRequestMessage request, out string token) {
			token = null;
			IEnumerable<string> authorizeHeaders;
			if (!request.Headers.TryGetValues("Authorization", out authorizeHeaders) || authorizeHeaders.Count() > 1) return false;
			string bearerToken = authorizeHeaders.ElementAt(0);
			token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
			return true;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			if (request.Method == HttpMethod.Options) return base.SendAsync(request, cancellationToken);
			string token = string.Empty;
			HttpStatusCode statusCode = HttpStatusCode.NotImplemented;
			if (!tryRetrieveToken(request, out token)) {
				statusCode = HttpStatusCode.Unauthorized;
				return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(statusCode));
			}
			try {
				X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadOnly);
//				X509Certificate2 cert = store.Certificates.Find(X509FindType.FindByThumbprint, "D40F229E22DFDA28CC7FE5B4B3B29D5F902838F2", false)[0];
				X509Certificate2 cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)[0];
				store.Close();
				JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
				TokenValidationParameters validationParameters = new TokenValidationParameters() {
//					AllowedAudience = "urn:poormansactas",
					AllowedAudience = allowedAudience,
//					ValidIssuer = "https://gts-acsissuer.accesscontrol.windows.net/",
					ValidIssuer = trustedIssuer,
					SigningToken = new X509SecurityToken(cert)
				};
				Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, validationParameters);
				return base.SendAsync(request, cancellationToken);
			} catch (SecurityTokenValidationException) {
				statusCode = HttpStatusCode.Unauthorized;
			} catch (Exception) { statusCode = HttpStatusCode.InternalServerError; }
			return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(statusCode));
		}

	}


}
