using ConsolR.Core.Models;
using Roslyn.Compilers.CSharp;

namespace ConsolR.Core.Services
{
	public interface ICSharpCompilationProvider
	{
		Compilation Compile(SourceCode post);
	}
}
