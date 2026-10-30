using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

// No form Load, database connection, settings save or physical printing.
class ReleaseTypePromptProbe
{
    static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    static Form form;
    static readonly List<string> prompts = new List<string>();
    static int checks;
    static bool accept;
    static object Member(string name) { return form.GetType().GetProperty(name, Flags).GetValue(form, null); }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
    static void Invoke(string method) { form.GetType().GetMethod(method, Flags).Invoke(form, null); }
    [STAThread] static int Main(string[] args)
    {
        bool queued = bool.Parse(args[1]);
        using (var timer = new Timer { Interval = 20 })
        try
        {
            Check(ConfigurationManager.AppSettings["QueueingEnabled"] == queued.ToString(), "Owned probe configuration");
            var app = Assembly.LoadFrom(args[0]);
            form = (Form)Activator.CreateInstance(app.GetType("WorkOrderEntry.frmItemLookUp", true));
            form.GetType().GetField("isLoadingRecalledOrder", Flags).SetValue(form, true);
            var grid = (DataGridView)Member("gridSelectItem");
            Check(grid.Columns.Count == 20, "Selected grid has its real columns");
            Invoke("UpdateQueueingColumnVisibility");
            Check(((DataGridViewColumn)Member("chkPickLoc")).Visible == queued, "Pick location visibility follows queue setting");
            Check(((DataGridViewColumn)Member("CustPrep")).Visible == queued, "Prepared visibility follows queue setting");
            timer.Tick += delegate {
                foreach (Form dialog in Application.OpenForms.Cast<Form>().ToArray())
                {
                    if (dialog.GetType().Name != "frmAppPrompt") continue;
                    string text = ((Control)dialog.GetType().GetProperty("txtMessage", Flags).GetValue(dialog, null)).Text;
                    prompts.Add(text);
                    dialog.DialogResult = text.Contains("Please Re-Check") ? DialogResult.OK : accept ? DialogResult.Yes : DialogResult.No;
                    dialog.Close();
                }
            };
            timer.Start();
            foreach (int itemCount in new[] { 1, 6 })
            foreach (bool delivery in new[] { true, false }) foreach (bool confirmed in new[] { true, false })
            {
                grid.Rows.Clear();
                ((RadioButton)Member(delivery ? "rbtnPickup" : "rbtnDelivery")).Checked = true;
                for (int index = 0; index < itemCount; index++)
                {
                    var row = grid.Rows[grid.Rows.Add()];
                    row.Cells[0].Value = "LOCAL-ITEM-" + index; row.Cells[2].Value = 3d; row.Cells[3].Value = 112d;
                    row.Cells[17].Value = false; row.Cells[18].Value = 2;
                }
                prompts.Clear(); accept = confirmed;
                // Switch the actual radios: both CheckedChanged events fire.
                // Calling releaseType directly would miss duplicate event calls.
                var selected = (RadioButton)Member(delivery ? "rbtnDelivery" : "rbtnPickup");
                selected.Checked = true;
                Check(prompts.Count == (queued && !delivery && confirmed ? 2 : 1), "Expected confirmation/reminder count");
                string expected = delivery ? queued ? "Changing Release Type to Delivery will Automatically be Picked in UP-STORE\r\n\r\nDo You want to Change ?" : "Do You want to Change Release Type to Delivery?" : "Do You want to Change Release Type to Pick-up?";
                Check(prompts[0] == expected, "Correct release confirmation text");
                if (queued && !delivery && confirmed) Check(prompts[1] == "Please Re-Check the Pick Location and Prepared Qty.", "Queued pickup retains original reminder");
                if (!queued) Check(prompts.All(p => !p.Contains("UP-STORE") && !p.Contains("Prepared") && !p.Contains("Pick Location")), "Direct orders never mention hidden queue fields");
                foreach (DataGridViewRow row in grid.Rows)
                {
                    Check(Convert.ToDouble(row.Cells[2].Value) == 3d && Convert.ToDouble(row.Cells[3].Value) == 112d, "Quantity and price unchanged");
                    Check(Convert.ToInt32(row.Cells[18].Value) == 2, "Prepared value unchanged");
                    Check(Convert.ToBoolean(row.Cells[17].Value) == (confirmed && delivery), "Existing delivery pick-location behavior preserved");
                }
                Check((string)form.GetType().GetMethod("getReleaseType").Invoke(form, null) == (delivery ? "Delivery" : "Pick-up"), "Release type value preserved");
                prompts.Clear(); selected.Checked = true;
                Check(prompts.Count == 0, "Selecting the already-checked release type does not prompt again");
            }
            grid.Rows.Clear(); prompts.Clear();
            ((RadioButton)Member("rbtnDelivery")).Checked = !((RadioButton)Member("rbtnDelivery")).Checked;
            Check(prompts.Count == 0, "Empty order does not prompt");
            Console.WriteLine("PASS: " + checks + " release-type prompt/UI checks; QueueingEnabled=" + queued + "; no SQL or form Load.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { timer.Stop(); if (form != null) form.Dispose(); }
    }
}
