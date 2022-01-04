using Villermen.RuneScapeCacheTools.File;

namespace Villermen.RuneScapeCacheTools.Model
{
    /// <summary>
    /// The identifier for a parameter of an <see cref="ItemDefinitionFile" />.
    /// </summary>
    public enum ItemProperty
    {
        WeaponRange = 13,
        Unbankable = 59,
        EquipOption1 = 528,
        EquipOption2 = 529,
        EquipOption3 = 530,
        EquipOption4 = 531,
        DropSoundId = 537,
        StrengthBonus = 641,
        RangedBonus = 643,
        /// <summary>
        /// Can only be traded for an equal amount of items with the same category.
        /// </summary>
        RestrictedTrade = 689,
        UnusedHandCannonWarning = 690,
        MobilisingArmiesSquad = 802,
        /// <summary>
        /// Used instead of the default ("A magical force prevents you from banking this item").
        /// </summary>
        UnbankableMessage = 824,
        MagicBonus = 965,
        /// <summary>
        /// 0 = attack, 1 = defence, 2= strength, 4 = ranged, 5 = prayer, 6 = magic
        /// </summary>
        EquipSkillRequired = 749,
        EquipLevelRequired = 750,
        EquipSkillRequired2 = 751,
        EquipLevelRequired2 = 752,
        EquipOption5 = 1211,
        BankInventoryOption1 = 1264,
        BankInventoryOption2 = 1265,
        LifePointBonus = 1326,
        GeCategory = 2195,
        BeastOfBurdenStorable = 2240,
        ConstructionMaterialsRequired = 2522,
        MeleeAffinity = 2866,
        RangedAffinity = 2867,
        MagicAffinity = 2868,
        ArmourBonus = 2870,
        PrayerBonus = 2946,
        PotionEffectValue = 3000,
        UnknownPopItemCharge = 3109,
        WeaponAccuracy = 3267,
        RepairCost = 3383,
        CombatCharges = 3385,
        PortentOfDegradationHealAmount = 3698,
        Broken = 3793,
        MtxDescription = 4085,
        SpecialAttackCost = 4332,
        SpecialAttackName = 4333,
        SpecialAttackDescription = 4334,
        DestroyForGp = 4907,
        DestroyText = 5417,
        ZarosItem = 5440,
        UnknownBookcaseReclaimCost = 5637,
        UnknownFayreTokenRelated = 6405,
        SigilCooldownDefault = 6520,
        SigilCooldown = 6521,
        SigilMaxCharges = 6522,
        EquipOption6 = 6712,
        EquipOption7 = 6713,
        EquipOption8 = 6714,
        PofFarmLevel = 7477,
    }
}
