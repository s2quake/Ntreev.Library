$majorVersion = 5
$minorVersion = 0
$buildVersion = 0
$revision = "unversioned"
$propsPath = "$PSScriptRoot\base.props"

$version = "$majorVersion.$minorVersion.$buildVersion-`$(TargetFramework)-$revision"
$assemblyVersion = "$majorVersion.$minorVersion"
$fileVersion = "$majorVersion.$minorVersion.$buildVersion"

function Confirm-Command {
    param (
        [string]$CommandName,
        [string]$ErrorMessage
    )
    try {
        Invoke-Expression -Command $CommandName | Out-Null
        Write-Host "$CommandName ..OK"
    }
    catch {
        Write-Error $_.Exception.Message
        Write-Host "Please visit the site below and install it."
        Write-Host "https://dotnet.microsoft.com/download/dotnet-core/$needVersion"
        Write-Host 'Press any key to continue...';
        $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
        exit 1
    }
}

# Confirm-Command "dotnet"
# Confirm-Command "git"

# function Get-Framework40-Family-Version() {
#     $result = -1
#     if (Is-Key-Present "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Client" "Install" -or Is-Key-Present "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full" "Install") {
#         # .net 4.0 is installed
#         $result = 0
#         $version = Get-Framework-Value "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
        
#         if ($version -ge 378389) {
#             # .net 4.5
#             Write-Host "Installed .Net Framework 4.5"
#             $result = 1
#         }   
#     }
# }

# validate dotnet version
try {
    [System.Version]$needVersion = "3.1"
    [System.Version]$realVersion = dotnet --version
    if ($realVersion -lt $needVersion) {
        throw "NET Core $needVersion or higher version must be installed to build this project."
    }
}
catch {
    Write-Error $_.Exception.Message
    Write-Host "Please visit the site below and install it."
    Write-Host "https://dotnet.microsoft.com/download/dotnet-core/$needVersion"
    Write-Host 'Press any key to continue...';
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
    exit 1
}

# get head revision of this repository
try {
    $revision = Invoke-Expression -Command "git rev-parse head" 2>&1 -ErrorVariable errout
    if ($LastExitCode -ne 0) {
        throw $errout
    }
}
catch {
    Write-Warning $_.Exception.Message
    Write-Warning "revision is '$revision'"
}

try {
    $changes = Invoke-Expression "git status --porcelain"
    if ($changes) {
        throw "git repository has changes. build aborted."
    }
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}

# recored version
[xml]$doc = Get-Content $propsPath -Encoding UTF8
foreach ($obj in $doc.Project.PropertyGroup) {        
    if ($obj.Version) {
        $obj.Version = $version
    }
    if ($obj.FileVersion) {
        $obj.FileVersion = $fileVersion
    }
    if ($obj.AssemblyVersion) {
        $obj.AssemblyVersion = $assemblyVersion
    }
}
$doc.Save($propsPath)

Invoke-Expression "dotnet restore"
Invoke-Expression "dotnet clean"
Invoke-Expression "dotnet build"
Invoke-Expression "git checkout $propsPath"

Write-Host "AssemblyVersion: $assemblyVersion"
Write-Host "FileVersion: $fileVersion"
Write-Host "Version: $version"
