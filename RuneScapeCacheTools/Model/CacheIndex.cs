namespace Villermen.RuneScapeCacheTools.Model
{
    /// <summary>
    ///
    /// </summary>
    public enum CacheIndex
    {
        AnimationFrames = 0,
        AnimationFrameBases = 1,
        /// <summary>
        /// See <see cref="CacheArchive" /> for contents.
        /// </summary>
        Archives = 2,
        Interfaces = 3,
        /// <summary>
        /// Before 772 (RS3): SoundEffects.
        /// </summary>
        Empty4 = 4,
        Maps = 5,
        /// <summary>
        /// Before build 772 (RS3): MIDIs.
        /// </summary>
        Empty6 = 6,
        Models = 7,
        Sprites = 8,
        /// <summary>
        /// Before build 839: Textures.
        /// </summary>
        Empty9 = 9,
        /// <summary>
        /// Huffman encoding.
        /// </summary>
        Binary = 10,
        /// <summary>
        /// Before build 772 (RS3): Jingles.
        /// </summary>
        Empty11 = 11,
        ClientScripts = 12,
        FontMetrics = 13,
        /// <summary>
        /// Before build 772 (RS3): MidiInstruments.
        /// </summary>
        SoundEffects = 14,
        /// <summary>
        /// Before Build 772 (RS3): MidiInstrumentConfigurations.
        /// </summary>
        Empty15 = 15,
        /// <summary>
        /// Commonly called objects.
        /// </summary>
        Locations = 16,
        Enums = 17,
        Npcs = 18,
        ItemDefinitions = 19,
        Sequences = 20,
        SpotAnimations = 21,
        /// <summary>
        /// Before build 745: VarBits.
        /// </summary>
        Structs = 22,
        WorldMap = 23,
        Quickchat = 24,
        GlobalQuickchat = 25,
        Materials = 26,
        Particles = 27,
        Defaults = 28,
        /// <summary>
        /// Used by models to determine textures and scaling.
        /// </summary>
        Billboards = 29,
        NativeLibraries = 30,
        Shaders = 31,
        /// <summary>
        /// Fonts and images.
        /// </summary>
        LoadingSprites = 32,
        LoadingScreens = 33,
        /// <summary>
        /// In Jagex format.
        /// </summary>
        RawLoadingSprites = 34,
        Cutscenes = 35,
        /// <summary>
        /// Now empty.
        /// </summary>
        VorbisFiles = 36,
        /// <summary>
        /// Before build 839: GfxConfigs.
        /// </summary>
        Empty37 = 37,
        Unknown38 = 38,
        Unknown39 = 39,
        /// <summary>
        /// JAGA &amp; OGG files.
        /// </summary>
        Music = 40,
        WorldMapLabels = 41,
        WorldMapAreas = 42,
        DiffuseTextures = 43,
        HdrTextures = 44,
        DdsTextures = 45,
        MippedTextures = 46,
        /// <summary>
        /// In .ob3 format.
        /// </summary>
        NxtModels = 47,
        NxtAnimations = 48,
        /// <summary>
        /// Best guess: Database table index.
        /// </summary>
        Unknown49 = 49,
        /// <summary>
        /// Best guess: DDS files.
        /// </summary>
        Unknown50 = 50,
        /// <summary>
        /// Best guess: PNG files.
        /// </summary>
        Unknown51 = 51,
        /// <summary>
        /// DDS files.
        /// </summary>
        NxtTextures = 52,
        /// <summary>
        /// PNG files.
        /// </summary>
        MobileTextures = 53,
        /// <summary>
        /// PNG files.
        /// </summary>
        MippedMobileTextures = 54,
        /// <summary>
        /// Best guess: KTX11 (OpenGL) images.
        /// </summary>
        Unknown55 = 55,
        AnimationKeyframes = 56,
        AchievementConfig = 57,

        ReferenceTables = 255
    }
}
