using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Configuration;

namespace ConsolR.Hosting
{
	public class BasicAuthenticator
	{
		private const string AuthCookieName = "consolr-auth";
		private readonly static string Username = ConfigurationManager.AppSettings.Get("consolr.username");
		private readonly static string Password = ConfigurationManager.AppSettings.Get("consolr.password");

		public static bool Authenticate(HttpContextBase context)
		{
			var authCookie = context.Request.Cookies[AuthCookieName];
			var authCookieValue = authCookie == null ? null : authCookie.Value;

			var credentials = GetCredentials(context.Request.Headers);

			if (credentials != null && credentials.UserName == Username && credentials.Password == Password)
			{
				authCookie = new HttpCookie(AuthCookieName, GetAuthToken());
				context.Response.Cookies.Add(authCookie);
				return true;
			}
			else if (authCookieValue != GetAuthToken())
			{
				context.Response.StatusCode = 401;
				context.Response.AddHeader("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", "ConsolR"));

				context.Response.Flush();
				return false;
			}

			return true;
		}

		private static NetworkCredential GetCredentials(NameValueCollection headers)
		{
			var httpAuthorizationHeader = headers["Authorization"];
			if (string.IsNullOrEmpty(httpAuthorizationHeader))
			{
				return null;
			}

			string[] httpAuthorization = httpAuthorizationHeader.Split(' ');
			if (httpAuthorization.Length != 2)
			{
				return null;
			}

			if (httpAuthorization[0] != "Basic")
			{
				return null;
			}

			return ParseCredentials(httpAuthorization[1]);
		}

		private static NetworkCredential ParseCredentials(string value)
		{
			byte[] encodedBytes = Convert.FromBase64String(value);

			string unencoded = Encoding.GetEncoding("iso-8859-1").GetString(encodedBytes);
			if (unencoded.IndexOf(':') < 0)
			{
				return null;
			}

			string username = unencoded.Remove(unencoded.IndexOf(':'));
			string password = unencoded.Substring(unencoded.IndexOf(':') + 1);

			return new NetworkCredential(username, password);
		}

		private static string GetAuthToken()
		{
			using (HashAlgorithm hashAlgorithm = new SHA1Managed())
			{
				var preToken = string.Format("{0}:{1}", Username, Password);

				return GetHashed(hashAlgorithm, preToken);
			}
		}

		public static string GetHashed(HashAlgorithm hashAlgorithm, string value, Encoding encoding = null)
		{
			encoding = encoding ?? Encoding.UTF8;
			var valueBytes = encoding.GetBytes(value);

			var hashBytes = hashAlgorithm.ComputeHash(valueBytes);

			var output = new StringBuilder();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				output.Append(hashBytes[i].ToString("x2"));
			}
			return output.ToString();
		}
	}
}
