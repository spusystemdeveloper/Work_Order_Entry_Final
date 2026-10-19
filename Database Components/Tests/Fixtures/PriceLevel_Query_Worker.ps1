param([Parameter(Mandatory=$true)][string]$RequestFile)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data,System.Data.Linq,System.Windows.Forms,System.Drawing
$request = Get-Content -LiteralPath $RequestFile -Raw | ConvertFrom-Json
if ($request.Server -notmatch '^\(localdb\)\\WO_Price_[a-f0-9]{8}$' -or $request.Database -notmatch '^WO_Price_[a-f0-9]{8}$' -or $request.Actor -notmatch '^PriceLevel_[a-f0-9]{8}_(Before|After)$') { throw 'Only an owned LocalDB fixture is permitted' }
$a = [Reflection.Assembly]::LoadFrom($request.Assembly)
$static = [Reflection.BindingFlags]'Static,Public,NonPublic'
$instance = [Reflection.BindingFlags]'Instance,Public,NonPublic'
$module = $a.GetType('WorkOrderEntry.ModConnectDB', $true)
function Set-Global($name,$value) { $module.GetField($name,$static).SetValue($null,$value) }
function Get-Global($name) { $module.GetField($name,$static).GetValue($null) }
foreach ($name in @('db','dbnew','dbInitial')) {
    $old = Get-Global $name
    if ($old) { if ($old.Connection.State -ne 'Closed') { throw 'Unexpected open context' }; $old.Dispose() }
    Set-Global $name $null
}
Set-Global 'dbs' ($request.Server + ';Integrated Security=True;Pooling=False;Application Name=' + $request.Actor)
Set-Global 'dbn' $request.Database
Set-Global 'dbu' ''; Set-Global 'dbp' ''
$resolved = $module.GetMethod('DB_Conn',$static).Invoke($null,@('constr'))
$target = [Data.SqlClient.SqlConnectionStringBuilder]::new($resolved)
if ($target.DataSource -ne $request.Server -or $target.InitialCatalog -ne $request.Database -or !$target.IntegratedSecurity -or $target.Pooling) { throw 'Fixture endpoint mismatch' }
# Only in-memory endpoint fields changed; registry/app config remain untouched.
$forms = $a.GetType('WorkOrderEntry.My.MyProject').GetProperty('Forms',$static).GetValue($null,$null)
$main = $forms.GetType().GetProperty('frmItemLookUp',$instance).GetValue($forms,$null)
$form = $forms.GetType().GetProperty('frmPriceLevel',$instance).GetValue($forms,$null)
$grid = $form.GetType().GetProperty('gridLevel',$instance).GetValue($form,$null)
$main.GetType().GetProperty('txtPriceLevel',$instance).GetValue($main,$null).Text = ''
$setup = [Data.SqlClient.SqlConnection]::new("Server=$($request.Server);Database=$($request.Database);Integrated Security=True;Pooling=False;Application Name=FixtureSetup")
$results = [Collections.Generic.List[object]]::new()
try {
    $setup.Open()
    foreach ($mask in @('PRICE (RETAIL)','PRICE A (WHOLESALE)','PRICE B (D1)','PRICE C (D2)','NONE')) {
        foreach ($max in 0..4) {
            foreach ($scenario in @('Normal','Missing','Changed')) {
                $cmd = $setup.CreateCommand()
                try {
                    $prices = if ($scenario -eq 'Changed') { '189.9375,180,178,160' } else { '178,171.125,0,-3.25' }
                    $cmd.CommandText = "UPDATE dbo.SOD_WO_Conf SET PriceLevel=@mask; DELETE dbo.Item; INSERT dbo.Item(ItemLookupCode,Price,PriceA,PriceB,PriceC,Cost) VALUES('TEST',$prices,99);"
                    $null = $cmd.Parameters.AddWithValue('@mask',$mask)
                    $null = $cmd.ExecuteNonQuery()
                } finally { $cmd.Dispose() }
                Set-Global 'sItemCode' $(if($scenario -eq 'Missing'){'MISSING'}else{'TEST'})
                Set-Global 'sItemName' 'Synthetic price query fixture'
                Set-Global 'iPrice' ([int]$max)
                Set-Global 'iPricePass' ([int]2)
                Set-Global 'dLowest' ([double]-999)
                Set-Global 'bPass' $true; Set-Global 'bAllow' $true
                Set-Global 'ispriceApproved' ([int]1)
                $watch = [Diagnostics.Stopwatch]::StartNew()
                $form.GetType().GetMethod('LoadLevels',$instance).Invoke($form,@()) | Out-Null
                $watch.Stop()
                $rows = @($grid.Rows | ForEach-Object { @([string]$_.Cells[0].Value,[double]$_.Cells[1].Value) })
                $results.Add([pscustomobject]@{
                    Key="$mask|$max|$scenario"; Rows=$rows; Lowest=(Get-Global 'dLowest')
                    Selected=$grid.CurrentRow.Index; SelectedColumn=$grid.CurrentCell.ColumnIndex
                    PricePass=(Get-Global 'iPricePass'); Pass=(Get-Global 'bPass')
                    Allow=(Get-Global 'bAllow'); Approved=(Get-Global 'ispriceApproved')
                    Milliseconds=$watch.Elapsed.TotalMilliseconds
                })
            }
        }
    }
    # Existing price-edit callers also use the single-level API. Check both
    # display-enabled and flag-suppressed paths without selecting/applying prices.
    foreach ($mask in @('PRICE (RETAIL)','PRICE A (WHOLESALE)','PRICE B (D1)','PRICE C (D2)','NONE')) {
        $cmd = $setup.CreateCommand()
        try { $cmd.CommandText='UPDATE dbo.SOD_WO_Conf SET PriceLevel=@mask'; $null=$cmd.Parameters.AddWithValue('@mask',$mask); $null=$cmd.ExecuteNonQuery() }
        finally { $cmd.Dispose() }
        foreach ($level in 0..4) {
            foreach ($blocked in @($false,$true)) {
                $grid.Rows.Clear()
                Set-Global 'dLowest' ([double]-999); Set-Global 'bLevelFlag' $blocked
                $watch=[Diagnostics.Stopwatch]::StartNew()
                $a.GetType('WorkOrderEntry.clsItemLookUp').GetMethod('LoadLevel',$static).Invoke($null,@([int]$level,'TEST',[int]0)) | Out-Null
                $watch.Stop()
                $results.Add([pscustomobject]@{
                    Key="Single|$mask|$level|$blocked"
                    Rows=@($grid.Rows | ForEach-Object { @([string]$_.Cells[0].Value,[double]$_.Cells[1].Value) })
                    Lowest=(Get-Global 'dLowest')
                    Selected=$(if($grid.CurrentRow){$grid.CurrentRow.Index}else{-1})
                    SelectedColumn=$(if($grid.CurrentCell){$grid.CurrentCell.ColumnIndex}else{-1})
                    PricePass=(Get-Global 'iPricePass'); Pass=(Get-Global 'bPass')
                    Allow=(Get-Global 'bAllow'); Approved=(Get-Global 'ispriceApproved')
                    Milliseconds=$watch.Elapsed.TotalMilliseconds
                })
            }
        }
    }
    if ((Get-Global 'ErrorCount') -ne 0) { throw 'Unexpected application error' }
    [IO.File]::WriteAllText($request.ResultFile,($results | ConvertTo-Json -Depth 7))
} finally { $setup.Dispose(); $form.Dispose(); $main.Dispose() }
