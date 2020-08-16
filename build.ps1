param(
    [string]$BuildPath = $PSScriptRoot,
    [string]$PropsPath = (Join-Path $PSScriptRoot "base.props"),
    [switch]$Force
)
$revision = "unversioned"
$frameworkOption = ""
$location = Get-Location

try {
    Set-Location $BuildPath

    $BuildPath = Resolve-Path $BuildPath
    $PropsPath = Resolve-Path $PropsPath

    Write-Host "BuildPath: $BuildPath"
    Write-Host "PropsPath: $PropsPath"
    
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
        Write-Warning "Please visit the site below and install it."
        Write-Warning "https://dotnet.microsoft.com/download/dotnet-core/$needVersion"
        Write-Host ""
        Write-Host 'Press any key to continue...';
        $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
        exit 1
    }

    # validate .netframework 4.5
    try {
        Invoke-Expression "dotnet msbuild -t:GetReferenceAssemblyPaths -v:n -p:TargetFramework=net45" | Out-Null
        if ($LastExitCode -ne 0) {
            throw ".net framework 4.5 or higher version must be installed to build this project"
        }
    }
    catch {
        Write-Warning $_.Exception.Message
        Write-Warning "TargetFramework net45 skipped."
        Write-Warning "If you want to build with .net framework 4.5, visit the site below and install it."
        if ([environment]::OSVersion.Platform -eq "Unix") {
            Write-Warning "https://www.mono-project.com"
        }
        elseif ([environment]::OSVersion.Platform -eq "Win32NT") {
            Write-Warning "https://aka.ms/msbuild/developerpacks"
        }
        Write-Host ""
        $frameworkOption = "--framework netcoreapp3.1"
    }

    # check if there are any changes in the repository.
    try {
        $changes = Invoke-Expression "git status --porcelain"
        if ($changes) {
            throw "git repository has changes. build aborted."
        }
    }
    catch {
        if ($Force -eq $False) {
            Write-Error $_.Exception.Message
            exit 1
        }
    }

    # get head revision of this repository
    try {
        $revision = Invoke-Expression -Command "git rev-parse HEAD" 2>&1 -ErrorVariable errout
        if ($LastExitCode -ne 0) {
            throw $errout
        }
    }
    catch {
        Write-Error $_.Exception.Message
        exit 1
    }

    # recored version to props file
    [xml]$doc = Get-Content $PropsPath -Encoding UTF8
    $doc.Project.PropertyGroup.Version = "$($doc.Project.PropertyGroup.FileVersion)-`$(TargetFramework)-$revision"
    $doc.Save($PropsPath)

    # build project
    Invoke-Expression "dotnet build $frameworkOption --verbosity minimal --nologo"
    if ($LastExitCode -ne 0) {
        Write-Error "build failed"
    }
    else {
        Write-Host ""
        Write-Host "AssemblyVersion: $assemblyVersion"
        Write-Host "FileVersion: $fileVersion"
        Write-Host "revision: $revision"
        Write-Host ""
    }

    # revert props file
    Invoke-Expression "git checkout $PropsPath" 2>&1 
}
finally {
    Set-Location $location
}