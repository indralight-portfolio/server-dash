using Common.Utility;
using Dash;
using Dash.Net;
using Dash.Protocol;
using DotNetty.Buffers;
using NLog;
using server_dash.Battle.Services.Entities;
using server_dash.Net.Sessions;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server_dash.Battle.Services.GamePlay.Recorder
{
    public interface IArenaRecorder
    {
        void RecordInbound(ISession session, IProtocol protocolMessage);
        void RecordOutbound(ISession session, IProtocol protocolMessage);
        void RecordBroadcast(IProtocol protocolMessage, bool isServerBeginMessage);
        Task Save(RecordMetadata metadata);
        void Release();
    }

    public static class ArenaRecorderFactory
    {
        private static BattleServerConfig _config;

        static ArenaRecorderFactory()
        {
            _config = ConfigManager.Get<BattleServerConfig>(Config.BattleServer);
        }

        public static IArenaRecorder Create(ArenaContext arenaContext)
        {
            bool loggingStatistics = _config.ArenaRecord.LoggingStatistics;
            bool recordBytes = _config.ArenaRecord.RecordBytes;
            if (_config.ArenaRecord.Recorder == BattleServerConfig.ArenaRecordConfig.RecorderType.Log)
            {
                return new LogArenaRecorder(arenaContext, recordBytes);
            }
            else if(_config.ArenaRecord.Recorder == BattleServerConfig.ArenaRecordConfig.RecorderType.File)
            {
                return new FileArenaRecorder(arenaContext, loggingStatistics, recordBytes);
            }
            else if (_config.ArenaRecord.Recorder == BattleServerConfig.ArenaRecordConfig.RecorderType.S3)
            {
                return new S3ArenaRecorder(arenaContext, loggingStatistics, recordBytes);
            }
            return new NullArenaRecorder();
        }
    }

    public abstract class ArenaRecorder : IArenaRecorder
    {
        private readonly bool _loggingStatistics;
        private readonly bool _recordBytes;

        private bool _stopRecording = false;
        protected ILogger logger;

        // dotnetty buffer pooling에 문제 있는 것 같아서;; 따로 구현
        private static Queue<IByteBuffer> _byteBufferPool = new Queue<IByteBuffer>();

        public virtual Task Save(RecordMetadata metadata)
        {
            _stopRecording = true;
            messageRecords.Metadata = metadata;
            SaveStatistics(metadata);
            return Task.CompletedTask;
        }

        public void Release()
        {
            lock (messageRecords.ServerInbound)
            {
                messageRecords.ServerInbound.Values.ForEach(v =>
                {
                    ReleaseBuffer(v);
                });
            }

            lock (messageRecords.ServerOutbound)
            {
                messageRecords.ServerOutbound.Values.ForEach(v =>
                {
                    ReleaseBuffer(v);
                });
            }
            lock (messageRecords.ServerBroadcast)
            {
                ReleaseBuffer(messageRecords.ServerBroadcast);
            }
        }

        protected readonly MessageRecords messageRecords = new MessageRecords();
        protected readonly ArenaContext arenaContext;

        protected ArenaRecorder(ILogger logger, ArenaContext arenaContext, bool loggingStatistics, bool recordBytes)
        {
            this.logger = logger;

            this.arenaContext = arenaContext;
            _loggingStatistics = loggingStatistics;
            _recordBytes = recordBytes;

            messageRecords.ServerInbound = new Dictionary<int, MessageStream>();
            messageRecords.ServerOutbound = new Dictionary<int, MessageStream>();
            messageRecords.ServerBroadcast = new MessageStream();
            messageRecords.ServerBroadcast.SetBuffer(MakeBuffer());
            foreach (PlayerEntity player in arenaContext.Players)
            {
                if (player.OidAccount == 0)
                {
                    continue;
                }

                var messageStream = new MessageStream();
                messageStream.SetBuffer(MakeBuffer());
                messageRecords.ServerInbound.Add(player.Id, messageStream);
                messageStream = new MessageStream();
                messageStream.SetBuffer(MakeBuffer());
                messageRecords.ServerOutbound.Add(player.Id, messageStream);
            }
            messageRecords.ServerBeginMessages = new Dictionary<int, (int, int)>();
        }

        private static IByteBuffer MakeBuffer()
        {
            lock (_byteBufferPool)
            {
                if (_byteBufferPool.Count > 0)
                {
                    return _byteBufferPool.Dequeue();
                }
            }

            return UnpooledByteBufferAllocator.Default.HeapBuffer(1024 * 512); // 0.5MiB
        }

        private static void ReleaseBuffer(MessageStream messageStream)
        {
            IByteBuffer buffer = messageStream.GetBuffer();
            if (buffer == null)
            {
                return;
            }

            messageStream.SetBuffer(null);
            buffer.ResetReaderIndex();
            buffer.ResetWriterIndex();
            lock (_byteBufferPool)
            {
                _byteBufferPool.Enqueue(buffer);
            }
        }

        private bool NeedRecord(int typeCode)
        {
            if (_stopRecording == true)
            {
                return false;
            }

            if (typeCode == ArenaReady.TypeCode)
            {
                return false;
            }

            return true;
        }

        // lock 내부에서만 호출
        private MessageStream GetPlayerInbound(int playerId)
        {
            if (messageRecords.ServerInbound.TryGetValue(playerId, out MessageStream stream) == true)
            {
                return stream;
            }

            var beAdded = new MessageStream();
            beAdded.SetBuffer(MakeBuffer());
            messageRecords.ServerInbound.Add(playerId, beAdded);
            return beAdded;
        }

        // lock 내부에서만 호출
        private MessageStream GetPlayerOutbound(int playerId)
        {
            if (messageRecords.ServerOutbound.TryGetValue(playerId, out MessageStream stream) == true)
            {
                return stream;
            }

            var beAdded = new MessageStream();
            beAdded.SetBuffer(MakeBuffer());
            messageRecords.ServerOutbound.Add(playerId, beAdded);
            return beAdded;
        }

        // call on IO Thread
        public void RecordInbound(ISession session, IProtocol protocolMessage)
        {
            PlayerSession playerSession = session as PlayerSession;
            int typeCode = protocolMessage.GetTypeCode();
            if (playerSession?.Player == null || NeedRecord(typeCode) == false)
            {
                return;
            }

            if (_recordBytes == true)
            {
                long timestamp = arenaContext?.ElapsedMillisecondsSafe ?? 0;
                ReadOnlySequence<byte> bytes = MessageSerializer.SerializeUnsafe(protocolMessage);
                lock (messageRecords.ServerInbound)
                {
                    GetPlayerInbound(playerSession.Player.Id).Write(
                        timestamp,
                        typeCode,
                        bytes,
                        _loggingStatistics
                        );
                }
            }
            else
            {
                lock (messageRecords.ServerInbound)
                {
                    GetPlayerInbound(playerSession.Player.Id).Write(
                        0,
                        typeCode, default, _loggingStatistics);
                }
            }
        }

        // call on Arena Thread
        public void RecordOutbound(ISession session, IProtocol protocolMessage)
        {
            PlayerSession playerSession = session as PlayerSession;
            if (playerSession?.Player == null || NeedRecord(protocolMessage.GetTypeCode()) == false)
            {
                return;
            }
            if (_recordBytes == true)
            {
                ReadOnlySequence<byte> bytes = MessageSerializer.SerializeUnsafe(protocolMessage);
                lock (messageRecords.ServerOutbound)
                {
                    GetPlayerOutbound(playerSession.Player.Id).Write(
                        arenaContext?.ElapsedMillisecondsSafe ?? 0,
                        protocolMessage.GetTypeCode(),
                        bytes,
                        _loggingStatistics
                    );
                }
            }
            else
            {
                lock (messageRecords.ServerOutbound)
                {
                    GetPlayerOutbound(playerSession.Player.Id).Write(
                        0,
                        protocolMessage.GetTypeCode(),
                        default,
                        _loggingStatistics
                        );
                }
            }
        }


        // call on ArenaThread
        public void RecordBroadcast(IProtocol protocolMessage, bool isServerBeginMessage)
        {
            int typeCode = protocolMessage.GetTypeCode();
            if (NeedRecord(typeCode) == false)
            {
                return;
            }

            if (_recordBytes == true)
            {
                ReadOnlySequence<byte> bytes = MessageSerializer.SerializeUnsafe(protocolMessage);
                lock (messageRecords.ServerBroadcast)
                {
                    messageRecords.ServerBroadcast.Write(
                        arenaContext?.ElapsedMillisecondsSafe ?? 0,
                        typeCode,
                        bytes,
                        _loggingStatistics
                    );
                }

                RecordServerBeginMessage(typeCode, (int)bytes.Length);
            }
            else
            {
                lock (messageRecords.ServerBroadcast)
                {
                    messageRecords.ServerBroadcast.Write(
                        0,
                        typeCode,
                        default,
                        _loggingStatistics
                    );
                }

                RecordServerBeginMessage(typeCode, 0);
            }

        }

        private void RecordServerBeginMessage(int typeCode, int payloadSize)
        {
            lock (messageRecords.ServerBeginMessages)
            {
                if (messageRecords.ServerBeginMessages.TryGetValue(typeCode, out (int count, int size)pair) == true)
                {
                    messageRecords.ServerBeginMessages[typeCode] = (pair.count + 1, pair.size + payloadSize);
                }
                else
                {
                    messageRecords.ServerBeginMessages.Add(typeCode, (1, payloadSize));
                }
            }
        }

        public class ProtocolInfo
        {
            public int Count;
            public int TotalMetadataSize;
            public int TotalPayloadSize;
        }

        private void SaveStatistics(RecordMetadata metadata)
        {
            {
                int totalInbound = 0;
                int totalOutbound = 0;
                int totalBroadcast = 0;
                lock (messageRecords.ServerInbound)
                {
                    totalInbound = messageRecords.ServerInbound.Values.Sum(v => v.GetTotalSize());
                }

                lock (messageRecords.ServerOutbound)
                {
                    totalOutbound = messageRecords.ServerOutbound.Values.Sum(v => v.GetTotalSize());
                }

                lock (messageRecords.ServerBroadcast)
                {
                    totalBroadcast = messageRecords.ServerBroadcast.GetTotalSize();
                }

                metadata.TotalMessageBytes = totalInbound + totalOutbound + totalBroadcast;
                logger.Info($"{arenaContext} Inbound[{UnitUtility.SizeSuffix(totalInbound)}] + Outbound[{UnitUtility.SizeSuffix(totalOutbound)}] + Broadcast[{UnitUtility.SizeSuffix(totalBroadcast)}] = {UnitUtility.SizeSuffix(metadata.TotalMessageBytes)}");
            }

            if (_loggingStatistics == false)
            {
                return;
            }

            Dictionary<int, Dictionary<int/*TypeCode*/, ProtocolInfo>> infosByPlayer = new Dictionary<int, Dictionary<int, ProtocolInfo>>();

            lock (messageRecords.ServerInbound)
            {
                foreach (KeyValuePair<int, MessageStream> pair in messageRecords.ServerInbound)
                {
                    infosByPlayer.Add(pair.Key, new Dictionary<int, ProtocolInfo>());
                    Dictionary<int, ProtocolInfo> infos = infosByPlayer[pair.Key];
                    if (pair.Value.Statistics != null)
                    {
                        foreach ((int typeCode, (int count, int payloadSize)) in pair.Value.Statistics)
                        {
                            if (infos.ContainsKey(typeCode) == false)
                            {
                                infos.Add(typeCode, new ProtocolInfo());
                            }

                            ProtocolInfo protocolInfo = infos[typeCode];

                            int typeCodeSize = MessagePackUtility.GetInt32BufferSize(typeCode);
                            protocolInfo.Count += 1;
                            protocolInfo.TotalMetadataSize += typeCodeSize + sizeof(ushort);
                            protocolInfo.TotalPayloadSize += payloadSize;
                        }
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            Dictionary<int, (int count, int size)> serverBeginMessages = null;
            lock (messageRecords.ServerBeginMessages)
            {
                serverBeginMessages = new Dictionary<int, (int, int)>(messageRecords.ServerBeginMessages);
            }

            {
                var sorted = serverBeginMessages.OrderByDescending(e =>
                {
                    int typeCode = e.Key;
                    int typeCodeSize = MessagePackUtility.GetInt32BufferSize(typeCode);
                    int metaSize = typeCodeSize + sizeof(ushort) * e.Value.count;
                    return e.Value.size + metaSize;
                });

                int i = 0;
                const int LogCount = 7;
                int miscSize = 0;
                logger.Info($"[Statistics] {arenaContext} ServerBeginMessages -start-");
                sb.AppendLine();
                foreach (KeyValuePair<int, (int count, int size)> pair in sorted)
                {
                    int typeCode = pair.Key;
                    int typeCodeSize = MessagePackUtility.GetInt32BufferSize(typeCode);
                    int metaSize = typeCodeSize + sizeof(ushort) * pair.Value.count;
                    if (i++ < LogCount)
                    {
                        sb.AppendLine(
                            $"[{MessageSerializer.ResolveTypeName(pair.Key)}] Count:{pair.Value.count} MetaSize:{metaSize} PayloadSize:{pair.Value.size}");
                    }
                    else
                    {
                        miscSize += metaSize + pair.Value.size;
                    }
                }

                sb.AppendLine(
                    $"[MiscTotal] {miscSize} bytes");
            }
            logger.Info(sb.ToString);
            logger.Info($"----------------------------------------------------------------------");


            foreach (KeyValuePair<int, Dictionary<int, ProtocolInfo>> playerData in infosByPlayer)
            {
                var sorted = playerData.Value.OrderByDescending(d => d.Value.Count);
                int playerId = playerData.Key;
                logger.Info($"[Statistics] {arenaContext} Player[{playerId}] -start-");
                sb.Clear();
                int totalCount = 0;
                int totalSize = 0;

                sb.AppendLine();
                foreach (KeyValuePair<int, ProtocolInfo> pair in sorted)
                {
                    if (_recordBytes == true)
                    {
                        sb.AppendLine(
                            $"[{MessageSerializer.ResolveTypeName(pair.Key)}] Count:{pair.Value.Count} MetaSize:{pair.Value.TotalMetadataSize} PayloadSize:{pair.Value.TotalPayloadSize}");
                    }
                    else
                    {
                        sb.AppendLine(
                            $"[{MessageSerializer.ResolveTypeName(pair.Key)}] Count:{pair.Value.Count}");
                    }
                    totalCount += pair.Value.Count;
                    totalSize += pair.Value.TotalMetadataSize + pair.Value.TotalPayloadSize;
                }

                sb.AppendLine($"Total Count : {totalCount} / {(_recordBytes ? $"Total Size : {totalSize}" : "")}");

                logger.Info(sb.ToString);
                logger.Info($"--------------------------------------------------------------------");
            }
        }
    }

    public class NullArenaRecorder : IArenaRecorder
    {
        public void RecordInbound(ISession session, IProtocol protocolMessage)
        {
        }

        public void RecordOutbound(ISession session, IProtocol protocolMessage)
        {
        }

        public void RecordServerBeginMessage(IProtocol protocolMessage)
        {
            
        }


        public void RecordBroadcast(IProtocol protocolMessage, bool isServerBeginMessage)
        {
        }

        public void Release()
        {
        }

        public Task Save(RecordMetadata metadata) => Task.CompletedTask;
    }
}