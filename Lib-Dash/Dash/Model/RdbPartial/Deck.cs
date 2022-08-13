using Dash.Types;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dash.Model.Rdb
{
    public partial class Deck
    {
        public static int DefaultCharacterId = 3;

        [IgnoreMember, JsonIgnore]
        public DeckType Type
        {
            get
            {
                if (Id == (byte)DeckType.Multi)
                    return DeckType.Multi;
                else if (Id >= (byte)DeckType.Solo && Id < (byte)DeckType.Duo)
                    return DeckType.Solo;
                else if (Id >= (byte)DeckType.Duo && Id < (byte)DeckType.Trio)
                    return DeckType.Duo;
                else if (Id >= (byte)DeckType.Trio)
                    return DeckType.Trio;
                else
                    return DeckType.Undefined;
            }
        }

        public bool IsUsing(int characterId)
        {
            return CharacterId == characterId || Ai1 == characterId || Ai2 == characterId;
        }
        public bool IsAI(int characterId)
        {
            return Ai1 == characterId || Ai2 == characterId;
        }

        public void Check()
        {
            Ai1 = (Ai1 == CharacterId || Type < DeckType.Duo) ? 0 : Ai1;
            Ai2 = (Ai1 == CharacterId || Type < DeckType.Trio) ? 0 : Ai2;
        }
        public bool IsCharacterSlot(int characterId)
        {
            return CharacterId == characterId;
        }
        public List<int> GetCharacterList()
        {
            List<int> result = new List<int>();
            result.Add(CharacterId);
            if (Ai1 > 0)
                result.Add(Ai1);
            if (Ai2 > 0)
                result.Add(Ai2);
            return result;
        }

        public static Deck GetDefault(ulong oidAccount, byte id)
        {
            return new Deck { OidAccount = oidAccount, Id = id, CharacterId = DefaultCharacterId };
        }
    }

    public enum DeckSlotType
    {
        Player,
        AI1,
        AI2
    }
}