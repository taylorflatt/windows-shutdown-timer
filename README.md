# Portable Windows Shutdown Timer
A basic portable Windows shutdown timer. I use this personally but it is available for anyone to use. 

## Function

Creates timer that will shutdown windows after a specified number of minutes. It is non-intrusive meaning once you set the timer, you can hide it or close it without any problems. At its core, it runs a shutdown command through the windows command prompt with a timer attached.

## Features

* Shutdown Windows after specified number of minutes.
* No installation/uninstallation required.
* Can minimize/close without impacting the shutdown timer.
* Can add/subtract additional time to a shutdown timer.
* Can stop an existing shutdown timer (created by this program or not). You will be prompted to confirm the cancellation.
* Timers created by this program will persist even if the program is shutdown.
* Ability to update from the program any time. It simply pulls the exe from GIT and places it in the directory from which the update was run. In other words, it puts the two versions in the same directory.

## Known Issues

There aren't any program breaking issues as of yet. If you discover any, please don't hesitate to contact me or raise an issue. But I am aware of some annoyances and am working on finding solutions to those:

* When the program first starts, if a timer was set by the program previously but has yet to elapse, a check will be run. The check will cause a shutdown notification and a "ding". It doesn't change anything other than simply verify the existence of a timer. I'm looking for ways around this small annoyance. It doesn't happen often though, which is nice.

## Requirements

* Windows 10, Windows 8, Windows 7*
* [.NET Framework 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42642) or better

\*Currently only tested working on Windows 10 and Windows 8. It should work just fine for Windows 7 but I have yet to test it. There isn't anything exclusive to Windows 10/8 so it should honestly work just fine on Windows 7.

## Usage

Download the [latest release](https://github.com/taylorflatt/windows-shutdown-timer/releases) and run it somewhere on your desktop. No installation required. Note, if you end up ever updating it via the updater, the new version will be downloaded to the same directory as the old version.

## Notes

Any user settings are stored in `%USERPROFILE%/AppData/Local/WindowsShutdownTimer`. 
