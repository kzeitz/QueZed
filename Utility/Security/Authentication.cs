using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.Security {
	using System.Diagnostics;
	using System.Security.Principal;
	using Azure = Microsoft.WindowsAzure.ActiveDirectory.Authentication;
//	using Azure = Microsoft.IdentityModel.Clients.ActiveDirectory;  ToDo

	public static partial class Extensions {
		public static Azure.KerberosCredential KerberosCredential(WindowsIdentity wi) { Tuple<string, string> domainUsername = parseDownLevelLogonName(wi.Name); return new Azure.KerberosCredential(domainUsername.Item2); }
		public static Azure.UsernamePasswordCredential UsernamePasswordCredential(string un, string pw) { Tuple<string, string> domainUsername = parseUsername(un); return new Azure.UsernamePasswordCredential(domainUsername.Item2, string.Format("{0}@{1}", domainUsername.Item1, domainUsername.Item2), pw); }
		public static bool UsernamePasswordEntered(this Azure.UsernamePasswordCredential upc) { return !string.IsNullOrEmpty(upc.Name) || !string.IsNullOrEmpty(upc.Password); }
//		public static string Oauth2AuthorizationHeader(this AssertionCredential ac) { return "Bearer"; }
		public static bool Expired(this Azure.AssertionCredential ac, TimeSpan skew) { DateTime now = DateTime.UtcNow; return ac.Expires < now.Subtract(skew) || ac.Created > now.Add(skew); }
		public static AssertionCredential ToAssertion(this Azure.AssertionCredential ac) { return new AssertionCredential() { Assertion = ac.Assertion }; }
		private static Tuple<string, string> parseUsername(string username) { Tuple<string, string> domainUsername = null; if (username.Contains('\\')) domainUsername = parseDownLevelLogonName(username); if (username.Contains('@')) domainUsername = parseUserPrincipalName(username); return domainUsername; }
		private static Tuple<string, string> parseUserPrincipalName(string userPrincipalName) { string[] sa = userPrincipalName.Split(new char[] { '@' }, 2); return new Tuple<string, string>(sa[0], sa[1]);}
		private static Tuple<string, string> parseDownLevelLogonName(string downLevelLogonName) { string[] sa = downLevelLogonName.Split(new char[] { '\\' }, 2); return new Tuple<string, string>(sa[1], sa[0]); }
	}

	public class AssertionCredential {
		public const string OAuth2AuthorizationHeader = "Bearer";
		public string Assertion { get; set; }
	}

	public class Authentication {
		private Azure.AuthenticationContext authenticationContext = null;
		private Azure.Credential credential = null;
		private Azure.AssertionCredential assertionCredential = null;
		private TimeSpan skew = new TimeSpan(0, 0, 30);
		public Authentication(string tenant, string username, string password, Func<string> passwordDelegate = null, TimeSpan? skew = null) {
			authenticationContext = new Azure.AuthenticationContext(tenant);
			credential = Extensions.KerberosCredential(WindowsIdentity.GetCurrent());
			if (!string.IsNullOrWhiteSpace(username) && string.IsNullOrEmpty(password) && passwordDelegate != null) password = passwordDelegate();
			if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)) credential = Extensions.UsernamePasswordCredential(username, password);
			if (skew != null) this.skew = skew.Value;
		}
		public TimeSpan Skew { get { return skew; } }
		public async Task<Azure.AssertionCredential> Token(string serviceRealm) {
			if (null == assertionCredential || assertionCredential.Expired(Skew)) assertionCredential = await token(serviceRealm);
			return assertionCredential;
		}

		private Task<Azure.AssertionCredential> token(string serviceRealm) {
			Trace.TraceInformation("Request Token");
			Azure.AssertionCredential assertionCredential = null;
			IList<Azure.IdentityProviderDescriptor> idpdList = authenticationContext.GetProviders(serviceRealm);
			foreach (Azure.IdentityProviderDescriptor idpd in idpdList) {
				foreach (string emailAddressSuffix in idpd.EmailAddressSuffixes) {
					if (string.Compare(credential.Resource, emailAddressSuffix, StringComparison.OrdinalIgnoreCase) == 0) {
						// Invoke AuthenticationContext.AcquireToken to obtain an AssertionCredential to access the service. 
						// It will use previously-created KerberosCredential or UsernamePasswordCredential to authenticate with the selected identity provider.
						Trace.TraceInformation("Using identity provider: {0}", idpd.Name);
						if (credential is Azure.UsernamePasswordCredential) Trace.TraceInformation("Using username-password credentials: {0}", ((Azure.UsernamePasswordCredential)credential).Name);
						if (credential is Azure.KerberosCredential) Trace.TraceInformation("Using Kerberos credentials: {0}", WindowsIdentity.GetCurrent().Name);
						assertionCredential = authenticationContext.AcquireToken(serviceRealm, credential);
						Trace.TraceInformation("Received token result from: {0}", assertionCredential.Resource);
						return Task.FromResult<Azure.AssertionCredential>(assertionCredential);
					}
				}
			}
			throw new InvalidOperationException(string.Format("[{0}] is not a supported domain for any identity providers on the target service realm : {1}", credential.Resource, serviceRealm));
		}
	}
}
