using System;
using System.IO;
using Compilify.Models;

namespace Compilify.Services
{
	public class CSharpExecutor
	{
		public CSharpExecutor()
			: this(new CSharpCompilationProvider()) { }

		public CSharpExecutor(ICSharpCompilationProvider compilationProvider)
		{
			compiler = compilationProvider;
		}

		private readonly ICSharpCompilationProvider compiler;

		public ExecutionResult Execute(SourceCode sourceCode, TimeSpan timeout)
		{
			var compilation = compiler.Compile(sourceCode);

			byte[] compiledAssembly;
			using (var stream = new MemoryStream())
			{
				var emitResult = compilation.Emit(stream);

				if (!emitResult.Success)
				{
					return new ExecutionResult { Result = "[Compilation failed]" };
				}

				compiledAssembly = stream.ToArray();
			}

			var sandbox = new Sandbox(compiledAssembly);
			return sandbox.Run("EntryPoint", "Result", timeout);
		}
	}
}
