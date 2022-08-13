using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using Common.Utility;
using Dash;
using MessagePack;
using NLog;
using server_dash.AWS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace server_dash.Battle.Services.GamePlay.Recorder
{
    public class S3ArenaRecorder : ArenaRecorder
    {
        private new static Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();
        private IAmazonS3 s3Client;
        private string s3Bucket;
        private string s3Path;
        private IAmazonDynamoDB dynamoDBClient;
        private string ddbTableSuffix;
        private const string ReplayTableName = "Replay";
        private const string ReplayAccountTableName = "ReplayAccount";

        public S3ArenaRecorder(ArenaContext arenaContext, bool loggingStatistics, bool recordBytes) : base(_logger, arenaContext, loggingStatistics, recordBytes)
        {
            s3Client = S3ClientFactory.Instance.GetClient();

            var serverConfig = ConfigManager.Get<BattleServerConfig>(Config.BattleServer);
            s3Bucket = serverConfig.ArenaRecord.S3Bucket;
            s3Path = serverConfig.ArenaRecord.S3Path;

            dynamoDBClient = DynamoDBClientFactory.Instance.GetClient();
            ddbTableSuffix = serverConfig.ArenaRecord.DDBTableSuffix;
        }

        public override async Task Save(RecordMetadata metadata)
        {
            await base.Save(metadata);
            // TODO: Serialize buffer pooling
            try
            {
                byte[] serialized = null;
                int inboundCount = 0;
                int outboundCount = 0;
                lock (messageRecords.ServerInbound)
                {
                    lock (messageRecords.ServerOutbound)
                    {
                        serialized = MessagePackSerializer.Serialize(messageRecords);
                        #if DEBUG
                        try
                        {
                            var testDeserialized = MessagePackSerializer.Deserialize<MessageRecords>(serialized);
                        }
                        catch (Exception e)
                        {
                            _logger.Fatal(e);
                            _logger.Error($"MessageRecords deserialize failed!");
                        }
                        #endif
                    }
                }

                string arenaKey = string.Empty;
                bool exist = false;
                do
                {
                    arenaKey = Guid.NewGuid().ToString().Replace("-", "");
                    exist = await ExistsDDBAsync(arenaKey);
                } while (exist);
                string s3Key = string.Join('/', s3Path, DateTime.UtcNow.ToString("yyyy'/'MM'/'dd"), $"{arenaKey}.bin");

                TransferUtility transferUtility = new TransferUtility(s3Client);
                TransferUtilityUploadRequest request = new TransferUtilityUploadRequest
                {
                    BucketName = s3Bucket,
                    Key = s3Key,
                    InputStream = new MemoryStream(serialized),
                    CannedACL = S3CannedACL.PublicRead,
                };

                await transferUtility.UploadAsync(request);

                await SaveDDBAsync(arenaKey, metadata);

                string url = $"https://{s3Client.Config.RegionEndpoint.GetEndpointForService("s3").Hostname}/{s3Bucket}/{s3Key}";
                _logger.Info($"{metadata.LogStr} : [{url}]");
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }
        }

        private async Task SaveDDBAsync(string arenaKey, RecordMetadata metadata)
        {
            var dateTimeStr = metadata.Date.ToString_ISO();
            var version = metadata.Version;

            {
                Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
                attributes["ArenaKey"] = new AttributeValue { S = arenaKey };
                attributes["DateTime"] = new AttributeValue { S = dateTimeStr };

                PutItemRequest request = new PutItemRequest
                {
                    TableName = $"{ReplayTableName}_{ddbTableSuffix}",
                    Item = attributes,
                };

                await dynamoDBClient.PutItemAsync(request);
            }

            var attrPlayers = new AttributeValue
            {
                L = metadata.Players?.Select(p =>
                    new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue> {
                                { "OidAccount", new AttributeValue { N = p.OidAccount.ToString() } },
                                { "Nickname", new AttributeValue { S = string.IsNullOrEmpty(p.Nickname) ? "_Empty_" : p.Nickname } },
                                { "Version", new AttributeValue { S = p.Version } },
                        }
                    }).ToList() ?? new List<AttributeValue>()
            };
            if (metadata.Players != null)
            {
                foreach (var player in metadata.Players)
                {
                    Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
                    attributes["OidAccount"] = new AttributeValue { N = player.OidAccount.ToString() };
                    attributes["DateTime"] = new AttributeValue { S = dateTimeStr };
                    attributes["ArenaKey"] = new AttributeValue { S = arenaKey };
                    attributes["Version"] = new AttributeValue { S = version };
                    attributes["Players"] = attrPlayers;

                    PutItemRequest request = new PutItemRequest
                    {
                        TableName = $"{ReplayAccountTableName}_{ddbTableSuffix}",
                        Item = attributes,
                    };

                    await dynamoDBClient.PutItemAsync(request);
                }
            }
        }

        private async Task<bool> ExistsDDBAsync(string arenaKey)
        {
            GetItemRequest request = new GetItemRequest
            {
                TableName = $"{ReplayTableName}_{ddbTableSuffix}",
                Key = new Dictionary<string, AttributeValue>() { { "ArenaKey", new AttributeValue { S = arenaKey } } },
            };
            var response = await dynamoDBClient.GetItemAsync(request);
            return response.IsItemSet;
        }
    }
}