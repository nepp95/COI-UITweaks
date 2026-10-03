param(
    [string]$GameRoot = ${env:COI_ROOT},
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'

if (!$GameRoot)
{
    $GameRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Captain of Industry'
}

$managed = Join-Path $GameRoot 'Captain of Industry_Data\Managed'

if (!(Test-Path -LiteralPath (Join-Path $managed 'Mafi.Unity.dll')))
{
    throw 'Game not found. Pass -GameRoot with its installation directory.'
}

Push-Location $PSScriptRoot

try
{
    if (!$SkipRestore)
    {
        dotnet restore UITweaks.sln --locked-mode

        if ($LASTEXITCODE)
        {
            throw 'Dependency restore failed.'
        }
    }

    dotnet build UITweaks/UITweaks.csproj -c Release --no-restore "-p:COI_ROOT=$GameRoot"

    if ($LASTEXITCODE)
    {
        throw 'Mod build failed.'
    }

    $previousGameRoot = $env:COI_ROOT

    try
    {
        $env:COI_ROOT = $GameRoot
        dotnet test UITweaks.Tests/UITweaks.Tests.csproj -c Release --no-restore
    }
    finally
    {
        $env:COI_ROOT = $previousGameRoot
    }

    if ($LASTEXITCODE)
    {
        throw 'Verification failed.'
    }

    $version = (Get-Content UITweaks/manifest.json -Raw | ConvertFrom-Json).version
    $package = Join-Path $PSScriptRoot 'artifacts\UITweaks'
    New-Item -ItemType Directory -Force -Path $package | Out-Null

    # Explicit file list: never package game DLLs or local inspection files.
    $buildFiles = @('UITweaks.dll', '0Harmony.dll', 'manifest.json', 'config.json')

    foreach ($file in $buildFiles)
    {
        $source = Join-Path 'UITweaks\bin\Release\net48' $file
        Copy-Item -LiteralPath $source -Destination $package
    }

    Copy-Item -LiteralPath README.md -Destination (Join-Path $package 'readme.txt')
    Copy-Item -LiteralPath THIRD-PARTY-NOTICES.txt -Destination $package

    $allowed = $buildFiles + @('readme.txt', 'THIRD-PARTY-NOTICES.txt')
    $unexpected = Get-ChildItem -LiteralPath $package -Force |
        Where-Object { $_.Name -notin $allowed }

    if ($unexpected)
    {
        throw 'Unexpected files in artifacts/UITweaks; inspect them before packaging.'
    }

    $zip = Join-Path $PSScriptRoot "artifacts\UITweaks-$version.zip"
    Compress-Archive -LiteralPath $package -DestinationPath $zip -Force
    Write-Host "Local package: $zip"
}
finally
{
    Pop-Location
}

