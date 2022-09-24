# Handle2
An open source alternative to the [Sysinternals Handle](https://learn.microsoft.com/en-us/sysinternals/downloads/handle) that can be used to find a process that locks a particular file or folder.
The program relies on the closed source Sysinternals `PROCEXP152.SYS` driver.

## Features
* The original Sysinternals Handle has the following bugs that are not present in this program:
  * Unicode names are printed as "????.pdf" instead of "файл.pdf"
  * Command `handle64.exe "C:\Program Files"` will also erroneously print locking information for `C:\Program Files (x86)` folder
  * If a path ends with a `\` like `C:\Windows\` or if `/` symbol is used as a separator like `C:/Windows`, no results will be given.
* Json can be used as an output format
* Only a subset of Sysinternals Handle's functionality related to finding what locks a file or folder is supported

## System requirements
* Windows 10 and higher.
* Admin rights.

## How to install
It is a standalone executable. No installation is required. Download the latest [release](https://github.com/PolarGoose/Handle2/releases) and unzip it.

## Usage
```
handle2.exe [--help] [--json] file_or_folder_name

--help                 display this help and exit
--json                 print output as a json
file_or_folder_name    file or folder name. '\' and '/' separators can be used.
```

## Examples
* `handle2.exe "C:\my repository\project\out\file.exe"`
* `handle2.exe --json "C:/my repository/project/out"`

## How to build
* To work with the codebase `Visual Studio 2022` can be used.
* To build the project run `build.ps1` script from `Developer PowerShell for VS 2022`

## References
* [Backstab](https://github.com/Yaxser/Backstab) - a good example of usage of `PROCEXP152.SYS` driver.
