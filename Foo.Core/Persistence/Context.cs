using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Foo.Core.Model;

namespace Foo.Core.Persistence
{
	public class Context : DbContext
	{
		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, Configuration>());
		}
	}
}
