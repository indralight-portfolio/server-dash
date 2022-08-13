using System.Collections.Generic;
using Common.Utility;
#if Common_Server
using System.ComponentModel.DataAnnotations.Schema;
#endif
using MessagePack;
using Newtonsoft.Json;

namespace Dash.Model.Rdb
{
    public partial class MultipleDeck
    {
        private List<MultipleDeckData> _multipleDeckDatas;
#if Common_Server
        [NotMapped]
#endif
#if !Admin_Server
        [JsonIgnore]
#endif
        [IgnoreMember]
        public List<MultipleDeckData> MultipleDeckDatas
        {
            get
            {
                if(_multipleDeckDatas == null && DeckData?.Length > 0)
                {
                    try
                    {
                        _multipleDeckDatas = JsonGzipSerializer.Deserialize<List<MultipleDeckData>>(DeckData);
                    }
                    catch
                    {
                        return new List<MultipleDeckData>();
                    }
                }
                return _multipleDeckDatas;
            }
            set
            {
                _multipleDeckDatas = value;
                DeckData = JsonGzipSerializer.Serialize(_multipleDeckDatas);
            }

        }
        public bool IsUsing(int characterId)
        {
            return _multipleDeckDatas.FindIndex(e => e.IsUsing(characterId)) != -1;
        }
    }

    [MessagePackObject()]
    public class MultipleDeckData
    {
        [Key(0)]
        public int CharacterId { get; set; }
        [Key(1)]
        public int Ai1 { get; set; }
        [Key(2)]
        public int Ai2 { get; set; }
        public bool IsUsing(int characterId)
        {
            return CharacterId == characterId || Ai1 == characterId || Ai2 == characterId;
        }
    }
}
