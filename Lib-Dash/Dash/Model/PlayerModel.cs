using System.Collections.Generic;
using System.Text;
using Dash.State;
using Dash.Types;
using MessagePack;

namespace Dash.Model
{
    using Common.StaticInfo;
    using Dash.StaticData.Entity;
    using Rdb;
    using System;

    [MessagePackObject()]
    public class PlayerModel : ISerializableState
    {
        public int GetTypeCode() => TypeCode;
        public static readonly int TypeCode = Common.Utility.StringHash.GetHashCode(nameof(PlayerModel));

        [Key(0)] public ulong OidAccount;
        [Key(1)] public string Nickname;
        [Key(2)] public int PlayerId;
        [Key(3)] public bool IsHost;
        [Key(4)] public PlayerType Type;
        [Key(5)] public Character Character;
        [Key(6)][MPackStateField] public EquipRune EquipRune;
        [Key(7)][MPackStateField] public List<Equipment> Equipments;
        [Key(8)][MPackStateField] public List<Collection> Collections;
        [Key(9)][MPackStateField] public List<CollectionHistory> CollectionHistories;

        public override string ToString()
        {
            StaticInfo.StaticInfo.Instance.CharacterInfo.TryGet(Character.Id, out var characterInfo);
            StaticInfo.StaticInfo.Instance.RuneGroupInfo.TryGet(characterInfo.RuneGroupInfoId, out var runeGroupInfo);
            string strEquipRuneId = "";
            if (EquipRune != null)
            {
                foreach (var p in EquipRune.RuneIndexes)
                {
                    byte tier = p.Key;
                    sbyte runeIndex = p.Value;
                    if (runeGroupInfo.RuneInfos.TryGetValue(tier, out var runeInfos) == false)
                    {
                        continue;
                    }
                    RuneInfo runeInfo = runeInfos[runeIndex];
                    strEquipRuneId += $"[{runeInfo.Id}]";
                }
            }
            return $"[{OidAccount}][{Nickname}][{PlayerId}] |Character| [{Character.Id}][{Character.Rank}] |Rune| {strEquipRuneId}";
        }
    }
}