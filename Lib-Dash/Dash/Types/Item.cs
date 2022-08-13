using Common.StaticInfo;
using Common.Utility;
using Dash.Model.Rdb;
using Dash.StaticData.Item;
using Dash.StaticInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dash.Types
{
    //중간에 끼어들것을 고려해서 10 단위로 증가
    public enum Rarity : ushort
    {
        Undefined = 0,
        Common = 10,
        Rare = 20,
        Epic = 30,
        Legendary = 40,
        Infiity = 65535,
    }

    public enum ItemIdRange : int
    {
        Character_Start = 1,    //1~99
        Character_End = 99,
        CharacterSoul_Start = 101,
        CharacterSoul_End = 199,
        Weapon_Start = 1100001,   //1100001~1199999
        Weapon_End = 1199999,
        Armor_Start = 1200001,   //1200001~1299999
        Armor_End = 1299999,
        Material_Start = 2100001,   //2100001~2199999
        Material_End = 2199999,
        EventCoin_Start = 2200001, //2200001~2299999
        EventCoin_End = 2299999,
        RewardBox_Start = 3100001,  //3100001~3199999
        RewardBox_End = 3199999,
        MoneyBox_Start = 3200001,  //3200001~3299999
        MoneyBox_End = 3299999,
        Ticket_Start = 5100001,  //5100001~5199999
        Ticket_End = 5199999,
    }

    public enum NonItemType
    {
        Undefined = 0,
        AccountExp,
        CharacterExp,

        Jewel,
        Gold,
        Stamina,
        Mileage,
        GachaTicket,
        GachaLimitedTicket,
        SeasonPoint,
        DailyQuestPoint,
        WeeklyQuestPoint,
        AchievementPoint,
        ConquestPoint,
    }

    public enum ItemType
    {
        Undefined = 0,
        Character,
        Weapon,
        Armor,
        CharacterSoul,
        Material,
        RewardBox,
        MoneyBox,
        EventCoin,
        Ticket,
    }

    public enum ItemDbType
    {
        Undefined = 0,
        Character,
        Equipment,
        Consume,
    }

    public enum WeaponType
    {
        Undefined = 0,
        Staff,
        Sword,
        Pistol,
        TwoHandSword,
        Spear,
        Bow,
    }

    public enum MaterialType
    {
        Undefined = 0,
        CharacterLevelUp,
        CharacterOvercomeUp,
        WeaponLevelUp,
        WeaponOvercomeUp,
        ArmorLevelUp,
        RuneGroupUnlock,
    }

    public enum TicketType
    {
        Undefined = 0,
        SweepEpisode,
    }

    public enum RewardBoxType
    {
        Undefined = 0,
        Fixed,
        Random,
        Selectable,
    }

    public enum EquipmentSlotType
    {
        Undefined = 0,
        Weapon = 1,
        Armor1 = 2,
        Armor2 = 3,
        Armor3 = 4,
        Armor4 = 5,
    }

    public class RarityComparer : IEqualityComparer<Rarity>
    {
        public bool Equals(Rarity x, Rarity y)
        {
            return x == y;
        }

        public int GetHashCode(Rarity x)
        {
            return (int)x;
        }
    }

    public static class EquipmentTypeHelper
    {
        public static bool IsEquipable(EquipmentInfo info, EquipSlot slot)
        {
            if (slot.IsEmpty()) return false;
            return IsEquipable(info, slot.CharacterId, slot.SlotIndex);
        }
        public static bool IsEquipable(EquipmentInfo info, int chracterId, byte slotIndex)
        {
            EquipmentSlotType slotType = EnumInfo<EquipmentSlotType>.ConvertByte(slotIndex);
            return IsEquipable(info, chracterId, slotType);
        }
        public static bool IsEquipable(EquipmentInfo info, int chracterId, EquipmentSlotType slotType)
        {
            bool result = info.EquipmentSlotType == slotType;
            if (info is WeaponInfo weaponInfo)
            {
                StaticInfo.StaticInfo.Instance.CharacterInfo.TryGet(chracterId, out var characterInfo);
                result &= characterInfo?.WeaponType == weaponInfo.WeaponType;
            }
            return result;
        }
    }
    public static class ItemTypeHelper
    {
        public static bool IsInRange(ItemIdRange start, ItemIdRange end, int id)
        {
            return (int)start <= id && id <= (int)end;
        }

        public static ItemType GetItemType(int id)
        {
            if (IsInRange(ItemIdRange.Character_Start, ItemIdRange.Character_End, id))
                return ItemType.Character;
            else if (IsInRange(ItemIdRange.Weapon_Start, ItemIdRange.Weapon_End, id))
                return ItemType.Weapon;
            else if (IsInRange(ItemIdRange.Armor_Start, ItemIdRange.Armor_End, id))
                return ItemType.Armor;
            else if (IsInRange(ItemIdRange.CharacterSoul_Start, ItemIdRange.CharacterSoul_End, id))
                return ItemType.CharacterSoul;
            else if (IsInRange(ItemIdRange.Material_Start, ItemIdRange.Material_End, id))
                return ItemType.Material;
            else if (IsInRange(ItemIdRange.EventCoin_Start, ItemIdRange.EventCoin_End, id))
                return ItemType.EventCoin;
            else if (IsInRange(ItemIdRange.RewardBox_Start, ItemIdRange.RewardBox_End, id))
                return ItemType.RewardBox;
            else if (IsInRange(ItemIdRange.MoneyBox_Start, ItemIdRange.MoneyBox_End, id))
                return ItemType.MoneyBox;
            else if (IsInRange(ItemIdRange.Ticket_Start, ItemIdRange.Ticket_End, id))
                return ItemType.Ticket;
            else
                return ItemType.Undefined;
        }

        public static ItemDbType GetItemDbType(int id)
        {
            return GetItemDbType(GetItemType(id));
        }
        public static ItemDbType GetItemDbType(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Character:
                    return ItemDbType.Character;
                case ItemType.Weapon:
                case ItemType.Armor:
                    return ItemDbType.Equipment;
                case ItemType.CharacterSoul:
                case ItemType.Material:
                case ItemType.RewardBox:
                case ItemType.MoneyBox:
                case ItemType.EventCoin:
                case ItemType.Ticket:
                    return ItemDbType.Consume;
                default:
                    return ItemDbType.Undefined;
            }
        }

        public static IKeyValueData<int> GetItemInfo(int id)
        {
            switch (GetItemType(id))
            {
                case ItemType.Character:
                    StaticInfo.StaticInfo.Instance.CharacterInfo.TryGet(id, out var characterInfo);
                    return characterInfo;
                case ItemType.Weapon:
                    StaticInfo.StaticInfo.Instance.WeaponInfo.TryGet(id, out var weaponInfo);
                    return weaponInfo;
                case ItemType.Armor:
                    StaticInfo.StaticInfo.Instance.ArmorInfo.TryGet(id, out var armorInfo);
                    return armorInfo;
                case ItemType.CharacterSoul:
                    StaticInfo.StaticInfo.Instance.CharacterSoulInfo.TryGet(id, out var characterSoulInfo);
                    return characterSoulInfo;
                case ItemType.Material:
                    StaticInfo.StaticInfo.Instance.MaterialInfo.TryGet(id, out var materialInfo);
                    return materialInfo;
                case ItemType.RewardBox:
                    StaticInfo.StaticInfo.Instance.BoxInfo.TryGet(id, out var rewardBoxInfo);
                    return rewardBoxInfo;
                case ItemType.MoneyBox:
                    StaticInfo.StaticInfo.Instance.MoneyBoxInfo.TryGet(id, out var moneyBoxInfo);
                    return moneyBoxInfo;
                case ItemType.EventCoin:
                    StaticInfo.StaticInfo.Instance.EventCoinInfo.TryGet(id, out var eventCoinInfo);
                    return eventCoinInfo;
                case ItemType.Ticket:
                    StaticInfo.StaticInfo.Instance.TicketInfo.TryGet(id, out var ticketInfo);
                    return ticketInfo;
                default:
                    return null;
            }
        }

        public static IHasName GetItemNameInfo(int id)
        {
            var info = GetItemInfo(id);
            if (info is IHasName infoName)
            {
                return infoName;
            }
            return null;
        }

        public static bool IsInRange(this ItemType type, int id)
        {
            return GetItemType(id) == type;
        }

        public static bool IsInRange(this ItemDbType type, int id)
        {
            return GetItemDbType(id) == type;
        }
        public static bool IsInRange(this ItemDbType type, ItemType itemType)
        {
            return GetItemDbType(itemType) == type;
        }

        public static bool IsStackableItem(int id)
        {
            return IsStackableItem(GetItemType(id));
        }
        public static bool IsStackableItem(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Character:
                case ItemType.Weapon:
                case ItemType.Armor:
                    return false;
                default:
                    return true;
            }
        }


        public static bool IsUsableItem(int id)
        {
            return IsUsableItem(GetItemType(id));
        }
        public static bool IsUsableItem(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.RewardBox:
                case ItemType.MoneyBox:
                    return true;
            }
            return false;
        }

        public static int GetSoulId(int id)
        {
            if (ItemType.CharacterSoul.IsInRange(id))
            {
                StaticInfo.StaticInfo.Instance.CharacterSoulInfo.TryGet(id, out var characterSoulInfo);
                return characterSoulInfo?.CharacterId ?? 0;
            }
            return 0;
        }

        public static bool CheckVerify(int key)
        {
            return StaticInfo.StaticInfo.Instance.ItemKeys.Contains(key);
        }

        public static Rarity GetRarity(int id)
        {
            var info = GetItemInfo(id);
            if (info is IRarity rarityInfo)
            {
                return rarityInfo.Rarity;
            }
            return Rarity.Undefined;
        }

        public static CodeLocale GetLocaleName(this NonItemType type)
        {
            switch (type)
            {
                case NonItemType.AccountExp:
                    return CodeLocale.Dash_Types_NonItemType_AccountExp_Name;
                case NonItemType.CharacterExp:
                    return CodeLocale.Dash_Types_NonItemType_CharacterExp_Name;

                /* Money */
                case NonItemType.Jewel:
                    return CodeLocale.Dash_Types_NonItemType_Jewel_Name;
                case NonItemType.Gold:
                    return CodeLocale.Dash_Types_NonItemType_Gold_Name;
                case NonItemType.Stamina:
                    return CodeLocale.Dash_Types_NonItemType_Stamina_Name;
                case NonItemType.Mileage:
                    return CodeLocale.Dash_Types_NonItemType_Mileage_Name;
                case NonItemType.GachaTicket:
                    return CodeLocale.Dash_Types_NonItemType_GachaTicket_Name;
                case NonItemType.GachaLimitedTicket:
                    return CodeLocale.Dash_Types_NonItemType_GachaLimitedTicket_Name;
                /* Special Money */
                case NonItemType.SeasonPoint:
                    return CodeLocale.Dash_Types_NonItemType_SeasonPoint_Name;
                case NonItemType.ConquestPoint:
                    return CodeLocale.Dash_Types_NonItemType_ConquestPoint_Name;
                /* Quest */
                case NonItemType.DailyQuestPoint:
                    return CodeLocale.Dash_Types_NonItemType_DailyQuestPoint_Name;
                case NonItemType.WeeklyQuestPoint:
                    return CodeLocale.Dash_Types_NonItemType_WeeklyQuestPoint_Name;
                case NonItemType.AchievementPoint:
                    return CodeLocale.Dash_Types_NonItemType_AchievementPoint_Name;
            }
            return CodeLocale.Undefined;
        }

        public static CodeLocale GetLocaleDesc(this NonItemType type)
        {
            switch (type)
            {
                case NonItemType.AccountExp:
                    return CodeLocale.Dash_Types_NonItemType_AccountExp_Desc;
                case NonItemType.CharacterExp:
                    return CodeLocale.Dash_Types_NonItemType_CharacterExp_Desc;

                case NonItemType.Jewel:
                    return CodeLocale.Dash_Types_NonItemType_Jewel_Desc;
                case NonItemType.Gold:
                    return CodeLocale.Dash_Types_NonItemType_Gold_Desc;
                case NonItemType.Stamina:
                    return CodeLocale.Dash_Types_NonItemType_Stamina_Desc;
                case NonItemType.Mileage:
                    return CodeLocale.Dash_Types_NonItemType_Mileage_Desc;
                case NonItemType.GachaTicket:
                    return CodeLocale.Dash_Types_NonItemType_GachaTicket_Desc;
                case NonItemType.GachaLimitedTicket:
                    return CodeLocale.Dash_Types_NonItemType_GachaLimitedTicket_Desc;
                case NonItemType.SeasonPoint:
                    return CodeLocale.Dash_Types_NonItemType_SeasonPoint_Desc;
                case NonItemType.ConquestPoint:
                    return CodeLocale.Dash_Types_NonItemType_ConquestPoint_Desc;
                case NonItemType.DailyQuestPoint:
                    return CodeLocale.Dash_Types_NonItemType_DailyQuestPoint_Desc;
                case NonItemType.WeeklyQuestPoint:
                    return CodeLocale.Dash_Types_NonItemType_WeeklyQuestPoint_Desc;
                case NonItemType.AchievementPoint:
                    return CodeLocale.Dash_Types_NonItemType_AchievementPoint_Desc;
            }
            return CodeLocale.Undefined;
        }

        public static CodeLocale GetLocale(this Rarity grade)
        {
            switch (grade)
            {
                case Rarity.Common:
                    return CodeLocale.Dash_Types_Rarity_Common;
                case Rarity.Rare:
                    return CodeLocale.Dash_Types_Rarity_Rare;
                case Rarity.Epic:
                    return CodeLocale.Dash_Types_Rarity_Epic;
                case Rarity.Legendary:
                    return CodeLocale.Dash_Types_Rarity_Legendary;
            }
            return CodeLocale.Undefined;
        }

        public static CodeLocale GetLocale(this ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Character:
                    return CodeLocale.Dash_Types_ItemType_Character;
                case ItemType.Weapon:
                    return CodeLocale.Dash_Types_ItemType_Weapon;
                case ItemType.Armor:
                    return CodeLocale.Dash_Types_ItemType_Armor;
                case ItemType.CharacterSoul:
                    return CodeLocale.Dash_Types_ItemType_CharacterSoul;
                case ItemType.Material:
                    return CodeLocale.Dash_Types_ItemType_Material;
                case ItemType.RewardBox:
                    return CodeLocale.Dash_Types_ItemType_RewardBox;
                case ItemType.MoneyBox:
                    return CodeLocale.Dash_Types_ItemType_MoneyBox;
                case ItemType.EventCoin:
                    return CodeLocale.Dash_Types_ItemType_EventCoin;
                case ItemType.Ticket:
                    return CodeLocale.Dash_Types_ItemType_Ticket;
                default:
                    return CodeLocale.Undefined;
            }
        }

        public static CodeLocale GetLocale(this TicketType ticketType)
        {
            switch (ticketType)
            {
                case TicketType.SweepEpisode:
                    return CodeLocale.Dash_Types_TicketType_SweepEpisode;
                default:
                    return CodeLocale.Dash_Types_ItemType_Ticket;
            }
        }

        public static CodeLocale GetLocale(this WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Staff:
                    return CodeLocale.Dash_Types_WeaponType_Staff;
                case WeaponType.Sword:
                    return CodeLocale.Dash_Types_WeaponType_Sword;
                case WeaponType.Pistol:
                    return CodeLocale.Dash_Types_WeaponType_Pistol;
                case WeaponType.Bow:
                    return CodeLocale.Dash_Types_WeaponType_Bow;
                case WeaponType.TwoHandSword:
                    return CodeLocale.Dash_Types_WeaponType_TwoHandSword;
                default:
                    return CodeLocale.Undefined;
            }
        }

        public static CodeLocale GetLocale(this EquipmentSlotType slotType)
        {
            switch (slotType)
            {

                case EquipmentSlotType.Weapon:
                    return CodeLocale.Dash_Types_EquipmentSlotType_Weapon;
                case EquipmentSlotType.Armor1:
                    return CodeLocale.Dash_Types_EquipmentSlotType_Armor1;
                case EquipmentSlotType.Armor2:
                    return CodeLocale.Dash_Types_EquipmentSlotType_Armor2;
                case EquipmentSlotType.Armor3:
                    return CodeLocale.Dash_Types_EquipmentSlotType_Armor3;
                case EquipmentSlotType.Armor4:
                    return CodeLocale.Dash_Types_EquipmentSlotType_Armor4;
                default:
                    return CodeLocale.Undefined;
            }
        }
        public static int GetRarityColorIndex(this Rarity rarity)
        {
            switch (rarity)
            {
                default:
                case Rarity.Common:
                    return 0;
                case Rarity.Rare:
                    return 1;
                case Rarity.Epic:
                    return 2;
                case Rarity.Legendary:
                    return 3;
            }
        }
    }
}