RSCacheTool
===========

A command line tool written in C# for extracting and manipulating RuneScape's cache files.

RSCacheTool can:
 - Extract the entire cache or just one archive into separate files in a given directory.
 - Combine sound chunks (.ogg) from a given archive into full-fledged soundtracks.

A pre-compiled archive is available on [my website](https://villermen.com/browser/?d=rscachetool).

RSCacheTool makes use of the following tools and libraries:
 - [SoX](http://sox.sourceforge.net/), a cross-platform command line utility for editing sound files.
 - [SharpZipLip](http://icsharpcode.github.io/SharpZipLib/), a .NET zipping library.
 - [NDesk.Options](http://www.ndesk.org/Options), a command line argument parser.
