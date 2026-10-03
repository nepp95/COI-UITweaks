[CmdletBinding(DefaultParameterSetName = 'Increment', SupportsShouldProcess = $true)]
param(
    [Parameter(ParameterSetName = 'Increment', Position = 0)]
    [ValidateSet('Patch', 'Minor', 'Major')]
    [string]$Part = 'Patch',

    [Parameter(ParameterSetName = 'Explicit', Mandatory = $true)]
    [ValidatePattern('^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

# Examples: ./bump-version.ps1, ./bump-version.ps1 Minor,
# ./bump-version.ps1 -Version 1.2.0, ./bump-version.ps1 -WhatIf
$projectPath = Join-Path $PSScriptRoot 'UITweaks\UITweaks.csproj'
$manifestPath = Join-Path $PSScriptRoot 'UITweaks\manifest.json'
$projectText = [System.IO.File]::ReadAllText($projectPath)
$manifestText = [System.IO.File]::ReadAllText($manifestPath)
$project = [xml]$projectText
$manifest = $manifestText | ConvertFrom-Json
$projectVersions = @($project.SelectNodes('/Project/PropertyGroup/Version'))

if ($projectVersions.Count -ne 1)
{
    throw 'Expected exactly one Version property in UITweaks.csproj.'
}

$currentVersion = $projectVersions[0].InnerText

if ($currentVersion -ne $manifest.version)
{
    throw 'Project and manifest versions differ. Align them before bumping the version.'
}

if ($currentVersion -notmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$')
{
    throw 'The current version must have the format major.minor.patch.'
}

$current = [version]$currentVersion

if ($PSCmdlet.ParameterSetName -eq 'Increment')
{
    $Version = switch ($Part)
    {
        'Major' { '{0}.0.0' -f ($current.Major + 1) }
        'Minor' { '{0}.{1}.0' -f $current.Major, ($current.Minor + 1) }
        'Patch' { '{0}.{1}.{2}' -f $current.Major, $current.Minor, ($current.Build + 1) }
    }
}

$next = [version]$Version

if ($next -le $current)
{
    throw "New version $Version must be greater than $currentVersion."
}

# SDK-generated assembly versions require components below 65535.
if ($next.Major -gt 65534 -or $next.Minor -gt 65534 -or $next.Build -gt 65534)
{
    throw 'Version components must be between 0 and 65534.'
}

# Preserve the existing formatting and every unrelated value.
$projectPattern = '<Version>\s*' + [regex]::Escape($currentVersion) + '\s*</Version>'
$manifestPattern = '"version"\s*:\s*"' + [regex]::Escape($currentVersion) + '"'

if ([regex]::Matches($projectText, $projectPattern).Count -ne 1 -or
    [regex]::Matches($manifestText, $manifestPattern).Count -ne 1)
{
    throw 'Could not uniquely locate both version declarations. No files were changed.'
}

$newProject = [regex]::Replace($projectText, $projectPattern, "<Version>$Version</Version>")
$newManifest = [regex]::Replace($manifestText, $manifestPattern, {
    param($match)
    $match.Value.Replace('"' + $currentVersion + '"', '"' + $Version + '"')
})

if ($PSCmdlet.ShouldProcess('UITweaks.csproj and manifest.json', "Bump version from $currentVersion to $Version"))
{
    $encoding = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($projectPath, $newProject, $encoding)

    try
    {
        [System.IO.File]::WriteAllText($manifestPath, $newManifest, $encoding)
    }
    catch
    {
        [System.IO.File]::WriteAllText($projectPath, $projectText, $encoding)
        throw
    }

    Write-Host "Version bumped: $currentVersion -> $Version"
}
