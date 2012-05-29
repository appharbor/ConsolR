using System;
using System.Text;
using ConsolR.Core.Extensions;

namespace ConsolR.Core.Models
{
	public class ExecutionResult
	{
		public string ConsoleOutput { get; set; }
		public TimeSpan ProcessorTime { get; set; }
		public string Result { get; set; }
		public long TotalMemoryAllocated { get; set; }

		public string GetResultString()
		{
			var builder = new StringBuilder();

			if (!string.IsNullOrEmpty(ConsoleOutput))
			{
				builder.AppendLine(ConsoleOutput);
			}

			builder.AppendLine(Result);

			builder.AppendLine();
			builder.AppendFormat("CPU Time: {0}" + Environment.NewLine, ProcessorTime);

			builder.AppendFormat("Bytes Allocated: {0}" + Environment.NewLine,
				TotalMemoryAllocated.ToByteSizeString());

			return builder.ToString();

		}
	}
}
