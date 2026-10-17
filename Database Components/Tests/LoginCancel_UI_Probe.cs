// Separate x86 STA process: fake host, actual login callbacks and actual prompt.
// Login.OnLoad is intentionally suppressed; no authentication/SQL/config writes.
using System;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using WorkOrderEntry;
internal static class LoginCancelProbe {
    internal static string Mode;
    internal static Exception Failure;
    internal static DialogResult Result;
    internal static int Prompts;
    [STAThread] private static int Main(string[] args) {
        Mode=args[0];
        try {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            using(var host=new TestHost()) Application.Run(host);
            if(Failure!=null) { Console.WriteLine("ERROR: "+Failure.GetType().Name+": "+Failure.Message);return 2; }
            var expected=Mode=="Success"?DialogResult.OK:DialogResult.Cancel;
            if(Result!=expected || Prompts!=1) throw new Exception("Unexpected dialog result or closing prompt count: "+Result+" / "+Prompts);
            Console.WriteLine("PASS: "+Mode+" returned "+Result+"; one custom closing prompt; normal host exit.");
            return 0;
        } catch(Exception ex) {Console.WriteLine("ERROR: "+ex);return 3;}
    }
    private sealed class SafeLogin:frmLogin {
        protected override void OnLoad(EventArgs e) { /* Suppress real SQL login setup. */ }
    }
    private sealed class TestHost:Form {
        internal TestHost(){ShowInTaskbar=false;Opacity=0;}
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            BeginInvoke(new Action(()=>{
                using(var login=new SafeLogin()) using(var trigger=new Timer()) {
                    login.ShowInTaskbar=false;login.Opacity=0;trigger.Interval=50;
                    trigger.Tick+=(s,a)=>{
                        trigger.Stop();
                        try {
                            if(Mode=="WindowClose") login.Close();
                            else typeof(frmLogin).GetMethod(Mode=="Success"?"Timer1_Tick":"Button2_Click",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(login,new object[]{login,EventArgs.Empty});
                        }catch(Exception ex){while(ex.InnerException!=null)ex=ex.InnerException;Failure=ex;login.DialogResult=DialogResult.Cancel;login.Close();}
                    };
                    trigger.Start();Result=login.ShowDialog(this);
                }
                if(!IsDisposed) Close();
            }));
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            Prompts++;
            using(var dismiss=new Timer()) {
                dismiss.Interval=20;
                dismiss.Tick+=(s,a)=>{
                    // Snapshot before closing the synthetic confirmation.
                    var forms=Application.OpenForms.Cast<Form>().ToArray();
                    foreach(var f in forms)if(f is frmAppPrompt){f.DialogResult=DialogResult.OK;dismiss.Stop();break;}
                };
                dismiss.Start();
                AppPrompt.Show(this,"Synthetic draft warning; no draft is written.","Regression fixture",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            base.OnFormClosing(e);
        }
    }
}

