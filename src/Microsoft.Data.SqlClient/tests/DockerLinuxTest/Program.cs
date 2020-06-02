// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using static Dapper.SqlMapper;

namespace Microsoft.Data.SqlClient.DockerLinuxTest
{
    class Program
    {
        const string Server = "xxx.database.windows.net";
        const string Password = "xxx";
        const string Username = "xxx";

        private static SqlConnectionStringBuilder s_builder;

        static async Task Main()
        {
            s_builder = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                InitialCatalog = "skynet",
                Password = Password,
                UserID = Username,
            };

            await SetupAsync();

            var accounts = Enumerable.Range(0, 10000).Select(x => Guid.NewGuid());

            Console.Write("Querying....");
            var items = await DoAsync(accounts);
            Console.WriteLine($"done with {items.Count()} records");
        }

        static async Task SetupAsync()
        {
            const string keySql = @"
                IF NOT EXISTS (
		            SELECT 1
		            FROM sys.table_types o
		            WHERE o.NAME = 'UniqueIdentifierKeyList' AND o.schema_id = schema_id('dbo'))
                CREATE TYPE [dbo].[UniqueIdentifierKeyList] AS TABLE([SourceKey] [uniqueidentifier] NULL)";
    
            using var connection = new SqlConnection(s_builder.ConnectionString);

            await connection.ExecuteAsync(keySql);

            const string tableSql = @"
                IF (OBJECT_ID('[dbo].[Data]', 'U') IS NULL)
                    CREATE TABLE [dbo].[Data] ([Id] UNIQUEIDENTIFIER NOT NULL)";

            await connection.ExecuteAsync(tableSql);
        }

        static async Task<IEnumerable<Guid>> DoAsync(IEnumerable<Guid> accounts)
        {
            using var connection = new SqlConnection(s_builder.ConnectionString);

            return await connection.QueryAsync<Guid>("SELECT * FROM Data WHERE Id IN (SELECT SourceKey FROM @tblAccounts)", new
            {
                tblAccounts = accounts?.AsTableValuedParameter("dbo.UniqueIdentifierKeyList", "SourceKey"),
            });
        }
    }

    public static class DapperExtensions
    {
        public static ICustomQueryParameter AsTableValuedParameter<T>(this IEnumerable<T> values, string sqlTypeName, string columnName)
        {
            var tbl = new DataTable();
            tbl.Columns.Add(columnName);

            foreach (var key in values ?? Enumerable.Empty<T>())
            {
                var rw = tbl.NewRow();
                rw[0] = key;

                tbl.Rows.Add(rw);
            }

            return tbl.AsTableValuedParameter(sqlTypeName);
        }
    }
}
