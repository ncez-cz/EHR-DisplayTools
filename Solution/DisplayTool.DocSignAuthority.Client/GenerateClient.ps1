#!/usr/bin/env pwsh

$CurrentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

Write-Host
Write-Host "Generating HTTP clients"

dotnet build $CurrentPath

Write-Host
Write-Host "Script finished"
