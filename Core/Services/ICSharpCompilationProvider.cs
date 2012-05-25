using ConsolR.Models;
using Roslyn.Compilers.CSharp;

namespace ConsolR.Services
{
	public interface ICSharpCompilationProvider
	{
		Compilation Compile(SourceCode post);
	}
}
