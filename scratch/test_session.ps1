$qs = New-Object -ComObject QSRules.SessionClass
$qs | Get-Member | Out-File -FilePath .\scratch\session_members.txt
Write-Host "Session members written."
