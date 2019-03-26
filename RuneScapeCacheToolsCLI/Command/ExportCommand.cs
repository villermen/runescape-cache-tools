using System;
using System.Collections.Generic;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class ExportCommand : BaseCommand
    {
        public ExportCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
        }

        public override IList<string> Configure(IEnumerable<string> arguments)
        {
            this.ArgumentParser.Add(ParserOption.FileFilter, ParserOption.SourceCache, ParserOption.OverwriteFiles);

            return this.ArgumentParser.ParseArguments(arguments);
        }

        public override int Run()
        {
            throw new NotImplementedException();
        }
    }
}
