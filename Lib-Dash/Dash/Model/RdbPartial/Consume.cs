using Dash.StaticData.Item;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public partial class Consume
    {
        private ConsumeInfo _info;
        [IgnoreMember, JsonIgnore]
        public ConsumeInfo Info
        {
            get
            {
                if (_info == null)
                {
                    switch (ItemType)
                    {
                        case ItemType.CharacterSoul:
                            StaticInfo.Instance.CharacterSoulInfo.TryGet(Id, out var characterSoulInfo);
                            _info = characterSoulInfo;
                            break;
                        case ItemType.Material:
                            StaticInfo.Instance.MaterialInfo.TryGet(Id, out var materialInfo);
                            _info = materialInfo;
                            break;
                        case ItemType.RewardBox:
                            StaticInfo.Instance.BoxInfo.TryGet(Id, out var rewardBoxInfo);
                            _info = rewardBoxInfo;
                            break;
                        case ItemType.MoneyBox:
                            StaticInfo.Instance.MoneyBoxInfo.TryGet(Id, out var moneyBoxInfo);
                            _info = moneyBoxInfo;
                            break;
                        case ItemType.EventCoin:
                            StaticInfo.Instance.EventCoinInfo.TryGet(Id, out var eventCoinInfo);
                            _info = eventCoinInfo;
                            break;
                        case ItemType.Ticket:
                            StaticInfo.Instance.TicketInfo.TryGet(Id, out var ticketInfo);
                            _info = ticketInfo;
                            break;
                    }
                }
                return _info;
            }
        }

        public Consume(int id, int count = 1)
        {
            Id = id;
            Count = count;
        }

        [JsonIgnore]
        [IgnoreMember]
        public ItemType ItemType => ItemTypeHelper.GetItemType(Id);
    }
}
