using Common.Utility;
using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace Dash.Model.Rdb
{
    public partial class Money
    {
        [IgnoreMember, JsonIgnore]
        public int Value => Free + Paid;

        [IgnoreMember, JsonIgnore]
        public MoneyType MoneyType
        {
            get
            {
                if (EnumInfo<MoneyType>.TryParse(Type, out var moneyType) == false)
                    return MoneyType.Undefined;
                return moneyType;
            }
        }

        public Money(ulong oidAccount, MoneyType moneyType, int free = 0, int paid = 0)
        {
            OidAccount = oidAccount;
            Type = moneyType.ToString();
            Free = free;
            Paid = paid;
        }

        public virtual Money Add(int delta, bool paid = false)
        {
            if (paid == true)
                Paid += delta;
            else
                Free += delta;

            return this;
        }

        public virtual bool Use(int delta)
        {
            if (Value < delta)
                return false;

            if (Free < delta)
            {
                delta -= Free;
                Free = 0;
            }
            else
            {
                Free -= delta;
                delta = 0;
            }
            Paid -= delta;

            return true;
        }

        public bool Use(int delta, out int deltaFree, out int deltaPaid)
        {
            deltaFree = 0;
            deltaPaid = 0;
            if (Value < delta)
                return false;

            if (Free < delta)
            {
                deltaFree = Free;
                Free = 0;
            }
            else
            {
                deltaFree = delta;
                Free -= deltaFree;
            }
            deltaPaid = delta - deltaFree;
            Paid -= deltaPaid;

            return true;
        }
    }

    [NonDaoModel]
    public class Jewel : Money
    {
        public Jewel(Money other) : base(other) { }
        public Jewel(ulong oidAccount) : base(oidAccount, MoneyType.Jewel) { }
    }
    [NonDaoModel]
    public class Gold : Money
    {
        public Gold(Money other) : base(other) { }
        public Gold(ulong oidAccount) : base(oidAccount, MoneyType.Gold) { }
    }

    [NonDaoModel]
    public class Stamina : Money
    {
        public Stamina(Money other) : base(other) { }
        public Stamina(ulong oidAccount) : base(oidAccount, MoneyType.Stamina) { }
    }

    public static class MoneyHelper
    {
        public static Money Get(this List<Money> monies, MoneyType moneyType, ulong oidAccount = 0)
        {
            var index = monies.FindIndex(e => e.Type == moneyType.ToString());
            if (index >= 0)
                return monies[index];
            else
                return new Money(oidAccount, moneyType, 0);
        }
        public static Money Get(this Dictionary<MoneyType, Money> monies, MoneyType moneyType, ulong oidAccount = 0)
        {
            monies.TryGetValue(moneyType, out var money);
            if (money != null)
                return money;
            else
                return new Money(oidAccount, moneyType, 0);
        }

        public static (int min, float rate) CalcExchangeRate(MoneyType srcType, MoneyType tarType)
        {
            var serviceLogicInfo = StaticInfo.StaticInfo.Instance.ServiceLogicInfo.Get();

            if (srcType == MoneyType.Jewel && tarType == MoneyType.Gold)
                return (1, serviceLogicInfo.Money.GoldByJewel);
            else
                return (0, 0f);
        }

        public static int GetGoldForCharacterExp(int exp)
        {
            var serviceLogicInfo = StaticInfo.StaticInfo.Instance.ServiceLogicInfo.Get();
            return (int)(exp * serviceLogicInfo.Character.ExpGoldRate);
        }

        public static int GetGoldForWeaponExp(int exp)
        {
            var serviceLogicInfo = StaticInfo.StaticInfo.Instance.ServiceLogicInfo.Get();
            return (int)(exp * serviceLogicInfo.Equipment.WeaponExpGoldRate);
        }
        public static int GetGoldForArmorExp(int exp)
        {
            var serviceLogicInfo = StaticInfo.StaticInfo.Instance.ServiceLogicInfo.Get();
            return (int)(exp * serviceLogicInfo.Equipment.ArmorExpGoldRate);
        }
    }
}
