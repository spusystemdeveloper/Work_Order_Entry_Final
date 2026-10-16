# Real compiled search helper, owned LocalDB only. No real login or store writes.
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Data,System.Data.Linq,System.Windows.Forms
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$suffix=[guid]::NewGuid().ToString('N').Substring(0,8)
$instance="SODWO_Search_$suffix";$database=$instance
$directory=Join-Path $repo "TestResults\ItemSearch-$suffix"
$null=New-Item -ItemType Directory -Path $directory
$localdb=(Get-Command SqlLocalDB.exe -ErrorAction Stop).Source
$created=$false;$connection=$null;$master=$null
$code=@'
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
public class ItemSearchProbe {
 [DllImport("user32.dll")] static extern bool IsWindowEnabled(IntPtr handle);
 static int checks;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);checks++;}
 public static string Run(string app,string server,string database){
  Check(server.StartsWith("(localdb)\\SODWO_Search_") && database.StartsWith("SODWO_Search_"),"Only owned fixture endpoints");
  var a=Assembly.LoadFrom(app);var m=a.GetType("WorkOrderEntry.ModConnectDB",true);var flags=BindingFlags.Public|BindingFlags.Static;
  foreach(string name in new string[]{"db","dbnew","dbInitial"}){
   var old=m.GetField(name,flags).GetValue(null);var c=(IDbConnection)old.GetType().GetProperty("Connection").GetValue(old,null);
   Check(c.State==ConnectionState.Closed,"Legacy contexts have not opened SQL");((IDisposable)old).Dispose();
  }
  m.GetField("dbs",flags).SetValue(null,server+";Integrated Security=True;Pooling=False;Application Name=ItemSearchFixture");
  m.GetField("dbn",flags).SetValue(null,database);m.GetField("dbu",flags).SetValue(null,"");m.GetField("dbp",flags).SetValue(null,"");
  var cs=(string)m.GetMethod("DB_Conn",flags).Invoke(null,new object[]{"constr"});var builder=new SqlConnectionStringBuilder(cs);
  Check(builder.DataSource==server && builder.InitialCatalog==database && builder.IntegratedSecurity && !builder.Pooling,"Application SQL points only at owned fixture");
  foreach(string name in new string[]{"db","dbnew","dbInitial"})m.GetField(name,flags).SetValue(null,Activator.CreateInstance(a.GetType("WorkOrderEntry.ItemLookUpDataContext",true),new object[]{cs}));
  const string query="SELECT ID,ItemLookupCode,Price,Quantity,QuantityCommitted FROM dbo.SearchFixture WHERE Inactive=0 AND FullDesc LIKE '%55157%'";
  var expected=new DataTable();using(var c=new SqlConnection(cs))using(var da=new SqlDataAdapter(query,c)){c.Open();da.Fill(expected);}
  using(var f=(Form)Activator.CreateInstance(a.GetType("WorkOrderEntry.frmItemLookUp",true))){
   f.WindowState=FormWindowState.Normal;f.StartPosition=FormStartPosition.Manual;f.Location=new Point(-30000,-30000);
   var owner=f.Handle;var instanceFlags=BindingFlags.NonPublic|BindingFlags.Instance;
   int ticks=0;bool locked=false;bool closePrevented=false;bool positionSafe=true;
   using(var timer=new Timer{Interval=25}){
    timer.Tick+=(s,e)=>{
     ticks++;locked|=!IsWindowEnabled(owner);
     foreach(Form dialog in Application.OpenForms){if(dialog.Text=="Item search"){
      // CenterParent centers a dialog on-screen when its test owner is hidden.
      // Move this fixture window away; the application form is never shown.
      dialog.Location=new Point(-30000,-30000);
      positionSafe &= dialog.Left < -10000;
      if(ticks==3){dialog.Close();closePrevented=!dialog.IsDisposed && dialog.Visible;}
     }}
    };
    timer.Start();var result=(DataTable)f.GetType().GetMethod("LoadItemSearchData",instanceFlags).Invoke(f,new object[]{"WAITFOR DELAY '00:00:01'; "+query});timer.Stop();
    Check(positionSafe,"Progress fixture stays off-screen");
    Check(ticks>=5,"UI message timer continues while SQL executes");
    Check(locked && IsWindowEnabled(owner),"Modal search blocks owner actions then restores enabled state");
    Check(closePrevented,"Early close cannot return unfinished search results");
    Check(result.Rows.Count==expected.Rows.Count && result.Columns.Count==expected.Columns.Count,"Same contains-search row/column count");
    for(int row=0;row<result.Rows.Count;row++)for(int col=0;col<result.Columns.Count;col++)Check(Object.Equals(result.Rows[row][col],expected.Rows[row][col]),"Every returned value matches synchronous SQL");
    Check(Object.ReferenceEquals(m.GetField("sql_dt",flags).GetValue(null),result),"Existing result publication contract preserved");
    expected.Dispose();result.Dispose();
   }
  }
  foreach(string name in new string[]{"db","dbnew","dbInitial"})((IDisposable)m.GetField(name,flags).GetValue(null)).Dispose();
  return "PASS: "+checks+" isolated search checks; delayed SQL keeps UI messages flowing with identical results.";
 }
}
'@
try{
    & $localdb create $instance -s | Out-Null
    if($LASTEXITCODE -ne 0){throw 'LocalDB fixture creation failed'};$created=$true
    $server="(localdb)\$instance"
    $master=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $cmd=$master.CreateCommand()
    try{$cmd.CommandText="CREATE DATABASE [$database]; ALTER DATABASE [$database] SET COMPATIBILITY_LEVEL=130;";$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
    $connection=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=$database;Integrated Security=True;Pooling=False");$connection.Open()
    $cmd=$connection.CreateCommand()
    try{$cmd.CommandText=@'
CREATE TABLE dbo.SearchFixture(ID int PRIMARY KEY,ItemLookupCode varchar(25),FullDesc nvarchar(200),Inactive bit,Price decimal(19,4),Quantity float,QuantityCommitted float);
INSERT dbo.SearchFixture VALUES(1,'55157RMS','55157RMS | BOOK PAPER',0,178.25,10.5,2.25),
(2,'55157CSE','55157CSE | PAPER CASE',0,890,0,0),(3,'OTHER','OTHER | UNRELATED',0,1,5,1),
(4,'55157OLD','55157OLD | INACTIVE PAPER',1,1,5,1);
'@;$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
    $source=Join-Path $directory 'Probe.cs';$dll=Join-Path $directory 'Probe.dll'
    [IO.File]::WriteAllText($source,$code)
    & "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:library /platform:x86 /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ("/out:"+$dll) $source
    if($LASTEXITCODE -ne 0){throw 'Probe compile failed'}
    $null=[Reflection.Assembly]::LoadFrom($dll)
    $storeConfig=$app+'.config';$hash=(Get-FileHash -LiteralPath $storeConfig).Hash
    try{[ItemSearchProbe]::Run($app,$server,$database)}finally{if((Get-FileHash -LiteralPath $storeConfig).Hash -ne $hash){throw 'Store configuration changed'}}
    Write-Output "Artifacts: $directory"
}finally{
    if($connection){$connection.Dispose()}
    if($master){try{
        if($database -notmatch '^SODWO_Search_[0-9a-f]{8}$'){throw 'Unexpected fixture cleanup target'}
        $cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID('$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END;";$null=$cmd.ExecuteNonQuery();$cmd.Dispose()
    }finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k | Out-Null;& $localdb delete $instance | Out-Null}
}
