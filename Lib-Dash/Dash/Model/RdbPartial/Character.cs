using Dash.StaticData.Entity;
using MessagePack;
using Newtonsoft.Json;

namespace Dash.Model.Rdb
{
    using StaticInfo = StaticInfo.StaticInfo;

    public partial class Character
    {
        public Character(ulong oidAcocunt, int id) : this(id)
        {
            OidAccount = oidAcocunt;
        }
        public Character(int id)
        {
            Id = id;
            Level = CharacterInfo.MinLevel;
            Overcome = CharacterInfo.MinOvercome;
            Rank = CharacterInfo.MinRank;
            RuneTier = CharacterInfo.MinRuneTier;
        }
        private CharacterInfo _info;
        [IgnoreMember, JsonIgnore]
        public CharacterInfo Info
        {
            get
            {
                if (_info == null)
                {
                    StaticInfo.Instance.CharacterInfo.TryGet(Id, out _info);
                }
                return _info;
            }
        }

        public static Character GetDefaultChracter(ulong oidAccount, int characterId)
        {
            StaticInfo.Instance.CharacterInfo.TryGet(characterId, out var characterInfo);

            var character = new Character(characterId);
            character.OidAccount = oidAccount;
            return character;
        }

        public bool Equals(Character other)
        {
            return OidAccount.Equals(other.OidAccount) &&
                Id.Equals(other.Id) &&
                Exp.Equals(other.Exp) &&
                Level.Equals(other.Level) &&
                Overcome.Equals(other.Overcome) &&
                Rank.Equals(other.Rank) &&
                RuneTier.Equals(other.RuneTier);
        }
    }
}
