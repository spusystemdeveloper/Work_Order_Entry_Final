$qs = New-Object -ComObject QSRules.SessionClass
Write-Host "QSRules.SessionClass created successfully: $($qs -ne $null)"
$trans = $qs.Transaction
Write-Host "qs.Transaction: $($trans -ne $null)"
$trans | Get-Member | Out-File -FilePath .\scratch\trans_members.txt
Write-Host "Members written to trans_members.txt"
