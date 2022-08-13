#if Admin_Server
using Common.StaticInfo;
using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData;
using Dash.StaticData.Entity;
using Dash.StaticData.Episode;
using Dash.StaticData.Event;
using Dash.StaticData.Item;
using Dash.StaticData.Mission;
using Dash.StaticData.Shop;
using Dash.StaticInfo;
using Dash.Types;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    [ModelMetadataType(typeof(Metadata))]
    public partial class Auth
    {
        internal sealed class Metadata
        {
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Account
    {
        internal sealed class Metadata
        {
        }

        public void Update(Account newData)
        {
            Level = newData.Level;
            Exp = newData.Exp;
        }
        public void Update_Static(Account newData)
        {
            Nickname = newData.Nickname;
            Country = newData.Country;
            TimeOffset = newData.TimeOffset;
        }
        public void Update_Footprint(Account newData)
        {
            Created = newData.Created;
            LatestLogon = newData.LatestLogon;
            ReturnTime = newData.ReturnTime;
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Equipment
    {
        internal sealed class Metadata
        {
        }

        [JsonIgnore]
        public EquipmentSlotType EquipmentSlotType => Info?.EquipmentSlotType ?? EquipmentSlotType.Undefined;
        [JsonIgnore]
        public string Name => Info?.GetNameComment();
        [JsonIgnore]
        public Rarity Rarity => Info?.Rarity ?? Rarity.Undefined;

        public bool IsValid()
        {
            return Info != null;
        }

        public void Update(Equipment newData)
        {
            newData.Slot = Slot;
            newData.MainStatIndex = MainStatIndex;
            newData.SubStatIndexes = SubStatIndexes;
            AdminModel_Extensions.Update(this, newData);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Consume
    {
        internal sealed class Metadata
        {
        }

        [JsonIgnore]
        public ItemType Type => Info?.ItemType ?? ItemType.Undefined;
        [JsonIgnore]
        public string Name => Info.GetNameComment();
        [JsonIgnore]
        public Rarity Rarity => Info?.Rarity ?? Rarity.Undefined;

        public bool IsValid()
        {
            return Info != null;
        }

        public void Clamp()
        {
            var max = StaticInfo.Instance.ServiceLogicInfo.Get().Limit.MaxItemStackCount;
            Count = Math.Clamp(Count, 0, max);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Character
    {
        internal sealed class Metadata
        {
        }

        [JsonIgnore]
        public string Name => Info.GetNameComment();
        [JsonIgnore]
        public Rarity Rarity => Info?.Rarity ?? Rarity.Undefined;

        public bool IsValid()
        {
            return StaticInfo.Instance.CharacterInfo.Exist(Id);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class EpisodeClear
    {
        internal sealed class Metadata
        {
        }

        [NotMapped]
        public int Id { get { return EpisodeId; } set { EpisodeId = value; } }

        public string Name => Info?.GetNameComment();

        public int TotalStageCount { get { return Info?.TotalStageCount ?? 0; } }

        public bool IsValid()
        {
            return StaticInfo.Instance.EpisodeInfo.Exist(Id);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class EpisodeEntryCount
    {
        internal sealed class Metadata
        {
        }

        public EpisodeInfo Info { get { StaticInfo.Instance.EpisodeInfo.TryGet(Id, out var info); return info; } }

        public string Name => Info?.GetNameComment();

        public bool IsValid()
        {
            return StaticInfo.Instance.EpisodeInfo.Exist(Id);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Mail
    {
        internal sealed class Metadata
        {
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class MailTemplate
    {
        internal sealed class Metadata
        {
        }

        public MailTargetType TargetType => EnumInfo<MailTargetType>.Parse(Target);
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class MailTarget
    {
        internal sealed class Metadata
        {
        }

        public MailTargetStatus StatusType => EnumInfo<MailTargetStatus>.ConvertSByte(Status);
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class CompletedMission
    {
        internal sealed class Metadata
        {
        }

        [NotMapped]
        public int Id { get { return MissionId; } set { MissionId = value; } }

        public MissionInfo Info { get { StaticInfo.Instance.MissionInfos.TryGetValue(Id, out MissionInfo info); return info; } }

        public MissionType MissionType => Info?.Type ?? MissionType.Undefined;

        public BattleTutorialType BattleTutorialType
        {
            get { return (Info is BattleTutorialInfo tutorialInfo) ? tutorialInfo.TutorialType : BattleTutorialType.Undefined; }
        }
        public LobbyTutorialType LobbyTutorialType
        {
            get { return (Info is LobbyTutorialInfo tutorialInfo) ? tutorialInfo.TutorialType : LobbyTutorialType.Undefined; }
        }

        public string Type
        {
            get
            {
                string type = MissionType.ToString();
                if (MissionType == MissionType.BattleTutorial && BattleTutorialType != BattleTutorialType.Undefined)
                    type += " - " + BattleTutorialType.ToString();
                else if (MissionType == MissionType.LobbyTutorial && LobbyTutorialType != LobbyTutorialType.Undefined)
                    type += " - " + LobbyTutorialType.ToString();
                return type;
            }
        }

        public bool IsValid()
        {
            return StaticInfo.Instance.MissionInfos.ContainsKey(MissionId);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class ShopHistory
    {
        internal sealed class Metadata
        {
        }

        [NotMapped]
        public int Id { get { return ProductId; } set { ProductId = value; } }

        public ProductInfo Info { get { StaticInfo.Instance.ProductInfo.TryGet(Id, out ProductInfo info); return info; } }

        public string Name => Info.GetNameComment();

        public bool IsValid()
        {
            return StaticInfo.Instance.ProductInfo.Exist(ProductId);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class TimeReward
    {
        internal sealed class Metadata
        {
        }

        [NotMapped]
        public string Id => RewardType;
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Coupon
    {
        internal sealed class Metadata
        {
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class SeasonPass
    {
        internal sealed class Metadata
        {
        }

        public string Name => Info.GetComment();

        public void Update(SeasonPass newData, bool reset = false)
        {
            OidAccount = newData.OidAccount;
            Id = newData.Id;
            PassPoint = newData.PassPoint;
            Premium = newData.Premium;

            if (newData.Premium == false)
                RewardedPremium = null;

            if (reset == true)
            {
                RewardedCommon = null;
                RewardedPremium = null;
            }
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class DailyReward
    {
        internal sealed class Metadata
        {
        }

        public string Name => Info.GetComment();
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Friend
    {
        internal sealed class Metadata
        {
        }

        [NotMapped]
        public FriendInviteState InviteStateType
        {
            get { return (FriendInviteState)InviteState; }
            set { InviteState = (byte)value; }
        }

        public Friend ConvertOpponent()
        {
            FriendInviteState newInviteStateType;
            switch (InviteStateType)
            {
                case FriendInviteState.Inviter:
                    newInviteStateType = FriendInviteState.Invitee;
                    break;
                case FriendInviteState.Invitee:
                    newInviteStateType = FriendInviteState.Inviter;
                    break;
                default:
                    newInviteStateType = FriendInviteState.Accepted;
                    break;
            }
            return new Friend
            {
                OidAccount = OidFriend,
                OidFriend = OidAccount,
                InviteStateType = newInviteStateType,
            };
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class Attendance
    {
        internal sealed class Metadata
        {
        }

        public AttendanceInfo Info { get { StaticInfo.Instance.AttendanceInfo.TryGet(Id, out AttendanceInfo info); return info; } }

        public string Name => Info?.EventInfo.GetNameComment();

        public bool IsValid()
        {
            return StaticInfo.Instance.AttendanceInfo.Exist(Id); ;
        }

        public void Update(Attendance newData)
        {
            AdminModel_Extensions.Update(this, newData);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class PeriodOverride
    {
        internal sealed class Metadata
        {
        }

        public string Name
        {
            get
            {
                var nameInfo = PeriodType.GetInfo(Id) as IHasName;
                return nameInfo?.GetNameComment();
            }
        }

        public bool IsValid()
        {
            return PeriodType.GetInfo(Id) != null;
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class CollectionHistory
    {
        internal sealed class Metadata
        {
        }

        public IHasName Info
        {
            get
            {
                if (ItemType.Character.IsInRange(Id) == true)
                {
                    StaticInfo.Instance.CharacterInfo.TryGet(Id, out CharacterInfo info);
                    return info;
                }
                else if (ItemType.Weapon.IsInRange(Id) == true)
                {
                    StaticInfo.Instance.WeaponInfo.TryGet(Id, out WeaponInfo info);
                    return info;
                }
                else
                    return null;
            }
        }

        public string Name => Info?.GetNameComment() ?? string.Empty;

        public bool IsValid()
        {
            return StaticInfo.Instance.CharacterInfo.Exist(Id) || StaticInfo.Instance.WeaponInfo.Exist(Id);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class ConquestScore
    {
        internal sealed class Metadata
        {
        }

        public ConquestSeasonInfo SeasonInfo { get { StaticInfo.Instance.ConquestSeasonInfo.TryGet(SeasonId, out var info); return info; } }

        public void Update(ConquestScore newData)
        {
            newData.Data = Data;
            AdminModel_Extensions.Update(this, newData);
        }
    }

    [ModelMetadataType(typeof(Metadata))]
    public partial class WorldMissionScore
    {
        internal sealed class Metadata
        {
        }

        public WorldMissionEventInfo EventInfo { get { StaticInfo.Instance.WorldMissionEventInfo.TryGet(EventId, out var info); return info; } }

        public void Update(WorldMissionScore newData)
        {
            newData.Data = Data;
            AdminModel_Extensions.Update(this, newData);
        }
    }
}
#endif