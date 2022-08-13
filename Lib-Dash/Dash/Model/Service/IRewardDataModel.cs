using Dash.StaticData;

namespace Dash.Model.Service
{
    public interface IRewardDataModel<T>
    {
        bool HasReward();
        T GetReward();
        bool TryGetReward(out T reward);
        void SetReward(T reward);
    }
}