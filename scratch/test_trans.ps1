$qs = New-Object -ComObject QSRules.SessionClass
$qs.InitializeTransaction()
$trans = $qs.Transaction
$trans | Get-Member | Out-File -FilePath .\scratch\trans_members.txt -Encoding ascii
Write-Host "Transaction members written."
