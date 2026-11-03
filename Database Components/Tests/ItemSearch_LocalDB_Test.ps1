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
 // Drives the actual Enter and Search button handlers with the real generated SQL.
 static void SearchBy(Form f,MethodInfo action,object[] arguments,string typed,string[] unexpected,BindingFlags instanceFlags){
  var box=(TextBox)f.GetType().GetProperty("txtSearch",instanceFlags).GetValue(f,null);
  var tree=(TreeView)f.GetType().GetProperty("TreeView1",instanceFlags).GetValue(f,null);
  tree.Nodes.Clear();box.Text=typed;
  using(var timer=new Timer{Interval=25}){
   timer.Tick+=(s,e)=>{foreach(Form open in Application.OpenForms){
    if(open==f)continue;
    if(open.Text=="Item search"){open.Location=new Point(-30000,-30000);continue;}
    unexpected[0]=open.Text;open.Close();
   }};
   timer.Start();action.Invoke(f,arguments);timer.Stop();
  }
 }
 static void CheckApostropheSearch(Form f,Assembly a,Type m,BindingFlags instanceFlags){
  var flags=BindingFlags.Public|BindingFlags.Static;
  var query=a.GetType("WorkOrderEntry.ItemSearchQuery",true).GetMethod("Build",flags);
  var load=m.GetMethods(flags);MethodInfo loadData=null;foreach(var candidate in load)if(candidate.Name=="load_data"&&candidate.GetParameters().Length==1)loadData=candidate;
  var rows=(DataTable)loadData.Invoke(null,new object[]{query.Invoke(null,new object[]{"MEN'S"})});
  Check(rows.Rows.Count==1 && (string)rows.Rows[0]["ItemLookupCode"]=="MENS1","Generated SQL finds an apostrophe description");
  rows=(DataTable)loadData.Invoke(null,new object[]{query.Invoke(null,new object[]{"MEN'S,SHIRT"})});
  Check(rows.Rows.Count==1,"Apostrophe keyword combines with another keyword");
  rows=(DataTable)loadData.Invoke(null,new object[]{query.Invoke(null,new object[]{"x'; DROP TABLE dbo.SearchFixture; --"})});
  Check(rows.Rows.Count==0,"Quoted text stays a search value");
  rows=(DataTable)loadData.Invoke(null,new object[]{"SELECT ID FROM dbo.SearchFixture"});
  Check(rows.Rows.Count==5,"Fixture table survives a quoted search");
  var grid=(DataGridView)f.GetType().GetProperty("gridItem",instanceFlags).GetValue(f,null);
  var unexpected=new string[1];
  SearchBy(f,f.GetType().GetMethod("txtSearch_KeyPress",instanceFlags),new object[]{null,new KeyPressEventArgs((char)Keys.Enter)},"MEN'S",unexpected,instanceFlags);
  Check(unexpected[0]==null,"Enter shows no message for an apostrophe: "+unexpected[0]);
  Check(grid.RowCount==1,"Enter finds the apostrophe item");
  SearchBy(f,f.GetType().GetMethod("cmdSearch_Click",instanceFlags),new object[]{null,EventArgs.Empty},"MEN'S",unexpected,instanceFlags);
  Check(unexpected[0]==null,"Search button shows no message for an apostrophe: "+unexpected[0]);
  Check(grid.RowCount==1,"Search button finds the apostrophe item");
 }
 static int WithDialogsClosed(Form f,Action action){
  int closed=0;
  using(var timer=new Timer{Interval=25}){
   timer.Tick+=(s,e)=>{var open=new System.Collections.ArrayList(Application.OpenForms);
    foreach(Form dialog in open){if(dialog==f)continue;closed++;dialog.Close();}};
   timer.Start();action();timer.Stop();
  }
  return closed;
 }
 static void CheckFailedQueryKeepsSession(Form f,Type m){
  var flags=BindingFlags.Public|BindingFlags.Static;MethodInfo loadData=null;
  foreach(var candidate in m.GetMethods(flags))if(candidate.Name=="load_data"&&candidate.GetParameters().Length==1)loadData=candidate;
  DataTable rows=null;
  int dialogs=WithDialogsClosed(f,()=>{rows=(DataTable)loadData.Invoke(null,new object[]{"SELECT * FROM dbo.MissingFixtureTable"});});
  Check(dialogs>=1,"A failed query still tells the user");
  Check(rows!=null&&rows.Rows.Count==0,"A failed query returns an empty result");
  Check(!f.IsDisposed,"A failed query leaves the order form open");
 }
 static void CheckBlankApprovalRejected(Assembly a){
  var confirm=a.GetType("WorkOrderEntry.clsItemLookUp",true).GetMethod("ConfPass",BindingFlags.Public|BindingFlags.Static);
  // No SOD_WO_Conf table exists here, so reaching SQL would show an error dialog.
  Check((int)confirm.Invoke(null,new object[]{""})==0,"A blank approval password is rejected before SQL");
  Check((int)confirm.Invoke(null,new object[]{"   "})==0,"An all-spaces approval password is rejected before SQL");
 }
 static void CheckFormClearsAfterCommittedSave(Form f,Assembly a,Type m,BindingFlags instanceFlags){
  var flags=BindingFlags.Public|BindingFlags.Static;
  var grid=(DataGridView)f.GetType().GetProperty("gridSelectItem",instanceFlags).GetValue(f,null);
  var label=f.GetType().GetProperty("lblOrderNo",instanceFlags).GetValue(f,null);
  var text=label.GetType().GetProperty("Text");
  grid.AllowUserToAddRows=false;grid.Rows.Clear();grid.Rows.Add();text.SetValue(label,"Work Order #: 5",null);
  var dbField=m.GetField("db",flags);var working=dbField.GetValue(null);
  var broken=Activator.CreateInstance(a.GetType("WorkOrderEntry.ItemLookUpDataContext",true),new object[]{"Server=(localdb)\\Missing;"});
  ((IDisposable)broken).Dispose();dbField.SetValue(null,broken);
  try{WithDialogsClosed(f,()=>f.GetType().GetMethod("ClearFormAfterCommittedSave",instanceFlags).Invoke(f,null));}
  finally{dbField.SetValue(null,working);}
  Check(grid.Rows.Count==0,"A saved order leaves the screen when the refresh fails");
  Check((string)text.GetValue(label,null)=="Order #: ","A saved order number is cleared when the refresh fails");
 }
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
   CheckApostropheSearch(f,a,m,instanceFlags);
   CheckFailedQueryKeepsSession(f,m);
   CheckBlankApprovalRejected(a);
   CheckFormClearsAfterCommittedSave(f,a,m,instanceFlags);
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
(4,'55157OLD','55157OLD | INACTIVE PAPER',1,1,5,1),(5,'MENS1','MENS1 | MEN''S SHIRT',0,250,4,0);
'@;$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
    $cmd=$connection.CreateCommand()
    try{$cmd.CommandText='CREATE VIEW dbo.SOD_VIEWITEMSWO AS SELECT * FROM dbo.SearchFixture;';$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
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
