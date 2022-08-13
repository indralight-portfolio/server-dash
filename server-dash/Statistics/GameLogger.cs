using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dash.Model.Rdb;
using Dash.Model.Service;
using Dash.Protocol;
using Dash.Server.Statistics;
using Dash.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace server_dash.Statistics
{
    public class GameLogWriter<T> where T : IPayload
    {
        [JsonIgnore] public CommandType CommandType { get; set; }
        public ulong OidAccount;
        public DateTime Time;
        public string Command;
        [JsonProperty("Level")]
        public int AccountLevel;
        public T PayLoad;
        public virtual string ToJson()
        {
            if (PayLoad is IDeltaPayload deltaPayload)
            {
                Command = Commands.GetValueWithDelta(CommandType, deltaPayload.GetDelta());
            }
            else
            {
                Command = Commands.GetValue(CommandType);
            }
            JsonSerializerSettings settings = new JsonSerializerSettings();

            IsoDateTimeConverter dateConverter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss"
            };
            settings.Converters.Add(dateConverter);
            settings.ContractResolver = new LowercaseContractResolver();

            return JsonConvert.SerializeObject(this, settings);
        }
    }

    public static class GameLogger
    {
        private static NLog.Logger _logger = NLog.LogManager.GetLogger("GameLogger");

        public static void Write<T>(ulong oidAccount, int accountLevel, CommandType commandType, T payload, DateTime? now = null) where T : IPayload
        {
            DateTime _now = now ?? DateTime.UtcNow;
            GameLogWriter<T> logWriter = new GameLogWriter<T>()
            {
                OidAccount = oidAccount,
                Time = _now,
                CommandType = commandType,
                AccountLevel = accountLevel,
                PayLoad = payload
            };

            _logger.Info(logWriter.ToJson);
        }

        public static void OnCreate(ulong oidAccount, int accountLevel, string country)
        {
            GameLogWriter<CreatePayload> logWriter = new GameLogWriter<CreatePayload>
            {
                OidAccount = oidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.Create,
                AccountLevel = accountLevel,
                PayLoad = new CreatePayload
                {
                    Country = country,
                }
            };
            _logger.Info(logWriter.ToJson());
        }
        public static void OnLogIn(ulong oidAccount, int accountLevel, string country, DateTime created)
        {
            GameLogWriter<LogInPayload> logWriter = new GameLogWriter<LogInPayload>
            {
                OidAccount = oidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.Login,
                AccountLevel = accountLevel,
                PayLoad = new LogInPayload {
                    Country = country,
                    Created = created,
                }
            };
            _logger.Info(logWriter.ToJson());
        }

        public static void WriteLogs(List<string> logs)
        {
            for(int i = 0; i < logs.Count; ++i)
            {
                _logger.Info(logs[i]);
            }
        }
        public static void AccountChange(Account account, ChangeReason reason, ChangedColumns changedColumns, LogAndProgress logAndProgress)
        {
            if (changedColumns == null)
            {
                return;
            }

            foreach (var change in changedColumns)
            {
                switch (change.Key)
                {
                    case AccountColumns.Exp:
                        int newLevel = changedColumns.ContainsKey(AccountColumns.Level) ? changedColumns[AccountColumns.Level] : account.Level;
                        logAndProgress.AddGameLog(GameLogHelper.GetChangeExpString(account, reason, change.Value, newLevel));
                        break;
                    case AccountColumns.UsingHeroId:
                        break;
                    case AccountColumns.Gold:
                        logAndProgress.AddGameLog(GameLogHelper.GetChangeValueString(account, CommandType.Gold, reason, change.Value));
                        break;
                    case AccountColumns.Jewel:
                        logAndProgress.AddGameLog(GameLogHelper.GetChangeValueString(account, CommandType.Jewel, reason, change.Value));
                        break;
                    case AccountColumns.Stamina:
                        logAndProgress.AddGameLog(GameLogHelper.GetChangeValueString(account, CommandType.Stamina, reason, change.Value));
                        break;
                }
            }
        }
        public static void EquipmentChanges(ulong oidAccount, int accountLevel, string reason, List<Equipment> gainEquipments, List<Equipment> removeEquipments, LogAndProgress logAndProgress)
        {
            for (int i = 0; removeEquipments != null && i < removeEquipments.Count; ++i)
            {
                EquipmentChange(oidAccount, accountLevel, reason, removeEquipments[i], false, logAndProgress);
            }
            for (int i = 0; gainEquipments != null && i < gainEquipments.Count; ++i)
            {
                EquipmentChange(oidAccount, accountLevel, reason, gainEquipments[i], true, logAndProgress);
            }
        }
        public static void EquipmentChange(ulong oidAccount, int accountLevel, string reason, Equipment equipment, bool isGain, LogAndProgress logAndProgress)
        {
            GameLogWriter<EquipmentItemGainPayload> writer = new GameLogWriter<EquipmentItemGainPayload>
            {
                OidAccount = oidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.EquipmentItem,
                AccountLevel = accountLevel,
                PayLoad = new EquipmentItemGainPayload
                {
                    Reason = reason.ToString(),
                    Serial = equipment.Serial,
                    ItemId = equipment.Id,
                    Level = equipment.Level,
                    ItemGrade = Common.Utility.EnumInfo<ItemGrade>.ConvertUshort(equipment.Grade).ToString(),
                },
            };
            logAndProgress.AddGameLog(writer.ToJson());
        }
        public static void ConsumeChanges(ulong oidAccount, int accountLevel, string reason, IReadOnlyList<Consume> originConsumes, List<Consume> changedConsumes, LogAndProgress logAndProgress)
        {
            for (int i = 0; i < changedConsumes.Count; ++i)
            {
                var findConsume = originConsumes.FirstOrDefault(e => e.Id == changedConsumes[i].Id);
                ConsumeChange(oidAccount, accountLevel, reason, findConsume, changedConsumes[i], logAndProgress);
            }
        }
        public static void ConsumeChange(ulong oidAccount, int accountLevel, string reason, Consume originConsume, Consume changedConsume, LogAndProgress logAndProgress)
        {
            int delta = changedConsume.Count - originConsume?.Count ?? 0;
            GameLogWriter<ChangeConsumePayload> writer = new GameLogWriter<ChangeConsumePayload>
            {
                OidAccount = oidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.ConsumeItem,
                AccountLevel = accountLevel,
                PayLoad = new ChangeConsumePayload
                {
                    ItemId = changedConsume.Id,
                    Reason = reason.ToString(),
                    DeltaCount = delta,
                    OldCount = originConsume?.Count ?? 0,
                    NewCount = changedConsume.Count,
                },
            };
            logAndProgress.AddGameLog(writer.ToJson());
        }
        public static void HeroChanges(ulong oidAccount, int accountLevel, string reason, IReadOnlyList<Hero> originHeroes, List<Hero> changedHeroes, LogAndProgress logAndProgress)
        {
            for (int i = 0; i < changedHeroes.Count; ++i)
            {
                var findHero = originHeroes.FirstOrDefault(e => e.Id == changedHeroes[i].Id);
                HeroChange(oidAccount, accountLevel, reason, findHero, changedHeroes[i], logAndProgress);
            }
        }
        public static void HeroChange(ulong oidAccount, int accountLevel, string reason, Hero origin, Hero changed, LogAndProgress logAndProgress)
        {
            int delta = changed.SoulCount - origin?.SoulCount ?? 0;
            GameLogWriter<ChangeConsumePayload> writer = new GameLogWriter<ChangeConsumePayload>
            {
                OidAccount = oidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.Hero,
                AccountLevel = accountLevel,
                PayLoad = new ChangeConsumePayload
                {
                    Reason = reason.ToString(),
                    ItemId = changed.Id,
                    DeltaCount = delta,
                    OldCount = origin?.SoulCount ?? 0,
                    NewCount = changed.SoulCount,
                },
            };
            logAndProgress.AddGameLog(writer.ToJson());
        }
        public static void BoxKeyChanges(ulong oidAccount, int accountLevel, string reason, IReadOnlyList<BoxKey> origin, List<BoxKey> changed, LogAndProgress logAndProgress)
        {
            for (int i = 0; i < changed.Count; ++i)
            {
                var findConsume = origin.FirstOrDefault(e => e.BoxId == changed[i].BoxId);
                BoxKeyChange(oidAccount, accountLevel, reason, findConsume, changed[i], logAndProgress);
            }
        }
        public static void BoxKeyChange(ulong oidAccount, int accountLevel, string reason, BoxKey origin, BoxKey changed, LogAndProgress logAndProgress)
        {
            int delta = changed.Count - origin?.Count ?? 0;
            GameLogWriter<ChangeBoxKeyPayload> writer = new GameLogWriter<ChangeBoxKeyPayload>
            {
                OidAccount = oidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.ConsumeItem,
                AccountLevel = accountLevel,
                PayLoad = new ChangeBoxKeyPayload
                {
                    BoxId = changed.BoxId,
                    Reason = reason.ToString(),
                    DeltaCount = delta,
                    OldCount = origin?.Count ?? 0,
                    NewCount = changed.Count,
                },
            };
            logAndProgress.AddGameLog(writer.ToJson());
        }

        public static void OnShop_Buy(ulong oidAccount, int accountLevel, int productId, PaymentType paymentType, int price, LogAndProgress logAndProgress)
        {
            GameLogWriter<ShopBuyPayload> writer = new GameLogWriter<ShopBuyPayload>
            {
                OidAccount = oidAccount,
                Time = DateTime.UtcNow,
                CommandType = CommandType.Shop_Buy,
                AccountLevel = accountLevel,
                PayLoad = new ShopBuyPayload
                {
                    ProductId = productId,
                    PaymentType = paymentType.ToString(),
                    Price = price,
                },
            };
            logAndProgress.AddGameLog(writer.ToJson());
        }
    }    
}
namespace Newtonsoft.Json.Serialization
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

}
