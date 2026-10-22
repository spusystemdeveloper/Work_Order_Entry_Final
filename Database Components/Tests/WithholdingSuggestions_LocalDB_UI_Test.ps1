# Customer SELECTs and compiled display checks in an owned localhost fixture.
# Never shows/loads the real application's form or changes installed config.
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
 & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath
 exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Data,System.Data.Linq,System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$exe=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash=(Get-FileHash ($exe+'.config')).Hash
$asm=[Reflection.Assembly]::LoadFrom($exe)
$module=$asm.GetType('WorkOrderEntry.ModConnectDB',$true)
$static=[Reflection.BindingFlags]'Public,Static'
$instance=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8)
$db='SOD_Withholding_'+$suffix
$server='(localdb)\MSSQLLocalDB'
$output=Join-Path $repo ('TestResults\Withholding-'+$suffix)
[void](New-Item -ItemType Directory -Path $output)
$cx=New-Object System.Data.SqlClient.SqlConnection "Server=$server;Database=master;Integrated Security=True;Pooling=False;Connect Timeout=20"
$form=$null;$hostForm=$null;$checks=0;$saved=@{};$created=$false
function Sql($sql){$c=$cx.CreateCommand();try{$c.CommandText=$sql;$c.CommandTimeout=20;[void]$c.ExecuteNonQuery()}finally{$c.Dispose()}}
function Check($pass,$name){if(-not $pass){throw "FAILED: $name"};$script:checks++;"PASS: $name"}
function Control($name){$form.GetType().GetProperty($name,$instance).GetValue($form,$null)}
function Render { $null=$form.GetType().GetMethod('UpdateWithholdingSuggestions',$instance).Invoke($form,$null) }
function State($name,$value){$module.GetField($name,$static).SetValue($null,$value)}
try{
 $cx.Open();Sql "CREATE DATABASE [$db]";$created=$true;$cx.ChangeDatabase($db)
 Sql @'
CREATE TABLE dbo.Customer(ID int PRIMARY KEY,CustomNumber3 float NOT NULL,CustomNumber4 float NOT NULL,CustomNumber5 float NOT NULL,CustomText2 nvarchar(30) NOT NULL);
CREATE TABLE dbo.CustomCaption(Style int NOT NULL,Caption nvarchar(80));
INSERT dbo.CustomCaption VALUES(2,N'W/Tax 15%'),(8,N'W/Tax 1%'),(9,N'W/Tax 2%'),(10,N'W/Tax 5%');
INSERT dbo.Customer VALUES(1,1,2,5,N'15'),(2,0,2,0,N''),(3,0,0,0,N''),(4,2,1,15,N'5');
INSERT dbo.Customer VALUES(8,0,0,0,N'');
'@
 # Replace only this test process's in-memory endpoint, before application SQL.
 foreach($name in @('db','dbnew','dbInitial')){
  $old=$module.GetField($name,$static).GetValue($null)
  if($null -ne $old){if($old.Connection.State -ne 'Closed'){throw 'Unexpected open application SQL connection'};$old.Dispose()}
 }
 foreach($name in @('dbs','dbn','dbu','dbp','iCusID','bTaxExcempt')){$saved[$name]=$module.GetField($name,$static).GetValue($null)}
 State 'dbs' ($server+';Integrated Security=True;Pooling=False;Application Name=WithholdingOwnedFixture')
 State 'dbn' $db; State 'dbu' '';State 'dbp' '';State 'iCusID' 0;State 'bTaxExcempt' $false
 $endpoint=[string]$module.GetMethod('DB_Conn',$static).Invoke($null,@('constr'))
 $builder=New-Object System.Data.SqlClient.SqlConnectionStringBuilder $endpoint
 if($builder.DataSource -ne $server -or $builder.InitialCatalog -ne $db -or -not $builder.IntegratedSecurity){throw 'Fixture endpoint guard failed'}
 foreach($name in @('db','dbnew','dbInitial')){$module.GetField($name,$static).SetValue($null,[WorkOrderEntry.ItemLookUpDataContext]::new($endpoint))}

 $form=[WorkOrderEntry.frmItemLookUp]::new()
 $form.GetType().GetField('isLoadingRecalledOrder',$instance).SetValue($form,$true)
 # Create only a native handle. Never call Show/OnLoad on frmItemLookUp.
 $null=[Windows.Forms.Control].GetMethod('CreateHandle',[Reflection.BindingFlags]'NonPublic,Instance').Invoke($form,$null)
 $hostForm=New-Object Windows.Forms.Form
 $hostForm.StartPosition='Manual';$hostForm.Location=New-Object Drawing.Point(-20000,-20000)
 $hostForm.ShowInTaskbar=$false
 $totals=Control 'panelTotal';$hostForm.Controls.Add($totals)
 $hostForm.ClientSize=New-Object Drawing.Size(1212,171)
 $hostForm.Show()
 $work=Control 'chkWorkOrder';$quote=Control 'chkQuote';$customer=Control 'txtCustomer'
 $sub=Control 'txtSub';$vat=Control 'txtVat';$total=Control 'txtTotal';$group=Control 'grpWithholding'
 $work.Checked=$true;$quote.Checked=$false
 State 'iCusID' 1;$customer.Text='Fixture customer'
 $sub.Text='982.14';$vat.Text='117.86';$total.Text='1,100.00';Render
 foreach($pair in @(@('lblWithholding1','1%: 9.82'),@('lblWithholding2','2%: 19.64'),@('lblWithholding5','5%: 49.11'),@('lblWithholding15','15%: 147.32'))){
  Check ((Control $pair[0]).Text -eq $pair[1] -and (Control $pair[0]).Visible) ('Separate enabled rate: '+$pair[1])
 }
 Check ($group.Visible -and (Control 'lblWithholdingBasis').Text -eq 'VAT-exclusive: 982.14') 'actual subtotal displayed as calculation basis'
 Check ($sub.Text -eq '982.14' -and $vat.Text -eq '117.86' -and $total.Text -eq '1,100.00') 'suggestions leave all three totals unchanged'
 Check (-not $group.TabStop -and -not (Control 'withholdingLayout').TabStop) 'no additional keyboard tab stops'

 foreach($width in @(1212,1766)){
  $hostForm.ClientSize=New-Object Drawing.Size($width,171)
  Render;$hostForm.PerformLayout();$totals.PerformLayout()
  $box=Control 'GroupBox2'
  Check ($group.Bottom -lt $box.Top -and $group.Width -ge ($totals.ClientSize.Width-12)) ('full-width strip above totals at panel width '+$width)
  Check ((Control 'lblWithholding1').Font.Bold -and (Control 'lblWithholding1').Font.Size -ge 13) ('larger bold suggestion amounts at panel width '+$width)
  $labels=@((Control 'lblWithholding1'),(Control 'lblWithholding2'),(Control 'lblWithholding5'),(Control 'lblWithholding15'))
  Check ($labels[0].Top -eq $labels[3].Top -and $labels[0].Right -le $labels[1].Left -and $labels[1].Right -le $labels[2].Left -and $labels[2].Right -le $labels[3].Left) ('four separate amounts share one row at panel width '+$width)
  $bmp=New-Object Drawing.Bitmap($totals.Width,$totals.Height)
  try{$totals.DrawToBitmap($bmp,(New-Object Drawing.Rectangle(0,0,$bmp.Width,$bmp.Height)));$bmp.Save((Join-Path $output ('Totals-'+$width+'.png')))}finally{$bmp.Dispose()}
 }

 # Every combination must pack left, including a sole 15% and customer changes.
 $layout=Control 'withholdingLayout'
 foreach($mask in 0..15){
  $n1=if($mask -band 1){1}else{0};$n2=if($mask -band 2){2}else{0}
  $n5=if($mask -band 4){5}else{0};$t15=if($mask -band 8){'15'}else{''}
  Sql "UPDATE dbo.Customer SET CustomNumber3=$n1,CustomNumber4=$n2,CustomNumber5=$n5,CustomText2=N'$t15' WHERE ID=8"
  State 'iCusID' 0;Render;State 'iCusID' 8;Render;$totals.PerformLayout()
  $names=@('lblWithholding1','lblWithholding2','lblWithholding5','lblWithholding15')
  $packed=$true;$nextColumn=0
  foreach($index in 0..3){
   $label=Control $names[$index];$enabled=($mask -band (1 -shl $index)) -ne 0
   if($enabled){$packed=$packed -and $label.Visible -and $layout.GetColumn($label) -eq $nextColumn;$nextColumn++}
   else{$packed=$packed -and -not $label.Visible}
  }
  Check ($packed -and $group.Visible -eq ($mask -ne 0)) ('enabled rates pack left without gaps for combination '+$mask)
  if($mask -eq 8){
   $bmp=New-Object Drawing.Bitmap($totals.Width,$totals.Height)
   try{$totals.DrawToBitmap($bmp,(New-Object Drawing.Rectangle(0,0,$bmp.Width,$bmp.Height)));$bmp.Save((Join-Path $output 'Only15-Left.png'))}finally{$bmp.Dispose()}
  }
 }
 State 'iCusID' 1;Render
 Check ($layout.GetColumn((Control 'lblWithholding1')) -eq 0 -and $layout.GetColumn((Control 'lblWithholding15')) -eq 3) 'all-rate customer restores original rate order after compact layouts'

 $form.ClientSize=New-Object Drawing.Size(1366,704);Render;$form.PerformLayout()
 Check ((Control 'gridSelectItem').Height -ge 75 -and (Control 'panelItem').Height -ge (Control 'Panel5').Height) '1366x768 retains selected-item viewport and customer controls with strip visible'

 # A changed database row is not fetched again for every quantity/price edit.
 Sql 'UPDATE dbo.Customer SET CustomNumber3=0 WHERE ID=1'
 $sub.Text='1,100.00';$vat.Text='0.00';Render
 Check ((Control 'lblWithholding1').Text -eq '1%: 11.00') 'tax-exempt actual subtotal used without dividing by 1.12; cached rates reused'
 $sub.Text='3.33';Render
 Check ((Control 'lblWithholding15').Text -eq '15%: 0.50') 'fractional amount refresh'
 $sub.Text='123,456,789.00';Render
 Check ((Control 'lblWithholding15').Text -eq '15%: 18,518,518.35') 'large amount retains complete suggestion text'
 Check ((Control 'withholdingToolTip').GetToolTip((Control 'lblWithholding15')).Contains('18,518,518.35')) 'full large amount available in tooltip'
 $hostForm.ClientSize=New-Object Drawing.Size(1212,171);Render;$totals.PerformLayout()
 $largeFits=$true
 foreach($name in @('lblWithholding1','lblWithholding2','lblWithholding5','lblWithholding15')){
  $label=Control $name;$needed=[Windows.Forms.TextRenderer]::MeasureText($label.Text,$label.Font)
  $largeFits=$largeFits -and ($label.ClientSize.Width-$label.Padding.Horizontal -ge $needed.Width)
 }
 Check $largeFits 'large bold amounts fit without truncation at 1366-screen panel width'

 # Same display name, different customer ID: the explicit recalculation hook reloads.
 State 'iCusID' 2;Render
 Check ((Control 'lblWithholding2').Visible -and -not (Control 'lblWithholding1').Visible -and -not (Control 'lblWithholding5').Visible -and -not (Control 'lblWithholding15').Visible) 'customer switch clears previous enabled rates'
 State 'iCusID' 3;Render;Check (-not $group.Visible) 'no enabled flags hides suggestions'
 Check ((Control 'panelTotalsContainer').Height -eq 79 -and (Control 'GroupBox2').Top -eq 9) 'no flags restores compact footer and original totals position'
 State 'iCusID' 4;Render;Check (-not $group.Visible) 'incorrect flag values do not enable rates'
 State 'iCusID' 999;Render;Check (-not $group.Visible) 'missing customer clears suggestions'

 Sql "UPDATE dbo.CustomCaption SET Caption=N'Credit Terms:' WHERE Style=9"
 State 'iCusID' 2;Render;Check (-not $group.Visible) 'unrelated branch caption is not treated as withholding'
 Sql "UPDATE dbo.CustomCaption SET Caption=N'W/Tax 2%' WHERE Style=9"
 State 'iCusID' 0;$customer.Text='';Render
 State 'iCusID' 2;$customer.Text='Fixture customer';$sub.Text='500.00';Render
 Check ((Control 'lblWithholding2').Text -eq '2%: 10.00' -and $group.Visible) 'reset then customer selection reloads current fields'
 $quote.Checked=$true;Render;Check (-not $group.Visible) 'quotation has no withholding display'
 Check ((Control 'panelTotalsContainer').Height -eq 79) 'quotation restores compact footer'
 $work.Checked=$true;Render;Check $group.Visible 'switching back to work order restores suggestions'
 $sub.Text='not a number';Render;Check (-not $group.Visible) 'invalid subtotal cannot leave a stale estimate visible'
 $sub.Text='-1.00';Render;Check ((Control 'lblWithholding2').Text -eq '2%: -0.02') 'signed subtotal is preserved'

 # Exercise the actual totals caller used after imports, recall and item edits.
 $grid=Control 'gridSelectItem';$grid.AllowUserToAddRows=$false
 $row=$grid.Rows.Add();$grid.Rows[$row].Cells['Price'].Value=[decimal]1100
 $grid.Rows[$row].Cells['QTY'].Value=[decimal]1
 $grid.Rows[$row].Cells[10].Value=1
 $form.UpdateAmt()
 Check ($sub.Text -eq '982.14' -and $vat.Text -eq '117.86' -and $total.Text -eq '1,100.00') 'actual UpdateAmt preserves existing taxable totals'
 Check ((Control 'lblWithholding2').Text -eq '2%: 19.64') 'actual UpdateAmt refreshes suggested tender amount'
 State 'bTaxExcempt' $true;$form.UpdateAmt()
 Check ($sub.Text -eq '1,100.00' -and $vat.Text -eq '0.00' -and $total.Text -eq '1,100.00') 'actual UpdateAmt preserves existing tax-exempt totals'
 Check ((Control 'lblWithholding2').Text -eq '2%: 22.00') 'tax-exempt recalculation refreshes suggestion using actual subtotal'
 $grid.Rows.Clear();$form.UpdateAmt()
 Check ((Control 'lblWithholding2').Text -eq '2%: 0.00') 'removing all items clears previous amount'
 State 'iCusID' 0;$customer.Text='';Render;Check (-not $group.Visible) 'clearing customer clears suggestions'
 Check ((Get-FileHash ($exe+'.config')).Hash -eq $configHash) 'installed store configuration unchanged'
 "PASS: $checks localhost fixture/UI checks; screenshots: $output"
}finally{
 if($hostForm){$hostForm.Dispose()};if($form){$form.Dispose()}
 foreach($name in @('db','dbnew','dbInitial')){
  $field=$module.GetField($name,$static);$value=$field.GetValue($null)
  if($null -ne $value){if($value.Connection.State -ne 'Closed'){ $value.Connection.Close() };$value.Dispose()};$field.SetValue($null,$null)
 }
 foreach($name in $saved.Keys){$module.GetField($name,$static).SetValue($null,$saved[$name])}
 if($cx.State -eq 'Open' -and $created){$cx.ChangeDatabase('master');Sql "ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]"};$cx.Dispose()
 if((Get-FileHash ($exe+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
