using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Utility;
using Dash;
using Dash.Model;
using Dash.Model.Rdb;
using Dash.Server.Dao.Cache;
using Dash.StaticInfo;
using Dash.Types;

namespace server_dash.Internal.Services
{
    public static class PlayerService
    {
        private static ISingleDBCache<StaticAccount> _staticAccountCache;
        private static IMultipleDBCache<ChapterAchievement> _chapterAchievementCache;

        public static void Init(DaoCache daoCache)
        {
            _staticAccountCache = daoCache.GetSingle<StaticAccount>();
            _chapterAchievementCache = daoCache.GetMultiple<ChapterAchievement>();
        }

        public static async Task<PlayerModel> MakePlayerModel(ulong oidAccount, string nickname)
        {
            PlayerModel result = new PlayerModel();
            (Hero character, List<Equipment> equipments, List<Talent> talents, List<ChapterAchievement> chapterAchievements) = await TaskUtility.WaitAll4(
                InventoryService.GetUsingHero(oidAccount),
                InventoryService.GetEquipmentsInSlot(oidAccount),
                TalentService.GetTalents(oidAccount),
                _chapterAchievementCache.GetAll(oidAccount)
            );

            if (character == null)
            {
                return null;
            }

            if (equipments == null)
            {
                return null;
            }


            result.OidAccount = oidAccount;
            result.Nickname = nickname;
            result.Equipments = equipments;
            result.Talents = talents;
            result.HeroId = character.Id;
            result.HeroLevel = character.HeroLevel;
            result.EmblemId = character.EmblemId;
            result.ReachedChapterId = RdbModelExtension.GetReachedChapterId(chapterAchievements);

            return result;
        }
    }
}