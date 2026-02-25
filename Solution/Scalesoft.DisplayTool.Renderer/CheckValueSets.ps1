param(
    [string]$Root = ".",
# File patterns to scan (adjust as needed)
    [string[]]$Include = @("*.cs","*.ts","*.tsx","*.js","*.jsx","*.java","*.kt","*.xml","*.json","*.txt"),
# Output file for the check results
    [string]$OutFile = "valueset_check_results.csv",
# Output file that lists URLs that are NOT ValueSets (e.g., CodeSystems)
    [string]$NonValueSetFile = "non_valueset_urls.txt",
# If true, still call the ValueSet endpoint for non-ValueSet URLs (usually returns 0 results)
    [switch]$CheckNonValueSets
)

# Ensure we get files per Include
$pathForInclude = Join-Path -Path (Resolve-Path $Root) -ChildPath "*"
#echo $pathForInclude
$files = Get-ChildItem -Path $pathForInclude -Recurse -Filter *.cs -ErrorAction SilentlyContinue

if (-not $files) {
    Write-Host "No files matched patterns under '$Root'."
    exit 0
}

# Regex: EnumLabel(<anything>, "<url>" or '<url>'), robust to whitespace and escapes
# Captures the URL string (without quotes) in the 'url' group.
$pattern = '(?is)EnumLabel\s*\(\s*[^,]+,\s*(?<q>["' + "'" + '])(?<url>(?:\\.|(?!\k<q>).)*)\k<q>'

# Sets to deduplicate
$valueSetUrls   = [System.Collections.Generic.HashSet[string]]::new()
$otherUrls      = [System.Collections.Generic.HashSet[string]]::new()

# Helper: decode typical string escapes by leveraging JSON parsing
function Decode-StringLiteral([string]$s) {
    try { return (ConvertFrom-Json ('"' + $s + '"')) } catch { return $s }
}

# Scan files and collect URLs
foreach ($f in $files) {
    try {
        $content = Get-Content -Path $f.FullName -Raw -ErrorAction Stop
    } catch {
        Write-Warning "Could not read $($f.FullName): $($_.Exception.Message)"
        continue
    }

    $matches = [regex]::Matches($content, $pattern, 'IgnoreCase, Singleline')
    foreach ($m in $matches) {
        $rawUrl = $m.Groups['url'].Value
        $url = Decode-StringLiteral $rawUrl
        if ([string]::IsNullOrWhiteSpace($url)) { continue }

        if ($url -match '(?i)\bvalue-?set\b') {
            [void]$valueSetUrls.Add($url)
        } else {
            [void]$otherUrls.Add($url)
            if ($CheckNonValueSets) {
                # Optionally also check non-ValueSet URLs against ValueSet search (will usually be 0 results)
                [void]$valueSetUrls.Add($url)
            }
        }
    }
}

Write-Host ("Found {0} ValueSet-like URLs and {1} non-ValueSet URLs." -f $valueSetUrls.Count, $otherUrls.Count)

# Verify ValueSets on the server
$results = New-Object System.Collections.Generic.List[object]
foreach ($u in $valueSetUrls) {
    $encoded = [System.Uri]::EscapeDataString($u)
    $checkUrl = "https://termapitest.mzcr.cz/fhir/ValueSet?url=$encoded"

    $statusCode = $null
    $exists = $false
    $total = $null
    $error = $null

    try {
        $resp = Invoke-WebRequest -Method GET -Uri $checkUrl -Headers @{ "Accept" = "application/fhir+json" } -TimeoutSec 30 -ErrorAction Stop
        $statusCode = [int]$resp.StatusCode
        try {
            $body = $resp | ConvertFrom-Json
            if ($body.resourceType -eq 'Bundle' -and $body.type -eq 'searchset') {
                $total = $body.total
                $exists = ([int]$total -ge 1)
            }
        } catch {
            $error = "JSON parse failed: $($_.Exception.Message)"
        }
    } catch {
        $statusCode = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { $null }
        $error = $_.Exception.Message
    }

    $results.Add([pscustomobject]@{
        Url        = $u
        CheckUrl   = $checkUrl
        StatusCode = $statusCode
        Total      = $total
        Exists     = $exists
        Note       = if ($u -match '(?i)\bvalue-?set\b') { $null } else { 'Non-ValueSet URL' }
        Error      = $error
    })
}

# Save results
$results | Sort-Object Exists, Url | Export-Csv -Path $OutFile -NoTypeInformation -Encoding UTF8
Write-Host "Wrote ValueSet check results to '$OutFile'."

# Save non-ValueSet URLs for manual review (likely CodeSystems)
if ($otherUrls.Count -gt 0) {
    $otherUrls | Sort-Object | Set-Content -Path $NonValueSetFile -Encoding UTF8
    Write-Host "Wrote non-ValueSet URLs to '$NonValueSetFile'."
}

# Print a brief summary
$ok = ($results | Where-Object Exists).Count
$missing = ($results | Where-Object { -not $_.Exists }).Count
Write-Host ("Summary: Exists={0}, Missing/Zero={1}, Checked={2}, Non-ValueSet (separate list)={3}" -f $ok, $missing, $results.Count, $otherUrls.Count)
