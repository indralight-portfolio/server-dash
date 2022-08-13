using Common.Utility;
using Dash.Model.Service;
using Dash.StaticData.Event;
using MessagePack;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public partial class GameEvent
    {
        public GameEvent(ulong oidAccount, int eventId)
        {
            OidAccount = oidAccount;
            EventId = eventId;
        }

        private EventInfo _info;
        [IgnoreMember, JsonIgnore]
        public EventInfo Info
        {
            get
            {
                if (_info == null)
                {
                    StaticInfo.Instance.EventInfo.TryGet(EventId, out _info);
                }
                return _info;
            }
        }

        private EventDataModel eventData;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public EventDataModel EventData
        {
            get
            {
                if (eventData == null && Data?.Length > 0)
                {
                    switch (Info.Type)
                    {
                        case EventType.LotteryBoard:
                            {
                                eventData = JsonGzipSerializer.Deserialize<LotteryEventDataModel>(Data);
                                break;
                            }

                    }
                }
                return eventData;
            }
            set
            {
                eventData = value;
                if (eventData != null)
                    Data = JsonGzipSerializer.Serialize(eventData);
                else
                    Data = null;
            }
        }
    }
}
