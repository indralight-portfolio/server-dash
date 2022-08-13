using Common.Utility;
using Dash.StaticData.Item;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public struct EquipSlot
    {
        public int CharacterId;
        public byte SlotIndex;

        public EquipSlot(int characterId, byte slotIndex)
        {
            CharacterId = characterId;
            SlotIndex = slotIndex;
        }

        public bool IsEmpty()
        {
            return CharacterId == 0 || SlotIndex == 0;
        }

        public bool IsEquipped()
        {
            return IsEmpty() == false;
        }

        public bool IsEquipped(int characterId)
        {
            return IsEmpty() == false && CharacterId == characterId;
        }
    }

    public partial class Equipment
    {
        private EquipmentInfo _info;
        [IgnoreMember, JsonIgnore]
        public EquipmentInfo Info
        {
            get
            {
                if (_info == null)
                {
                    if (ItemType == ItemType.Weapon)
                    {
                        StaticInfo.Instance.WeaponInfo.TryGet(Id, out var weaponInfo);
                        _info = weaponInfo;
                    }
                    else if (ItemType == ItemType.Armor)
                    {
                        StaticInfo.Instance.ArmorInfo.TryGet(Id, out var armorInfo);
                        _info = armorInfo;
                    }
                }
                return _info;
            }
        }

        public Equipment(int id)
        {
            Id = id;
            Level = EquipmentInfo.MinLevel;
        }

        [IgnoreMember, JsonIgnore]
        public ItemType ItemType => ItemTypeHelper.GetItemType(Id);

        public static List<Equipment> GetDefaultEquipments(ulong oidAccount, int characterId)
        {
            return new List<Equipment>()
            {
                GetDefaultWeapon(oidAccount, characterId),
            };
        }

        public static Equipment GetDefaultWeapon(ulong oidAccount, int characterId)
        {
            if (StaticInfo.Instance.CharacterInfo.TryGet(characterId, out var info) == false ||
                StaticInfo.Instance.WeaponInfo.TryGet(info.DefaultWeaponId, out var wewaponInfo) == false)
                return null;

            return new Equipment()
            {
                OidAccount = oidAccount,
                Id = wewaponInfo.Id,
                Level = EquipmentInfo.MinLevel,
            };
        }

        [IgnoreMember, JsonIgnore]
        public EquipmentSlotType SlotType => EnumInfo<EquipmentSlotType>.ConvertByte(SlotIndex ?? 0);

        private EquipSlot _slot;
#if Common_Server
        [NotMapped]
#endif
        [IgnoreMember, JsonIgnore]
        public EquipSlot Slot
        {
            get
            {
                if (_slot.IsEmpty() && CharacterId > 0 && SlotIndex > 0)
                {
                    _slot = new EquipSlot((int)CharacterId, (byte)SlotIndex);
                }
                return _slot;
            }
            set
            {
                _slot = value;
                if (_slot.IsEmpty() == true)
                {
                    CharacterId = null;
                    SlotIndex = null;
                }
                else
                {
                    CharacterId = _slot.CharacterId;
                    SlotIndex = _slot.SlotIndex;
                }
            }
        }

#if Common_Server
        [NotMapped]
#endif
        [IgnoreMember, JsonIgnore]
        public List<int> SubStatIndexes
        {
            get
            {
                var list = new List<int>();
                if (SubStat1Index != null)
                    list.Add((int)SubStat1Index);
                else
                    return list;

                if (SubStat2Index != null)
                    list.Add((int)SubStat2Index);
                else
                    return list;

                if (SubStat3Index != null)
                    list.Add((int)SubStat3Index);
                else
                    return list;

                if (SubStat4Index != null)
                    list.Add((int)SubStat4Index);
                else
                    return list;

                if (SubStat5Index != null)
                    list.Add((int)SubStat5Index);
                else
                    return list;

                if (SubStat6Index != null)
                    list.Add((int)SubStat6Index);
                else
                    return list;

                return list;
            }
            set
            {
                var list = value ?? new List<int>();
                if (list.Count > 0)
                    SubStat1Index = list[0];
                else
                    SubStat1Index = null;

                if (list.Count > 1)
                    SubStat2Index = list[1];
                else
                    SubStat2Index = null;

                if (list.Count > 2)
                    SubStat3Index = list[2];
                else
                    SubStat3Index = null;

                if (list.Count > 3)
                    SubStat4Index = list[3];
                else
                    SubStat4Index = null;

                if (list.Count > 4)
                    SubStat5Index = list[4];
                else
                    SubStat5Index = null;

                if (list.Count > 5)
                    SubStat6Index = list[5];
                else
                    SubStat6Index = null;
            }
        }

        public void MakeArmorSubStatus()
        {
            if (Info is ArmorInfo armorInfo)
            {
                var dicMainStatIndexes = StaticInfo.Instance.ArmorMainStatInfo.GetList().Where(e => e.Rarity == armorInfo.Rarity && e.SlotType == armorInfo.EquipmentSlotType).ToDictionary(e => e.Index, e => 1.0d);
                MainStatIndex = ThreadLocalRandom.Choose(dicMainStatIndexes);

                var indexes = new List<int>();
                StaticInfo.Instance.ArmorRarityInfo.TryGet(armorInfo.Rarity, out var rarityInfo);
                for (var j = 0; j < rarityInfo.SubStatusUnlockLevels.Count; j++)
                {
                    var dicSubStatIndexes = StaticInfo.Instance.ArmorSubStatInfo.GetList().Where(e => e.Rarity == armorInfo.Rarity).ToDictionary(e => e.Index, e => 1.0d);
                    var index = ThreadLocalRandom.Choose(dicSubStatIndexes);
                    indexes.Add(index);
                }
                SubStatIndexes = indexes;
            }
        }

        #region Query
        public static string GetBySlotQuery = $"SELECT * FROM {nameof(Equipment)}" +
                                    $" WHERE {nameof(OidAccount)} = @{nameof(OidAccount)}" +
                                    $" AND {nameof(CharacterId)} = @{nameof(CharacterId)}" +
                                    $" AND {nameof(SlotIndex)} = @{nameof(SlotIndex)}";
        public static List<KeyValuePair<string, object>> GetBySlotQueryParam(ulong oidAccount, int characterId, byte slotIndex)
        {
            return new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>($"@{nameof(OidAccount)}", oidAccount),
                new KeyValuePair<string, object>($"@{nameof(CharacterId)}", characterId),
                new KeyValuePair<string, object>($"@{nameof(SlotIndex)}", slotIndex),
            };
        }
        #endregion
    }
}
