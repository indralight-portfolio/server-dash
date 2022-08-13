using Dash;
using Dash.Model.Rdb;
using Dash.Model.Service;
using Dash.Server.Dao.Cache;
using Dash.Server.Dao.Cache.Transaction;
using Dash.StaticData;
using Dash.Types;
using System;
using System.Threading.Tasks;
using Account = Dash.Model.Rdb.Account;

namespace server_dash.Internal.Services
{
    public class MailService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        protected DaoCache _daoCache;
        protected IMemCache _memCache;

        protected IMultipleDBCache<Mail> _mailCache;

        public MailService(DaoCache daoCache)
        {
            _daoCache = daoCache;
            _memCache = daoCache.GetMemCache();
            _mailCache = daoCache.GetMultiple<Mail>();
        }

        public async Task SendMailExcessReward(ulong oidAccount, RewardInfo excessReward)
        {
            Mail mail = new Mail
            {
                MailType = MailType.OverLimit,
                OidAccount = oidAccount,
                Created = DateTime.UtcNow,
                Expire = DateTime.MaxValue,
                DataObject = new RewardMailDataModel
                {
                    Body = new MailBodyModel
                    {
                        Title = CodeLocale.MailService_Mail_ExcessItem_Title.GetKey(),
                        Message = CodeLocale.MailService_Mail_ExcessItem_Message.GetKey(),
                    },
                    Reward = excessReward,
                },
            };

            _mailCache.Set(mail);
            // Mail 은  IsAutoIncrementMainKey = true 이므로 insert 시 Cache 를 삭제해줘야 한다.
            _memCache.Del($"{nameof(Mail)}:{oidAccount}");
        }
    }
}