using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dash;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Buffers;
using MessagePack;
using NLog;
using server_dash.Net.Handlers;
using server_dash.Battle.Services.Entities;
using server_dash.Net.Sessions;

namespace server_dash.Battle.Services.GamePlay.Recorder
{
    public class LogArenaRecorder : ArenaRecorder
    {
        private new static Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public LogArenaRecorder(ArenaContext arenaContext, bool recordBytes) : base(_logger, arenaContext, true, recordBytes)
        {
        }

        public override async Task Save(RecordMetadata metadata)
        {
            await base.Save(metadata);
        }
    }
}