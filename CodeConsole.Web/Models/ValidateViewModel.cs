using Newtonsoft.Json;
using Roslyn.Compilers;

namespace ConsolR.Web.Models
{
	[JsonObject]
	public class EditorError
	{
		public FileLinePositionSpan Location { get; set; }
		public string Message { get; set; }
	}

	[JsonObject]
	public class EditorLocation
	{
		[JsonProperty("line")]
		public int Line { get; set; }

		[JsonProperty("ch")]
		public int Character { get; set; }
	}
}