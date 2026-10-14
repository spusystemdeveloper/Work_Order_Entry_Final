using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorkOrderDatabaseUpdater
{
    public sealed class UpdaterForm : Form
    {
        private readonly TextBox server = new TextBox { Text = "localhost", Width = 145 };
        private readonly TextBox database = new TextBox { Text = "GSC_STORE_DB", Width = 160 };
        private readonly ComboBox authentication = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly TextBox user = new TextBox { Width = 110, Enabled = false };
        private readonly TextBox password = new TextBox { Width = 110, UseSystemPasswordChar = true, Enabled = false };
        private readonly ComboBox profile = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells, BackgroundColor = Color.White };
        private readonly TextBox preview = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both,
            WordWrap = false, Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };
        private readonly TextBox messages = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
        private readonly Button check = new Button { Text = "Check target", AutoSize = true };
        private readonly Button apply = new Button { Text = "Review and apply...", AutoSize = true, Enabled = false };
        private readonly Label target = new Label { AutoSize = true, Text = "Check the target before applying. Nothing is selected automatically." };
        private readonly FlowLayoutPanel connections = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        private readonly FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        private bool busy;
        private string verifiedConnection;
        private string packageDirectory;

        public UpdaterForm()
        {
            Text = "Work Order Database Updater"; Width = 1180; Height = 820; MinimumSize = new Size(960, 680);
            StartPosition = FormStartPosition.CenterScreen; Font = new Font("Segoe UI", 9);
            authentication.Items.AddRange(new object[] { "Windows login", "SQL login" }); authentication.SelectedIndex = 0;
            profile.Items.AddRange(new object[] { "GSC", "DVO", "Portable" }); profile.SelectedIndex = 0;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { Text = "Select database changes, inspect the SQL, then review and apply. Windows administrator access is not required.",
                AutoSize = true, Padding = new Padding(0, 0, 0, 8) }, 0, 0);
            AddField(connections, "Server", server); AddField(connections, "Database", database);
            AddField(connections, "Authentication", authentication); AddField(connections, "User", user); AddField(connections, "Password", password);
            connections.Controls.Add(check); layout.Controls.Add(connections, 0, 1);
            AddField(actions, "Branch profile", profile);
            var reload = new Button { Text = "Reload package", AutoSize = true };
            var choose = new Button { Text = "Choose package folder...", AutoSize = true };
            var add = new Button { Text = "Add SQL script...", AutoSize = true };
            var clear = new Button { Text = "Clear selection", AutoSize = true };
            var help = new Button { Text = "Help / instructions...", AutoSize = true };
            actions.Controls.AddRange(new Control[] { reload, choose, add, clear, apply, help }); layout.Controls.Add(actions, 0, 2);
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 250 };
            split.Panel1.Controls.Add(grid); split.Panel2.Controls.Add(preview); layout.Controls.Add(split, 0, 3);
            layout.Controls.Add(messages, 0, 4); layout.Controls.Add(target, 0, 5); Controls.Add(layout);
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Selected", HeaderText = "Apply", Width = 45 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Script", HeaderText = "Script (runs in this order)", ReadOnly = true, Width = 280 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Type", ReadOnly = true, Width = 130 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Recorded", HeaderText = "Deployment record", ReadOnly = true, Width = 160 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Review notes", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            foreach (DataGridViewColumn column in grid.Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.CurrentCellDirtyStateChanged += (s, e) => { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
            grid.CellValueChanged += (s, e) => RefreshApply();
            grid.SelectionChanged += (s, e) => ShowSql();
            grid.CellClick += (s, e) => ShowSql();
            foreach (var field in new TextBox[] { server, database, user, password }) field.TextChanged += (s, e) => InvalidateTarget();
            authentication.SelectedIndexChanged += (s, e) => { user.Enabled = password.Enabled = authentication.SelectedIndex == 1; InvalidateTarget(); };
            profile.SelectedIndexChanged += (s, e) => LoadPackage();
            reload.Click += (s, e) => LoadPackage();
            choose.Click += (s, e) => {
                using (var dialog = new FolderBrowserDialog { Description = "Select the Database Components folder (contains Deployment and Migrations)." })
                    if (dialog.ShowDialog(this) == DialogResult.OK) { packageDirectory = dialog.SelectedPath; LoadPackage(); }
            };
            add.Click += (s, e) => {
                using (var dialog = new OpenFileDialog { Filter = "SQL scripts (*.sql)|*.sql" })
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        try { AddRow(UpdateScript.Load(dialog.FileName, "Custom/" + Path.GetFileName(dialog.FileName), "Custom SQL",
                            "Trusted SQL only; no USE, SQLCMD, or transaction commands. Test on a restored database first.")); }
                        catch (Exception ex) { ShowError(ex.Message); }
            };
            clear.Click += (s, e) => { foreach (DataGridViewRow row in grid.Rows) row.Cells[0].Value = false; };
            help.Click += (s, e) => ShowInstructions();
            check.Click += async (s, e) => await CheckTarget();
            apply.Click += async (s, e) => await ApplyPlan();
            FormClosing += (s, e) => { if (busy) { e.Cancel = true; MessageBox.Show(this, "Wait for the database operation to finish before closing."); } };
            Shown += (s, e) => { packageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database Components"); LoadPackage(); };
        }
        private static void AddField(FlowLayoutPanel panel, string caption, Control control)
        {
            panel.Controls.Add(new Label { Text = caption, AutoSize = true, Margin = new Padding(4, 7, 3, 0) });
            panel.Controls.Add(control);
        }
        private void AddRow(UpdateScript script)
        {
            if (grid.Rows.Cast<DataGridViewRow>().Any(r => ((UpdateScript)r.Tag).Id == script.Id))
                throw new InvalidOperationException("This script is already listed.");
            int row = grid.Rows.Add(false, Path.GetFileName(script.Path), script.Category, "Not checked", script.Notes);
            grid.Rows[row].Tag = script;
        }
        private void LoadPackage()
        {
            if (string.IsNullOrEmpty(packageDirectory)) return;
            try
            {
                var scripts = UpdateCatalog.Load(packageDirectory, profile.Text);
                grid.Rows.Clear(); foreach (var script in scripts) AddRow(script);
                ShowSql(); InvalidateTarget();
                messages.Text = "Package: " + packageDirectory + Environment.NewLine + "Profile: " + profile.Text +
                    ". Select only the changes this store needs. Object updates can replace branch business rules; inspect their SQL.";
            }
            catch (Exception ex) { grid.Rows.Clear(); InvalidateTarget(); ShowError(ex.Message); }
        }
        private void ShowSql()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Tag == null) return;
            var script = (UpdateScript)grid.CurrentRow.Tag;
            preview.Text = "-- " + script.Id + "\r\n-- SHA256: " + script.Hash + "\r\n" + script.Sql;
        }
        private string GetConnection() { return UpdateRunner.ConnectionString(server.Text, database.Text, user.Text, password.Text, authentication.SelectedIndex == 0); }
        private List<UpdateScript> Selection() { return grid.Rows.Cast<DataGridViewRow>().Where(r => Equals(r.Cells[0].Value, true)).Select(r => (UpdateScript)r.Tag).ToList(); }
        private void InvalidateTarget() { verifiedConnection = null; target.Text = "Target not checked. Check again after changing connection or package."; RefreshApply(); }
        private void RefreshApply() { apply.Enabled = !busy && verifiedConnection != null && Selection().Count > 0; }
        private void SetBusy(bool value)
        {
            busy = value; connections.Enabled = actions.Enabled = grid.Enabled = !value;
            UseWaitCursor = value; RefreshApply();
        }
        private void ShowError(string text) { messages.AppendText(Environment.NewLine + text); MessageBox.Show(this, text, "Database updater", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        private void ShowInstructions()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "USER_GUIDE.md");
                if (!File.Exists(path)) throw new FileNotFoundException("USER_GUIDE.md is missing. Extract the complete updater package beside the EXE.");
                using (var instructions = new Form { Text = "Work Order Database Updater — instructions", Width = 1000, Height = 780,
                    MinimumSize = new Size(700, 500), StartPosition = FormStartPosition.CenterParent })
                {
                    instructions.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical, WordWrap = true, Font = new Font("Consolas", 10), Text = File.ReadAllText(path) });
                    instructions.ShowDialog(this);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { ShowError(ex.Message); }
        }
        private async Task CheckTarget()
        {
            string connection = null;
            try
            {
                connection = GetConnection(); string expected = database.Text.Trim(); SetBusy(true);
                var info = await Task.Run(() => UpdateRunner.Check(connection, expected));
                verifiedConnection = connection; target.Text = info.Summary; messages.AppendText(Environment.NewLine + info.Summary);
                foreach (DataGridViewRow row in grid.Rows)
                {
                    var script = (UpdateScript)row.Tag; string recorded;
                    row.Cells[3].Value = info.RecordedHashes.TryGetValue(script.Id, out recorded) ?
                        (recorded == script.Hash ? "Recorded: same file" : "Recorded: file changed") : "No tool record";
                }
            }
            catch (Exception ex) { InvalidateTarget(); ShowError(connection == null ? ex.Message : UpdateRunner.DescribeError(ex, connection)); }
            finally { SetBusy(false); }
        }
        private async Task ApplyPlan()
        {
            string connection = null;
            try
            {
                grid.EndEdit(); connection = GetConnection();
                if (connection != verifiedConnection) throw new InvalidOperationException("Check the target again.");
                var scripts = Selection(); foreach (var script in scripts) script.EnsureUnchanged();
                string expected = database.Text.Trim();
                using (var review = new ReviewForm(server.Text.Trim(), expected, scripts))
                    if (review.ShowDialog(this) != DialogResult.OK) return;
                SetBusy(true); messages.AppendText(Environment.NewLine + "Applying selected scripts. Wait for the committed or failed result...");
                string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkOrderDatabaseUpdater", "Runs");
                var result = await Task.Run(() => UpdateRunner.Apply(connection, expected, scripts, root));
                messages.AppendText(Environment.NewLine + "COMMITTED. Run files: " + result.RunDirectory + Environment.NewLine + result.Warning);
                target.Text = "Update committed. Check target again to refresh deployment records.";
                verifiedConnection = null;
                MessageBox.Show(this, "Selected database changes committed.\r\n" + result.Warning + "\r\nRun files: " + result.RunDirectory,
                    "Update complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { InvalidateTarget(); ShowError(connection == null ? ex.Message : UpdateRunner.DescribeError(ex, connection)); }
            finally { SetBusy(false); }
        }
    }

    internal sealed class ReviewForm : Form
    {
        public ReviewForm(string server, string database, IList<UpdateScript> scripts)
        {
            Text = "Review database changes"; Width = 920; Height = 740; StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(760, 560); Font = new Font("Segoe UI", 9);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { Text = "Target: " + server + " / " + database + "\r\n" + scripts.Count +
                " script(s), in the order below. The whole selection uses one transaction. A SQL failure stops the update and rolls it back.\r\n" +
                "The schema snapshot is not a full database backup. Choose a quiet period for index changes.", AutoSize = true, MaximumSize = new Size(860, 0) }, 0, 0);
            var sql = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false,
                Font = new Font("Consolas", 10), Text = string.Join("\r\n\r\n", scripts.Select((s, i) => "-- " + (i + 1) + ". " + s.Id + "\r\n-- SHA256 " + s.Hash + "\r\n" + s.Sql)) };
            layout.Controls.Add(sql, 0, 1);
            var ready = new CheckBox { Text = "I reviewed the branch rules, tested these scripts, and have a current database backup.", AutoSize = true };
            layout.Controls.Add(ready, 0, 2);
            var entry = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            var confirm = new TextBox { Width = 240 }; entry.Controls.Add(new Label { Text = "Type target database: " + database, AutoSize = true }); entry.Controls.Add(confirm);
            layout.Controls.Add(entry, 0, 3);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var apply = new Button { Text = "Apply selected changes", AutoSize = true, Enabled = false, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
            EventHandler changed = (s, e) => apply.Enabled = ready.Checked && string.Equals(confirm.Text.Trim(), database, StringComparison.Ordinal);
            ready.CheckedChanged += changed; confirm.TextChanged += changed;
            buttons.Controls.Add(apply); buttons.Controls.Add(cancel); layout.Controls.Add(buttons, 0, 4);
            CancelButton = cancel; Controls.Add(layout);
        }
    }
}
