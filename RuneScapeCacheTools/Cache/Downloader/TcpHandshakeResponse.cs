namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public enum TcpHandshakeResponse
    {
        Undefined = -1,

        Success = 0,
        Outdated = 6,
        InvalidKey = 48
    }
}