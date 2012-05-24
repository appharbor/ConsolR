using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foo.Core.Model
{
	public class User : Entity
	{
		public string Name { get; set; }
		public string EmailAddress { get; set; }
	}
}
