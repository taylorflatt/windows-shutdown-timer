# Windows Shutdown Timer
A basic Windows shutdown timer. I use this personally but it is available for anyone to use. 

## Function

Creates timer that will shutdown windows after a specified number of minutes. It is non-intrusive meaning once you set the timer, you can hide it or close it without any problems.

## Features

* Shutdown windows after specified number of minutes.
* Can minimize/close without impacting the shutdown timer.
* Can add/subtract additional time to a shutdown timer.
* Can stop an existing shutdown timer (created by this program or not). You will be prompted to confirm the cancellation. _Note: I am currently working on saving states after stopping the program so it loads back an existing timer and checks for current timers. It is a simple solution that I haven't gotten around to implementing._

## Requirements

* Windows 10
* [.NET Framework 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42642) or better

Currently only tested working on Windows 10. It should work just fine for Windows 8 and Windows 7 but I have yet to test it on those.
