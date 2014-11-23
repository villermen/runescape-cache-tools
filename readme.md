#RSCacheTool

######A command line tool written in C# for extracting and manipulating RuneScape's cache files.

A pre-compiled binary is available for [download from my website](https://villermen.com/browser/?d=rscachetool). Just run the tool without arguments and it will tell you the available options.

####RSCacheTool can:

 - Extract the entire cache or just one archive into separate files in a given directory. Decompressing and handing out extensions where appropriate.
 - Combine sound chunks (.ogg) from a given archive into full-fledged soundtracks. It can even give them their in-game name based on another file from the archive.
 
####Usage examples:

 - Get help: `rscachetool`, done.
 - If you're aiming only at ripping named complete music from your cache these are your 2 golden commands: `rscachetool -e=40 -c` and `rscachetool -e=17 -n`.
 - Extract all archives, combine music (including incomplete) and try to name the tracks `rscachetool -e -c -i -n`.
 - Recombine sound 21713.jaga extracted to D:\cache and rename it `rscachetool -c -f=21713 -o -n=21713 D:\cache`. (Basically this tries to fix a file if SoX mangled it before.)

####RSCacheTool makes use of the following tools and libraries:

 - [NDesk.Options](http://www.ndesk.org/Options), a command line argument parser.
 - [SharpZipLip](http://icsharpcode.github.io/SharpZipLib/), a .NET zipping library. Used to decompress archive files
 - [SoX](http://sox.sourceforge.net/) (not included in repository), a command line utility for editing sound files. Used to merge soundchunks.

####Be a nice guy and help me out

If you like this tool you can show your appreciation by helping me complete my music archive. If you have any named extraced music files that do not exist in [my archive](https://villermen.com/browser/?d=music/Runescape%203), please share them with me in whatever way possible (dropbox, drive, puush, you name it). A small PayPal donation (villermen@gmail.com) wouldn't hurt either too!
