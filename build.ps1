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
$isArm64Host = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64'

Info "Run x64 tests"
dotnet test --nologo `
            --configuration Release `
            -verbosity:minimal `
            /property:Platform=x64 `
            --logger:"console;verbosity=normal" `
            "$root/src/test/Test.csproj"
CheckReturnCodeOfPreviousCommand "x64 tests failed"

if ($isArm64Host) {
  Info "Run ARM64 tests"
  dotnet test --nologo `
              --configuration Release `
              -verbosity:minimal `
              /property:Platform=ARM64 `
              --logger:"console;verbosity=normal" `
              "$root/src/test/Test.csproj"
  CheckReturnCodeOfPreviousCommand "ARM64 tests failed"
} else {
  Info "Skip ARM64 tests on a non-ARM64 host"
}

Info "Publish x64 Native AOT executable. Version '$version'"
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
CheckReturnCodeOfPreviousCommand "x64 publish failed"

Info "Publish ARM64 Native AOT executable. Version '$version'"
dotnet publish --nologo `
               --configuration Release `
               --runtime win-arm64 `
               -verbosity:minimal `
               /property:Platform=ARM64 `
               /property:PublishAot=true `
               /property:PublishTrimmed=true `
               /property:TrimMode=full `
               /property:DebugType=None `
               /property:Version=$version `
               "$root/src/Handle2/Handle2.csproj"
CheckReturnCodeOfPreviousCommand "ARM64 publish failed"

Info "Create zip archives"
Compress-Archive -Force -Path "$buildDir/x64/Release/Handle2/net11.0-windows/win-x64/native/Handle2.exe" -DestinationPath "$buildDir/Handle2_x64.zip"
Compress-Archive -Force -Path "$buildDir/ARM64/Release/Handle2/net11.0-windows/win-arm64/native/Handle2.exe" -DestinationPath "$buildDir/Handle2_ARM64.zip"
