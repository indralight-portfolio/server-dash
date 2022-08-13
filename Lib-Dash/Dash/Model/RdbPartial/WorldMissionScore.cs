using Common.Utility;
using Dash.Model.Service;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace Dash.Model.Rdb
{
    public partial class WorldMissionScore
    {
        private PlayerData _playerData;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public PlayerData PlayerData
        {
            get
            {
                if (_playerData == null)
                {
                    if (Data?.Length > 0)
                    {
                        try
                        {
                            _playerData = JsonGzipSerializer.Deserialize<PlayerData>(Data);
                        }
                        catch { _playerData = new PlayerData { NickName = "Dummy" }; }
                    }
                    else
                        _playerData = new PlayerData { NickName = "Dummy" };
                }
                return _playerData;
            }
            set
            {
                _playerData = value;
                Data = JsonGzipSerializer.Serialize(_playerData);
            }
        }
        public static List<KeyValuePair<string, object>> GetListQueryParam(int eventId)
        {
            return new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>($"@{nameof(EventId)}", eventId)
            };
        }
    }
}
