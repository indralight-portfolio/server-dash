#if Common_Unity
using Common.Unity.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Common.Unity.Social
{
    public class UnitySocialPlatform : ISocialPlatform
    {
        private UnitySocialLocalUser uLocalUser = null;

        public UnitySocialPlatform(string socialId)
        {
            this.uLocalUser = new UnitySocialLocalUser(this);
            this.uLocalUser.id = socialId;
        }

        public ILocalUser localUser
        {
            get { return uLocalUser; }
        }

        public void Authenticate(ILocalUser unused, Action<bool> callback)
        {
            Authenticate((bool success, string msg) => callback(success));
        }

        public void Authenticate(ILocalUser unused, Action<bool, string> callback)
        {
            Authenticate(callback);
        }

        public void Authenticate(Action<bool, string> callback)
        {
            bool success = true;
            string msg = string.Empty;
            callback(success, msg);
        }

        public IAchievement CreateAchievement()
        {
            throw new NotImplementedException();
        }

        public ILeaderboard CreateLeaderboard()
        {
            throw new NotImplementedException();
        }

        public bool GetLoading(ILeaderboard board)
        {
            throw new NotImplementedException();
        }

        public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadFriends(ILocalUser user, Action<bool> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadScores(string leaderboardID, Action<IScore[]> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadScores(ILeaderboard board, Action<bool> callback)
        {
            throw new NotImplementedException();
        }

        public void LoadUsers(string[] userIDs, Action<IUserProfile[]> callback)
        {
            throw new NotImplementedException();
        }

        public void ReportProgress(string achievementID, double progress, Action<bool> callback)
        {
            throw new NotImplementedException();
        }

        public void ReportScore(long score, string board, Action<bool> callback)
        {
            throw new NotImplementedException();
        }

        public void ShowAchievementsUI()
        {
            throw new NotImplementedException();
        }

        public void ShowLeaderboardUI()
        {
            throw new NotImplementedException();
        }
    }

    public class UnitySocialLocalUser : ILocalUser
    {
        internal UnitySocialPlatform uPlatform;

        internal UnitySocialLocalUser(UnitySocialPlatform platform)
        {
            uPlatform = platform;
        }

        public IUserProfile[] friends => throw new NotImplementedException();

        public bool authenticated
        {
            get
            {
                return string.IsNullOrEmpty(id) == false;
            }
        }

        public bool underage => throw new NotImplementedException();

        public string userName => id;

        public string id { get; set; }

        public bool isFriend => throw new NotImplementedException();

        public UserState state => throw new NotImplementedException();

        public Texture2D image => throw new NotImplementedException();

        public void Authenticate(Action<bool> callback)
        {
            uPlatform.Authenticate(this, callback);
        }

        public void Authenticate(Action<bool, string> callback)
        {
            uPlatform.Authenticate(this, callback);
        }

        public void LoadFriends(Action<bool> callback)
        {
            throw new NotImplementedException();
        }
    }
}
#endif