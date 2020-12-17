using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.IO {
	using System.Diagnostics;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using QueZed.Utility.Security;

	public class HttpHeaders {
		public static string userAgent = "user-agent";
		public static string userAgentValue = "QueZed.Utility.IO";
		public static string MediaTypeJSON = "application/json";
	}

	public abstract class WebAPI : QueZed.Utility.IO.HttpHeaders {
		private Uri uri = null;
		private string api = string.Empty;
		private AssertionCredential assertionCredential = null;
		protected HttpResponseMessage httpResponseMessage = null;

		public WebAPI(Uri uri, string api, AuthenticationHeaderValue headerValue) : this(uri, api, new AssertionCredential() { Assertion = headerValue.Parameter }) { }
		public WebAPI(Uri uri, string api, AssertionCredential assertionCredential) {
			if (null == uri) throw new NullReferenceException("URI cannot be null.");
			if (null == assertionCredential) throw new NullReferenceException("Credential cannot be null.");
			if (string.IsNullOrEmpty(api)) throw new NullReferenceException("API cannot be null or empty.");
			this.uri = uri;
			this.api = api;
			this.assertionCredential = assertionCredential;
		}
		// perhaps HttpClientFactory class is the thing to use here...  some documentation on it would be nice!
		protected HttpClient HttpClient() {
			HttpClient client = new HttpClient();
			client.BaseAddress = uri;
			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AssertionCredential.OAuth2AuthorizationHeader, assertionCredential.Assertion);
			client.DefaultRequestHeaders.Add(userAgent, userAgentValue);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeJSON));
			Trace.TraceInformation("Calling target web service: {0}", client.BaseAddress);
			return client;
		}
		protected string API { get { return string.Format(@"api/{0}", api); } }
	}

	public class WebAPIPost : WebAPI {
		public WebAPIPost(Uri uri, string api, AssertionCredential assertionCredential) : base(uri, api, assertionCredential) { }
		public WebAPIPost(Uri uri, string api, AuthenticationHeaderValue headerValue) : base(uri, api, headerValue) { }
		public async Task<HttpResponseMessage> ResponseAsync(HttpContent content) { using (HttpClient client = base.HttpClient()) { return await client.PostAsync(base.API, content); } }
		public async Task<HttpResponseMessage> ResponseAsync(MultipartFormDataContent content) { using (HttpClient client = base.HttpClient()) { return await client.PostAsync(base.API, content); } }
		public async Task<HttpResponseMessage> ResponseAsync<T>(T ob) { using (HttpClient client = base.HttpClient()) { return await client.PostAsJsonAsync<T>(base.API, ob); } }
	}

	public class WebAPIGet : WebAPI {
		public WebAPIGet(Uri uri, string api, AssertionCredential assertionCredential) : base(uri, api, assertionCredential) { }
		public WebAPIGet(Uri uri, string api, AuthenticationHeaderValue headerValue) : base(uri, api, headerValue) { }
		public async Task<HttpResponseMessage> ResponseAsync() { using (HttpClient client = base.HttpClient()) { return await client.GetAsync(base.API); } }
	}

	public class WebAPIPut : WebAPI {
		public WebAPIPut(Uri uri, string api, AssertionCredential assertionCredential) : base(uri, api, assertionCredential) { }
		public WebAPIPut(Uri uri, string api, AuthenticationHeaderValue headerValue) : base(uri, api, headerValue) { }
		public async Task<HttpResponseMessage> ResponseAsync(HttpContent content) { using (HttpClient client = base.HttpClient()) { return await client.PutAsync(base.API, content); } }
		public async Task<HttpResponseMessage> ResponseAsync(MultipartFormDataContent content) { using (HttpClient client = base.HttpClient()) { return await client.PutAsync(base.API, content); } }
		public async Task<HttpResponseMessage> ResponseAsync<T>(T ob) { using (HttpClient client = base.HttpClient()) { return await client.PutAsJsonAsync<T>(base.API, ob); } }
	}

	public class WebAPIDelete : WebAPI {
		public WebAPIDelete(Uri uri, string api, AssertionCredential assertionCredential) : base(uri, api, assertionCredential) { }
		public WebAPIDelete(Uri uri, string api, AuthenticationHeaderValue headerValue) : base(uri, api, headerValue) { }
		public async Task<HttpResponseMessage> ResponseAsync() { using (HttpClient client = base.HttpClient()) { return await client.DeleteAsync(base.API); } }
	}

	public class HTTPGet : QueZed.Utility.IO.HttpHeaders {
		private Uri uri = null;
		public HTTPGet(Uri uri) { this.uri = uri; }
		protected HttpClient HttpClient() {
			HttpClient client = new HttpClient();
			client.BaseAddress = uri;
			client.DefaultRequestHeaders.Add(userAgent, userAgentValue);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeJSON));
			Trace.TraceInformation("Calling target web service: {0}", client.BaseAddress);
			return client;
		}
		public async Task<HttpResponseMessage> ResponseAsync() { using (HttpClient client = HttpClient()) { return await client.GetAsync(uri); } }
	}
}

