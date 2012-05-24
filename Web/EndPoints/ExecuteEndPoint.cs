using System;
using System.Configuration;
using System.Threading.Tasks;
using Compilify.Models;
using Compilify.Services;
using Newtonsoft.Json;
using SignalR;
using SignalR.Hosting;

namespace Compilify.Web.EndPoints
{
	public class ExecuteEndPoint : PersistentConnection
	{
		static ExecuteEndPoint()
		{
			int timeout;
			if (!int.TryParse(ConfigurationManager.AppSettings["Compilify.ExecutionTimeout"], out timeout))
			{
				timeout = DefaultExecutionTimeout;
			}

			ExecutionTimeout = TimeSpan.FromSeconds(timeout);
		}

		private const int DefaultExecutionTimeout = 30;

		private static readonly TimeSpan ExecutionTimeout;
		private static readonly CSharpExecutor Executer = new CSharpExecutor();

		protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
		{
			var post = JsonConvert.DeserializeObject<Post>(data);

			var result = Executer.Execute(post);

			return Connection.Send(connectionId, new { status = "ok", data = result.Result });
		}
	}
}