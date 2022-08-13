using System.Collections.Generic;

namespace Dash
{
    public class ProgressKey
    {
        public static readonly string GainGold = nameof(GainGold);
        public static readonly string ConsumeGold = nameof(ConsumeGold);
        public static readonly string GainJewel = nameof(GainJewel);
        public static readonly string ConsumeJewel = nameof(ConsumeJewel);
        public static readonly string GainStamina = nameof(GainStamina);
        public static readonly string ConsumeStamina = nameof(ConsumeStamina);
        public static readonly string AccountLevelUp = nameof(AccountLevelUp);

        public static readonly string GainItem = nameof(GainItem);
        public static readonly string GainChar = nameof(GainChar);
        public static readonly string PlayGame = nameof(PlayGame);
        public static readonly string PlayMultiplay = nameof(PlayMultiplay);
        public static readonly string WeaponOvercome = nameof(WeaponOvercome);
        public static readonly string WeaponReforge = nameof(WeaponReforge);
        public static readonly string EquipLvUp = nameof(EquipLvUp);
        public static readonly string EquipDecom = nameof(EquipDecom);
        public static readonly string CharLvUp = nameof(CharLvUp);
        public static readonly string CharOvercome = nameof(CharOvercome);
        public static readonly string CharRnkUp = nameof(CharRnkUp);
        public static readonly string EpClear = nameof(EpClear);
        public static readonly string EpGroupClear = nameof(EpGroupClear);
        public static readonly string EpClear_M = nameof(EpClear_M);
        public static readonly string EpGroupClear_M = nameof(EpGroupClear_M);
        public static readonly string EpClear_S = nameof(EpClear_S);
        public static readonly string EpGroupClear_S = nameof(EpGroupClear_S);
        public static readonly string GachaOpen = nameof(GachaOpen);

        public static List<string> GainItemKeys(int id)
        {
            List<string> keys = new List<string>();
            keys.Add(GainItem);
            if (StaticInfo.StaticInfo.Instance.EquipmentInfos.TryGetValue(id, out var info))
            {
                keys.Add(GainItem + $"_{info.ItemType}");
                keys.Add(GainItem + $"_{info.Rarity}");
                keys.Add(GainItem + $"_{info.ItemType}|{info.Rarity}");
            }
            return keys;
        }
        public static List<string> EquipmentLevelUpKeys(int id)
        {
            if (StaticInfo.StaticInfo.Instance.EquipmentInfos.TryGetValue(id, out var info) == false)
            {
                return null;
            }
            return new List<string>
            {
                EquipLvUp,
                EquipLvUp + $"_{info.ItemType}",
            };
        }
        public static List<string> EquipmentDecomposeKeys(int id)
        {
            if(StaticInfo.StaticInfo.Instance.EquipmentInfos.TryGetValue(id, out var info) == false)
            {
                return null;
            }
            return new List<string>
            {
                EquipDecom,
                EquipDecom + $"_{info.ItemType}",
            };
        }
        public static List<string> GainCharacterKeys(int id)
        {
            if(StaticInfo.StaticInfo.Instance.CharacterInfo.TryGet(id, out var characterInfo) == false)
            {
                return null;
            }
            return new List<string>
            {
                GainChar,
                GainChar + $"{characterInfo.Rarity}",
            };
        }
        public static List<string> CharacterLevelUpKeys(int id)
        {
            return new List<string>
            {
                CharLvUp,
            };
        }
        public static List<string> CharacterOvercomeKeys(int id)
        {
            if (StaticInfo.StaticInfo.Instance.CharacterInfo.TryGet(id, out var info) == false)
            {
                return null;
            }
            return new List<string>
            {
                CharOvercome,
            };
        }
        public static List<string> CharacterRankUpKeys(int id, byte rank)
        {
            return new List<string>
            {
                CharRnkUp,
                CharRnkUp + $"_R{rank}",
            };
        }
        public static List<string> GetEpisodeClearKeys(int id, bool isMultiPlay)
        {
            if (StaticInfo.StaticInfo.Instance.EpisodeInfo.TryGet(id, out var info) == false)
            {
                return null;
            }
            // do not change order
            List<string> keys = new List<string>();
            keys.Add(EpClear);
            keys.Add(EpClear + $"_{info.Id}");
            keys.Add(EpGroupClear + $"_{info.EpisodeGroupId}");
            keys.Add(EpGroupClear + $"_{info.GroupType}");
            if (isMultiPlay)
            {
                keys.Add(EpClear_M);
                keys.Add(EpClear_M + $"_{info.Id}");
                keys.Add(EpGroupClear_M + $"_{info.EpisodeGroupId}");
                keys.Add(EpGroupClear_M + $"_{info.GroupType}");
            }
            else
            {
                keys.Add(EpClear_S);
                keys.Add(EpClear_S + $"_{info.Id}");
                keys.Add(EpGroupClear_S + $"_{info.EpisodeGroupId}");
                keys.Add(EpGroupClear_S + $"_{info.GroupType}");
            }
            return keys;
        }
        public static List<string> WeaponReforgeKeys(int id, int reforge)
        {
            return new List<string>
            {
                WeaponReforge,
                WeaponReforge + $"_R{reforge}"
            };
        }
        public static List<string> WeaponOvercomeKeys(int id)
        {
            if(StaticInfo.StaticInfo.Instance.WeaponInfo.TryGet(id, out var weaponInfo) == false)
            {
                return null;
            }
            return new List<string>
            {
                WeaponOvercome,
            };
        }
        public static List<string> GachaOpenKeys(int id)
        {
            if(StaticInfo.StaticInfo.Instance.GachaInfo.TryGet(id, out var gachaInfo) == false)
            {
                return null;
            }
            return new List<string> 
            {
                GachaOpen,
            };
        }
    }
}