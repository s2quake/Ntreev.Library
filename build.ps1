$majorVersion = 5
$minorVersion = 0
$buildVersion = 0
$revision = "unversioned"
$propsPath = "$PSScriptRoot\base.props"
$frameworkOption = ""

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

# check if .netframework 4.5 is installed on windows
try {
    if ([environment]::OSVersion.Platform -eq "Win32NT") {
        function Test-KeyPresent([string]$path, [string]$key) {
            if (!(Test-Path $path)) { return $false }
            if ($null -eq (Get-ItemProperty $path).$key) { return $false }
            return $true
        }
        function Get-Framework-Value([string]$path, [string]$key) {
            if (!(Test-Path $path)) { return "-1" }
        
            return (Get-ItemProperty $path).$key  
        }
        if (Test-KeyPresent "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Client" "Install" -or Test-KeyPresent "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full" "Install") {
            $version = Get-Framework-Value "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
            if ($version -lt 378389) {
                throw ".Net Framework 4.5 not installed."
            }   
        }    
    }
}
catch {
    Write-Warning $_.Exception.Message
}

# check if there are any changes in the repository.
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

# recored version
$version = "$majorVersion.$minorVersion.$buildVersion-`$(TargetFramework)-$revision"
$assemblyVersion = "$majorVersion.$minorVersion"
$fileVersion = "$majorVersion.$minorVersion.$buildVersion"
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

# build project
Invoke-Expression "dotnet build $frameworkOption --verbosity minimal --nologo"

# revert file
Invoke-Expression "git checkout $propsPath"

# print version
Write-Host "AssemblyVersion: $assemblyVersion"
Write-Host "FileVersion: $fileVersion"
Write-Host "Version: $version"
