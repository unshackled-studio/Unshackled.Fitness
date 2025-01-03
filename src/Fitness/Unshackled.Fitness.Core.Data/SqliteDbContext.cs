﻿using Microsoft.EntityFrameworkCore;
using Unshackled.Studio.Core.Client.Configuration;
using Unshackled.Studio.Core.Data;

namespace Unshackled.Fitness.Core.Data;

public class SqliteDbContext : FitnessDbContext
{
	public SqliteDbContext(DbContextOptions<SqliteDbContext> options,
		ConnectionStrings connectionStrings,
		DbConfiguration dbConfig) : base(options, connectionStrings, dbConfig) { }

	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		if (!string.IsNullOrEmpty(ConnectionStrings.DefaultDatabase))
		{
			string prefix = DbConfig.TablePrefix.EndsWith("_") ? DbConfig.TablePrefix : $"{DbConfig.TablePrefix}_";
			// connect to MS Sql Server database
			options.UseSqlite(ConnectionStrings.DefaultDatabase, o =>
			{
				o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				o.MigrationsHistoryTable($"{prefix}_EFMigrationsHistory");
			});
		}
	}
}
