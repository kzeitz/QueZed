using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.Security {
	using System.Threading;
	using System.Web;
	using System.Net;
	using System.Xml;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel.Security;
	using System.IdentityModel.Tokens;
	using System.IdentityModel.Metadata;

	// ToDo: Upgrade to System.IdentityModel.Tokens.JWT
	// Just updating the reference breaks things so it needs to be done carefully 
//	using Microsoft.IdentityModel.Tokens.JWT;

//	using Microsoft.WindowsAzure;

//	using System.Configuration;
	using System.Security.Claims;
	using QueZed.Utility.Configuration;

	// What's the difference between 'active' and passive federation?
	// That's a good question.  Here are some references...
	// http://stackoverflow.com/questions/2775203/active-and-passive-federation-in-wif
	// http://msdn.microsoft.com/en-us/magazine/ff872350.aspx
	// As far as we're concerned here...
	// 'active' is your app collects the the credentials
	// 'passive' is your browser collects the credentials

	// Here we provide two classes, one for use in each situation.
	// Having two classes saves us from adding a reference to Microsoft.IdentityModel.Tokens.JWT to all the 'active'
 	// users of the dll.
	// The passive case needs to inherit from JWTSecurityTokenHandler

	internal class JWTTokenHandler {
		public static bool tryRetrieveToken(HttpRequest request, out string token) {
			token = null;
			string bearerToken = request.Headers["Authorization"];
			if (string.IsNullOrWhiteSpace(bearerToken)) return false;
			// Remove the bearer token scheme prefix and return the rest as ACS token
			token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
			return true;
		}

		public static byte[] signingCertificate(string metadataURI) {
			if (metadataURI == null) throw new ArgumentNullException(metadataURI);
			using (XmlReader metadataReader = XmlReader.Create(metadataURI)) {
				MetadataSerializer serializer = new MetadataSerializer() { CertificateValidationMode = X509CertificateValidationMode.None };
				EntityDescriptor entityDescriptor = serializer.ReadMetadata(metadataReader) as EntityDescriptor;
				if (entityDescriptor != null) {
					SecurityTokenServiceDescriptor stsd = entityDescriptor.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();
					if (stsd != null) {
						X509RawDataKeyIdentifierClause clause = stsd.Keys.First().KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First();
						if (clause != null) return clause.GetX509RawData();
						throw new Exception("The SecurityTokenServiceDescriptor in the metadata does not contain a RawData signing certificate");
					}
					throw new Exception("The Federation Metadata document does not contain a SecurityTokenServiceDescriptor");
				}
				throw new Exception("Invalid Federation Metadata document");
			}
		}
	}

	// looks like this could be single static call, we'll leave this way for now.
	public class JWTTokenHandlerActive {
		private static string allowedAudience = string.Empty;
		private static string tenant = string.Empty;
		private static string federationMetadataEndpoint = string.Empty;
		static JWTTokenHandlerActive() {
			//allowedAudience = ConfigurationManager.AppSettings["AllowedAudience"];
			//tenant = ConfigurationManager.AppSettings["Tenant"];
			//federationMetadataEndpoint = ConfigurationManager.AppSettings["FedMetadataEndpoint"];
			//allowedAudience = CloudConfigurationManager.GetSetting("AllowedAudience");
			//tenant = CloudConfigurationManager.GetSetting("Tenant");
			//federationMetadataEndpoint = CloudConfigurationManager.GetSetting("FedMetadataEndpoint");
			allowedAudience = ConfigurationManager.GetSetting("AllowedAudience");
			tenant = ConfigurationManager.GetSetting("Tenant");
			federationMetadataEndpoint = ConfigurationManager.GetSetting("FedMetadataEndpoint");
		}
		public HttpStatusCode ValidatePrincipal(HttpRequest request) {
			HttpStatusCode statusCode = HttpStatusCode.Unauthorized;
			string token = string.Empty;
			if (!JWTTokenHandler.tryRetrieveToken(request, out token)) return statusCode;
			try {
				JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
				// interesting initialization syntax here...
				// Fetch the signing token from the FederationMetadata document of the tenant.
				TokenValidationParameters parameters = new TokenValidationParameters() { 
					AllowedAudience = allowedAudience, 
					ValidIssuer = tenant,
					SigningToken = new X509SecurityToken(new X509Certificate2(JWTTokenHandler.signingCertificate(federationMetadataEndpoint)))
				};
				// Set the ClaimsPrincipal returned by ValidateToken to Thread.CurrentPrincipal and HttpContext.Current.User
				HttpContext.Current.User = Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, parameters);
				statusCode = HttpStatusCode.OK;
			} catch (SecurityTokenValidationException) { statusCode = HttpStatusCode.Unauthorized;
			} catch (Exception) { statusCode = HttpStatusCode.InternalServerError; }
			return statusCode;
		}
	}
	
//	public class JWTTokenHandlerPassive : JWTSecurityTokenHandler {
	public class JWTTokenHandlerPassive : JwtSecurityTokenHandler {
		private static string allowedAudience = string.Empty;
		private static string tenant = string.Empty;
		private static string federationMetadataEndpoint = string.Empty;

		// I don't know exactly what's up here other than the standard for which claim is the 'name' changed at some point.
		//protected override string NameIdentifierClaimType(JWTSecurityToken jwt) { return ClaimTypes.GivenName; }
		//protected override string NameIdentifierClaimType(JwtSecurityToken jwt) { return ClaimTypes.Name; }

//		public override ClaimsPrincipal ValidateToken(JwtSecurityToken jwt, TokenValidationParameters validationParameters) {
		public override ClaimsPrincipal ValidateToken(JwtSecurityToken jwt) {
			if (string.IsNullOrEmpty(allowedAudience)) allowedAudience = ConfigurationManager.GetSetting("AllowedAudience");
			if (string.IsNullOrEmpty(tenant)) tenant = ConfigurationManager.GetSetting("Tenant");
			if (string.IsNullOrEmpty(federationMetadataEndpoint)) federationMetadataEndpoint = ConfigurationManager.GetSetting("FedMetadataEndpoint");
			TokenValidationParameters validationParameters = new TokenValidationParameters();
			validationParameters.AllowedAudience = allowedAudience;
			validationParameters.ValidIssuer = tenant;
			validationParameters.SigningToken = new X509SecurityToken(new X509Certificate2(JWTTokenHandler.signingCertificate(federationMetadataEndpoint)));
			ClaimsPrincipal cp = base.ValidateToken(jwt, validationParameters);
			// This call makes the raw claims information available to the web page
			// which in turn can make the information available to javaScript for subsequent AJAX
			(cp.Identity as ClaimsIdentity).AddClaim(new Claim("raw", jwt.RawData));
			return cp;
		}

	}

}
