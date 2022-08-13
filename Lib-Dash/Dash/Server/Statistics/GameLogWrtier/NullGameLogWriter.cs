#if Common_Server
using System.Collections.Generic;

namespace Dash.Server.Statistics.GameLogWriter
{
    public class NullGameLogWriter : IGameLogWriter
    {
        public void Write(GameLogContext log)
        {
        }

        public void Write(List<GameLogContext> logs)
        {
        }
    }
}
#endif