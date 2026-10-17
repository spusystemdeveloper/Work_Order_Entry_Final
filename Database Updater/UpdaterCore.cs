using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WorkOrderDatabaseUpdater
{
    // GO is a client separator, never sent to SQL Server. Mask literals, identifiers,
    // and nested comments first so a GO inside a module definition stays intact.
    public static class SqlBatches
    {
        public static string Mask(string sql)
        {
            var result = sql.ToCharArray();
            int state = 0, depth = 0; // normal, quote, bracket, double quote, line comment, block comment
            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i], next = i + 1 < sql.Length ? sql[i + 1] : '\0';
                if (state == 0)
                {
                    if (c == '\'') state = 1;
                    else if (c == '[') state = 2;
                    else if (c == '"') state = 3;
                    else if (c == '-' && next == '-') state = 4;
                    else if (c == '/' && next == '*') { state = 5; depth = 1; result[i++] = ' '; }
                    else continue;
                }
                else if (state == 4 && (c == '\n' || c == '\r')) { state = 0; continue; }
                else if (state == 5)
                {
                    if (c == '/' && next == '*') { depth++; result[i++] = ' '; }
                    else if (c == '*' && next == '/') { depth--; result[i++] = ' '; if (depth == 0) state = 0; }
                }
                else if ((state == 1 && c == '\'') || (state == 2 && c == ']') || (state == 3 && c == '"'))
                {
                    if (next == c) result[i++] = ' ';
                    else state = 0;
                }
                if (sql[i] != '\r' && sql[i] != '\n') result[i] = ' ';
            }
            if (state != 0 && state != 4) throw new InvalidOperationException("SQL contains an unfinished string, identifier, or comment.");
            return new string(result);
        }

        public static IList<string> Split(string sql, bool allowInternalTransactions)
        {
            var masked = Mask(sql);
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline;
            if (Regex.IsMatch(masked, @"^\s*[:!]", options) || sql.Contains("$("))
                throw new InvalidOperationException("SQLCMD directives/variables are unsupported. Select the individual packaged objects instead.");
            if (Regex.IsMatch(masked, @"\bUSE\b|\b(?:CREATE|ALTER|DROP|BACKUP|RESTORE)\s+DATABASE\b|\bRECONFIGURE\b", options))
                throw new InvalidOperationException("Scripts must stay in the selected database. USE and server/database maintenance statements are unsupported.");
            if (!allowInternalTransactions && Regex.IsMatch(masked, @"\bBEGIN\s+(?:DISTRIBUTED\s+)?TRAN(?:SACTION)?\b|\bCOMMIT\b|\bROLLBACK\b", options))
                throw new InvalidOperationException("For an added script, remove transaction commands; the updater supplies one transaction for the whole selection.");
            var batches = new List<string>();
            int start = 0;
            foreach (Match match in Regex.Matches(masked, @"^[ \t]*GO\b[^\r\n]*", options))
            {
                if (!Regex.IsMatch(match.Value, @"^\s*GO\s*$", RegexOptions.IgnoreCase))
                    throw new InvalidOperationException("Use GO on its own line. GO repeat counts are unsupported.");
                if (!string.IsNullOrWhiteSpace(masked.Substring(start, match.Index - start)))
                    batches.Add(sql.Substring(start, match.Index - start));
                // Preserve trailing comments in the next batch, including block
                // comments that continue onto another line after a separator.
                start = match.Index + Regex.Match(match.Value, @"GO", RegexOptions.IgnoreCase).Index + 2;
            }
            if (!string.IsNullOrWhiteSpace(masked.Substring(start))) batches.Add(sql.Substring(start));
            if (batches.Count == 0) throw new InvalidOperationException("The SQL script is empty.");
            return batches.AsReadOnly();
        }
    }

    public sealed class UpdateScript
    {
        public string Id { get; private set; }
        public string Path { get; private set; }
        public string Category { get; private set; }
        public string Notes { get; private set; }
        public string Sql { get; private set; }
        public string Hash { get; private set; }
        public IList<string> Batches { get; private set; }

        public static UpdateScript Load(string path, string id, string category, string notes, bool packagedMigration = false)
        {
            var bytes = File.ReadAllBytes(path);
            string sql;
            using (var stream = new MemoryStream(bytes))
            using (var reader = new StreamReader(stream, Encoding.UTF8, true)) sql = reader.ReadToEnd();
            return new UpdateScript { Id = id, Path = System.IO.Path.GetFullPath(path), Category = category,
                Notes = notes, Sql = sql, Hash = HashBytes(bytes), Batches = SqlBatches.Split(sql, packagedMigration) };
        }
        public void EnsureUnchanged()
        {
            if (HashBytes(File.ReadAllBytes(Path)) != Hash)
                throw new InvalidOperationException("A selected SQL file changed after review. Reload the package and review again: " + Id);
        }
        private static string HashBytes(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }
    }

    public static class UpdateCatalog
    {
        public static IList<UpdateScript> Load(string components, string profile)
        {
            if (profile != "GSC" && profile != "DVO" && profile != "Portable") throw new ArgumentException("Unknown branch profile.");
            var result = new List<UpdateScript>();
            string[] migrations = { "Ensure_Queueing_Compatibility_Tables", "Ensure_Work_Order_Draft_Tables",
                "Enable_WorkOrder_History", "Prepare_WorkOrder_Edit_Checks", "Improve_WorkOrder_ItemLookup",
                "Improve_WorkOrder_ItemLogLookup", "Improve_RMS_VoucherLookup" };
            foreach (var name in migrations)
                result.Add(UpdateScript.Load(System.IO.Path.Combine(components, "Migrations", name + ".sql"),
                    "Migrations/" + name + ".sql", name.Contains("Checks") || name.Contains("Lookup") ? "Index / performance" : "Migration",
                    name == "Enable_WorkOrder_History"
                        ? "Creates/updates history triggers and converts approved legacy IDENTITY history to a sequence, preserving events, IDs and permissions. Back up first; no separate history fix needed."
                        : name == "Improve_WorkOrder_ItemLogLookup"
                            ? "Optional item-log index only; trigger and business rows are unchanged. Requires a maintenance window and restore test. Stops on incompatible references or ambiguous latest timestamps."
                            : name == "Improve_RMS_VoucherLookup"
                                ? "Optional RMS voucher lookup index only; reduces Item scans that conflicted with Work Order saves in the Davao restore test. No tender IDs, triggers or business rows changed. Test each branch restore and deploy in a maintenance window."
                                : "Optional; existing tables are preserved. Review prerequisites in the SQL.", true));
            string deployment = System.IO.Path.GetFullPath(System.IO.Path.Combine(components, "Deployment"));
            string driver = profile == "Portable" ? "Deploy_PORTABLE_REQUIRED_OBJECTS.sql" : "Deploy_" + profile + "_STORE_DB.sql";
            foreach (string line in File.ReadAllLines(System.IO.Path.Combine(deployment, driver)))
            {
                if (!line.StartsWith(":r .\\Objects\\", StringComparison.OrdinalIgnoreCase)) continue;
                string relative = line.Substring(5).Trim();
                string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(deployment, relative));
                if (!path.StartsWith(deployment + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("An object path escapes the package.");
                result.Add(UpdateScript.Load(path, "Deployment/" + relative.Replace('\\', '/'),
                    System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path)),
                    "Profile: " + profile + ". Review warehouse, branch, and business-rule references before replacing an object."));
            }
            return result.AsReadOnly();
        }
    }

    public sealed class TargetInfo
    {
        public string Summary { get; internal set; }
        public Dictionary<string, string> RecordedHashes { get; internal set; }
    }
    public sealed class UpdateResult
    {
        public string RunDirectory { get; internal set; }
        public string Warning { get; internal set; }
    }

    public static class UpdateRunner
    {
        public static string ConnectionString(string server, string database, string user, string password, bool windows)
        {
            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
                throw new ArgumentException("Enter a server and target database.");
            var builder = new SqlConnectionStringBuilder { DataSource = server.Trim(), InitialCatalog = database.Trim(),
                IntegratedSecurity = windows, ConnectTimeout = 10, ApplicationName = "Work Order Database Updater", Pooling = false };
            if (!windows) { builder.UserID = user; builder.Password = password; }
            return builder.ConnectionString;
        }

        public static string DescribeError(Exception ex, string connectionString)
        {
            string message = ex.Message;
            var sql = ex as SqlException;
            if (sql != null) message = "SQL " + sql.Number + ", line " + sql.LineNumber + ": " + message;
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.Password)) message = message.Replace(builder.Password, "[redacted]");
            return message;
        }

        private static SqlCommand Command(SqlConnection connection, string sql, SqlTransaction transaction = null)
        {
            return new SqlCommand(sql, connection, transaction) { CommandTimeout = 120 };
        }
        private static void Guard(SqlConnection connection, string expected, SqlTransaction transaction = null)
        {
            using (var command = Command(connection, @"
SELECT DB_NAME(), CONVERT(int, SERVERPROPERTY('ProductMajorVersion')),
 CASE WHEN DATABASEPROPERTYEX(DB_NAME(), 'Updateability') = 'READ_WRITE' THEN 1 ELSE 0 END,
 HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'VIEW DEFINITION'),
 OBJECT_ID(N'dbo.Item',N'U'), OBJECT_ID(N'dbo.[Order]',N'U'), OBJECT_ID(N'dbo.OrderEntry',N'U');", transaction))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                string actual = reader.GetString(0);
                if (!string.Equals(actual, expected.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    new[] { "master", "model", "msdb", "tempdb" }.Contains(actual.ToLowerInvariant()))
                    throw new InvalidOperationException("Target database mismatch or system database. Nothing was applied.");
                if (reader.GetInt32(1) < 13) throw new InvalidOperationException("SQL Server 2016 or later is required.");
                if (reader.GetInt32(2) != 1) throw new InvalidOperationException("The target database is read-only.");
                if (reader.IsDBNull(3) || reader.GetInt32(3) != 1) throw new InvalidOperationException("VIEW DEFINITION permission is required to save the schema snapshot.");
                if (reader.IsDBNull(4) || reader.IsDBNull(5) || reader.IsDBNull(6))
                    throw new InvalidOperationException("Required RMS Item, Order, or OrderEntry tables are missing.");
            }
        }

        public static TargetInfo Check(string connectionString, string expected)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open(); Guard(connection, expected);
                var result = new TargetInfo { RecordedHashes = new Dictionary<string, string>(StringComparer.Ordinal),
                    Summary = connection.DataSource + " / " + connection.Database + " — target verified. SQL permissions are checked when scripts execute." };
                using (var command = Command(connection, @"
IF OBJECT_ID(N'dbo.SOD_WO_DeploymentHistory',N'U') IS NOT NULL
 SELECT ScriptId, ScriptHash FROM (SELECT ScriptId, ScriptHash,
 ROW_NUMBER() OVER(PARTITION BY ScriptId ORDER BY DeploymentId DESC) rn
 FROM dbo.SOD_WO_DeploymentHistory) h WHERE rn=1;"))
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) result.RecordedHashes[reader.GetString(0)] = reader.GetString(1);
                return result;
            }
        }

        public static UpdateResult Apply(string connectionString, string expected, IEnumerable<UpdateScript> scripts, string logRoot)
        {
            var plan = scripts.ToList();
            if (plan.Count == 0) throw new ArgumentException("Select at least one script.");
            if (plan.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != plan.Count)
                throw new ArgumentException("The plan contains duplicate script IDs.");
            foreach (var script in plan) script.EnsureUnchanged();
            string folder = System.IO.Path.Combine(logRoot, DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var log = new StringBuilder();
            var builder = new SqlConnectionStringBuilder(connectionString);
            log.AppendLine("Target: " + builder.DataSource + " / " + expected);
            log.AppendLine("Started UTC: " + DateTime.UtcNow.ToString("O"));
            for (int i = 0; i < plan.Count; i++)
            {
                log.AppendLine((i + 1) + ". " + plan[i].Id + " SHA256=" + plan[i].Hash);
                File.WriteAllText(System.IO.Path.Combine(folder, (i + 1).ToString("D3") + "-script.sql"), plan[i].Sql, Encoding.UTF8);
            }
            string logPath = System.IO.Path.Combine(folder, "result.txt");
            File.WriteAllText(logPath, log.ToString() + "Status: NOT COMMITTED\r\n", Encoding.UTF8);
            bool committed = false;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open(); Guard(connection, expected);
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = Command(connection, @"
SET XACT_ABORT ON; SET LOCK_TIMEOUT 5000;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'SOD_WO_DatabaseUpdater', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000;
IF @lock<0 THROW 52200, 'Another database update is running. Try again later.', 1;", transaction)) command.ExecuteNonQuery();
                            // Definitions/settings and index inventory are saved before the first change.
                            using (var command = Command(connection, @"
SELECT SCHEMA_NAME(o.schema_id) AS [Schema], o.name AS [Name], o.type_desc AS [Type],
 m.definition AS [Definition], m.uses_ansi_nulls AS [AnsiNulls], m.uses_quoted_identifier AS [QuotedIdentifier]
 FROM sys.sql_modules m JOIN sys.objects o ON o.object_id=m.object_id ORDER BY o.object_id;
SELECT SCHEMA_NAME(o.schema_id) AS [Schema], o.name AS [Table], i.name AS [Index], i.type_desc AS [Type],
 i.is_unique AS [Unique], i.is_disabled AS [Disabled], i.filter_definition AS [Filter],
 c.name AS [Column], ic.key_ordinal AS [KeyOrder], ic.is_included_column AS [Included], ic.is_descending_key AS [Descending]
 FROM sys.indexes i JOIN sys.objects o ON o.object_id=i.object_id
 LEFT JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
 LEFT JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
 WHERE o.is_ms_shipped=0 ORDER BY i.object_id,i.index_id,ic.index_column_id;", transaction))
                            using (var adapter = new SqlDataAdapter(command))
                            using (var snapshot = new DataSet("BeforeDeployment"))
                            {
                                adapter.Fill(snapshot); snapshot.Tables[0].TableName = "Modules"; snapshot.Tables[1].TableName = "Indexes";
                                snapshot.WriteXml(System.IO.Path.Combine(folder, "before-schema.xml"), XmlWriteMode.WriteSchema);
                            }
                            using (var command = Command(connection, @"
IF OBJECT_ID(N'dbo.SOD_WO_DeploymentHistory') IS NOT NULL AND OBJECT_ID(N'dbo.SOD_WO_DeploymentHistory',N'U') IS NULL
 THROW 52201, 'Deployment history object has an incompatible type.', 1;
IF OBJECT_ID(N'dbo.SOD_WO_DeploymentHistory',N'U') IS NULL
 CREATE TABLE dbo.SOD_WO_DeploymentHistory (
 DeploymentId bigint IDENTITY(1,1) NOT NULL PRIMARY KEY, RunId uniqueidentifier NOT NULL,
 ScriptId nvarchar(450) NOT NULL, ScriptHash char(64) NOT NULL,
 AppliedAtUtc datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(), AppliedBy nvarchar(128) NOT NULL DEFAULT ORIGINAL_LOGIN());", transaction)) command.ExecuteNonQuery();
                            Guid runId = Guid.NewGuid();
                            foreach (var script in plan)
                            {
                                int batch = 0;
                                foreach (var sql in script.Batches)
                                {
                                    log.AppendLine("Executing: " + script.Id + " batch " + (++batch));
                                    using (var command = Command(connection, sql, transaction)) command.ExecuteNonQuery();
                                    Guard(connection, expected, transaction);
                                    using (var command = Command(connection, "SELECT @@TRANCOUNT, XACT_STATE();", transaction))
                                    using (var reader = command.ExecuteReader())
                                    {
                                        reader.Read();
                                        if (Convert.ToInt32(reader.GetValue(0)) != 1 || Convert.ToInt32(reader.GetValue(1)) != 1)
                                            throw new InvalidOperationException("A script changed the updater's transaction. Stop and review its transaction commands.");
                                    }
                                }
                                using (var command = Command(connection, "INSERT dbo.SOD_WO_DeploymentHistory(RunId,ScriptId,ScriptHash) VALUES(@run,@id,@hash);", transaction))
                                {
                                    command.Parameters.Add("@run", SqlDbType.UniqueIdentifier).Value = runId;
                                    command.Parameters.Add("@id", SqlDbType.NVarChar, 450).Value = script.Id;
                                    command.Parameters.Add("@hash", SqlDbType.Char, 64).Value = script.Hash;
                                    command.ExecuteNonQuery();
                                }
                            }
                            // Fail before commit if the execution log cannot be persisted.
                            File.WriteAllText(logPath, log.ToString() + "Status: AWAITING COMMIT\r\n", Encoding.UTF8);
                            transaction.Commit(); committed = true;
                        }
                        catch { try { transaction.Rollback(); } catch (InvalidOperationException) { } catch (SqlException) { } throw; }
                    }
                }
                var result = new UpdateResult { RunDirectory = folder };
                try { File.WriteAllText(logPath, log.ToString() + "Status: COMMITTED\r\n", Encoding.UTF8); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                { result.Warning = "Database changes committed, but the final local log could not be written. Confirm using database deployment history."; }
                return result;
            }
            catch (Exception ex)
            {
                string status = committed ? "COMMITTED; follow-up failed" : "FAILED; transaction rolled back (connection loss may leave outcome uncertain; check history)";
                try { File.WriteAllText(logPath, log.ToString() + "Status: " + status + "\r\n" + DescribeError(ex, connectionString), Encoding.UTF8); }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { }
                if (committed) return new UpdateResult { RunDirectory = folder,
                    Warning = "Database changes committed. A follow-up operation failed: " + DescribeError(ex, connectionString) };
                throw new InvalidOperationException(DescribeError(ex, connectionString) + "\r\nRun files: " + folder, ex);
            }
        }
    }
}
