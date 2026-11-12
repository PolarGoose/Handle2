Function Info($msg) {
  Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
  Write-Host -ForegroundColor Red "`n`nERROR: $msg"
  exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
  if(-Not $?) {
    Error "${msg}. Error code: $LastExitCode"
  }
}

Function GetVersion() {
  try { $commitTag = git describe --exact-match --tags HEAD 2> $null } catch { }
  if(-Not $?) {
    Info "The commit is not tagged. Use 'v0.0-dev' as a tag instead"
    $commitTag = "v0.0-dev"
  }
  return "$($commitTag.Substring(1))"
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Resolve-Path $PSScriptRoot
$buildDir = "$root/build"
$version = GetVersion

Info "Build the project. Version=$version"
dotnet build --nologo `
             --configuration Release `
             -verbosity:minimal `
             /property:DebugType=None `
             /property:Version=$version `
             "$root/Handle2.slnx"
CheckReturnCodeOfPreviousCommand "Build failed"

Info "Run tests"
dotnet test --nologo `
            --no-build `
            --configuration Release `
            -verbosity:minimal `
            --logger:"console;verbosity=normal" `
            "$root/Handle2.slnx"
CheckReturnCodeOfPreviousCommand "Tests failed"

Compress-Archive -Force -Path "$buildDir/Release/Handle2/net472/Handle2.exe" -DestinationPath "$buildDir/Handle2.zip"
