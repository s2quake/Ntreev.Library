$majorVersion=4
$minorVersion=0
$sourcePath = Join-Path (Split-Path $myInvocation.MyCommand.Definition) ".\Ntreev.Library.AssemblyInfo\AssemblyInfo.cs" -Resolve
$version="$majorVersion.$minorVersion"
$fileVersion="$majorVersion.$minorVersion"+"."+(Get-Date -Format yy)+(Get-Date).DayOfYear+"."+(Get-Date -Format HHmm)
$csproj="Ntreev.Library\Ntreev.Library.csproj"

$content = Get-Content $sourcePath -Encoding UTF8

$pattern1 = "(AssemblyVersion[(]`").+(`"[)]])"
if ($content -match $pattern1) {
    $content = $content -replace $pattern1, "`${1}$version`$2"
}
else {
    throw "AssemblyVersion not found."
}

$pattern2 = "(AssemblyFileVersion[(]`").+(`"[)]])"
if ($content -match $pattern2) {
    $content = $content -replace $pattern2, "`${1}$fileVersion`$2"
}
else {
    throw "AssemblyFileVersion not found."
}

$pattern3 = "(AssemblyInformationalVersion[(]`").+(`"[)]])"
if ($content -match $pattern3) {
    $content = $content -replace $pattern3, "`${1}$fileVersion`$2"
}
else {
    throw "AssemblyFileVersion not found."
}

Set-Content $sourcePath $content -Encoding UTF8
(Get-Content $csproj) -replace "(<Version>)(.*)(</Version>)", "`${1}$version`$3" -replace "(<FileVersion>)(.*)(</FileVersion>)", "`${1}$version`$3" -replace "(<AssemblyVersion>)(.*)(</AssemblyVersion>)", "`${1}$majorVersion.$minorVersion.0.0`$3" | Set-Content $csproj

Set-Content version.txt $fileVersion
Write-Host $fileVersion

