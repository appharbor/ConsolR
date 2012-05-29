using System.IO;
using System.Web.Hosting;
using SignalR;

namespace ConsolR.Hosting
{
	public static class Helper
	{
		public static string ReadFile(string filename)
		{
			return File.ReadAllText(HostingEnvironment.MapPath(string.Format("~/{0}", filename)));
		}

		public static void Log(string input)
		{
			var endpoint = GlobalHost.ConnectionManager.GetConnectionContext<ExecuteEndPoint>().Connection;
			var logMessage = new LogMessage(input);

			endpoint.Broadcast(logMessage);
		}

		private class LogMessage
		{
			public LogMessage(string message)
			{
				data = message;
			}

			public string status { get { return "ok"; } }
			public string data { get; private set; }
		}
	}
}
