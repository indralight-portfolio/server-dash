using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dash.Model.Rdb;

namespace server_dash.Context
{
    //흠.. 이것들은 서버에서만 쓰는데
    public class RewardHeroContext
    {
        public List<Hero> Heroes;
        public Consume CommonSoul;
        public Dictionary<int/*heroId*/, int/*commonSoulCount*/> ChangedCommonSouls = new Dictionary<int, int>();
        public int ExceessCommonSoulCount;
    }
    public class RewardConsumeContext
    {
        public List<Consume> ChangedConsumes = new List<Consume>();
    }
    public class RewardEquipmentContext
    {
        public List<Equipment> ChangedEquipments = new List<Equipment>();
    }
}
