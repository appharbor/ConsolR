using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Compilify.Models;
using Compilify.Services;
using Roslyn.Scripting.CSharp;

namespace Compilify
{
	public sealed class Sandbox : IDisposable
	{
		private readonly byte[] _assemblyBytes;
		private bool _disposed;
		private readonly AppDomain _domain;

		public Sandbox(string name, byte[] compiledAssemblyBytes)
		{
			_assemblyBytes = compiledAssemblyBytes;

			var evidence = new Evidence();
			evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

			var permissions = SecurityManager.GetStandardSandbox(evidence);
			permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
			permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));

			var setup = new AppDomainSetup
			{
				ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				ApplicationName = name,
				DisallowBindingRedirects = true,
				DisallowCodeDownload = true,
				DisallowPublisherPolicy = true
			};

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

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
			}
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
