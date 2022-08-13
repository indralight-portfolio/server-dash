using Dash.StaticData.Collection;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

#nullable disable

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public partial class Collection
    {
        private CollectionInfo _info;
        [IgnoreMember, JsonIgnore]
        public CollectionInfo Info
        {
            get
            {
                if (_info == null)
                {
                    StaticInfo.Instance.CollectionInfo.TryGet(Id, out _info);
                }
                return _info;
            }
        }

        [IgnoreMember, JsonIgnore]
        public int MinimumRank
        {
            get
            {
                const int notAppliedRank = -1;
                if (Info == null)
                {
                    return notAppliedRank;
                }

                var list = new List<int>();
                if (Param1 != null)
                    list.Add((int)Param1);
                if (Param2 != null)
                    list.Add((int)Param2);
                if (Param3 != null)
                    list.Add((int)Param3);
                if (Param4 != null)
                    list.Add((int)Param4);
                if (Param5 != null)
                    list.Add((int)Param5);

                if (list.Count == 0)
                    return notAppliedRank;

                return list.Min();
            }
        }

        public bool TryUpdate(int targetId, int rank)
        {
            int index = Info.TargetIds.FindIndex(e => e == targetId);

            if (index < 0)
            {
                return false;
            }

            if (index == 0 && Param1 < rank)
            {
                Param1 = (sbyte)rank;
                return true;
            }
            else if (index == 1 && Param2 < rank)
            {
                Param2 = (sbyte)rank;
                return true;
            }
            else if (index == 2 && Param3 < rank)
            {
                Param3 = (sbyte)rank;
                return true;
            }
            else if (index == 3 && Param4 < rank)
            {
                Param4 = (sbyte)rank;
                return true;
            }
            else if (index == 4 && Param5 < rank)
            {
                Param5 = (sbyte)rank;
                return true;
            }
            return false;
        }


    }
}
