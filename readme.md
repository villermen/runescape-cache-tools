# Viller's RuneScape Cache Tools

###### A .NET library and interface for extracting and manipulating RuneScape's cache files.
A pre-compiled executable of the latest release is available from the [releases page on GitHub](https://github.com/villermen/runescape-cache-tools/releases). Extract the archive and run RuneScapeCacheToolsGUI.exe to open the user interface.

#### RuneScape Cache Tools can:
 - Extract the entire cache or single archives into separate files in a given directory. Decompressing and handing out extensions where appropriate.
 - Combine sound chunks from the soundtrack index into full-fledged, named tracks.
 - Show a list of all soundtrack names, or a list of all the ones not present in the output directory (so you know what to load into the cache in-game).
 - Download files straight from Jagex's servers instead of your own incomplete cache.

#### Soundtrack
I've uploaded all named tracks to youtube. Playlist can be found [here](https://www.youtube.com/playlist?list=PLLCViMm56RAFqVJKXi13VEFwz7Q_Bi4gR).
You can download the soundtrack we have created together with the tool from [my website](https://villermen.com/browser/music). It's as complete as possible. If you have extracted any tracks that are not (yet) downloadable in the file browser, you can contact me at any time. [An email](mailto:villermen@gmail.com) would probably be the best way to contact me.

### External dependencies
On non-Windows platforms, oggCat needs to be made available in order to combine the soundtrack. You can accomplish this by installing [Ogg Video Tools](http://www.streamnik.de/oggvideotools.html) (`apt-get install oggvideotools` on Debian-based platforms).

### Credits
I would like to thank some great community members who helped me in getting the cache figured out:
- Pea2nuts
- Method
- Graham
- \`Discardedx2
- Sean

#### Show your support!
I've put a lot of love and hard work into this project. If you like the tools, you can show your appreciation via a small PayPal donation (villermen@gmail.com).
