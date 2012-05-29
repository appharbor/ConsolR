using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using ConsolR.Core.Models;
using ConsolR.Core.Services;
using Newtonsoft.Json;
using SignalR;
using SignalR.Hosting;

namespace ConsolR.Hosting
{
	public class ExecuteEndPoint : PersistentConnection
	{
		public static TimeSpan ExecutionTimeout;
		private static readonly CSharpExecutor Executer = new CSharpExecutor();

		static ExecuteEndPoint()
		{
			int timeout;
			if (!int.TryParse(ConfigurationManager.AppSettings["ConsolR.ExecutionTimeout"], out timeout))
			{
				timeout = 30;
			};
			ExecutionTimeout = TimeSpan.FromSeconds(timeout);
		}

		public override Task ProcessRequestAsync(HostContext context)
		{
			var httpContext = (HttpContextWrapper)context.Items["System.Web.HttpContext"];
			BasicAuthenticator.Authenticate(httpContext);

			return base.ProcessRequestAsync(context);
		}

		protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
		{
			var sourceCode = JsonConvert.DeserializeObject<SourceCode>(data);
			var result = Executer.Execute(sourceCode, ExecutionTimeout);

			return Connection.Send(connectionId, new
			{
				status = "ok",
				data = HttpUtility.HtmlEncode(result.GetResultString()),
			});
		}
	}
}
