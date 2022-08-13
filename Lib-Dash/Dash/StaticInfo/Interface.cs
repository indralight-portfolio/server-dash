using Dash.Types;

namespace Dash.StaticInfo
{
    public interface IRarity
    {
        Rarity Rarity { get; set; }
    }
    public interface IPublicable
    {
        bool IsPublic { get; set; }
    }
}
