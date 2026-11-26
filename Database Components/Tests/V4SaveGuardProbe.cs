using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;
using WorkOrderEntry;

// Compiled legacy input gates and call ordering only: no app startup or SQL.
internal static class V4SaveGuardProbe
{
    private static int checks;
    private static void Check(bool pass, string message)
    {
        if (!pass) throw new Exception(message);
        checks++;
        Console.WriteLine("PASS: " + message);
    }

    [STAThread]
    private static int Main()
    {
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Check(!V4SaveValidation.CustomerSelected(0, "Named customer"), "name without selected ID rejected");
            Check(!V4SaveValidation.CustomerSelected(-1, "Named customer"), "negative selected ID rejected");
            Check(!V4SaveValidation.CustomerSelected(1, "   "), "blank selected name rejected");
            foreach (int type in new[] { 2, 3 })
            {
                Check(V4SaveValidation.HeaderIssue(0, "Name", 1, "Rep", "CHARGE", type, false, true) == "Customer", "new type " + type + " needs customer ID");
                Check(V4SaveValidation.HeaderIssue(1, "Name", 0, "Rep", "CHARGE", type, false, true) == "SalesRep", "new type " + type + " needs Sales Rep ID");
                Check(V4SaveValidation.HeaderIssue(1, "Name", 1, "Rep", "", type, false, true) == (type == 2 ? "Payment" : ""), "new type " + type + " preserves payment requirement");
                Check(V4SaveValidation.HeaderIssue(1, "Name", 1, "Rep", "CHARGE", type, false, false) == "Items", "empty new type " + type + " rejected");
                Check(V4SaveValidation.HeaderIssue(1, "Name", 1, "Rep", "CHARGE", type, false, true) == "", "valid new type " + type + " passes");
            }
            Check(V4SaveValidation.HeaderIssue(0, "", 0, "", "", 3, true, true) == "", "existing quotation optional headers preserved");
            Check(V4SaveValidation.HeaderIssue(0, "Name", 1, "Rep", "CHARGE", 2, true, true) == "Customer", "recalled work order requires selected customer");
            Check(V4SaveValidation.HeaderIssue(1, "Name", 1, "Rep", "", 2, true, true) == "Payment", "recalled work order requires payment");
            Check(V4SaveValidation.HeaderIssue(1, "Name", 1, "Rep", "CHARGE", 0, false, true) == "EntryType", "missing entry type rejected");

            using (var grid = NewGrid())
            {
                grid.AllowUserToAddRows = true;
                Check(!V4SaveValidation.HasItems(grid), "placeholder row is not an order item");
                grid.AllowUserToAddRows = false;
                AddValid(grid);
                Check(V4SaveValidation.HasItems(grid), "real row recognized");
                int row = -1, col = -1;
                Check(V4SaveValidation.RowIssue(grid, ref row, ref col) == "", "valid legacy row passes");
                foreach (var scenario in new[] {
                    new object[] {0, ""}, new object[] {11, 0}, new object[] {11, "not an ID"},
                    new object[] {2, null}, new object[] {2, DBNull.Value}, new object[] {2, "invalid"},
                    new object[] {2, 0}, new object[] {2, -1}, new object[] {2, double.NaN},
                    new object[] {2, double.PositiveInfinity}, new object[] {3, "-"},
                    new object[] {3, null}, new object[] {3, DBNull.Value}, new object[] {3, double.NaN},
                    new object[] {3, 1e18}, new object[] {9, "bad cost"}, new object[] {12, "bad full price"},
                    new object[] {18, "bad prepared"}, new object[] {18, 2147483648L}, new object[] {18, 3}
                })
                {
                    grid.Rows.Clear(); AddValid(grid); AddValid(grid);
                    int field = (int)scenario[0];
                    grid.Rows[1].Cells[field].Value = scenario[1];
                    Check(V4SaveValidation.RowIssue(grid, ref row, ref col) != "" && row == 1 && col == field,
                        "invalid second-row field " + field + " blocked before write: " + Convert.ToString(scenario[1]));
                }
                grid.Rows.Clear(); AddValid(grid);
                grid.Rows[0].Cells[2].Value = 2.5;
                grid.Rows[0].Cells[3].Value = 0;
                grid.Rows[0].Cells[18].Value = 2;
                Check(V4SaveValidation.RowIssue(grid, ref row, ref col) == "", "fractional quantity and zero price remain representable");
                grid.Rows[0].Cells[2].Value = 2.9;
                grid.Rows[0].Cells[18].Value = 3;
                Check(V4SaveValidation.RowIssue(grid, ref row, ref col) == "", "V4 integer prepared comparison retains its original quantity rounding");
                grid.Rows[0].Cells[9].Value = null;
                grid.Rows[0].Cells[12].Value = null;
                grid.Rows[0].Cells[18].Value = null;
                Check(V4SaveValidation.RowIssue(grid, ref row, ref col) == "", "legacy Nothing to numeric zero conversion retained for hidden defaults");
            }
            Check(V4SaveValidation.TotalsValid("117.86", "1,100.00"), "normal totals accepted without recalculation");
            Check(V4SaveValidation.TotalsValid("0", "0"), "zero totals not prohibited by new gate");
            Check(!V4SaveValidation.TotalsValid("not VAT", "1,100.00"), "invalid VAT blocked");
            Check(!V4SaveValidation.TotalsValid("0", "NaN"), "non-finite total blocked");
            Check(!V4SaveValidation.TotalsValid("0", "999999999999999999"), "SQL Money overflow blocked");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            Check(V4SaveValidation.TotalsValid("117,86", "1.100,00"), "legacy current-culture number conversion retained");

            var form = typeof(V4SaveValidation).Assembly.GetType("WorkOrderEntry.frmItemLookUp", true);
            var save = Calls(form, "SaveOrder");
            Check(save.IndexOf("ValidateSaveInputs") < save.IndexOf("EndEdit") && save.IndexOf("EndEdit") < save.LastIndexOf("ValidateSaveInputs") && save.LastIndexOf("ValidateSaveInputs") < save.IndexOf("ValidateAllQty"), "compiled save gates surround edit commit and precede quantity SQL");
            Check(Before(form, "InsertEntry", "ValidateSaveInputs", "SOD_sp_InsertQoute"), "compiled insert validates before header/ID allocation");
            Check(Before(form, "UpdateOrder", "ValidateSaveInputs", "UpdateOrder"), "compiled edit validates before legacy update");
            Check(Before(form, "SelectPrice", "CustomerSelected", "getItemQty"), "compiled Select/keyboard price path checks customer before lookup");
            Check(Calls(form, "cmdOk_Click").Contains("SaveOrder") && Calls(form, "frmItemLookUp_KeyDown").Contains("SaveOrder"), "Save button and F12 retain common guarded entry point");
            Console.WriteLine("PASS: " + checks + " V4 checks; no application form loaded, SQL connection, save or print executed.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static DataGridView NewGrid()
    {
        var grid = new DataGridView { AllowUserToAddRows = false };
        for (int i = 0; i < 19; i++) grid.Columns.Add("C" + i, "C" + i);
        return grid;
    }
    private static void AddValid(DataGridView grid)
    {
        object[] values = new object[19];
        values[0] = "SAMPLE"; values[2] = 2.0; values[3] = 112.0;
        values[9] = 50.0; values[11] = 1; values[12] = 112.0; values[18] = 0;
        grid.Rows.Add(values);
    }
    private static bool Before(Type form, string method, string gate, string operation)
    {
        var calls = Calls(form, method);
        return calls.IndexOf(gate) >= 0 && calls.IndexOf(operation) > calls.IndexOf(gate);
    }
    private static List<string> Calls(Type type, string name)
    {
        var method = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        var body = method.GetMethodBody().GetILAsByteArray();
        var codes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.FieldType == typeof(OpCode)).Select(x => (OpCode)x.GetValue(null))
            .ToDictionary(x => (ushort)x.Value);
        var result = new List<string>();
        for (int i = 0; i < body.Length; )
        {
            ushort number = body[i++];
            if (number == 0xfe) number = (ushort)(0xfe00 | body[i++]);
            var op = codes[number];
            if (op.OperandType == OperandType.InlineMethod) result.Add(method.Module.ResolveMethod(BitConverter.ToInt32(body, i)).Name);
            switch (op.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineBrTarget: case OperandType.ShortInlineI: case OperandType.ShortInlineVar: i++; break;
                case OperandType.InlineVar: i += 2; break;
                case OperandType.InlineI8: case OperandType.InlineR: i += 8; break;
                case OperandType.InlineSwitch: int count = BitConverter.ToInt32(body, i); i += 4 + count * 4; break;
                default: i += 4; break;
            }
        }
        return result;
    }
}
