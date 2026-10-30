using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

class FirstAccountProbe {
 static BindingFlags S=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic,I=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static Type module,users;static Form login;static int checks;static string mode;static int stage;static bool sawSetup,sawPrompt;
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS: "+label);}
 static object Sql(SqlConnection cx,string text){using(var c=cx.CreateCommand()){c.CommandText=text;return c.ExecuteScalar();}}
 static Control C(Form f,string name){return f.Controls.Find(name,true).Single();}
 static bool Has(){return (bool)users.GetMethod("HasAnyWOUsers",S).Invoke(null,null);}
 static bool Create(string username,string pass,int register){return (bool)users.GetMethod("CreateInitialWOUser",S).Invoke(null,new object[]{username,pass,register});}
 static void Rejected(Action action,string message){bool failed=false;try{action();}catch(TargetInvocationException){failed=true;}Check(failed,message);}
 static void Tick(object sender,EventArgs e){
  foreach(Form f in Application.OpenForms.Cast<Form>().ToArray()){
   if(f.GetType().Name=="frmAppPrompt") {sawPrompt=true;f.DialogResult=DialogResult.OK;f.Close();continue;}
   if(f.Text!="Create first Work Order account")continue;
   sawSetup=true;
   if(stage==0){stage=1;
    Check(((TextBox)C(f,"initialPassword")).UseSystemPasswordChar&&((TextBox)C(f,"initialConfirmation")).UseSystemPasswordChar,"Setup masks password and confirmation");
    Check(C(f,"initialPassword").Text==""&&C(f,"initialConfirmation").Text=="","Setup supplies no default password");
    Check(((NumericUpDown)C(f,"initialRegister")).Minimum==1,"Setup requires a positive register");
    if(mode=="Cancel"){f.DialogResult=DialogResult.Cancel;return;}
    C(f,"initialUsername").Text="fixture-admin";
    C(f,"initialPassword").Text=mode=="Blank"?"":"fixture-only-secret";
    C(f,"initialConfirmation").Text=mode=="Mismatch"?"different-fixture-secret":C(f,"initialPassword").Text;
    ((NumericUpDown)C(f,"initialRegister")).Value=9;
    ((Button)C(f,"createInitialUser")).PerformClick();
   }else if(sawPrompt&&mode!="Create"){f.DialogResult=DialogResult.Cancel;}
  }
 }
 [STAThread] static int Main(string[] args){string db="SOD_FirstAccount_"+Guid.NewGuid().ToString("N");bool created=false;
 using(var cx=new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False"))
 using(var timer=new Timer{Interval=30})
 try{
  cx.Open();Sql(cx,"CREATE DATABASE ["+db+"]");created=true;cx.ChangeDatabase(db);
  Sql(cx,"CREATE TABLE dbo.SOD_WO_User(UserID int IDENTITY PRIMARY KEY,UserFullName varchar(max),UserName varchar(max) NOT NULL,UserPass varchar(max) NOT NULL,UserRegister int);");
  var app=Assembly.LoadFrom(Path.GetFullPath(args[0]));module=app.GetType("WorkOrderEntry.ModConnectDB",true);users=app.GetType("WorkOrderEntry.clsUser",true);
  module.GetField("dbs",S).SetValue(null,"(localdb)\\MSSQLLocalDB;Integrated Security=True;Pooling=False");module.GetField("dbn",S).SetValue(null,db);module.GetField("dbu",S).SetValue(null,"");module.GetField("dbp",S).SetValue(null,"");
  string endpoint=(string)module.GetMethod("DB_Conn",S).Invoke(null,new object[]{"constr"});var target=new SqlConnectionStringBuilder(endpoint);
  Check(target.InitialCatalog==db&&target.DataSource=="(localdb)\\MSSQLLocalDB"&&target.IntegratedSecurity,"Owned fixture endpoint only");
  foreach(string field in new[]{"db","dbnew","dbInitial"}){var old=module.GetField(field,S).GetValue(null) as IDisposable;if(old!=null)old.Dispose();module.GetField(field,S).SetValue(null,Activator.CreateInstance(app.GetType("WorkOrderEntry.ItemLookUpDataContext",true),new object[]{endpoint}));}
  Check(!Has(),"Empty table recognized");
  Rejected(()=>Create("","fixture",1),"Empty username rejected");Rejected(()=>Create("fixture","   ",1),"Blank password rejected");Rejected(()=>Create("fixture","fixture",0),"Invalid register rejected");
  Check(Convert.ToInt32(Sql(cx,"SELECT COUNT(*) FROM SOD_WO_User"))==0,"Invalid inputs write no account");
  var a=Task.Run(()=>Create("fixture-a","fixture-only-a",1));var b=Task.Run(()=>Create("fixture-b","fixture-only-b",2));Task.WaitAll(a,b);
  Check(a.Result!=b.Result&&Convert.ToInt32(Sql(cx,"SELECT COUNT(*) FROM SOD_WO_User"))==1,"Competing first-account creations allow one winner only");
  string before=(string)Sql(cx,"SELECT * FROM SOD_WO_User FOR XML RAW;");
  Check(Has()&&!Create("replacement","fixture-only",3),"Existing users block initial creation");
  Check((string)Sql(cx,"SELECT * FROM SOD_WO_User FOR XML RAW;")==before,"Existing account values unchanged");
  Sql(cx,"DELETE SOD_WO_User;");
  login=(Form)Activator.CreateInstance(app.GetType("WorkOrderEntry.frmLogin",true));timer.Tick+=Tick;timer.Start();
  foreach(string scenario in new[]{"Cancel","Blank","Mismatch","Create"}){
   mode=scenario;stage=0;sawSetup=false;sawPrompt=false;
   login.GetType().GetMethod("Button1_Click",I).Invoke(login,new object[]{login,EventArgs.Empty});
   Check(sawSetup,scenario+" actual login opens first-account setup");
   Check(Convert.ToInt32(Sql(cx,"SELECT COUNT(*) FROM SOD_WO_User"))==(scenario=="Create"?1:0),scenario+" expected creation/cancel result");
  }
  Check((string)Sql(cx,"SELECT UserName FROM SOD_WO_User")=="fixture-admin"&&Convert.ToInt32(Sql(cx,"SELECT UserRegister FROM SOD_WO_User"))==9,"UI saves chosen username and register");
  Check(((TextBox)login.GetType().GetProperty("txtPassword",I).GetValue(login,null)).Text=="","Login password cleared after setup");
  Check((bool)users.GetMethod("verifyUsername",S).Invoke(null,new object[]{"fixture-admin"}),"New account uses existing username lookup");
  Check((string)users.GetMethod("getUsernamePassword",S).Invoke(null,new object[]{"fixture-admin"})=="fixture-only-secret","New account uses existing password lookup");
  sawSetup=false;sawPrompt=false;
  ((TextBox)login.GetType().GetProperty("txtUsername",I).GetValue(login,null)).Text="fixture-admin";
  ((TextBox)login.GetType().GetProperty("txtPassword",I).GetValue(login,null)).Text="wrong-fixture-value";
  login.GetType().GetMethod("Button1_Click",I).Invoke(login,new object[]{login,EventArgs.Empty});
  Check(!sawSetup&&sawPrompt&&Convert.ToInt32(Sql(cx,"SELECT COUNT(*) FROM SOD_WO_User"))==1,"Existing account keeps normal wrong-password check without setup");
  Sql(cx,"DROP TABLE SOD_WO_User;");Rejected(()=>Has(),"Missing schema propagates an error, never empty-user fallback");
  Console.WriteLine("PASS: "+checks+" checks; actual V8 Login/setup callers, concurrent creation, cancellation and validation. Only owned LocalDB writes; no normal application login/register changes.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e.GetType().Name+": test failed. "+e.Message);return 1;}
 finally{timer.Stop();if(login!=null)login.Dispose();if(module!=null)foreach(string n in new[]{"db","dbnew","dbInitial"}){var d=module.GetField(n,S).GetValue(null) as IDisposable;if(d!=null)d.Dispose();}SqlConnection.ClearAllPools();if(created){cx.ChangeDatabase("master");Sql(cx,"ALTER DATABASE ["+db+"] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE ["+db+"]");}}
 }
}
