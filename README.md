# Handle2

An open-source alternative to the [Sysinternals Handle](https://learn.microsoft.com/en-us/sysinternals/downloads/handle)<tr>
* Identifies processes that are locking specific files or folders
* Shows information about all handles in the system
* Supports JSON output
* Full Unicode support
  * Note: `Sysinternals Handle` doesn't support Unicode file names ([more details](https://superuser.com/questions/1761951/sysinternals-handle-prints-question-marks-instead-of-non-ascii-symbols))

## System requirements

* Windows 7 x64 and higher.

## Usage

```
> Handle2.exe --help

Handle2 1.0
A console utility that displays information about system handles and identifies the processes locking a specific file or folder.
https://github.com/PolarGoose/Handle2

Usage:
  Handle2.exe [--json] [--path FILE_OR_FOLDER_FULL_NAME|--dump-all-handles]
Examples:
  Handle2.exe --path C:\Windows\System32
  Handle2.exe --json --path C:\Windows\System32\ntdll.dll
  Handle2.exe --json --path C:\Windows\explorer.exe
  Handle2.exe --json --dump-all-handles

Command-line options:

  --json                (Default: false) Json output

  --path                Required. (Default: false) path

  --dump-all-handles    Required. (Default: false) Displays information about all system handles.

  --help                Display this help screen.

  --version             Display version information.
```

## How to build

* To work with the codebase, `Visual Studio 2022` can be used.
* To build the project, run `build.ps` script (`git.exe` should be in the PATH)
