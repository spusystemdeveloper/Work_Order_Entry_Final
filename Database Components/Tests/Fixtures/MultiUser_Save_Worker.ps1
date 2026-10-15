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
    $row=$grid.Rows.Add()
    foreach($cell in $grid.Rows[$row].Cells){if($cell -is [Windows.Forms.DataGridViewCheckBoxCell]){$cell.Value=$false}else{$cell.Value=0}}
    $grid.Item(0,$row).Value='FIXTURE-SKU'
    $grid.Item(1,$row).Value='Synthetic multi-user item'
    $grid.Item(2,$row).Value=[double]$request.Quantity
    $grid.Item(3,$row).Value=[decimal]112
    $grid.Item(9,$row).Value=[decimal]80
    $grid.Item(11,$row).Value=[int]$request.ItemID
    $grid.Item(12,$row).Value=[decimal]112
    $grid.Item(13,$row).Value='Synthetic multi-user item'
    $grid.Item(16,$row).Value=[int]$request.EntryID
    $grid.Item(17,$row).Value=$false
    $grid.Item(18,$row).Value=0
    $grid.Rows[$row].Cells['Taxable'].Value=1
    $vat=$form.GetType().GetProperty('txtVat',$instanceFlags).GetValue($form)
    $total=$form.GetType().GetProperty('txtTotal',$instanceFlags).GetValue($form)
    $vat.Text=([decimal]$request.Quantity*12).ToString()
    $total.Text=([decimal]$request.Quantity*112).ToString()
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
    $method=$form.GetType().GetMethod('SaveExistingOrderChanges',$instanceFlags)
    $null=$method.Invoke($form,@([int]$request.OrderID,[string]$request.Comment))
    Publish-Result ([pscustomobject]@{Status='Committed';Actor=$request.Actor})
} catch {
    $errorObject=$_.Exception
    while($null -ne $errorObject.InnerException){$errorObject=$errorObject.InnerException}
    $status=if($errorObject.GetType().FullName -eq 'WorkOrderEntry.OrderEditConflictException'){'Conflict'}else{'Error'}
    if($null -ne $request){Publish-Result ([pscustomobject]@{Status=$status;Type=$errorObject.GetType().FullName;Message=$errorObject.Message})}
    if($status -eq 'Error'){Write-Error $errorObject.Message;exit 1}
} finally {if($null -ne $form){$form.Dispose()}}
