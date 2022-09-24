Function Info($msg) {
    Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
    Write-Host `n`n
    Write-Error $msg
    exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
    if(-Not $?) {
        Error "${msg}. Error code: $LastExitCode"
    }
}

Function CreateZipArchive($file, $archiveFile) {
    Info "Create a zip archive from `n  '$file' `n to `n  '$archiveFile'"
    Compress-Archive -Force -Path $file -DestinationPath $archiveFile
}

Function GetVersion() {
    $gitCommand = Get-Command -Name git

    $tag = & $gitCommand describe --exact-match --tags HEAD
    if(-Not $?) {
        Info "The commit is not tagged. Use 'v0.0-dev' as a tag instead"
        $tag = "v0.0-dev"
    }

    $commitHash = & $gitCommand rev-parse --short HEAD
    CheckReturnCodeOfPreviousCommand "Failed to get git commit hash"

    return "$($tag.Substring(1))-$commitHash"
}

Function ForceCopy($srcFile, $dstFile) {
    Info "Copy `n  '$srcFile' `n to `n  '$dstFile'"
    New-Item $dstFile -Force -ItemType File > $null
    Copy-Item $srcFile -Destination $dstFile -Force
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Resolve-Path $PSScriptRoot
$buildDir = "$root/build"
$publishDir = "$buildDir/publish"
$version = GetVersion

Info "version=$version"

Info "Build the project"
dotnet build `
  --nologo `
  --configuration Release `
  -verbosity:minimal `
  /property:DebugType=None `
  /property:Version=$version `
  "$root/Handle2.sln"
CheckReturnCodeOfPreviousCommand "Cmake generation phase failed"

Info "Run tests"
dotnet test `
  --nologo `
  --no-build `
  --configuration Release `
  -verbosity:minimal `
  --logger:"console;verbosity=normal" `
  "$root/Handle2.sln"
CheckReturnCodeOfPreviousCommand "Tests failed"

ForceCopy "$buildDir/Release/Handle2/net462/Handle2.exe" "$publishDir/Handle2.exe"
CreateZipArchive "$publishDir/Handle2.exe" "$publishDir/Handle2.zip"
