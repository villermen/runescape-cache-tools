# Viller's RuneScape Cache Tools
A .NET library and command-line interface for manipulating RuneScape's cache.

A precompiled executable of the latest release is available from the [releases page on GitHub](https://github.com/villermen/runescape-cache-tools/releases).
Extract the archive and run `rsct.exe` to list available options.

## Features
 - Extract the entire cache or single indexes into separate files in a given directory (`rsct.exe extract`). Decompressing and handing out extensions where appropriate.
 - Combine sound chunks into full-fledged, named tracks (`rsct.exe audio`). By default the lossy OGG format is used, but there is also an option to combine the tracks into FLAC files without (minor) loss of quality.
 - Download files straight from Jagex's servers instead of your own incomplete cache.

## "The program opens and closes right after"
RSCT is a command-line application.
That means it is a text-only application that should be run from your terminal with arguments.
Running it directly will only show the manual and exit again.
If you're not familiar with the command-line give [this page on getting started](https://www.davidbaumgold.com/tutorials/command-line/) a read.

## Example commands
- Extract and save files 1-100 from the soundtrack index (40) of your java client's cache: `rsct.exe extract --java 40/1-100`.
- Download and combine the track "Soundscape": `rsct.exe audio --download --filter=soundscape`.
- Download and print all items that start with "kwuarm": `rsct.exe items --download --print="name:kwuarm*"`

## Soundtrack
I've uploaded all named tracks to YouTube.
Their playlist can be found [here](https://www.youtube.com/playlist?list=PLLCViMm56RAFqVJKXi13VEFwz7Q_Bi4gR).
You can download the soundtrack we have created together with the tool from [my website](https://villermen.com/browser/music).
It's as complete as possible and is usually updated within one day after updates.

## External dependencies
[SoX - Sound eXchange](http://sox.sourceforge.net/), a command-line audio editing tool, is used by the cache tools to stitch soundtrack files together.
If you plan on combining soundtrack files SoX needs to be available.
On non-Windows platforms this can be accomplished by installing the sox package using something similar to `sudo apt install sox` on Debian-based platforms.
For Windows, either put the SoX binary and its dependencies (dlls) into the same directory as the cache tools, or install the application and add it to your PATH.

## Credits
Thanks to all the great community members who helped me in getting the cache figured out and making this a better tool.
I'm not listing you here for privacy reasons (unless you want to), but you know who you are!

If you have any remaining questions feel free to [send me an email](mailto:villermen@gmail.com).
