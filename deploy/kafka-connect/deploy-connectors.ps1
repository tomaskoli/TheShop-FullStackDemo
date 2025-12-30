# Deploy Kafka Connect connectors for PostgreSQL -> Neo4j ETL
# Usage: .\deploy-connectors.ps1 [-ConnectUrl "http://localhost:8083"]

param(
    [string]$ConnectUrl = "http://localhost:8083"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Kafka Connect ETL Deployment ===" -ForegroundColor Cyan
Write-Host "Connect URL: $ConnectUrl"
Write-Host ""

# Wait for Kafka Connect to be ready
Write-Host "Waiting for Kafka Connect to be ready..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
while ($attempt -lt $maxAttempts) {
    try {
        $response = Invoke-RestMethod -Uri "$ConnectUrl/" -Method GET -ErrorAction Stop
        Write-Host "Kafka Connect is ready!" -ForegroundColor Green
        break
    }
    catch {
        $attempt++
        if ($attempt -ge $maxAttempts) {
            Write-Host "Kafka Connect not ready after $maxAttempts attempts. Exiting." -ForegroundColor Red
            exit 1
        }
        Write-Host "  Attempt $attempt/$maxAttempts - waiting..."
        Start-Sleep -Seconds 2
    }
}

# Function to deploy a connector
function Deploy-Connector {
    param(
        [string]$ConfigFile
    )
    
    $connectorName = (Get-Item $ConfigFile).BaseName
    $config = Get-Content $ConfigFile -Raw
    
    Write-Host ""
    Write-Host "Deploying connector: $connectorName" -ForegroundColor Cyan
    
    # Check if connector already exists
    try {
        $existing = Invoke-RestMethod -Uri "$ConnectUrl/connectors/$connectorName" -Method GET -ErrorAction Stop
        Write-Host "  Connector exists, updating..." -ForegroundColor Yellow
        
        # Delete and recreate
        Invoke-RestMethod -Uri "$ConnectUrl/connectors/$connectorName" -Method DELETE | Out-Null
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Host "  Creating new connector..." -ForegroundColor Green
    }
    
    # Create connector
    try {
        $result = Invoke-RestMethod -Uri "$ConnectUrl/connectors" -Method POST -ContentType "application/json" -Body $config
        Write-Host "  SUCCESS: $($result.name) deployed" -ForegroundColor Green
    }
    catch {
        Write-Host "  FAILED: $_" -ForegroundColor Red
    }
}

# Deploy connectors in order (sources first, then sinks)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$connectorsDir = Join-Path $scriptDir "connectors"

# Deploy source connectors first
Write-Host ""
Write-Host "=== Deploying Source Connectors ===" -ForegroundColor Cyan
Deploy-Connector -ConfigFile (Join-Path $connectorsDir "pg-brands-source.json")
Deploy-Connector -ConfigFile (Join-Path $connectorsDir "pg-categories-source.json")
Deploy-Connector -ConfigFile (Join-Path $connectorsDir "pg-products-source.json")

# Wait for topics to be created
Write-Host ""
Write-Host "Waiting for topics to be created..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Deploy sink connector
Write-Host ""
Write-Host "=== Deploying Sink Connector ===" -ForegroundColor Cyan
Deploy-Connector -ConfigFile (Join-Path $connectorsDir "neo4j-sink.json")

# Show status
Write-Host ""
Write-Host "=== Connector Status ===" -ForegroundColor Cyan
$connectors = Invoke-RestMethod -Uri "$ConnectUrl/connectors" -Method GET
foreach ($connector in $connectors) {
    $status = Invoke-RestMethod -Uri "$ConnectUrl/connectors/$connector/status" -Method GET
    $state = $status.connector.state
    $taskStates = ($status.tasks | ForEach-Object { $_.state }) -join ", "
    
    $color = switch ($state) {
        "RUNNING" { "Green" }
        "PAUSED" { "Yellow" }
        default { "Red" }
    }
    
    Write-Host "  $connector : $state (tasks: $taskStates)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Monitor connectors:"
Write-Host "  - Kafka Connect REST: $ConnectUrl/connectors"
Write-Host "  - Kafka UI: http://localhost:8081"
Write-Host "  - Neo4j Browser: http://localhost:7474"

