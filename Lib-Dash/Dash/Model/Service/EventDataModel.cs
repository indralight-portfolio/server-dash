using MessagePack;
using System.Collections.Generic;

namespace Dash.Model.Service
{
    public abstract class EventDataModel
    {
    }
    [MessagePackObject]
    public class LotteryEventDataModel : EventDataModel
    {
        [Key(0)]
        public List<int> RewardedIds { get; set; } = new List<int>();
    }

}
