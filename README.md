# [Portable Windows Shutdown Timer](https://github.com/taylorflatt/windows-shutdown-timer/releases/tag/2.1.0.0)
A basic portable Windows shutdown timer. I use this personally but it is available for anyone to use. 

## Function

Creates timer that will shutdown windows after a specified number of minutes. It is non-intrusive meaning once you set the timer, you can hide it or close it without any problems. At its core, it runs a scheduled task whose actions is a shutdown command through the windows command prompt.

## Requirements

* Windows 10, Windows 8.1/8, Windows 7
* [.NET Framework 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42642) or better

## Features

* Shutdown Windows after specified number of minutes.
* No installation/uninstallation required.
* Can minimize/close without impacting the shutdown timer.
* Can add/subtract additional time to a shutdown timer.
* Can stop an existing shutdown timer (created by this program or not). You will be prompted to confirm the cancellation.
* Timers created by this program will persist even if the program is shutdown.
* Ability to update from the program any time. It simply pulls the exe from GIT and places it in the directory from which the update was run. In other words, it puts the two versions in the same directory.

## Usage

Download the [latest release](https://github.com/taylorflatt/windows-shutdown-timer/releases) and run it somewhere on your desktop. No installation required. Note, if you end up ever updating it via the updater, the new version will be downloaded to the same directory as the old version.

## Updating

To update, simply choose the "Update" option in the program and it will determine if there is a newer version available. If there is, you have the option to download it. The newer version will be downloaded and placed in the same place as the older version. Once downloaded, it is then safe to remove the old version.

Otherwise, you can simply download the latest version from the [Release Section](https://github.com/taylorflatt/windows-shutdown-timer/releases).

## Known Issues

There are currently no known issues at this time. If you discover one, please submit a [bug report](https://github.com/taylorflatt/windows-shutdown-timer/issues).

## Notes

Any user settings are stored in `%USERPROFILE%/AppData/Local/WindowsShutdownTimer`. Deleting this file and re-running the program will result in the file being recreated.
