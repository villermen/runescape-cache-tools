using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;

namespace Villermen.RuneScapeCacheTools.Cache.FlatFile
{
    [Obsolete("Unfinished")]
    public class FlatFileCache : CacheBase
    {
        public override IEnumerable<Index> Indexes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override BinaryFile FetchFile(Index index, int fileId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<int> GetFileIds(Index index)
        {
            throw new NotImplementedException();
        }

        protected override void PutFile(BinaryFile file)
        {
            throw new NotImplementedException();
        }

        public override CacheFileInfo GetFileInfo(Index index, int fileId)
        {
            throw new NotImplementedException();
        }
    }
}