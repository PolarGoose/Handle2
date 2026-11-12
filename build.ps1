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

Info "Run tests"
dotnet test --nologo `
            --configuration Release `
            -verbosity:minimal `
            /property:Platform=x64 `
            --logger:"console;verbosity=normal" `
            "$root/src/test/Test.csproj"
CheckReturnCodeOfPreviousCommand "Tests failed"

Info "Publish Native AOT executable. Version '$version'"
dotnet publish --nologo `
               --configuration Release `
               --runtime win-x64 `
               -verbosity:minimal `
               /property:Platform=x64 `
               /property:PublishAot=true `
               /property:PublishTrimmed=true `
               /property:TrimMode=full `
               /property:DebugType=None `
               /property:Version=$version `
               "$root/src/Handle2/Handle2.csproj"
CheckReturnCodeOfPreviousCommand "Publish failed"

Info "Create zip archive"
Compress-Archive -Force -Path "$buildDir/x64/Release/Handle2/net10.0-windows/win-x64/native/Handle2.exe" -DestinationPath "$buildDir/Handle2.zip"
