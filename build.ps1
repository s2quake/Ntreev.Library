$location = Get-Location
try {
    Set-Location $PSScriptRoot
    Invoke-WebRequest -Uri "https://gist.githubusercontent.com/s2quake/57ae08b7598f1f4978d8c50326b4086d/raw/dca3009fc111b25897f18688c94ece6a74e777bb/build.ps1" -OutFile .\.vscode\build.ps1
    .\.vscode\build.ps1 -BuildPath $PSScriptRoot -PropsPath "base.props"
}
finally {
    Set-Location $location
}
