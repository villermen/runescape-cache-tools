# Viller's RuneScape Cache Tools

[![Travis CI build status](https://api.travis-ci.org/villermen/runescape-cache-tools.svg)](https://travis-ci.org/villermen/runescape-cache-tools "Travis CI build status")

###### A .NET library and command-line interface for manipulating RuneScape's cache.
A pre-compiled executable of the latest release is available from the [releases page on GitHub](https://github.com/villermen/runescape-cache-tools/releases).
Extract the archive and run `rsct.exe` to list available options.

#### RuneScape Cache Tools can:
 - Extract the entire cache or single indexes into separate files in a given directory. Decompressing and handing out extensions where appropriate.
 - Combine sound chunks from the soundtrack index into full-fledged, named tracks. By default the lossy OGG format is used, but there is also an option to combine the tracks into FLAC files without loss of quality.
 - Download files straight from Jagex's servers instead of your own incomplete cache.

#### Soundtrack
I've uploaded all named tracks to YouTube. Their playlist can be found [here](https://www.youtube.com/playlist?list=PLLCViMm56RAFqVJKXi13VEFwz7Q_Bi4gR).
You can download the soundtrack we have created together with the tool from [my website](https://villermen.com/browser/music). It's as complete as possible and is updated frequently.

### External dependencies
[SoX - Sound eXchange](http://sox.sourceforge.net/), a command-line audio editing tool, is used by the cache tools to stitch soundtrack files together.
If you plan on combining soundtrack files SoX needs to be available.
On non-Windows platforms this can be accomplished by installing the sox package using something similar to `sudo apt install sox` on Debian-based platforms.
For Windows, either put the SoX binary and its dependencies (dlls) into the same directory as the cache tools, or install the application and add it to your PATH.

### Credits
I would like to thank some great community members who helped me in getting the cache figured out:
- Pea2nuts
- Method
- Graham
- \`Discardedx2
- Sean

#### Show your support!
I've put a lot of love and hard work into this project.
If you like the tools, you can show your appreciation via a small PayPal donation (villermen@gmail.com).

If you have any remaining questions feel free to [send me an email](mailto:villermen@gmail.com).
