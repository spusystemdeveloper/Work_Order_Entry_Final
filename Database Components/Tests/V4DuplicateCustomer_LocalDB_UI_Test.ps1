# Actual V4 customer query/selection against an owned disposable localhost fixture.
[CmdletBinding()]
param([string]$ApplicationPath)
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
 & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath -ApplicationPath $ApplicationPath
 exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Data,System.Data.Linq,System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if(-not $ApplicationPath){$ApplicationPath=Join-Path $repo 'Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\EXE-Only\Work Order Entry_V4.exe'}
$asm=[Reflection.Assembly]::LoadFrom($ApplicationPath)
$static=[Reflection.BindingFlags]'Public,NonPublic,Static'
$instance=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$module=$asm.GetType('WorkOrderEntry.ModConnectDB',$true)
$dbName='SOD_V4Duplicate_'+[Guid]::NewGuid().ToString('N')
$server='(localdb)\MSSQLLocalDB'
$connection=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=master;Integrated Security=True;Pooling=False;Connect Timeout=20")
$created=$false;$main=$null;$lookup=$null;$gridHost=$null;$checks=0
$configs=@{}
foreach($path in @('Work Order Entry\bin\Debug\Work Order Entry_V8.exe.config','Work Order Entry\bin\Release\Work Order Entry_V8.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Debug\Work Order Entry_V4.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe.config')){
 $full=Join-Path $repo $path;if(Test-Path -LiteralPath $full){$configs[$full]=(Get-FileHash -LiteralPath $full).Hash}
}
function Sql($sql){$command=$connection.CreateCommand();try{$command.CommandText=$sql;[void]$command.ExecuteNonQuery()}finally{$command.Dispose()}}
function State($name,$value){$module.GetField($name,$static).SetValue($null,$value)}
function Global($name){$module.GetField($name,$static).GetValue($null)}
function Control($form,$name){$form.GetType().GetProperty($name,$instance).GetValue($form,$null)}
function Check($value,$message){if(-not $value){throw "FAILED: $message"};$script:checks++;"PASS: $message"}
function Search($text){return ,($asm.GetType('WorkOrderEntry.clsCustomer').GetMethod('SearchCustomer',$static).Invoke($null,@($text)))}
function NewLookup($text){
 if($script:gridHost){$script:gridHost.Dispose()}
 $script:lookup=[Activator]::CreateInstance($asm.GetType('WorkOrderEntry.frmCustomer'))
 $script:lookup.BindingContext=[Windows.Forms.BindingContext]::new()
 $grid=Control $script:lookup 'gridCustomer'
 $grid.BindingContext=$script:lookup.BindingContext
 $grid.DataSource=Search $text
 [void][Windows.Forms.Control].GetMethod('CreateHandle',$instance).Invoke($script:lookup,$null)
 $grid.CreateControl()
 # Render only the bound grid in a synthetic offscreen host, never show the lookup.
 $script:gridHost=[Windows.Forms.Form]::new()
 $script:gridHost.ShowInTaskbar=$false
 $script:gridHost.StartPosition='Manual';$script:gridHost.Location=[Drawing.Point]::new(-20000,-20000)
 $script:gridHost.Controls.Add($grid)
 $script:gridHost.Show();[Windows.Forms.Application]::DoEvents()
 return $grid
}
try{
 $connection.Open();Sql "CREATE DATABASE [$dbName]";$created=$true;$connection.ChangeDatabase($dbName)
 Sql @'
CREATE TABLE dbo.Customer(ID int PRIMARY KEY,AccountNumber nvarchar(20),Company nvarchar(50),PriceLevel smallint,Employee bit,Title nvarchar(20),TaxExempt bit,CustomText2 nvarchar(30),CustomText4 nvarchar(30),CreditLimit money,AccountBalance money,SalesRepID int);
CREATE TABLE dbo.SalesRep(ID int PRIMARY KEY,Name nvarchar(30));
CREATE TABLE dbo.SOD_WO_CustType(ID int PRIMARY KEY,CustType varchar(50),PriceBound varchar(50),PricePassAt varchar(50));
CREATE TABLE dbo.SOD_WO_CustTypeUser(ID int PRIMARY KEY,CustTypeID int,USERID varchar(50));
CREATE TABLE dbo.SOD_WO_Conf(id int PRIMARY KEY,PriceLevel nvarchar(50));
INSERT dbo.SOD_WO_Conf VALUES(1,N'PRICE B (D1)');
INSERT dbo.SalesRep VALUES(11,N'Rep A'),(22,N'Rep B');
INSERT dbo.Customer VALUES(101,N'GSC-A',N'SAME COMPANY',1,0,N'A',0,N'',N'Default',1000,100,11),(202,N'GSC-B',N'SAME COMPANY',3,1,N'B',1,N'',N'Default',2000,200,22);
'@
 # No real app SQL runs before all three in-process contexts are replaced.
 foreach($name in @('db','dbnew','dbInitial')){
  $old=Global $name;if($old){if($old.Connection.State -ne 'Closed'){throw 'Unexpected open application context'};$old.Dispose()};State $name $null
 }
 State 'dbs' ($server+';Integrated Security=True;Pooling=False;Application Name=V4DuplicateOwnedFixture')
 State 'dbn' $dbName;State 'dbu' '';State 'dbp' ''
 $endpoint=[string]$module.GetMethod('DB_Conn',$static).Invoke($null,@('constr'))
 $target=[Data.SqlClient.SqlConnectionStringBuilder]::new($endpoint)
 if($target.DataSource -ne $server -or $target.InitialCatalog -ne $dbName -or -not $target.IntegratedSecurity){throw 'Owned fixture endpoint guard failed'}
 foreach($name in @('db','dbnew','dbInitial')){State $name ([Activator]::CreateInstance($asm.GetType('WorkOrderEntry.ItemLookUpDataContext'),@($endpoint)))}
 $forms=$asm.GetType('WorkOrderEntry.My.MyProject').GetProperty('Forms',$static).GetValue($null,$null)
 $main=$forms.GetType().GetProperty('frmItemLookUp',$instance).GetValue($forms,$null)
 $main.GetType().GetField('usrID',$static).SetValue($null,'FixtureUser')
 $startErrors=Global 'ErrorCount'
 foreach($event in @('OK','DoubleClick','Enter','Rebound')){
  $grid=NewLookup $(if($event -eq 'Rebound'){'GSC-B'}else{'SAME COMPANY'})
  Check ($grid.Rows.Count -eq $(if($event -eq 'Rebound'){1}else{2})) "$event retains company/account search results"
  $row=if($event -eq 'Rebound'){0}else{1}
  $expected=[int]$grid.Rows[$row].Cells[6].Value
  $account=[string]$grid.Rows[$row].Cells[0].Value
  $price=[int]$grid.Rows[$row].Cells[2].Value
  $employee=[bool]$grid.Rows[$row].Cells[3].Value
  $exempt=[bool]$grid.Rows[$row].Cells[5].Value
  $grid.CurrentCell=$grid.Rows[$row].Cells[0]
  State 'iRowCust' $(if($event -eq 'Rebound'){99}else{0})
  if($event -eq 'OK' -or $event -eq 'Rebound'){
   [void]$lookup.GetType().GetMethod('cmdOk_Click',$instance).Invoke($lookup,@($null,[EventArgs]::Empty))
  }elseif($event -eq 'DoubleClick'){
   $cellEvent=[Windows.Forms.DataGridViewCellEventArgs]::new(0,$row)
   [void]$lookup.GetType().GetMethod('gridCustomer_CellDoubleClick',$instance).Invoke($lookup,@($grid,$cellEvent))
  }else{
   $keyEvent=[Windows.Forms.KeyEventArgs]::new([Windows.Forms.Keys]::Enter)
   [void]$lookup.GetType().GetMethod('gridCustomer_KeyDown',$instance).Invoke($lookup,@($grid,$keyEvent))
   Check ($keyEvent.Handled -and $keyEvent.SuppressKeyPress) 'Enter selects before the grid can move to another account'
  }
  Check ((Global 'iCusID') -eq $expected -and (Global 'sAcctNum') -eq $account) "$event uses selected ID/account despite stale row index"
  Check ((Global 'iPrice') -eq $price -and (Global 'iPricePass') -eq 2) "$event preserves selected customer price level and existing approval setting"
  Check ((Global 'bEmployee') -eq $employee -and (Global 'bTaxExcempt') -eq $exempt) "$event preserves selected customer employee/tax flags"
  $rep=if($expected -eq 101){11}else{22}
  Check ((Global 'iSalesID') -eq $rep -and (Control $main 'txtSales').Text -eq $(if($rep -eq 11){'Rep A'}else{'Rep B'})) "$event uses selected customer's primary Sales Rep"
  $lookup.Dispose();$lookup=$null
 }
 # Select the first duplicate too; both identities must work independently.
 $grid=NewLookup 'SAME COMPANY';$grid.CurrentCell=$grid.Rows[0].Cells[0]
 $firstID=[int]$grid.Rows[0].Cells[6].Value;State 'iRowCust' 1
 [void]$lookup.GetType().GetMethod('cmdOk_Click',$instance).Invoke($lookup,@($null,[EventArgs]::Empty))
 Check ((Global 'iCusID') -eq $firstID) 'first duplicate is selectable independently of second account'
 $lookup.Dispose();$lookup=$null
 $grid=NewLookup 'NOT FOUND';State 'iCusID' 101
 [void]$lookup.GetType().GetMethod('cmdOk_Click',$instance).Invoke($lookup,@($null,[EventArgs]::Empty))
 Check ($grid.Rows.Count -eq 0 -and (Global 'iCusID') -eq 101) 'empty results cannot replace the existing selected customer'
 $lookup.Dispose();$lookup=$null
 $grid=NewLookup 'SAME COMPANY';State 'iCusID' 101
 [void]$lookup.GetType().GetMethod('gridCustomer_CellDoubleClick',$instance).Invoke($lookup,@($grid,[Windows.Forms.DataGridViewCellEventArgs]::new(0,-1)))
 Check ((Global 'iCusID') -eq 101) 'header double-click cannot choose a different account'
 Check ((Global 'ErrorCount') -eq $startErrors) 'selection paths produced no application errors'
 foreach($path in $configs.Keys){Check ((Get-FileHash -LiteralPath $path).Hash -eq $configs[$path]) 'existing application configuration unchanged'}
 "PASS: $checks V4 duplicate-company checks on owned localhost fixture; no production Load, save or print."
}finally{
 if($lookup){$lookup.Dispose()};if($main){$main.Dispose()};if($gridHost){$gridHost.Dispose()}
 foreach($name in @('db','dbnew','dbInitial')){$context=Global $name;if($context){$context.Dispose()};State $name $null}
 if($created -and $connection.State -eq 'Open'){$connection.ChangeDatabase('master');Sql "ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$dbName]"}
 $connection.Dispose()
 foreach($path in $configs.Keys){if((Get-FileHash -LiteralPath $path).Hash -ne $configs[$path]){throw 'Existing application configuration changed'}}
}
