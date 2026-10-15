# Child cashier process for MultiUser_Save_LocalDB_Test.ps1. No UI is shown.
param([Parameter(Mandatory=$true)][string]$RequestFile)
$ErrorActionPreference='Stop'
$form=$null
function Publish-Result($value) {
    $temporary=$request.ResultFile+'.tmp'
    [IO.File]::WriteAllText($temporary,($value|ConvertTo-Json))
    Move-Item -LiteralPath $temporary -Destination $request.ResultFile
}
function Assert-FixtureEndpoint {
    $value=[string]$module.GetMethod('DB_Conn',$publicStatic).Invoke($null,@('constr'))
    $target=[Data.SqlClient.SqlConnectionStringBuilder]::new($value)
    if($target.DataSource -ne $request.Server -or $target.InitialCatalog -ne $request.Database -or
       -not $target.IntegratedSecurity -or $target.Pooling -or $target.ApplicationName -ne $request.Actor){throw 'Application connection did not resolve exclusively to the owned fixture.'}
    return $value
}
try {
    $request=Get-Content -LiteralPath $RequestFile -Raw|ConvertFrom-Json
    if($request.Server -notmatch '^\(localdb\)\\SODWO_Multi_[a-f0-9]{8}$' -or
       $request.Database -notmatch '^SODWO_Multi_[a-f0-9]{8}$' -or
       $request.Actor -notmatch '^MultiUser_[AB]_[a-f0-9]{8}$') {throw 'Only the owned synthetic LocalDB target is permitted.'}
    $operation=if($request.PSObject.Properties['Operation']){[string]$request.Operation}else{'Save'}
    if($operation -notin @('Save','Cancel','Settle')){throw 'Unknown fixture operation.'}
    Add-Type -AssemblyName System.Data,System.Data.Linq,System.Windows.Forms,System.Drawing
    $assembly=[Reflection.Assembly]::LoadFrom($request.Assembly)
    $module=$assembly.GetType('WorkOrderEntry.ModConnectDB',$true)
    $publicStatic=[Reflection.BindingFlags]'Static,Public'
    # The legacy initializer may read registry defaults, but opens no SQL connection.
    # Replace ALL contexts and endpoint fields before invoking any application SQL.
    foreach($name in @('db','dbnew','dbInitial')) {
        $field=$module.GetField($name,$publicStatic);$old=$field.GetValue($null)
        if($null -ne $old){if($old.Connection.State -ne [Data.ConnectionState]::Closed){throw 'An unexpected pre-existing SQL connection is open.'};$old.Dispose()}
    }
    # GetDB normally builds SQL authentication from globals. In this test process,
    # explicitly add Windows authentication/options to its in-memory server segment.
    # No app config, registry entry or production source is changed.
    $module.GetField('dbs',$publicStatic).SetValue($null,$request.Server+';Integrated Security=True;Pooling=False;Application Name='+$request.Actor)
    $module.GetField('dbn',$publicStatic).SetValue($null,[string]$request.Database)
    $module.GetField('dbu',$publicStatic).SetValue($null,'')
    $module.GetField('dbp',$publicStatic).SetValue($null,'')
    $connectionString=Assert-FixtureEndpoint
    foreach($name in @('db','dbnew','dbInitial')){$module.GetField($name,$publicStatic).SetValue($null,[WorkOrderEntry.ItemLookUpDataContext]::new($connectionString))}
    $module.GetField('iCusID',$publicStatic).SetValue($null,1)
    $module.GetField('iSalesID',$publicStatic).SetValue($null,1)
    $module.GetField('bTaxExcempt',$publicStatic).SetValue($null,$false)
    $module.GetField('ispriceApproved',$publicStatic).SetValue($null,0)
    [WorkOrderEntry.frmItemLookUp]::usrUsername=$request.Actor
    [WorkOrderEntry.frmItemLookUp]::usrRegister='FIXTURE'
    [Threading.Thread]::CurrentThread.CurrentCulture=[Globalization.CultureInfo]::GetCultureInfo('en-US')
    $form=[WorkOrderEntry.frmItemLookUp]::new()
    $instanceFlags=[Reflection.BindingFlags]'Instance,Public,NonPublic'
    $form.GetType().GetField('isBulkLoading',$instanceFlags).SetValue($form,$true)
    $form.GetType().GetField('isLoadingRecalledOrder',$instanceFlags).SetValue($form,$true)
    $workOrder=$form.GetType().GetProperty('chkWorkOrder',$instanceFlags).GetValue($form)
    $quote=$form.GetType().GetProperty('chkQuote',$instanceFlags).GetValue($form)
    $workOrder.Checked=$true
    $quote.Checked=$false
    $grid=$form.GetType().GetProperty('gridSelectItem',$instanceFlags).GetValue($form)
    $grid.AllowUserToAddRows=$false
    $grid.Rows.Clear()
    $lines=if($null -ne $request.Lines -and @($request.Lines).Count -gt 0){@($request.Lines)}else{@([pscustomobject]@{ItemID=$request.ItemID;EntryID=$request.EntryID;Quantity=$request.Quantity;PreparedQuantity=0})}
    $totalQuantity=[decimal]0
    foreach($line in $lines){
    $row=$grid.Rows.Add()
    foreach($cell in $grid.Rows[$row].Cells){if($cell -is [Windows.Forms.DataGridViewCheckBoxCell]){$cell.Value=$false}else{$cell.Value=0}}
    $grid.Item(0,$row).Value='FIXTURE-SKU'
    $grid.Item(1,$row).Value='Synthetic multi-user item'
    $grid.Item(2,$row).Value=[double]$line.Quantity
    $grid.Item(3,$row).Value=[decimal]112
    $grid.Item(9,$row).Value=[decimal]80
    $grid.Item(11,$row).Value=[int]$line.ItemID
    $grid.Item(12,$row).Value=[decimal]112
    $grid.Item(13,$row).Value='Synthetic multi-user item'
    $grid.Item(16,$row).Value=[int]$line.EntryID
    $grid.Item(17,$row).Value=$false
    $grid.Item(18,$row).Value=[int]$line.PreparedQuantity
    $grid.Rows[$row].Cells['Taxable'].Value=1
    $totalQuantity += [decimal]$line.Quantity
    }
    $vat=$form.GetType().GetProperty('txtVat',$instanceFlags).GetValue($form)
    $total=$form.GetType().GetProperty('txtTotal',$instanceFlags).GetValue($form)
    $vat.Text=($totalQuantity*12).ToString()
    $total.Text=($totalQuantity*112).ToString()
    $context=[WorkOrderEntry.ItemLookUpDataContext]::new($connectionString)
    try{$snapshot=[WorkOrderEntry.OrderEditGuard]::Capture($context,[int]$request.OrderID)}finally{$context.Dispose()}
    $form.GetType().GetField('recalledSnapshot',$instanceFlags).SetValue($form,$snapshot)
    $form.GetType().GetField('isBulkLoading',$instanceFlags).SetValue($form,$false)
    $form.GetType().GetField('isLoadingRecalledOrder',$instanceFlags).SetValue($form,$false)
    $null=Assert-FixtureEndpoint
    [IO.File]::WriteAllText($request.ReadyFile,'Ready: fixture endpoint validated and recall snapshot captured.')
    $deadline=[DateTime]::UtcNow.AddSeconds(30)
    while(-not (Test-Path -LiteralPath $request.GoFile)){if([DateTime]::UtcNow -gt $deadline){throw 'Coordinator did not release the cashier.'};Start-Sleep -Milliseconds 25}
    # Invoke the actual compiled save transaction, unchanged. This includes its
    # application lock, snapshot check, line/stock/queue/tax/history and commit.
    $null=Assert-FixtureEndpoint
    $transactionNumber=$null
    switch($operation){
        'Save' {
            $method=$form.GetType().GetMethod('SaveExistingOrderChanges',$instanceFlags)
            $null=$method.Invoke($form,@([int]$request.OrderID,[string]$request.Comment))
        }
        'Cancel' {[WorkOrderEntry.clsRecall]::CancelOrder([int]$request.OrderID,$snapshot)}
        'Settle' {
            # A synthetic POS participant, NOT QSRules/RMS POS automation. It
            # writes the known closed-order/stock/posting contract atomically,
            # using the unmodified deployed close-queue trigger and @@IDENTITY.
            $context=[WorkOrderEntry.ItemLookUpDataContext]::new($connectionString)
            try {
                $context.Connection.Open()
                [WorkOrderEntry.OrderHistory]::SetActor($context,$request.Actor,'FIXTURE-POS')
                $tx=$context.Connection.BeginTransaction()
                try {
                    $cmd=$context.Connection.CreateCommand()
                    try {
                        $cmd.Transaction=$tx;$cmd.CommandTimeout=20
                        $null=$cmd.Parameters.AddWithValue('@order',[int]$request.OrderID)
                        $cmd.CommandText=@'
UPDATE dbo.[Order] SET Closed=1,LastUpdated=GETDATE() WHERE ID=@order AND Closed=0;
IF @@ROWCOUNT<>1 THROW 52930,'Fixture settlement requires an open order.',1;
UPDATE i SET Quantity=i.Quantity-e.QuantityOnOrder,QuantityCommitted=i.QuantityCommitted-e.QuantityOnOrder
 FROM dbo.Item i JOIN dbo.OrderEntry e ON e.ItemID=i.ID WHERE e.OrderID=@order;
INSERT dbo.[Transaction](RecallID,CashierID,StoreID,RecallType,Total,SalesTax)
 SELECT ID,2,1,4,Total,Tax FROM dbo.[Order] WHERE ID=@order;
DECLARE @number int=CONVERT(int,@@IDENTITY);
INSERT dbo.TransactionEntry(TransactionNumber,ItemID,Quantity,Price,SalesTax)
 SELECT @number,ItemID,QuantityOnOrder,Price,QuantityOnOrder*12 FROM dbo.OrderEntry WHERE OrderID=@order;
INSERT dbo.TaxEntry(TransactionNumber,TransactionEntryID,SalesTax)
 SELECT @number,ID,SalesTax FROM dbo.TransactionEntry WHERE TransactionNumber=@number;
INSERT dbo.TenderEntry(TransactionNumber,Amount)
 SELECT @number,Total FROM dbo.[Order] WHERE ID=@order;
-- Pause with all settlement writes still uncommitted so the stale cashier
-- attempts its real save against the POS-held Order lock.
DECLARE @gate int;
SELECT @gate=ID FROM dbo.TestSaveGate WITH(UPDLOCK,HOLDLOCK) WHERE ID=1;
SELECT @number;
'@
                        $transactionNumber=[int]$cmd.ExecuteScalar()
                    }finally{$cmd.Dispose()}
                    $tx.Commit()
                }catch{$tx.Rollback();throw}finally{$tx.Dispose()}
            }finally{$context.Dispose()}
        }
    }
    Publish-Result ([pscustomobject]@{Status='Committed';Actor=$request.Actor;Operation=$operation;TransactionNumber=$transactionNumber})
} catch {
    $errorObject=$_.Exception
    while($null -ne $errorObject.InnerException){$errorObject=$errorObject.InnerException}
    $status=if($errorObject.GetType().FullName -eq 'WorkOrderEntry.OrderEditConflictException'){'Conflict'}else{'Error'}
    $sqlNumber=if($errorObject -is [Data.SqlClient.SqlException]){$errorObject.Number}else{$null}
    if($null -ne $request){Publish-Result ([pscustomobject]@{Status=$status;Type=$errorObject.GetType().FullName;Message=$errorObject.Message;SqlNumber=$sqlNumber})}
    if($status -eq 'Error'){Write-Error $errorObject.Message;exit 1}
} finally {if($null -ne $form){$form.Dispose()}}
