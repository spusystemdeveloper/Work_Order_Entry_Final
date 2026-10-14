# Exact local incident recovery. Defaults to a rollback rehearsal.
# A verified SQL Server backup is mandatory even for the rehearsal.
param([Parameter(Mandatory=$true)][string]$BackupPath,[switch]$Commit)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$directory=Join-Path $repo ('TestResults\LocalIdentityRecovery-'+[DateTime]::Now.ToString('yyyyMMdd-HHmmss')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $directory
$connection=[Data.SqlClient.SqlConnection]::new('Data Source=localhost;Initial Catalog=GSC_STORE_DB;Integrated Security=True;Pooling=False;Application Name=WorkOrder authorized local identity recovery')
$transaction=$null
function Execute([string]$sql,$tx=$null,[int]$timeout=90) {
    $cmd=$connection.CreateCommand()
    try {$cmd.CommandText=$sql;$cmd.Transaction=$tx;$cmd.CommandTimeout=$timeout;$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
}
function Scalar([string]$sql,$tx=$null) {
    $cmd=$connection.CreateCommand()
    try {$cmd.CommandText=$sql;$cmd.Transaction=$tx;$cmd.CommandTimeout=90;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()}
}
function Snapshot([string]$name,$tx=$null) {
    $cmd=$connection.CreateCommand();$cmd.Transaction=$tx;$cmd.CommandTimeout=90
    $cmd.CommandText=@'
SELECT * FROM dbo.[Transaction] WHERE TransactionNumber IN(10128,289655) ORDER BY TransactionNumber;
SELECT * FROM dbo.TransactionEntry WHERE ID IN(957676,957677) ORDER BY ID;
SELECT * FROM dbo.TenderEntry WHERE ID=390053;
SELECT * FROM dbo.AccountReceivable WHERE ID=133523;
SELECT * FROM dbo.OrderHistory WHERE ID=268282;
SELECT * FROM dbo.TaxEntry WHERE ID IN(957521,957522) ORDER BY ID;
SELECT * FROM dbo.Item WHERE ID IN(59622,1250) ORDER BY ID;
SELECT * FROM dbo.SOD_WO_History ORDER BY EventID;
SELECT name,is_disabled,OBJECT_DEFINITION(object_id) Definition FROM sys.triggers WHERE name LIKE 'SOD_trg_WO_History_%' ORDER BY name;
SELECT * FROM dbo.SOD_Item_Logs WHERE ReferenceID IN('10128','289655') AND [Type]='Sales Transaction Logs'
 AND ((ItemID=59622 AND [Date]='20261014 14:12:16.650') OR(ItemID=1250 AND [Date]='20261014 14:12:16.667')) ORDER BY ItemID;
'@
    $adapter=[Data.SqlClient.SqlDataAdapter]::new($cmd);$data=[Data.DataSet]::new($name)
    try {$null=$adapter.Fill($data);$data.WriteXml((Join-Path $directory ($name+'.xml')),[Data.XmlWriteMode]::WriteSchema);return $data}finally{$adapter.Dispose();$cmd.Dispose()}
}
try {
    $connection.Open()
    $verify=$connection.CreateCommand()
    try {
        $verify.CommandTimeout=600
        $verify.CommandText='RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM;'
        $null=$verify.Parameters.Add('@path',[Data.SqlDbType]::NVarChar,4000);$verify.Parameters['@path'].Value=$BackupPath
        $null=$verify.ExecuteNonQuery()
    }finally{$verify.Dispose()}
    $backupInfo=$connection.CreateCommand()
    try {
        $backupInfo.CommandText=@'
SELECT COUNT(*) FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON m.media_set_id=b.media_set_id
WHERE b.database_name='GSC_STORE_DB' AND b.type='D' AND b.is_copy_only=1 AND b.has_backup_checksums=1
 AND b.backup_finish_date>=CONVERT(date,GETDATE()) AND m.physical_device_name=@path;
'@
        $null=$backupInfo.Parameters.Add('@path',[Data.SqlDbType]::NVarChar,4000);$backupInfo.Parameters['@path'].Value=$BackupPath
        if($backupInfo.ExecuteScalar() -lt 1){throw 'A verified copy-only GSC full backup from today is required.'}
    }finally{$backupInfo.Dispose()}
    $before=Snapshot 'Before'
    $transaction=$connection.BeginTransaction()
    Execute 'SET LOCK_TIMEOUT 5000;' $transaction
    Execute ([IO.File]::ReadAllText((Join-Path $repo 'Database Components\Migrations\Fix_WorkOrder_History_Identity.sql'))) $transaction
    Execute ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Repair_GSC_Transaction_289655.sql'))) $transaction
    Execute 'SAVE TRANSACTION IdentityProbe;' $transaction
    Execute "IF OBJECT_ID(N'dbo.SOD_IdentityRecovery_Probe') IS NOT NULL THROW 52520, 'Recovery probe name is already in use.', 1;CREATE TABLE dbo.SOD_IdentityRecovery_Probe(ID int IDENTITY(800000,1));" $transaction
    Execute "CREATE TRIGGER dbo.SOD_IdentityRecovery_ProbeTrigger ON dbo.SOD_IdentityRecovery_Probe AFTER INSERT AS BEGIN SET NOCOUNT ON;INSERT dbo.SOD_WO_History(OrderID,EventName,Actor,Terminal,Details) VALUES(283845,N'Rollback identity probe',N'Recovery test',N'Local',N'No persistent event');END;" $transaction
    $value=Scalar 'INSERT dbo.SOD_IdentityRecovery_Probe DEFAULT VALUES;SELECT CONVERT(int,@@IDENTITY);' $transaction
    if($value -ne 800000){throw 'RMS identity compatibility probe failed'}
    Execute 'DROP TRIGGER dbo.SOD_IdentityRecovery_ProbeTrigger;DROP TABLE dbo.SOD_IdentityRecovery_Probe;' $transaction
    Execute 'ROLLBACK TRANSACTION IdentityProbe;' $transaction
    if((Scalar "SELECT COUNT(*) FROM sys.objects WHERE name IN(N'SOD_IdentityRecovery_Probe',N'SOD_IdentityRecovery_ProbeTrigger')" $transaction) -ne 0){throw 'Temporary identity probe objects must be absent before committing'}
    'Local RMS identity probe passed; its temporary audit event was rolled back.'
    # Existing audit records and trigger definitions must remain identical.
    $after=Snapshot 'Prepared' $transaction
    foreach($tableNumber in @(7,8)) {
        if($before.Tables[$tableNumber].Rows.Count -ne $after.Tables[$tableNumber].Rows.Count){throw 'History or trigger count changed'}
        for($r=0;$r -lt $before.Tables[$tableNumber].Rows.Count;$r++) {
            for($c=0;$c -lt $before.Tables[$tableNumber].Columns.Count;$c++) {
                if(-not [object]::Equals($before.Tables[$tableNumber].Rows[$r][$c],$after.Tables[$tableNumber].Rows[$r][$c])){throw 'An existing audit record or trigger definition changed'}
            }
        }
    }
    if($Commit){$transaction.Commit();'COMMITTED: history compatibility and exact local transaction links repaired.'}
    else {$transaction.Rollback();'REHEARSAL PASSED: schema conversion and full link repair verified, then rolled back.'}
    $transaction.Dispose();$transaction=$null
    $final=Snapshot 'After'
    if(-not $Commit) {
        foreach($n in 0..9) {
            # Compare typed row values, including original rowversions.
            if($before.Tables[$n].Rows.Count -ne $final.Tables[$n].Rows.Count){throw 'Rehearsal rollback row count differs'}
            for($r=0;$r -lt $before.Tables[$n].Rows.Count;$r++){for($c=0;$c -lt $before.Tables[$n].Columns.Count;$c++){
                $a=$before.Tables[$n].Rows[$r][$c];$b=$final.Tables[$n].Rows[$r][$c]
                if($a -is [byte[]]){$equal=[Convert]::ToBase64String($a) -eq [Convert]::ToBase64String($b)}else{$equal=[object]::Equals($a,$b)}
                if(-not $equal){throw 'Rehearsal rollback did not preserve a captured field'}
            }}
        }
        'Rollback verification passed for every captured field.'
    }
    @("Backup verified: $BackupPath","Mode: $(if($Commit){'Committed'}else{'Rehearsal rolled back'})",'Target: localhost / GSC_STORE_DB / transaction 289655 / order 283845') | Set-Content -LiteralPath (Join-Path $directory 'Result.txt')
    "Recovery snapshots: $directory"
} finally {
    if($transaction){try{if($transaction.Connection){$transaction.Rollback()}}finally{$transaction.Dispose()}}
    $connection.Dispose()
}
