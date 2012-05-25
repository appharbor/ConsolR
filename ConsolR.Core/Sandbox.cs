using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ConsolR.Core.Models;
using Roslyn.Scripting.CSharp;

namespace ConsolR.Core
{
	public sealed class Sandbox
	{
		private readonly byte[] _assemblyBytes;
		private readonly AppDomain _domain;

		public Sandbox(byte[] compiledAssemblyBytes)
		{
			_assemblyBytes = compiledAssemblyBytes;
			_domain = AppDomain.CurrentDomain;
		}

		static Sandbox()
		{
			AppDomain.MonitoringIsEnabled = true;
		}

		public ExecutionResult Run(string className, string resultProperty, TimeSpan timeout)
		{
			var task = Task<ExecutionResult>.Factory.StartNew(() => Execute(className, resultProperty));

			if (!task.Wait(timeout))
			{
				return new ExecutionResult { Result = "[Execution timed out]" };
			}

			return task.Result ?? new ExecutionResult { Result = "null" };
		}

		private ExecutionResult Execute(string className, string resultProperty)
		{
			var type = typeof(ByteCodeLoader);

			var loader = (ByteCodeLoader)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
			var unformattedResult = loader.Run(className, resultProperty, _assemblyBytes);

			var formatter = new ObjectFormatter(maxLineLength: 5120);
			return new ExecutionResult
			{
				Result = formatter.FormatObject(unformattedResult.ReturnValue),
				ConsoleOutput = unformattedResult.ConsoleOutput,
				ProcessorTime = _domain.MonitoringTotalProcessorTime,
				TotalMemoryAllocated = _domain.MonitoringTotalAllocatedMemorySize,
			};
		}

		private sealed class ByteCodeLoader : MarshalByRefObject
		{
			public ByteCodeLoader() { }

			public SandboxResult Run(string className, string resultProperty, byte[] compiledAssembly)
			{
				var assembly = Assembly.Load(compiledAssembly);
				assembly.EntryPoint.Invoke(null, new object[] { });

				var console = (StringWriter)assembly.GetType("Script").GetField("__Console").GetValue(null);

				var result = new SandboxResult
				{
					ConsoleOutput = console.ToString(),
					ReturnValue = assembly.GetType(className).GetProperty(resultProperty).GetValue(null, null)
				};

				return result;
			}
		}
	}
}
