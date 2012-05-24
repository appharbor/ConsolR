using Compilify.Models;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
	public interface ICSharpCompilationProvider
	{
		Compilation Compile(Post post);
	}
}
