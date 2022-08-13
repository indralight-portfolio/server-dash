using MessagePack.Formatters;

namespace Dash
{
    public static class MessagePackDefinition
    {
        public static IMessagePackFormatter[] GetCustomFormatters()
        {
            return new IMessagePackFormatter[]
            {
                new StaticData.Entity.EventInfoCustomFormatter(),
                new StaticData.Entity.BehaviorTreeNodeInfoCustomFormatter(),
                new MessageStreamCustomFormatter(),
                new Core.CommandKeyCustomFormatter()
            };
        }
    }
}