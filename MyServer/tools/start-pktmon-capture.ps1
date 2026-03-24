param(
    [int]$TcpPort = 0,
    [string]$OutputDir = "E:\UGit\CBA_Card\MyServer\captures",
    [int]$MaxFileSizeMb = 256
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

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$etlPath = Join-Path $OutputDir "pktmon_$timestamp.etl"
$metaPath = Join-Path $OutputDir "pktmon_$timestamp.info.txt"

pktmon stop | Out-Null
pktmon filter remove | Out-Null

if ($TcpPort -gt 0) {
    pktmon filter add "tcp_$TcpPort" -t TCP -p $TcpPort | Out-Null
} else {
    pktmon filter add "all_tcp" -t TCP | Out-Null
}

$adapter = Get-NetAdapter | Where-Object Status -eq Up | Select-Object -ExpandProperty Name

@(
    "capture_started_at=$(Get-Date -Format s)"
    "tcp_port=$TcpPort"
    "etl=$etlPath"
    "adapters=$($adapter -join ', ')"
) | Set-Content -Encoding UTF8 $metaPath

pktmon start --capture --pkt-size 0 --file-name $etlPath --file-size $MaxFileSizeMb --log-mode circular | Out-Null

Write-Host "PktMon started."
Write-Host "ETL: $etlPath"
Write-Host "Meta: $metaPath"
if ($TcpPort -gt 0) {
    Write-Host "Filter: TCP port $TcpPort"
} else {
    Write-Host "Filter: all TCP"
}
Write-Host "Now reproduce the Unity login / enter-game flow, then run stop-pktmon-capture.ps1."
