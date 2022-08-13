using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.StaticInfo;
using Common.Utility;
using Dash;
using Dash.Server.Dao.Cache;
using Dash.Model.Rdb;
using Dash.Types;
using Dash.Protocol;
using Dash.Server.Dao.Cache.Transaction;


namespace server_dash.Internal.Services
{
    public class ProgressChanges
    {
        public ProgressChanges() { }
        public ProgressChanges(Dictionary<string, ulong> progress)
        {
            _dictionary = progress;
        }
        private Dictionary<string, ulong> _dictionary = new Dictionary<string, ulong>();
        public IReadOnlyDictionary<string, ulong> Get() => _dictionary;
        public Dictionary<string, ulong> GetRaw() => _dictionary;

        public void Put(string key, ulong value)
        {
            if (_dictionary.ContainsKey(key) == true)
            {
                _dictionary[key] += value;
            }
            else
            {
                _dictionary.Add(key, value);
            }
        }
    }

    public static class ProgressService
    {
        private static object _lockObject = new object();
        private static Dictionary<string, List<KeyValuePair<string, object>>> _subKeys =
            new Dictionary<string, List<KeyValuePair<string, object>>>();

        private static IMultipleDBCache<Progress> _progressCache;

        public static void Init(DaoCache daoCache)
        {
            _progressCache = daoCache.GetMultiple<Progress>();
        }

        public static async Task<ulong> Get(ulong oidAccount, string key)
        {
            Progress progress = await GetProgress(oidAccount, key);
            return progress?.Number ?? 0;
        }

        public static async Task<Progress> GetProgress(ulong oidAccount, string key)
        {
            Progress progress = await _progressCache.Get(oidAccount, Progress.MakeSubKeysWithName(key));
            return progress;
        }

        public static async Task<bool> Increase(ulong oidAccount, string key, ulong increaseAmount = 1)
        {
            Progress progress = await GetProgress(oidAccount, key);
            if (progress != null)
            {
                Progress increased = new Progress();
                increased.OidAccount = progress.OidAccount;
                increased.Id = progress.Id;
                increased.Number = progress.Number + increaseAmount;
                return await _progressCache.Change(progress, increased);
            }
            else
            {
                Progress newProgress = new Progress();
                newProgress.OidAccount = oidAccount;
                newProgress.Id = key;
                newProgress.Number = increaseAmount;
                return await _progressCache.Set(newProgress);
            }
        }

        public static async Task<bool> Increase(ulong oidAccount,
            ProgressKey.Type key, ulong increaseAmount = 1)
        {
            return await Increase(oidAccount, key.ToString(), increaseAmount);
        }

        public static async Task<bool> SetAll(ulong oidAccount, ProgressChanges progressChanges)
        {
            return await SetAll(oidAccount, progressChanges.Get());
        }
        public static async Task<bool> SetAll(ulong oidAccount, IReadOnlyDictionary<string, ulong> progressChanges)
        {
            if (progressChanges.Count <= 0)
            {
                return true;
            }

            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach (KeyValuePair<string, ulong> changePair in progressChanges)
            {
                tasks.Add(Increase(oidAccount, changePair.Key, changePair.Value));
            }

            foreach (bool result in await Task.WhenAll(tasks))
            {
                if (result == false)
                {
                    return false;
                }
            }

            return true;
        }        
        
        public static void AccountChange(Account account, ChangedColumns changedColumns, LogAndProgress progress)
        {
            if (changedColumns == null)
            {
                return;
            }
            foreach (var change in changedColumns)
            {
                switch (change.Key)
                {
                    case AccountColumns.Gold:
                        if (change.Value - account.Gold > 0)
                        {
                            progress.AddChangeProgress(ProgressKey.GainGold.ToString(), (ulong)(change.Value - account.Gold));
                        }
                        else
                        {
                            progress.AddChangeProgress(ProgressKey.ConsumeGold.ToString(), (ulong)(account.Gold - change.Value));
                        }
                        break;
                    case AccountColumns.Jewel:
                        if (change.Value - account.Jewel > 0)
                        {
                            progress.AddChangeProgress(ProgressKey.GainJewel.ToString(), (ulong)(change.Value - account.Jewel));
                        }
                        else
                        {
                            progress.AddChangeProgress(ProgressKey.ConsumeJewel.ToString(), (ulong)(account.Jewel - change.Value));
                        }
                        break;
                    case AccountColumns.Stamina:
                        if (change.Value - account.Stamina > 0)
                        {
                            progress.AddChangeProgress(ProgressKey.GainStamina.ToString(), (ulong)(change.Value - account.Stamina));
                        }
                        else
                        {
                            progress.AddChangeProgress(ProgressKey.ConsumeStamina.ToString(), (ulong)(account.Stamina - change.Value));
                        }
                        break;
                }
            }
        }
    }
}