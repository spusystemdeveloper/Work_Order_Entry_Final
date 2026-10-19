# Owned synthetic LocalDB only. Exercises the compiled preflight and atomic
# commitment method at the allocation boundary, not the full prompted UI save.
$ErrorActionPreference='Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Data,System.Data.Linq,System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8)
$owned='WO_Prevalidate_'+$suffix
$server='(localdb)\'+$owned
$out=Join-Path $repo ('TestResults/Preallocation-'+$suffix)
[void](New-Item -ItemType Directory -Path $out)
$app=Join-Path $repo 'Work Order Entry/bin/Debug/Work Order Entry_V8.exe'
$configHash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$assembly=[Reflection.Assembly]::LoadFrom($app)
$instance=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$static=[Reflection.BindingFlags]'Public,NonPublic,Static'
$module=$assembly.GetType('WorkOrderEntry.ModConnectDB',$true)
# Legacy initializers construct closed contexts; no application Load is invoked.
foreach($name in @('db','dbnew','dbInitial')) {
    $field=$module.GetField($name,$static);$old=$field.GetValue($null)
    if($old){if($old.Connection.State -ne 'Closed'){throw 'Unexpected open application connection'};$old.Dispose()}
    $field.SetValue($null,$null)
}
$connection=$null;$master=$null;$form=$null;$created=$false;$checks=0
$localdb=(Get-Command SqlLocalDB.exe).Source
$cs="Server=$server;Database=$owned;Integrated Security=True;Pooling=False;Application Name=WO_Prevalidate_$suffix;"
$target=[Data.SqlClient.SqlConnectionStringBuilder]::new($cs)
if($target.DataSource -notmatch '^\(localdb\)\\WO_Prevalidate_[a-f0-9]{8}$' -or $target.InitialCatalog -ne $owned){throw 'Owned fixture endpoint required'}
function Sql([string]$text){$cmd=$connection.CreateCommand();try{$cmd.CommandText=$text;$cmd.CommandTimeout=20;[void]$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}
function Scalar([string]$text){$cmd=$connection.CreateCommand();try{$cmd.CommandText=$text;$cmd.CommandTimeout=20;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()}}
function Check($ok,[string]$name){if(!$ok){throw "FAILED: $name"};$script:checks++;Write-Output "Passed: $name"}
function Control([string]$name){return $form.GetType().GetProperty($name,$instance).GetValue($form,$null)}
function Map {
    $map=[Collections.Generic.Dictionary[int,WorkOrderEntry.Item]]::new()
    foreach($id in @(1,2)){
        $item=[WorkOrderEntry.Item]::new();$item.ID=$id
        $item.ItemType=[int16](Scalar "SELECT ItemType FROM dbo.Item WHERE ID=$id;")
        $item.QuantityCommitted=[double](Scalar "SELECT QuantityCommitted FROM dbo.Item WHERE ID=$id;")
        $map.Add($id,$item)
    }
    return ,$map
}
function Rows([int]$count){
    $grid.Rows.Clear()
    for($i=0;$i -lt $count;$i++){
        $index=$grid.Rows.Add();$row=$grid.Rows[$index]
        foreach($cell in $row.Cells){if($cell -is [Windows.Forms.DataGridViewCheckBoxCell]){$cell.Value=$false}else{$cell.Value=0}}
        $row.Cells[0].Value='FIXTURE';$row.Cells[1].Value='Synthetic item'
        $row.Cells[2].Value=[double]1.25;$row.Cells[3].Value=[decimal]112.125
        $row.Cells[5].Value=[double]140.15625;$row.Cells[9].Value=[decimal]80
        $row.Cells[11].Value=1;$row.Cells[12].Value=[decimal]120.5
        $row.Cells[13].Value='Synthetic item';$row.Cells[14].Value='Fixture'
        $row.Cells[17].Value=$false;$row.Cells[18].Value=0;$row.Cells['Taxable'].Value=1
    }
}
function Attempt([bool]$preflight,[bool]$race=$false){
    $map=Map
    $context=[WorkOrderEntry.ItemLookUpDataContext]::new($cs)
    $context.Connection.Open();$tx=$context.Connection.BeginTransaction();$context.Transaction=$tx
    $errorType='';$message='';$status='Committed'
    try{
        if($preflight){$form.GetType().GetMethod('ValidateNewOrderRows',$instance).Invoke($form,[object[]]@($map))|Out-Null}
        # Simulate another committed writer after the read-only check. No store is used.
        if($race){Sql 'UPDATE dbo.Item SET QuantityCommitted=-20 WHERE ID=1;'}
        for($start=0;$start -lt $grid.Rows.Count;$start+=6){
            $last=[Math]::Min($start+5,$grid.Rows.Count-1)
            $total=[double]0
            for($i=$start;$i -le $last;$i++){$total += [Convert]::ToDouble($grid.Rows[$i].Cells[5].Value)}
            $kind=if((Control 'chkWorkOrder').Checked){2}else{3}
            $order=$context.SOD_sp_InsertQoute('FIXTURE',1,1,0,[decimal]$total,'Fixture','Fixture',$kind)
            for($i=$start;$i -le $last;$i++){
                $row=$grid.Rows[$i]
                $cost=[Convert]::ToDouble($row.Cells[9].Value)
                $id=[Convert]::ToInt32($row.Cells[11].Value)
                $full=[Convert]::ToDouble($row.Cells[12].Value)
                $price=[Convert]::ToDouble($row.Cells[3].Value)
                $quantity=[Convert]::ToDouble($row.Cells[2].Value)
                $tax=if($module.GetField('bTaxExcempt',$static).GetValue($null)){0}else{[Convert]::ToInt32($row.Cells['Taxable'].Value)}
                $prepared=[Convert]::ToInt32($row.Cells[18].Value)
                $pick=if([Convert]::ToBoolean($row.Cells[17].Value)){'UP-STORE'}else{'STORE'}
                [void]$context.SOD_sp_InsertQouteEntry([Convert]::ToDecimal($cost),$order,$id,[Convert]::ToDecimal($full),[Convert]::ToDecimal($price),$quantity,1,$tax,[Convert]::ToString($row.Cells[13].Value),$pick,$prepared,$kind)
                if(!$map.ContainsKey($id)){throw [InvalidOperationException]::new("Item $id was not found while saving the order.")}
                if($kind -eq 2 -and $map[$id].ItemType -ne 7){
                    $delta=[WorkOrderEntry.OrderCommitmentRules]::CalculateChange(0,2,0,$quantity,$map[$id].ItemType)
                    [WorkOrderEntry.clsItemLookUp]::ApplyQuantityCommittedDifference($context,$id,$delta)
                }
            }
        }
        $tx.Commit()
    }catch{
        $tx.Rollback();$e=$_.Exception;while($e.InnerException){$e=$e.InnerException}
        $status='Rejected';$errorType=$e.GetType().FullName;$message=$e.Message
    }finally{$tx.Dispose();$context.Dispose()}
    return [pscustomobject]@{Status=$status;Type=$errorType;Message=$message;Identity=[int](Scalar "SELECT IDENT_CURRENT('dbo.[Order]');");Headers=[int](Scalar 'SELECT COUNT(*) FROM dbo.[Order];');Lines=[int](Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry;');Committed=[double](Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=1;');Stock=[double](Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=1;');Values=(Scalar "SELECT OrderID,ItemID,QuantityOnOrder,Price,FullPrice,Cost,Taxable,Prepared FROM dbo.OrderEntry ORDER BY ID FOR XML PATH('Line');")}
}
try{
    & $localdb create $owned -s | Out-Null;if($LASTEXITCODE){throw 'Owned LocalDB creation failed'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $cmd=$master.CreateCommand();$cmd.CommandText="CREATE DATABASE [$owned]; ALTER DATABASE [$owned] SET COMPATIBILITY_LEVEL=130;";[void]$cmd.ExecuteNonQuery();$cmd.Dispose()
    $connection=[Data.SqlClient.SqlConnection]::new($cs);$connection.Open()
    Sql @'
CREATE TABLE dbo.[Order](ID int IDENTITY(100,1),Total money,[Type] int);
CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemType smallint,QuantityCommitted float,Quantity float);
CREATE TABLE dbo.OrderEntry(ID int IDENTITY,OrderID int,ItemID int,QuantityOnOrder float,Price money,FullPrice money,Cost money,Taxable int,Prepared int);
'@
    Sql @'
CREATE PROCEDURE dbo.SOD_sp_InsertQoute @vComment varchar(255),@iCustID int,@iSalesRepID int,@mTax money,@mTotal money,@vRemarks varchar(255),@OPIS varchar(255),@Type int AS
BEGIN INSERT dbo.[Order](Total,[Type]) VALUES(@mTotal,@Type); RETURN CONVERT(int,SCOPE_IDENTITY()); END;
'@
    Sql @'
CREATE PROCEDURE dbo.SOD_sp_InsertQouteEntry @mCost money,@iOrderID bigint,@iItemID int,@mFullPrice money,@mPrice money,@fQuantityOnOrder float,@iSalesRepID int,@iTaxable int,@vDescription varchar(100),@vComment varchar(100),@QtyPrep int,@Type int AS
BEGIN INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice,Cost,Taxable,Prepared) VALUES(@iOrderID,@iItemID,@fQuantityOnOrder,@mPrice,@mFullPrice,@mCost,@iTaxable,@QtyPrep); RETURN 0; END;
'@
    Sql 'INSERT dbo.[Order](Total,[Type]) VALUES(0,2);'
    $form=[WorkOrderEntry.frmItemLookUp]::new()
    $form.GetType().GetField('isBulkLoading',$instance).SetValue($form,$true)
    $form.GetType().GetField('isLoadingRecalledOrder',$instance).SetValue($form,$true)
    $grid=Control 'gridSelectItem';$grid.AllowUserToAddRows=$false
    $cases=@('ValidFraction','ValidSplit','MissingLaterItem','BadLaterCost','BadLaterTotal','CostOverflow','PriceOverflow','BadPrepared','BadPickFlag','BadTax','TaxExempt','NegativeCommitment','RunningDuplicate','ZeroDifference','NonInventory','Quote','DuplicateAcrossSplit','ConcurrentChange')
    $results=@()
    foreach($case in $cases){
        $pair=@()
        foreach($before in @($true,$false)){
            Sql "DELETE dbo.OrderEntry;DELETE dbo.[Order];DBCC CHECKIDENT('dbo.[Order]',RESEED,99) WITH NO_INFOMSGS; INSERT dbo.[Order](Total,[Type]) VALUES(0,2);DELETE dbo.Item;INSERT dbo.Item VALUES(1,0,5,100.5),(2,0,2,50);"
            (Control 'chkWorkOrder').Checked=$true;(Control 'chkQuote').Checked=$false
            $module.GetField('bTaxExcempt',$static).SetValue($null,$false)
            $count=if($case -in @('ValidSplit','MissingLaterItem','BadLaterCost','BadLaterTotal','CostOverflow','PriceOverflow','DuplicateAcrossSplit')){7}else{1}
            Rows $count
            switch($case){
                'MissingLaterItem' {$grid.Rows[6].Cells[11].Value=999}
                'BadLaterCost' {$grid.Rows[6].Cells[9].Value='invalid'}
                'BadLaterTotal' {$grid.Rows[6].Cells[5].Value='invalid'}
                'CostOverflow' {$grid.Rows[0].Cells[9].Value=[double]1e29}
                'PriceOverflow' {$grid.Rows[0].Cells[3].Value=[double]1e29}
                'BadPrepared' {$grid.Rows[0].Cells[18].Value='invalid'}
                'BadPickFlag' {$grid.Rows[0].Cells[17].Value='invalid'}
                'BadTax' {$grid.Rows[0].Cells['Taxable'].Value='invalid'}
                'TaxExempt' {$grid.Rows[0].Cells['Taxable'].Value='invalid';$module.GetField('bTaxExcempt',$static).SetValue($null,$true)}
                'NegativeCommitment' {Sql 'UPDATE dbo.Item SET QuantityCommitted=-4 WHERE ID=1;'}
                'RunningDuplicate' {Rows 2;Sql 'UPDATE dbo.Item SET QuantityCommitted=2 WHERE ID=1;';$grid.Rows[0].Cells[2].Value=-1.25;$grid.Rows[1].Cells[2].Value=-1.25}
                'ZeroDifference' {Sql 'UPDATE dbo.Item SET QuantityCommitted=-4 WHERE ID=1;';$grid.Rows[0].Cells[2].Value=0.0}
                'NonInventory' {Sql 'UPDATE dbo.Item SET ItemType=7,QuantityCommitted=-4 WHERE ID=1;'}
                'Quote' {(Control 'chkWorkOrder').Checked=$false;(Control 'chkQuote').Checked=$true;Sql 'UPDATE dbo.Item SET QuantityCommitted=-4 WHERE ID=1;'}
                'DuplicateAcrossSplit' {for($i=0;$i -lt 7;$i++){$grid.Rows[$i].Cells[2].Value=-1.0}}
            }
            $result=Attempt (!$before) ($case -eq 'ConcurrentChange');$pair+=,$result
        }
        $pair|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out ($case+'.json'))
        $old=$pair[0];$new=$pair[1]
        Check ($old.Status -eq $new.Status) "$case keeps success/failure outcome"
        Check ($old.Stock -eq $new.Stock -and $new.Stock -eq 100.5) "$case leaves physical stock unchanged"
        if($old.Status -eq 'Committed'){
            Check ($old.Identity -eq $new.Identity -and $old.Values -ceq $new.Values -and $old.Committed -eq $new.Committed) "$case preserves saved values and commitment result"
        }else{
            Check ($new.Headers -eq 1 -and $new.Lines -eq 0) "$case rolls back all new headers and lines"
            if($case -eq 'ConcurrentChange'){
                Check ($new.Identity -eq 101 -and $new.Committed -eq -20) 'Final atomic check still rejects a concurrent change after preflight'
            }else{
                Check ($old.Identity -gt 100 -and $new.Identity -eq 100) "$case now fails before any order ID is consumed"
                Check ($old.Type -eq $new.Type) "$case preserves exception category"
            }
        }
        $results+=,[pscustomobject]@{Case=$case;WithoutPreflight=$old;WithPreflight=$new}
    }
    # This boundary test uses the compiled helper with mapped fixture SQL routines.
    # Verify its actual InsertEntry caller still precedes the first header and that
    # the original atomic update remains in the production save body.
    $source=[IO.File]::ReadAllText((Join-Path $repo 'Work Order Entry/Forms/frmItemLookUp.vb'))
    $body=$source.Substring($source.IndexOf('Private Function InsertEntry()'))
    Check ($body.IndexOf('ValidateNewOrderRows(itemMap)') -lt $body.IndexOf('dbSave.SOD_sp_InsertQoute(')) 'Actual InsertEntry preflights before header allocation'
    Check ($body.Contains('clsItemLookUp.ApplyQuantityCommittedDifference(dbSave, iItemID,')) 'Original atomic transaction update retained'
    $results|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'Results.json')
    Check ((Get-FileHash -LiteralPath ($app+'.config')).Hash -eq $configHash) 'Application configuration unchanged'
    Write-Output "PASS: $checks allocation-boundary checks across $($cases.Count) paired synthetic cases. Not full UI/RMS/store execution. Evidence: $out"
}finally{
    if($form){$form.Dispose()};if($connection){$connection.Dispose()}
    if($master){try{$cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID('$owned') IS NOT NULL BEGIN ALTER DATABASE [$owned] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$owned];END;";[void]$cmd.ExecuteNonQuery();$cmd.Dispose()}finally{$master.Dispose()}}
    if($created){& $localdb stop $owned -k|Out-Null;& $localdb delete $owned|Out-Null}
}
