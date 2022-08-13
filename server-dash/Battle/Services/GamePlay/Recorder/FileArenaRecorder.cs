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
    public class FileArenaRecorder : ArenaRecorder
    {
        private new static Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        public FileArenaRecorder(ArenaContext arenaContext, bool loggingStatistics, bool recordBytes) : base(_logger, arenaContext, loggingStatistics, recordBytes)
        {
        }

        public override async Task Save(RecordMetadata metadata)
        {
            await base.Save(metadata);
            try
            {
                byte[] serialized = null;
                int inboundCount = 0;
                int outboundCount = 0;
                lock (messageRecords.ServerInbound)
                {
                    lock (messageRecords.ServerOutbound)
                    {
                        inboundCount = messageRecords.ServerInbound.Sum(p => p.Value.Count);
                        outboundCount = messageRecords.ServerOutbound.Sum(p => p.Value.Count);
                        serialized = MessagePackSerializer.Serialize(messageRecords);
                    }
                }

                string directoryPath = $"{Directory.GetCurrentDirectory()}/Records/";
                string filePath = $"{directoryPath}{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}_{arenaContext.ToString().Replace(":","-")}.bin";
                if (Directory.Exists(directoryPath) == false)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (FileStream fileStream = File.Create(filePath))
                {
                    _logger.Info($"[FileArenaRecorder] Try to save [{arenaContext}] record, " +
                                 $"Inbound count : {inboundCount} Outbound count : {outboundCount} Total size : {serialized.Length}");
                    await fileStream.WriteAsync(serialized);
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }
        }
    }
}