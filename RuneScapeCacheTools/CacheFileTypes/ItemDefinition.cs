using System;

namespace Villermen.RuneScapeCacheTools.CacheFileTypes
{
    [Obsolete("Unfinished")]
    public class ItemDefinition
    {
        private enum Opcode
        {
            End = 0,
            ModelId = 1,
            Name = 2,
            Decription = 3,
            ModelZoom = 4,
            ModelRotation1 = 5,
            ModelRotation2 = 6,
            ModelOffset = 7,
            ModelSine = 8,
            UnknownShort = 10,
            Stackable = 12,
            Value = 13,
            MembersOnly = 16,
            MaleEquipPrimaryModel = 23,
            MaleEquipSecondaryModel = 24,
            FemaleEquipPrimaryModel = 25,
            FemaleEquipSecondaryModel = 26,
            GroundAction1 = 30,
            GroundAction2 = 31,
            GroundAction3 = 32,
            GroundAction4 = 33,
            GroundAction5 = 34,
            Attribute1 = 35,
            Attribute2 = 36,
            Attribute3 = 37,
            Attribute4 = 38,
            Attribute5 = 39,
            ModelColors = 40,
            MaleEmblem = 78,
            FemaleEmblem = 79,
            MaleDialogue = 90,
            FemaleDialogue = 91,
            MaleDialogueHat = 92,
            FemaleDialogueHat = 93,
            DiagonalRotation = 95,
            NoteIndex = 97,
            NoteTemplateIndex = 98,
            StackableAmount1 = 100, // TODO: StackableLimit?
            StackableAmount2 = 101,
            StackableAmount3 = 102,
            StackableAmount4 = 103,
            StackableAmount5 = 104,
            StackableAmount6 = 105,
            StackableAmount7 = 106,
            StackableAmount8 = 107,
            StackableAmount9 = 108,
            StackableAmount10 = 109,
            ModelSizeX = 110,
            ModelSizeY = 111,
            ModelSizeZ = 112,
            LightModifier = 113,
            ShadowModifier = 114,
            TeamId = 115
        }
    }
}
