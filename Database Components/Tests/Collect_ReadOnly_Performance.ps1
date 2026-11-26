# Bounded SELECT/DMV diagnostics only. No workload, schema/data/config changes,
# cache clearing, trace/XEvent creation, KILL, or application configuration access.
param(
    [Parameter(Mandatory=$true)][string]$Server,
    [Parameter(Mandatory=$true)][string]$Database,
    [string]$SqlUser,
    [ValidateRange(2,12)][int]$Samples=6,
    [ValidateRange(2,10)][int]$IntervalSeconds=5
)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$out=Join-Path $repo ('TestResults/ReadOnly-Performance-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
[void](New-Item -ItemType Directory -Path $out)
$builder=[Data.SqlClient.SqlConnectionStringBuilder]::new()
$builder['Data Source']=$Server;$builder['Initial Catalog']=$Database;$builder['Connect Timeout']=8
$builder['Application Name']='WO_ReadOnly_Performance_Audit';$builder['ApplicationIntent']='ReadOnly';$builder['Pooling']=$false
$secret=$null;$connection=$null
$errors=[Collections.Generic.List[object]]::new();$timings=[Collections.Generic.List[object]]::new()
function Scrub-Sql([string]$value){
    $value=[regex]::Replace($value,"(?i)N?'(?:''|[^'])*'","'[literal]'")
    $value=[regex]::Replace($value,'(?s)/\*.*?\*/',' ')
    $value=[regex]::Replace($value,'(?m)--[^\r\n]*',' ')
    return [regex]::Replace($value,'\b\d+(?:\.\d+)?\b','[number]')
}
function Capture([string]$name,[string]$sql){
    $command=$connection.CreateCommand();$command.CommandTimeout=10
    $command.CommandText=$sql
    $watch=[Diagnostics.Stopwatch]::StartNew()
    try{
        $table=[Data.DataTable]::new();$adapter=[Data.SqlClient.SqlDataAdapter]::new($command)
        try{[void]$adapter.Fill($table)}finally{$adapter.Dispose()}
        foreach($row in $table.Rows){foreach($column in $table.Columns){if($column.ColumnName -eq 'SqlText' -and ![Convert]::IsDBNull($row[$column])){$row[$column]=Scrub-Sql ([string]$row[$column])}}}
        $table|Export-Csv (Join-Path $out ($name+'.csv')) -NoTypeInformation
        return ,$table
    }catch{
        $errors.Add([pscustomobject]@{Section=$name;Error=$_.Exception.GetBaseException().Message})
        Write-Output "Diagnostic section unavailable: $name" | Out-Host
        return ,([Data.DataTable]::new())
    }finally{
        $watch.Stop();$timings.Add([pscustomobject]@{Section=$name;Utc=[DateTime]::UtcNow.ToString('o');Milliseconds=[math]::Round($watch.Elapsed.TotalMilliseconds,1)})
        $command.Dispose()
    }
}
$waitSql="SELECT wait_type,waiting_tasks_count,wait_time_ms,signal_wait_time_ms FROM sys.dm_os_wait_stats WHERE waiting_tasks_count>0;"
$ioSql="SELECT DB_NAME(v.database_id) AS DatabaseName,f.name,f.type_desc,v.num_of_reads,v.num_of_writes,v.num_of_bytes_read,v.num_of_bytes_written,v.io_stall_read_ms,v.io_stall_write_ms,v.size_on_disk_bytes FROM sys.dm_io_virtual_file_stats(DB_ID(),NULL) v JOIN sys.database_files f ON f.file_id=v.file_id;"
try{
    if($SqlUser){
        $secret=Read-Host 'SQL password (not saved)' -AsSecureString;$secret.MakeReadOnly()
        $connection=[Data.SqlClient.SqlConnection]::new($builder.ConnectionString,[Data.SqlClient.SqlCredential]::new($SqlUser,$secret))
    }else{$builder['Integrated Security']=$true;$connection=[Data.SqlClient.SqlConnection]::new($builder.ConnectionString)}
    $connection.Open()
    $target=Capture 'Target' @'
SET NOCOUNT ON; SET LOCK_TIMEOUT 1000; SET DEADLOCK_PRIORITY LOW;
SELECT SYSUTCDATETIME() AS CapturedUtc,@@SERVERNAME AS ServerName,DB_NAME() AS DatabaseName,
CONVERT(nvarchar(40),SERVERPROPERTY('ProductVersion')) AS ProductVersion,
CONVERT(nvarchar(128),SERVERPROPERTY('Edition')) AS Edition,
CONNECTIONPROPERTY('local_net_address') AS ConnectedAddress,@@SPID AS AuditSessionID;
'@
    if($target.Rows.Count -ne 1 -or $target.Rows[0].DatabaseName -cne $Database){throw 'Target database verification failed.'}
    [void](Capture 'Waits-Start' $waitSql);[void](Capture 'IO-Start' $ioSql)
    [void](Capture 'Server' @'
SELECT sqlserver_start_time,cpu_count,physical_memory_kb,committed_kb,committed_target_kb,max_workers_count FROM sys.dm_os_sys_info;
'@)
    [void](Capture 'ProcessMemory' 'SELECT physical_memory_in_use_kb,locked_page_allocations_kb,process_physical_memory_low,process_virtual_memory_low FROM sys.dm_os_process_memory;')
    [void](Capture 'Configuration' "SELECT name,value_in_use FROM sys.configurations WHERE name IN('max server memory (MB)','min server memory (MB)','max degree of parallelism','cost threshold for parallelism');")
    [void](Capture 'Database' "SELECT name,compatibility_level,recovery_model_desc,is_read_committed_snapshot_on,snapshot_isolation_state_desc,is_auto_create_stats_on,is_auto_update_stats_on,is_auto_update_stats_async_on FROM sys.databases WHERE database_id=DB_ID();")
    [void](Capture 'QueryStore' 'SELECT actual_state_desc,desired_state_desc,current_storage_size_mb,max_storage_size_mb,query_capture_mode_desc FROM sys.database_query_store_options;')
    [void](Capture 'Tables' @'
SELECT t.name,SUM(CASE WHEN p.index_id IN(0,1) THEN p.row_count ELSE 0 END) AS EstimatedRows,
SUM(p.reserved_page_count)*8.0/1024 AS ReservedMB
FROM sys.tables t JOIN sys.dm_db_partition_stats p ON p.object_id=t.object_id
WHERE t.name IN('Item','Order','OrderEntry','Transaction','TransactionEntry','Queueing','QueueingItems','SOD_Item_Logs','SOD_WO_LOGS','SOD_WO_History') GROUP BY t.name;
'@)
    [void](Capture 'Indexes' @'
SELECT t.name AS TableName,i.name AS IndexName,i.index_id,i.type_desc,i.is_disabled,i.has_filter,
c.name AS ColumnName,ic.key_ordinal,ic.is_descending_key,ic.is_included_column
FROM sys.tables t JOIN sys.indexes i ON i.object_id=t.object_id
LEFT JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
LEFT JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE t.name IN('Item','Order','OrderEntry','Transaction','TransactionEntry','Queueing','QueueingItems','SOD_Item_Logs','SOD_WO_LOGS','SOD_WO_History') ORDER BY t.name,i.index_id,ic.index_column_id;
'@)
    [void](Capture 'Statistics' @'
SELECT t.name AS TableName,s.name AS StatisticName,p.last_updated,p.rows,p.rows_sampled,p.modification_counter
FROM sys.tables t JOIN sys.stats s ON s.object_id=t.object_id OUTER APPLY sys.dm_db_stats_properties(s.object_id,s.stats_id) p
WHERE t.name IN('Item','Order','OrderEntry','SOD_Item_Logs','Queueing','Transaction','TransactionEntry');
'@)
    [void](Capture 'TriggerDefinitions' @'
SELECT OBJECT_NAME(tr.parent_id) AS TableName,tr.name,tr.is_disabled,
CONVERT(varchar(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),OBJECT_DEFINITION(tr.object_id))),2) AS DefinitionHash,
OBJECT_DEFINITION(tr.object_id) AS SqlText FROM sys.triggers tr
WHERE OBJECT_NAME(tr.parent_id) IN('Order','OrderEntry','Transaction','TransactionEntry','Queueing','QueueingItems','SOD_Item_Logs');
'@)
    [void](Capture 'TriggerRuntime' @'
SELECT OBJECT_NAME(object_id) AS TriggerName,cached_time,last_execution_time,execution_count,total_worker_time,total_elapsed_time,max_elapsed_time,total_logical_reads,total_logical_writes
FROM sys.dm_exec_trigger_stats WHERE database_id=DB_ID() ORDER BY total_elapsed_time DESC;
'@)
    [void](Capture 'CachedQueries' @'
;WITH Q AS (
 SELECT qs.execution_count,qs.last_execution_time,qs.total_worker_time,qs.total_elapsed_time,qs.max_elapsed_time,qs.total_logical_reads,qs.total_logical_writes,
 CONVERT(varchar(130),qs.plan_handle,1) AS PlanHandle,OBJECT_NAME(st.objectid,DB_ID()) AS ObjectName,
 SUBSTRING(st.text,qs.statement_start_offset/2+1,(CASE qs.statement_end_offset WHEN -1 THEN DATALENGTH(st.text) ELSE qs.statement_end_offset END-qs.statement_start_offset)/2+1) AS SqlText,
 ROW_NUMBER() OVER(ORDER BY qs.total_logical_reads DESC) AS ReadRank,ROW_NUMBER() OVER(ORDER BY qs.total_elapsed_time DESC) AS TimeRank
 FROM sys.dm_exec_query_stats qs CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
 WHERE st.dbid=DB_ID() OR EXISTS(SELECT 1 FROM sys.dm_exec_plan_attributes(qs.plan_handle) a WHERE a.attribute='dbid' AND CONVERT(int,a.value)=DB_ID())
) SELECT * FROM Q WHERE ReadRank<=15 OR TimeRank<=15 ORDER BY total_elapsed_time DESC;
'@)
    [void](Capture 'DeadlockTimes' @'
;WITH B AS (SELECT CONVERT(xml,t.target_data) AS X FROM sys.dm_xe_sessions s JOIN sys.dm_xe_session_targets t ON t.event_session_address=s.address WHERE s.name='system_health' AND t.target_name='ring_buffer')
SELECT TOP(10) e.n.value('@timestamp','datetime2') AS EventUtc,
e.n.value('count(data/value/deadlock/process-list/process)','int') AS ProcessCount,
e.n.value('count(data/value/deadlock/resource-list/*)','int') AS ResourceCount
FROM B CROSS APPLY X.nodes('/RingBufferTarget/event[@name="xml_deadlock_report"]') e(n) ORDER BY EventUtc DESC;
'@)
    [void](Capture 'DeadlockProcesses' @'
;WITH B AS (SELECT CONVERT(xml,t.target_data) AS X FROM sys.dm_xe_sessions s JOIN sys.dm_xe_session_targets t ON t.event_session_address=s.address WHERE s.name='system_health' AND t.target_name='ring_buffer'),
E AS (SELECT TOP(10) e.n.value('@timestamp','datetime2') AS EventUtc,e.n.query('data/value/deadlock') AS Graph FROM B CROSS APPLY X.nodes('/RingBufferTarget/event[@name="xml_deadlock_report"]') e(n) ORDER BY EventUtc DESC)
SELECT E.EventUtc,p.n.value('@id','nvarchar(100)') AS ProcessID,p.n.value('@spid','int') AS SessionID,p.n.value('@ecid','int') AS ExecutionContextID,
p.n.value('@currentdb','int') AS DatabaseID,p.n.value('@clientapp','nvarchar(200)') AS ApplicationName,p.n.value('@hostname','nvarchar(128)') AS HostName,
p.n.value('@waitresource','nvarchar(200)') AS WaitResource,p.n.value('@lockMode','nvarchar(30)') AS LockMode,p.n.value('@isolationlevel','nvarchar(80)') AS IsolationLevel,
p.n.value('(inputbuf/text())[1]','nvarchar(4000)') AS SqlText,
E.Graph.value('(deadlock/victim-list/victimProcess/@id)[1]','nvarchar(100)') AS VictimProcessID
FROM E CROSS APPLY E.Graph.nodes('deadlock/process-list/process') p(n);
'@)
    [void](Capture 'DeadlockResources' @'
;WITH B AS (SELECT CONVERT(xml,t.target_data) AS X FROM sys.dm_xe_sessions s JOIN sys.dm_xe_session_targets t ON t.event_session_address=s.address WHERE s.name='system_health' AND t.target_name='ring_buffer'),
E AS (SELECT TOP(10) e.n.value('@timestamp','datetime2') AS EventUtc,e.n.query('data/value/deadlock') AS Graph FROM B CROSS APPLY X.nodes('/RingBufferTarget/event[@name="xml_deadlock_report"]') e(n) ORDER BY EventUtc DESC)
SELECT E.EventUtc,r.n.value('local-name(.)','nvarchar(80)') AS ResourceType,r.n.value('@objectname','nvarchar(300)') AS ObjectName,
r.n.value('@indexname','nvarchar(200)') AS IndexName,r.n.value('@dbid','int') AS DatabaseID,
r.n.value('(owner-list/owner/@id)[1]','nvarchar(100)') AS OwnerProcessID,r.n.value('(owner-list/owner/@mode)[1]','nvarchar(30)') AS OwnerMode,
r.n.value('(waiter-list/waiter/@id)[1]','nvarchar(100)') AS WaiterProcessID,r.n.value('(waiter-list/waiter/@mode)[1]','nvarchar(30)') AS WaiterMode
FROM E CROSS APPLY E.Graph.nodes('deadlock/resource-list/*') r(n);
'@)
    [void](Capture 'ItemViewDefinitions' @'
SELECT o.name,o.type_desc,OBJECT_DEFINITION(o.object_id) AS SqlText
FROM sys.objects o WHERE o.object_id=OBJECT_ID('dbo.SOD_ViewItems')
OR o.object_id IN(SELECT referenced_id FROM sys.sql_expression_dependencies WHERE referencing_id=OBJECT_ID('dbo.SOD_ViewItems')) AND o.type IN('FN','IF','TF','V');
'@)
    [void](Capture 'FunctionRuntime' @'
SELECT TOP(15) OBJECT_NAME(object_id) AS FunctionName,execution_count,last_execution_time,total_worker_time,total_elapsed_time,total_logical_reads,max_elapsed_time
FROM sys.dm_exec_function_stats WHERE database_id=DB_ID() ORDER BY total_elapsed_time DESC;
'@)
    Write-Output "Metadata collected. Sampling live activity ($Samples samples, $IntervalSeconds seconds apart)."
    for($sample=1;$sample -le $Samples;$sample++){
        [void](Capture ('Requests-'+$sample) @'
SELECT SYSUTCDATETIME() AS SampleUtc,r.session_id,r.blocking_session_id,r.status,DB_NAME(r.database_id) AS DatabaseName,
s.host_name,s.program_name,r.command,r.wait_type,r.wait_time,r.wait_resource,r.total_elapsed_time,r.cpu_time,r.reads,r.writes,r.logical_reads,r.open_transaction_count,
SUBSTRING(st.text,r.statement_start_offset/2+1,(CASE r.statement_end_offset WHEN -1 THEN DATALENGTH(st.text) ELSE r.statement_end_offset END-r.statement_start_offset)/2+1) AS SqlText
FROM sys.dm_exec_requests r JOIN sys.dm_exec_sessions s ON s.session_id=r.session_id OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) st
WHERE s.is_user_process=1 AND r.session_id<>@@SPID ORDER BY r.total_elapsed_time DESC;
'@)
        [void](Capture ('Sessions-'+$sample) @'
SELECT SYSUTCDATETIME() AS SampleUtc,session_id,status,host_name,program_name,DB_NAME(database_id) AS DatabaseName,
last_request_start_time,last_request_end_time,open_transaction_count,cpu_time,reads,writes,logical_reads,transaction_isolation_level
FROM sys.dm_exec_sessions WHERE is_user_process=1 AND session_id<>@@SPID;
'@)
        [void](Capture ('Schedulers-'+$sample) 'SELECT SYSUTCDATETIME() AS SampleUtc,scheduler_id,current_tasks_count,runnable_tasks_count,current_workers_count,active_workers_count,pending_disk_io_count FROM sys.dm_os_schedulers WHERE status=''VISIBLE ONLINE'';')
        if($sample -lt $Samples){Start-Sleep -Seconds $IntervalSeconds}
    }
    [void](Capture 'Waits-End' $waitSql);[void](Capture 'IO-End' $ioSql)
    $timings|Export-Csv (Join-Path $out 'CollectionTiming.csv') -NoTypeInformation
    if($errors.Count){$errors|Export-Csv (Join-Path $out 'UnavailableSections.csv') -NoTypeInformation}
    Write-Output "READ-ONLY collection completed: $out; unavailable sections: $($errors.Count). SQL text literals/comments were removed."
}finally{if($connection){$connection.Dispose()};if($secret){$secret.Dispose()}}
