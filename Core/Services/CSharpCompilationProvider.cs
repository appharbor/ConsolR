using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compilify.Extensions;
using Compilify.Models;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
	public class CSharpCompilationProvider : ICSharpCompilationProvider
	{
		private const string EntryPoint =
			@"public class EntryPoint 
              {
                  public static object Result { get; set; }
                  
                  public static void Main()
                  {
                      Result = Script.Eval();
                  }
              }";

		private static readonly ReadOnlyArray<string> DefaultNamespaces =
			ReadOnlyArray<string>.CreateFrom(new[]
            {
                "System", 
                "System.IO", 
                "System.Net", 
                "System.Linq", 
                "System.Text", 
                "System.Text.RegularExpressions", 
                "System.Collections.Generic"
            });

		public Compilation Compile(Post post)
		{
			var console = SyntaxTree.ParseCompilationUnit("public static readonly StringWriter __Console = new StringWriter();",
														  options: new ParseOptions(kind: SourceCodeKind.Script));

			var entry = SyntaxTree.ParseCompilationUnit(EntryPoint);

			var prompt = SyntaxTree.ParseCompilationUnit(BuildScript(post.Content),
				fileName: "Prompt",options: new ParseOptions(kind: SourceCodeKind.Interactive))
					.RewriteWith<MissingSemicolonRewriter>();

			var editor = SyntaxTree.ParseCompilationUnit(post.Classes ?? string.Empty,
				fileName: "Editor", options: new ParseOptions(kind: SourceCodeKind.Script))
					.RewriteWith<MissingSemicolonRewriter>();

			var compilation = Compile("Untitled", new[] { entry, prompt, editor, console });

			var newPrompt = prompt.RewriteWith(new ConsoleRewriter("__Console", compilation.GetSemanticModel(prompt)));
			var newEditor = editor.RewriteWith(new ConsoleRewriter("__Console", compilation.GetSemanticModel(editor)));

			return compilation.ReplaceSyntaxTree(prompt, newPrompt)
				.ReplaceSyntaxTree(editor, newEditor);
		}

		private static Compilation Compile(string compilationName, params SyntaxTree[] syntaxTrees)
		{
			var options = new CompilationOptions(assemblyKind: AssemblyKind.ConsoleApplication, usings: DefaultNamespaces);

			var metadataReference = AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
				.Select(x => new AssemblyFileReference(x.Location));

			var compilation = Compilation.Create(compilationName, options, syntaxTrees, metadataReference);

			return compilation;
		}

		private static string BuildScript(string content)
		{
			var builder = new StringBuilder();

			builder.AppendLine("public static object Eval() {");
			builder.AppendLine("#line 1");
			builder.Append(content);
			builder.AppendLine("}");

			return builder.ToString();
		}

	}
}
