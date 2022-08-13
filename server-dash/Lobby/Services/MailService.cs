using Common.Utility;
using Dash.Model.Rdb;
using Dash.Model.Service;
using Dash.Protocol;
using Dash.Server.Dao.Cache;
using Dash.StaticData;
using Dash.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server_dash.Lobby.Services
{
    public class MailService : BaseService
    {
        private static NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private DaoCache _daoCache;
        private IMemCache _memCache;
        private ISingleDBCache<Account> _accountCache;
        private ISingleDBCache<Footprint> _footprintCache;
        private ISingleDBCache<MailTemplate> _mailTemplateCache;
        private IMultipleDBCache<MailTarget> _mailTargetCache;
        private IMultipleDBCache<Mail> _mailCache;

        private RewardService _rewardService;

        public MailService(DaoCache daoCache, RewardService rewardService)
        {
            _daoCache = daoCache;
            _memCache = daoCache.GetMemCache();
            _accountCache = daoCache.GetSingle<Account>();
            _footprintCache = daoCache.GetSingle<Footprint>();
            _mailTemplateCache = daoCache.GetSingle<MailTemplate>();
            _mailTargetCache = daoCache.GetMultiple<MailTarget>();
            _mailCache = daoCache.GetMultiple<Mail>();

            _rewardService = rewardService;
        }

        public async Task<ListMailResponse> List(ulong oidAccount)
        {
            ListMailResponse response = new ListMailResponse();

            string query = $"SELECT * FROM {nameof(Mail)} WHERE {nameof(Mail.OidAccount)} = @{nameof(Mail.OidAccount)}" +
                        $" AND `{nameof(Mail.Expire)}` > @{nameof(Mail.Expire)}" +
                        $" AND `{nameof(Mail.Created)}` < @{nameof(Mail.Created)}" +
                        $" AND `{nameof(Mail.ReadTime)}` IS NULL" +
                        $" ORDER BY `{nameof(Mail.Created)}` DESC";
            var param = new List<KeyValuePair<string, object>>{
                new KeyValuePair<string, object>($"@{nameof(Mail.OidAccount)}", oidAccount),
                new KeyValuePair<string, object>($"@{nameof(Mail.Created)}", DateTime.UtcNow),
                new KeyValuePair<string, object>($"@{nameof(Mail.Expire)}", DateTime.UtcNow),
            };

            var list = await _mailCache.GetListQuery(oidAccount, query, param) ?? new List<Mail>();
            response.MailList = list.OrderByDescending(x => x.Created).ToList();

            return response;
        }

        public async Task SendTemplateMail(ulong oidAccount)
        {
            List<MailTemplate> mailTemplates;
            {
                string query = $"SELECT * FROM {nameof(MailTemplate)} WHERE {nameof(MailTemplate.Start)} < @{nameof(MailTemplate.Start)} AND {nameof(MailTemplate.End)} > @{nameof(MailTemplate.End)}";
                var param = new List<KeyValuePair<string, object>>{
                    new KeyValuePair<string, object>($"@{nameof(MailTemplate.Start)}", DateTime.UtcNow),
                    new KeyValuePair<string, object>($"@{nameof(MailTemplate.End)}", DateTime.UtcNow),
                };
                mailTemplates = await _mailTemplateCache.GetListQuery(query, param) ?? new List<MailTemplate>();
            }

            if (mailTemplates.Count == 0) return;

            Dictionary<uint, MailTarget> mailTargets;
            {
                string query = "" +
                    $"  SELECT * FROM {nameof(MailTarget)}" +
                    $"  WHERE {nameof(MailTarget.OidAccount)} = @{nameof(MailTarget.OidAccount)}" +
                    $"      AND TemplateId IN ({string.Join(",", mailTemplates.Select(x => x.Id).ToArray())})";
                var param = new List<KeyValuePair<string, object>>{
                    new KeyValuePair<string, object>($"@{nameof(MailTarget.OidAccount)}", oidAccount),
                };

                var list = await _mailTargetCache.GetListQuery(oidAccount, query, param) ?? new List<MailTarget>();
                mailTargets = list.ToDictionary(x => x.TemplateId, x => x);
            }

            {
                var query = from a in mailTemplates
                            join b_ in mailTargets.Values on a.Id equals b_.TemplateId into b__
                            from b in b__.DefaultIfEmpty()
                            where a.Target == MailTargetType.All.ToString() && (b == null || b.Status == (sbyte)MailTargetStatus.Ready)
                                || a.Target == MailTargetType.Group.ToString() && b != null && b.Status == (sbyte)MailTargetStatus.Ready
                            select a;

                mailTemplates = query.ToList();
            }

            if (mailTemplates.Count == 0) return;

            (Account account,Footprint footprint) = await TaskUtility.WaitAll2(
                    _accountCache.Get(oidAccount),
                    _footprintCache.Get(oidAccount));

            var trans = DaoCache.Instance.Transaction();
            List<Mail> mails = new List<Mail>();
            foreach (var mailTemplate in mailTemplates)
            {
                // Check Condition
                if (mailTemplate.ConditionObject?.Check(footprint) == false)
                {
                    continue;
                }
                mails.Add(new Mail(oidAccount, mailTemplate));

                var newMailTarget = new MailTarget(oidAccount, mailTemplate.Id, MailTargetStatus.Sent);
                if (mailTargets == null || mailTargets.TryGetValue(mailTemplate.Id, out MailTarget mailTarget) == false)
                {
                    await _mailTargetCache.Set(newMailTarget, trans);
                }
                else
                {
                    await _mailTargetCache.Change(mailTarget, newMailTarget, trans);
                }
            }
            if (mails.Count > 0)
            {
                await _mailCache.SetAll(mails, trans);
                await trans.Execute();
                _memCache.Del($"{nameof(Mail)}:{oidAccount}");
            }
        }

        public async Task<ReceiveMailRewardResponse> ReceiveReward(ulong oidAccount, ulong Id)
        {
            ReceiveMailRewardResponse response = new ReceiveMailRewardResponse();
            response.Id = Id;

            var account = await _accountCache.Get(oidAccount);
            if (account == null)
            {
                response.SetResult(ErrorCode.NoAccount);
                return response;
            }
            Mail mail = await _mailCache.Get(oidAccount, Mail.MakeSubKeysWithName(Id));
            if (mail == null)
            {
                response.SetResult(ErrorCode.NoData);
                return response;
            }
            if (mail.DataObject.TryGetReward(out RewardInfo rewardInfo) == false || rewardInfo == null)
            {
                response.MailList = (await Read(oidAccount, Id)).MailList;
                return response;
            }
            if (mail.DataObject.Body.IsReceived == true)
            {
                response.SetResult(ErrorCode.AlreadyRewardReceived);
                return response;
            }

            response.RewardInfo = rewardInfo;
            LogAndProgress logAndProgress = new LogAndProgress(account);
            var trans = DaoCache.Instance.Transaction();
            RewardInfo excessReward;
            (response.RewardModel, excessReward) = await _rewardService.GiveReward(account, rewardInfo, ChangeReason.RewardMail, logAndProgress, trans);
            if(excessReward != null && excessReward.IsEmpty() == false)
            {
                response.SetResult(ErrorCode.OverLimit);
                return response;
            }

            Mail newMail = new Mail(mail);
            newMail.DataObject.Body.IsReceived = true;
            newMail.SetData();
            await _mailCache.Change(mail, newMail, trans);

            if (await trans.Execute())
            {
                await logAndProgress.Execute();
                response.MailList = (await Read(oidAccount, Id)).MailList;
            }
            else
            {
                response.RewardInfo = null;
                response.RewardModel = null;
                response.ErrorCode = ErrorCode.GiveRewardFailed;
            }
            return response;
        }

        public async Task<ReadMailResponse> Read(ulong oidAccount, ulong Id)
        {
            ReadMailResponse response = new ReadMailResponse();
            response.Id = Id;

            Mail mail = await _mailCache.Get(oidAccount, Mail.MakeSubKeysWithName(Id));
            if (mail == null)
            {
                response.SetResult(ErrorCode.NoData);
                return response;
            }
            Mail newMail = new Mail(mail);
            newMail.ReadTime = DateTime.UtcNow;

            await _mailCache.Change(mail, newMail);
            await _mailCache.DelCache(oidAccount, Id.ToString());

            response.MailList = (await List(oidAccount)).MailList;

            return response;
        }
    }
}