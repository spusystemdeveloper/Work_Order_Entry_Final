# Presentation only: no form Load, SQL, editing, selection or save actions.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app = Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash = (Get-FileHash -LiteralPath ($app + '.config')).Hash
$assembly = [Reflection.Assembly]::LoadFrom($app)
$flags = [Reflection.BindingFlags]'Public,NonPublic,Instance'
$form = [Activator]::CreateInstance($assembly.GetType('WorkOrderEntry.frmItemLookUp', $true))
$checks = 0
function Check($ok, $message) { if (!$ok) { throw $message }; $script:checks++ }
function C($name) { $form.GetType().GetProperty($name, $flags).GetValue($form, $null) }
try {
    $form.GetType().GetField('isLoadingRecalledOrder', $flags).SetValue($form, $true)
    $selected = C 'gridSelectItem'; $lookup = C 'gridItem'
    $longName = 'SYNTHETIC ITEM ' + ('LONG DESCRIPTION ' * 25)
    $rowIndex = $selected.Rows.Add()
    $row = $selected.Rows[$rowIndex]
    $row.Cells[1].Value = $longName
    $row.Cells[3].Value = [decimal]1234567.89
    $row.Cells[9].Value = [decimal]2000000
    $row.Cells[5].Value = [decimal]1543209.8625
    $row.DefaultCellStyle.BackColor = [Drawing.Color]::LightYellow
    $row.Cells[3].Style.BackColor = [Drawing.Color]::Orange
    $row.Cells[3].ToolTipText = 'Price Alert: Selling price is below minimum cost threshold.'
    $culture = [Globalization.CultureInfo]::CurrentCulture
    foreach ($runtime in @($false, $true)) {
        if ($runtime) {
            $form.GetType().GetMethod('FormatGrid', $flags).Invoke($form, $null) | Out-Null
            $form.GetType().GetMethod('GridColumnWidth', $flags).Invoke($form, $null) | Out-Null
        }
        foreach ($value in @([decimal]0, [decimal]1.25, [decimal]-300.125, [decimal]1234567.25)) {
            $row.Cells[2].Value = $value
            $text = [string]$row.Cells[2].FormattedValue
            Check ([decimal]::Parse($text, [Globalization.NumberStyles]::Number, $culture) -eq $value) 'Quantity display lost a fractional value'
            Check ($row.Cells[2].Value -eq $value) 'Display changed the stored quantity'
            $available = $value.ToString($lookup.Columns[4].DefaultCellStyle.Format, $culture)
            Check ([decimal]::Parse($available, [Globalization.NumberStyles]::Number, $culture) -eq $value) 'Available quantity display lost a fractional value'
        }
        foreach ($pair in @(@($selected, @(2,3,5)), @($lookup, @(2,4,17,18)))) {
            foreach ($index in $pair[1]) {
                Check ($pair[0].Columns[$index].AutoSizeMode -eq [Windows.Forms.DataGridViewAutoSizeColumnMode]::DisplayedCells) 'Numeric sizing must use only displayed cells'
            }
        }
        Check ($row.Cells[3].Value -eq [decimal]1234567.89 -and $row.Cells[5].Value -eq [decimal]1543209.8625) 'Price or calculated total value changed'
        Check ($row.Cells[3].FormattedValue -eq ([decimal]1234567.89).ToString('C', $culture)) 'Currency presentation changed'
        Check ($row.Cells[1].Value -eq $longName -and $lookup.ShowCellToolTips -and $selected.ShowCellToolTips) 'Long description or native tooltips changed'
        Check ($row.DefaultCellStyle.BackColor -eq [Drawing.Color]::LightYellow -and $row.Cells[3].Style.BackColor -eq [Drawing.Color]::Orange -and $row.Cells[3].ToolTipText -eq 'Price Alert: Selling price is below minimum cost threshold.') 'Existing warning style or tooltip changed'
        Check (!$row.Cells[2].ReadOnly -and !$row.Cells[3].ReadOnly -and $row.Cells[5].ReadOnly -and $lookup.ReadOnly) 'Edit permissions changed'
        Check ($selected.SelectionMode -eq [Windows.Forms.DataGridViewSelectionMode]::CellSelect) 'Editing cell selection mode changed'
    }
    "PASS: $checks isolated item-grid display checks. No SQL, editing or saved-order changes."
} finally {
    $form.Dispose()
    if ((Get-FileHash -LiteralPath ($app + '.config')).Hash -ne $configHash) { throw 'Store configuration changed' }
}
