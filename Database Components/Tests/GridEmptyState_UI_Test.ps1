# Isolated paint/binding checks only: never show/load forms or run searches.
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing,System.Data
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app = Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash = (Get-FileHash -LiteralPath ($app + '.config')).Hash
$a = [Reflection.Assembly]::LoadFrom($app)
$flags = [Reflection.BindingFlags]'Public,NonPublic,Instance'
$output = Join-Path $repo ('TestResults\GridEmptyState-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
$null = New-Item -ItemType Directory -Path $output
$checks = 0
function Check($ok, $message) { if (!$ok) { throw $message }; $script:checks++ }
function Paint-Hash($form, $grid, $method) {
    $bitmap = [Drawing.Bitmap]::new($grid.Width, $grid.Height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $stream = [IO.MemoryStream]::new()
    $hash = [Security.Cryptography.SHA256]::Create()
    try {
        $graphics.Clear([Drawing.Color]::White)
        if ($method) {
            $event = [Windows.Forms.PaintEventArgs]::new($graphics, $grid.ClientRectangle)
            try { $form.GetType().GetMethod($method, $flags).Invoke($form, @($grid, $event)) | Out-Null }
            finally { $event.Dispose() }
        }
        $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
        return [Convert]::ToBase64String($hash.ComputeHash($stream.ToArray()))
    } finally { $hash.Dispose(); $stream.Dispose(); $graphics.Dispose(); $bitmap.Dispose() }
}
try {
    foreach ($case in @(
        @('frmCustomer','gridCustomer'), @('frmSalesRep','gridSalesRep'),
        @('frmRecall','gridOrder'), @('frmItemLookUp','gridItem')
    )) {
        $f = [Activator]::CreateInstance($a.GetType('WorkOrderEntry.' + $case[0], $true))
        try {
            if ($case[0] -eq 'frmItemLookUp') { $f.GetType().GetField('isLoadingRecalledOrder', $flags).SetValue($f, $true) }
            $f.BindingContext = [Windows.Forms.BindingContext]::new()
            $g = $f.GetType().GetProperty($case[1], $flags).GetValue($f, $null)
            $g.Dock = 'None'; $g.Size = [Drawing.Size]::new(620,240)
            $method = $case[1] + '_Paint'
            $blank = Paint-Hash $f $g $null
            Check ((Paint-Hash $f $g $method) -eq $blank) 'Initial unbound grid must stay blank'
            $idColumn = $g.Columns[0].DataPropertyName
            $table = [Data.DataTable]::new(); $null = $table.Columns.Add($idColumn, [int])
            $g.DataSource = $table.DefaultView
            Check ($g.RowCount -eq 0) 'Empty result fixture has rows'
            $empty = Paint-Hash $f $g $method
            Check ($empty -ne $blank -and $g.RowCount -eq 0) 'Message missing or placeholder row added'
            $image = [Drawing.Bitmap]::new($g.Width,$g.Height)
            try { $g.DrawToBitmap($image, $g.ClientRectangle); $image.Save((Join-Path $output ($case[0] + '.png'))) }
            finally { $image.Dispose() }
            $null = $table.Rows.Add(123)
            Check ($g.RowCount -eq 1 -and (Paint-Hash $f $g $method) -eq $blank) 'Message must disappear for real results'
            $emptyFilter = '[' + $idColumn + '] = -1'
            $table.DefaultView.RowFilter = $emptyFilter
            $source = $g.DataSource
            Check ($g.RowCount -eq 0 -and (Paint-Hash $f $g $method) -eq $empty) 'Filtered-empty message missing'
            Check ([object]::ReferenceEquals($source,$g.DataSource) -and $table.DefaultView.RowFilter -eq $emptyFilter) 'Painting changed binding or filter'
            $table.DefaultView.RowFilter = ''
            Check ($g.RowCount -eq 1 -and (Paint-Hash $f $g $method) -eq $blank) 'Message persisted after filter clear'
            if ($case[0] -eq 'frmItemLookUp') {
                Check ($null -eq $f.GetType().GetProperty('lblItemSearchNotice', $flags)) 'There must be no above-grid notice'
                $g.DataSource = $table.Clone().DefaultView
                $source = $g.DataSource
                $tree = $f.GetType().GetProperty('TreeView1', $flags).GetValue($f, $null)
                foreach ($hasPreviousKeyword in @($false,$true)) {
                    $tree.Nodes.Clear()
                    if ($hasPreviousKeyword) { $null = $tree.Nodes.Add('BOOK','BOOK') }
                    $null = $tree.Nodes.Add('UNMATCHED','UNMATCHED')
                    $f.GetType().GetMethod('RebuildFilterTextFromTree', $flags).Invoke($f, @()) | Out-Null
                    $f.GetType().GetMethod('HandleEmptyItemSearch', $flags).Invoke($f, @()) | Out-Null
                    Check ($g.RowCount -eq 0 -and [object]::ReferenceEquals($source,$g.DataSource)) 'Failed search must keep the empty result, without fallback binding'
                    Check ((Paint-Hash $f $g $method) -eq $empty) 'Failed search must retain centered empty-grid guidance'
                    Check ($tree.Nodes.Count -eq [int]$hasPreviousKeyword -and !$tree.Nodes.ContainsKey('UNMATCHED')) 'Failed keyword cleanup changed'
                }
                $f.GetType().GetMethod('checkSearch1', $flags).Invoke($f, @()) | Out-Null
                Check ($g.RowCount -eq 0 -and (Paint-Hash $f $g $method) -eq $empty) 'Empty barcode result must retain centered guidance'
                $g.DataSource = $table.DefaultView
                Check ($g.RowCount -eq 1 -and (Paint-Hash $f $g $method) -eq $blank) 'New results must remove centered guidance'
            }
        } finally { $f.Dispose() }
    }
    "PASS: $checks empty-state paint checks; no SQL, searches or form Load. Images: $output"
} finally {
    if ((Get-FileHash -LiteralPath ($app + '.config')).Hash -ne $configHash) { throw 'Store configuration changed' }
}
