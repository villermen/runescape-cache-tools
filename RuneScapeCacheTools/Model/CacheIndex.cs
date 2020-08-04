namespace Villermen.RuneScapeCacheTools.Model
{
    public enum CacheIndex
    {
        AnimationFrames = 0,
        AnimationFrameBases = 1,
        Archives = 2, // See CacheArchive enum for contents.
        Interfaces = 3,
        UsedToBeSoundEffects = 4,  // Pre 772.
        Maps = 5,
        UsedToBeMidis = 6, // Pre 772.
        Models = 7,
        Sprites = 8,
        UsedToBeTextures = 9, // Pre 839.
        HuffmanEncoding = 10,
        UsedToBeJingles = 11, // Pre 772.
        ClientScripts = 12,
        FontMetrics = 13,
        SoundEffects = 14, // Pre 772: MIDI instruments.
        UsedToBeMidiInstrumentConfigurations = 15, // Pre 772.
        Locations = 16,
        Enums = 17,
        Npcs = 18,
        ItemDefinitions = 19,
        Sequences = 20,
        SpotAnimations = 21,
        Structs = 22, // Pre 745: Var bit.
        WorldMap = 23,
        Quickchat = 24,
        GlobalQuickchat = 25,
        Materials = 26,
        Particles = 27,
        Defaults = 28,
        Billboards = 29, // Model textures and scaling.
        NativeLibraries = 30,
        Shaders = 31,
        LoadingSprites = 32, // Fonts and images
        LoadingScreens = 33,
        RawLoadingSprites = 34,
        Cutscenes = 35,
        UsedToBeVorbisFiles = 36,
        UsedToBeGfxConfigs = 37, // Pre 839.
        Unknown38 = 38,
        Unknown39 = 39,
        Music = 40,
        WorldMapAreas = 41, // Might actually be the labels as opposed to 42.
        WorldMapLabels = 42,
        DiffuseTextures = 43,
        HdrTextures = 44,
        DdsTextures = 45,
        MippedTextures = 46,
        NxtModels = 47,
        NxtAnimations = 48, // Unsure.
        DatabaseTableIndex = 49, // Unsure.
        Unknown50 = 50, // DDS images?
        Unknown51 = 51, // PNG images?
        Unknown52 = 52, // DDS images?
        MobileTextures = 53,
        Unknown54 = 54, // PNG images?
        Unknown55 = 55, // KTX11 (OpenGL) images?
        Unknown56 = 56,

        ReferenceTables = 255
    }
}
