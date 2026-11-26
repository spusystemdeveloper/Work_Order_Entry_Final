// Isolated x86 STA probe. Override Load to prevent SQL/selection setup.
using System;
using System.Windows.Forms;
using WorkOrderEntry;
internal static class LookupKeyboardProbe {
    private sealed class Customer : frmCustomer {
        protected override void OnLoad(EventArgs e) { }
    }
    private sealed class SalesRep : frmSalesRep {
        protected override void OnLoad(EventArgs e) { }
    }
    [STAThread] private static int Main(string[] args) {
        bool legacy = args.Length > 0 && args[0] == "Legacy";
        try {
            foreach (bool customer in new[] { true, false }) {
                foreach (string name in new[] { "cmdOk", "cmdCancel" }) {
                    using (Form form = customer ? (Form)new Customer() : new SalesRep()) {
                        form.TopMost = false;
                        form.ShowInTaskbar = false;
                        form.Opacity = 0;
                        form.Show();
                        Application.DoEvents();
                        var button = (Button)form.Controls[name];
                        button.Focus();
                        if (!button.Focused) throw new Exception("Button did not receive focus");
                        var message = Message.Create(button.Handle, 0x100, (IntPtr)Keys.Escape, IntPtr.Zero);
                        bool handled = button.PreProcessMessage(ref message);
                        if (legacy) {
                            if (handled || form.DialogResult != DialogResult.None)
                                throw new Exception("Original Escape failure was not reproduced");
                        } else if (!handled || form.DialogResult != DialogResult.Cancel) {
                            throw new Exception("Escape failed to route to Cancel from " + name);
                        }
                        if (form.AcceptButton != null) throw new Exception("Enter action changed");
                        Console.WriteLine((legacy ? "CONFIRMED ORIGINAL ISSUE: " : "PASS: ") +
                            (customer ? "Customer" : "Sales Rep") + " Escape routing from " + name);
                    }
                }
            }
            return 0;
        } catch (Exception ex) { Console.WriteLine(ex); return 1; }
    }
}
