#!/usr/bin/env pwsh

$CurrentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

Write-Host
Write-Host "Generating HTTP clients"

$Environment = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = 'DevelopmentNSwag'
dotnet build $CurrentPath
$env:ASPNETCORE_ENVIRONMENT = $Environment

Write-Host
Write-Host "Script finished"
