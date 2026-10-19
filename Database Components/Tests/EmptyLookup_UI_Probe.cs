// Isolated x86 STA process: construct forms only; never Load or select a real row.
using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using WorkOrderEntry;
internal static class EmptyLookupProbe {
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static int checks;
    private static int prompts;
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
    private static void Call(Form f, string method, params object[] args) {
        try { f.GetType().GetMethod(method, Flags).Invoke(f, args); }
        catch (TargetInvocationException ex) { throw ex.InnerException; }
    }
    private static object Enter(string name) {
        return name == "frmRecall" ? (object)new KeyEventArgs(Keys.Enter) : new KeyPressEventArgs((char)13);
    }
    [STAThread] private static int Main(string[] args) {
        bool legacy = args.Length > 0 && args[0] == "Legacy";
        try {
            if (!legacy) {
                // Legacy globals construct contexts on first access. They must
                // still be closed; remove them before invoking any callback.
                var module = typeof(frmCustomer).Assembly.GetType("WorkOrderEntry.ModConnectDB");
                foreach (var name in new[] { "db", "dbnew", "dbInitial" }) {
                    var field = module.GetField(name, BindingFlags.Public | BindingFlags.Static);
                    var context = field.GetValue(null);
                    var connection = (IDbConnection)context.GetType().GetProperty("Connection").GetValue(context, null);
                    Check(connection.State == ConnectionState.Closed, "Unexpected open connection");
                    field.SetValue(null, null);
                    ((IDisposable)context).Dispose();
                }
            }
            using (var dismiss = new Timer()) {
                dismiss.Interval = 20;
                dismiss.Tick += (s, e) => {
                    var forms = new Form[Application.OpenForms.Count];
                    for (int i = 0; i < forms.Length; i++) forms[i] = Application.OpenForms[i];
                    foreach (var f in forms) if (f is frmAppPrompt) { prompts++; f.DialogResult = DialogResult.OK; }
                };
                dismiss.Start();
                foreach (var name in new[] { "frmCustomer", "frmSalesRep", "frmRecall" }) {
                    using (Form form = (Form)Activator.CreateInstance(typeof(frmCustomer).Assembly.GetType("WorkOrderEntry." + name))) {
                        string gridName = name == "frmCustomer" ? "gridCustomer" : name == "frmSalesRep" ? "gridSalesRep" : "gridOrder";
                        var grid = (DataGridView)form.GetType().GetProperty(gridName, Flags).GetValue(form, null);
                        string key = gridName + (name == "frmRecall" ? "_KeyDown" : "_KeyPress");
                        if (legacy) {
                            string method = name == "frmCustomer" ? "GetCustomerInfo" : name == "frmSalesRep" ? "GetSalesRepInfo" : key;
                            Exception failure = null;
                            try {
                                if (name == "frmRecall") Call(form, method, grid, Enter(name));
                                else Call(form, method, 0);
                            } catch (Exception ex) { failure = ex; }
                            Check(failure is NullReferenceException || failure is ArgumentOutOfRangeException, "Expected empty-row failure was not reproduced");
                            Console.WriteLine("CONFIRMED ORIGINAL ISSUE: " + name + " " + failure.GetType().Name);
                            continue;
                        }
                        form.BindingContext = new BindingContext();
                        for (int filtered = 0; filtered < 2; filtered++) {
                            if (filtered == 1) {
                                var table = new DataTable();
                                table.Columns.Add("ProbeID", typeof(int));
                                table.Rows.Add(123);
                                table.DefaultView.RowFilter = "ProbeID = -1";
                                grid.DataSource = table.DefaultView;
                            }
                            Check(grid.Rows.Count == 0 && grid.CurrentRow == null, "Fixture is not empty");
                            string ok = name == "frmRecall" ? "cmdOK_Click" : "cmdOk_Click";
                            string twice = name == "frmCustomer" ? "gridCustomer_CellDoubleClick" : gridName + "_DoubleClick";
                            Call(form, ok, grid, EventArgs.Empty);
                            Call(form, key, grid, Enter(name));
                            Call(form, twice, grid, name == "frmCustomer" ? (object)new DataGridViewCellEventArgs(-1, -1) : EventArgs.Empty);
                            Check(!form.IsDisposed && form.DialogResult == DialogResult.None && prompts == 0, "Empty selection closed the form or raised an error");
                            if (filtered == 1) {
                                ((DataView)grid.DataSource).RowFilter = "";
                                Check(grid.Rows.Count == 1, "Results could not be restored after empty selection");
                            }
                        }
                        if (name == "frmRecall") Call(form, "frmRecall_KeyDown", form, new KeyEventArgs(Keys.Escape));
                        else Call(form, key, grid, new KeyPressEventArgs((char)27));
                        Check(form.IsDisposed, "Escape failed to close empty-results test form");
                        Console.WriteLine("PASS: " + name + " empty/filtered OK, Enter, double-click, recovery and Escape");
                    }
                }
            }
            Check(prompts == 0, "Unexpected error prompt");
            Console.WriteLine("PASS: " + checks + " isolated checks; no form Load, SQL or real-row selection.");
            return 0;
        } catch (Exception ex) { Console.WriteLine(ex); return 1; }
    }
}
