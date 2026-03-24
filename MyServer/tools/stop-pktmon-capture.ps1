param(
    [string]$CaptureDir = "E:\UGit\CBA_Card\MyServer\captures"
)

$ErrorActionPreference = "Stop"

function Test-IsAdministrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdministrator)) {
    throw "pktmon needs an elevated PowerShell window. Re-run PowerShell as Administrator."
}

if (-not (Test-Path $CaptureDir)) {
    throw "Capture directory not found: $CaptureDir"
}

$etlFile = Get-ChildItem $CaptureDir -Filter *.etl | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $etlFile) {
    throw "No .etl capture file found under $CaptureDir"
}

$pcapPath = [System.IO.Path]::ChangeExtension($etlFile.FullName, ".pcapng")
$txtPath = [System.IO.Path]::ChangeExtension($etlFile.FullName, ".txt")

pktmon stop | Out-Null
pktmon etl2pcap $etlFile.FullName -o $pcapPath | Out-Null
pktmon etl2txt $etlFile.FullName -o $txtPath | Out-Null

Write-Host "PktMon stopped."
Write-Host "ETL:  $($etlFile.FullName)"
Write-Host "PCAP: $pcapPath"
Write-Host "TXT:  $txtPath"
Write-Host "Next step: send me the newest .txt file, or open the .pcapng in Wireshark and filter by tcp.port."
