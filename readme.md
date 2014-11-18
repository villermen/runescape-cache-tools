#RSCacheTool

######A command line tool written in C# for extracting and manipulating RuneScape's cache files.

A pre-compiled binary is available for [download from my website](https://villermen.com/browser/?d=rscachetool). Just run the tool without arguments and it will tell you the available options.

####RSCacheTool can:

 - Extract the entire cache or just one archive into separate files in a given directory. Decompressing and handing out extensions where appropriate.
 - Combine sound chunks (.ogg) from a given archive into full-fledged soundtracks.

####RSCacheTool makes use of the following tools and libraries:

 - [NDesk.Options](http://www.ndesk.org/Options), a command line argument parser.
 - [SharpZipLip](http://icsharpcode.github.io/SharpZipLib/), a .NET zipping library. Used to decompress archive files
 - [SoX](http://sox.sourceforge.net/) (not included in source), a cross-platform command line utility for editing sound files. Used to merge .ogg files.
 
####Next up:

 - Automatically name combined music, I'm pretty sure I saw a named index in one of the archives one day...
 - See if more extensions can be handed out than just .png, .ogg and .jaga.
