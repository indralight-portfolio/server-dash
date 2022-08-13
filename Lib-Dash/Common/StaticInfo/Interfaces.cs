using System;
using System.Collections.ObjectModel;

namespace Common.StaticInfo
{
    public interface IVerifiable
    {
        bool Verify(out string log);
    }

    public interface IPostProcessable
    {
        void PostProcess();
    }

    public interface IHasComment
    {
        string GetComment();
    }

    public interface IHasName
    {
        Locale.Locale GetName();
    }
    public interface IHasNameDesc : IHasName
    {
        Locale.Locale GetDesc();
    }
    public interface IHasDesc
    {
        Locale.Locale GetDesc();
    }

    public interface IHasPeriod
    {
        public void PeriodInit();
        public void PeriodReset();
        public void PeriodOverride(DateTime start, DateTime end);
        public bool InPeriod(DateTime dateTime);
    }

    public interface IFormatConvertible
    {
        void PrepareConvertFormat();
    }

    public interface IHasTag : IPostProcessable
    {
        ReadOnlyCollection<int> GetTags();
    }
}