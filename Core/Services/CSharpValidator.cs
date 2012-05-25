using System.Collections.Generic;
using System.IO;
using ConsolR.Models;
using Roslyn.Compilers.Common;

namespace ConsolR.Services
{
	public class CSharpValidator
	{
		public CSharpValidator(ICSharpCompilationProvider compilationProvider)
		{
			compiler = compilationProvider;
		}

		private readonly ICSharpCompilationProvider compiler;

		public IEnumerable<IDiagnostic> GetCompilationErrors(SourceCode post)
		{
			using (var stream = new MemoryStream())
			{
				var result = compiler.Compile(post).Emit(stream);
				return result.Diagnostics;
			}
		}
	}
}