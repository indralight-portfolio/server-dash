using Dash.Model.Service;
using MessagePack;
using System;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    public partial class Account
    {
        [IgnoreMember]
#if Common_Server
        [NotMapped]
#endif
        public bool IsNew => Created > DateTime.UtcNow.Date;
        [IgnoreMember]
#if Common_Server
        [NotMapped]
#endif
        public bool IsReturn => ReturnTime > DateTime.UtcNow.Date;

        [MessagePack.Key(11)]
#if Common_Server
        [NotMapped]
#endif
        public DateTime PrevLatestLogon { get; set; }

        private static int DaysForReturn = 21;
        private static int BenefitPeriod = 7;

        public bool IsReturnee(ServerTime serverTime)
        {
            if (ReturnTime != null)
            {
                var returnTime = ((DateTime)ReturnTime);
                return returnTime.AddDays(BenefitPeriod) > serverTime.Utc;
            }
            return false;
        }

        public bool IsNewbie(ServerTime serverTime)
        {
            return Created.AddDays(BenefitPeriod) > serverTime.Utc;
        }

        public DateTime NewbieEndData()
        {
            return Created.AddDays(BenefitPeriod);
        } 

        public DateTime GetExpireDateNewbie()
        {
            return Created.AddDays(BenefitPeriod);
        }

        public void Update(AuthReq authReq)
        {
            Country = authReq.Country;
            TimeOffset = authReq.TimeOffset;
        }

#if Common_Server
        public void Touch()
        {
            if ((DateTime.UtcNow.Date - LatestLogon.Date).Days >= DaysForReturn)
            {
                ReturnTime = DateTime.UtcNow;
            }
            PrevLatestLogon = LatestLogon;
            LatestLogon = DateTime.UtcNow;
        }
#endif
    }
}