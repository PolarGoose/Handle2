# Handle2
An open-source alternative to the [Sysinternals Handle](https://learn.microsoft.com/en-us/sysinternals/downloads/handle)<tr>

## Features
* Identifies processes that are locking specific files or folders
* Includes process modules in the search
* Shows information about all handles in the system
* Supports JSON output
* Full Unicode support
* Also shows locks on network shares and mounted drives

## System requirements
* Windows 10 and higher.

## Usage
Run `Handle2.exe --help` to see usage information.

### Usage examples
```
Handle2.exe --path C:\Windows\System32
Handle2.exe --path \\wsl.localhost\Ubuntu-24.04\home\user
Handle2.exe --json --path C:\Windows\System32\ntdll.dll
Handle2.exe --json --path C:/Windows/explorer.exe
Handle2.exe --dump-all-handles
Handle2.exe --json --dump-all-handles
```

## How to build
To build the project, run the `build.ps1` script (`git.exe` should be in the PATH)
