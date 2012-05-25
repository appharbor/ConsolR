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
		public static TimeSpan ExecutionTimeout;
		private static readonly CSharpExecutor Executer = new CSharpExecutor();

		static ExecuteEndPoint()
		{
			var timeout = int.Parse(ConfigurationManager.AppSettings["Compilify.ExecutionTimeout"]);
			ExecutionTimeout = TimeSpan.FromSeconds(timeout);
		}

		protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
		{
			var sourceCode = JsonConvert.DeserializeObject<SourceCode>(data);
			var result = Executer.Execute(sourceCode, ExecutionTimeout);

			return Connection.Send(connectionId, new 
			{
				status = "ok",
				data = result.Result,
			});
		}
	}
}