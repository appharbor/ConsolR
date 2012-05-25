using System;

namespace Compilify.Models
{
	public class ExecutionResult
	{
		public string ConsoleOutput { get; set; }
		public TimeSpan ProcessorTime { get; set; }
		public string Result { get; set; }
		public long TotalMemoryAllocated { get; set; }
	}
}
