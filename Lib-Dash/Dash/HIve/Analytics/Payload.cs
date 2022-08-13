#if Common_Server
using Common.Utility;
using Dash.Model.Rdb;
using System;
using System.Collections.Generic;

namespace Dash.Hive.Analytics
{
    public abstract class Payload
    {
        public const string GameName = "lunia";
        public const string Channel = "C2S";
        public const string Company = "C2S";

        const string UTC_TimeZone = "GMT+00:00";
        const string KST_TimeZone = "GMT+09:00";
        public virtual string category { get; }
        public string channel => Channel;
        public string dateTime = DateTime.UtcNow.ToKST().ToString_DateTime();

        public string timezone = KST_TimeZone;
        public string guid = Guid.NewGuid().ToString();
        public string serverId = Logger.HiveConfig.ServerZone.toString();
        public string company => Company;
        public string appId;

        public void Prepare(Account account, AccountExtra accountExtra, Auth auth, string serverIp)
        {
            appId = Helper.GetAppId(accountExtra?.MarketType ?? Types.MarketType.Undefined);
            long.TryParse(auth.AuthId, out var uid);
            long.TryParse(accountExtra?.DeviceId, out var did);
            var context = new Context
            {
                uid = uid,

                oid = (long)account.OidAccount,
                level = account.Level,
                country = account.Country,
                created = account.Created.ToKST().ToString_DateTime(),
                lastestLogon = account.LatestLogon.ToKST().ToString_DateTime(),

                market = Helper.GetMarketString(accountExtra?.MarketType ?? Types.MarketType.Undefined),
                deviceName = accountExtra?.Device,
                osVersion = accountExtra?.OsVersion,
                did = did,
                clientIp = accountExtra?.ClientIp,
                gameLanguage = accountExtra?.Language,

                serverIp = serverIp,
            };
            Convert(context);
        }

        protected abstract void Convert(Context context);

        public struct Context
        {
            public long uid;

            public long oid;
            public int level;
            public string country;
            public string created;
            public string lastestLogon;

            public string market;
            public string deviceName;
            public string osVersion;
            public long did;
            public string clientIp;
            public string gameLanguage;

            public string serverIp;
        }
    }

    public class Login_Payload : Payload
    {
        public override string category => $"{GameName}_login_log";

        public long userId { get; private set; }

        public long serverUid { get; private set; }
        public int level { get; private set; }
        public string country { get; private set; }
        public string lastLoginDate { get; private set; }

        public string device_name { get; private set; }
        public string os_version { get; private set; }
        public long did { get; private set; }
        public string clientIp { get; private set; }
        public string serverIp { get; private set; }
        public string gameLanguage { get; private set; }

        protected override void Convert(Context context)
        {
            userId = context.uid;

            serverUid = context.oid;
            level = context.level;
            country = context.country;
            lastLoginDate = context.lastestLogon;

            device_name = context.deviceName;
            os_version = context.osVersion;
            did = context.did;
            clientIp = context.clientIp;
            serverIp = context.serverIp;
            gameLanguage = context.gameLanguage;
        }
    }
    public class NewUser_Payload : Payload
    {
        public override string category => $"{GameName}_new_user_log";

        public long userId { get; private set; }

        public long serverUid { get; private set; }
        public string country { get; private set; }

        public string device_name { get; private set; }
        public string os_version { get; private set; }
        public long did { get; private set; }
        public string clientIp { get; private set; }
        public string serverIp { get; private set; }
        public string gameLanguage { get; private set; }

        protected override void Convert(Context context)
        {
            userId = context.uid;

            serverUid = context.oid;
            country = context.country;

            device_name = context.deviceName;
            os_version = context.osVersion;
            did = context.did;
            clientIp = context.clientIp;
            serverIp = context.serverIp;
            gameLanguage = context.gameLanguage;
        }
    }
    public class Asset_Payload : Payload
    {
        public override string category => $"{GameName}_asset_var_log";

        public string channelUid { get; private set; }

        public long accountId { get; private set; }
        public int accountLevel { get; private set; }
        public string country { get; private set; }

        public string market { get; private set; }
        public long deviceId { get; private set; }
        public string clientIp { get; private set; }
        public string serverIp { get; private set; }
        public string gameLanguage { get; private set; }

        public string game => GameName;
        public long characterId => 0;
        public int characterTypeId => 0;
        public int characterLevel => 0;

        /// <summary>
        /// 게임에서 유니크한 재화 변동에 관한 유저의 액션 ID, 
        /// 범위: [1–(2^31−1)] 액션은 게임 상에서 api or protocol로 구분되는 것들로, 
        /// 각 게임 서버에서는 액션에 대한 ID를 정의 해야 함.
        /// </summary>
        public int actionId;
        /// <summary>
        /// action_id에 1:1 매핑되는 값으로, action_id가 다르다면 action_name도 달라야함. (ex:밥주기, 농작물수확 등)
        /// 실데이터는 action_id기반으로 쌓임
        /// 변경되면 메타테이블에 자동 반영됨
        /// </summary>
        public string actionName;
        /// <summary>
        /// 재화 변동을 발생시킨 아이템 고유 식별자. 재화 변동 액션이 아이템과 연관이 없을 때는 0으로 표기하세요.
        /// 범위: [0–(2^31−1)]1부터 시작하는 숫자값으로, 값이 없는 경우 외에는 0이면 안됨
        /// 해당 액션이 아이템과 연관이 있을 경우에 필요한 파라미터이며, 각 게임에서 유니크한 아이템의 ID를 정의해야 함
        /// 한번 정의 후 변경되면 안됨.반드시 테스트 필수!	
        /// </summary>
        public long itemId;//아이템과 연관없으면 0
        /// <summary>
        /// item_id와 1:1 매핑 되는 값. item_id가 0인 경우는 “0”을 입력하고, 0이 아닌 경우를 제외하고는 설명이 존재해야 함.
        /// item_id를 알아보기 쉽도록 간단히 제공하는 설명임
        /// </summary>
        public string itemName;
        /// <summary>
        /// item_id의 상세 현황을 파악할 목적으로 추가한 item id	
        /// </summary>
        public long item2Id;//아이템과 연관없으면 0
        /// <summary>
        /// item2_id와 1:1 매핑되는 값.item2_id가 0인 경우는 “0”을 입력하고, 0이 아닌 경우를 제외하고는 설명이 존재해야 함.
        /// item2_id를 알아보기 쉽도록 간단히 제공하는 설명임
        /// </summary>
        public string item2Name;
        /// <summary>
        ///	현금성 재화/소셜포인트의 경우(1~100), 비현금성 재화의 경우 (101~)
        /// </summary>
        public int assetId;
        /// <summary>
        /// asset_id에 대한 간단한 설명 (예. bell, star, goldball, gold)
        /// </summary>
        public string assetName;
        /// <summary>
        /// 	asset_id에 해당하는 재화의 변동 직전 재화량
        /// </summary>
        public long amountPrev;
        /// <summary>
        /// asset_id에 해당하는 재화의 변동량.
        /// 재화 감소: 음수, 재화 증가: 양수
        /// </summary>
        public long amountVar;
        /// <summary>
        ///	asset_id에 해당하는 재화의 변동 직후 재화량.
        /// amount_curr = amount_prev + amount_var
        /// </summary>
        public long amountCurr;
        /// <summary>
        /// asset_id에 해당하는 무상재화의 변동 직전 재화량.
        /// (일본자금결제법 관련으로 추가)	
        /// </summary>
        public long amountFreePrev;
        /// <summary>
        ///	asset_id에 해당하는 무상재화의 변동량.
        /// (일본자금결제법 관련으로 추가)
        /// 재화 감소: 음수, 재화 증가: 양수
        /// </summary>
        public long amountFreeVar;
        /// <summary>
        ///	asset_id에 해당하는 무상재화의 변동 직후 재화량.
        /// (일본자금결제법 관련으로 추가)
        /// amount_free_curr = amount_free_prev + amount_free_var
        /// </summary>
        public long amountFreeCurr;
        /// <summary>
        /// asset_id에 해당하는 유상재화의 변동 직전 재화량.
        /// (일본자금결제법 관련으로 추가)	
        /// </summary>
        public long amountPaidPrev;
        /// <summary>
        ///	asset_id에 해당하는 유상재화의 변동량.
        /// (일본자금결제법 관련으로 추가)
        /// 재화 감소: 음수, 재화 증가: 양수
        /// </summary>
        public long amountPaidVar;
        /// <summary>
        ///	asset_id에 해당하는 유상재화의 변동 직후 재화량.
        /// (일본자금결제법 관련으로 추가)
        /// amount_free_curr = amount_free_prev + amount_free_var
        /// </summary>
        public long amountPaidCurr;
        /// <summary>
        /// 실제 소진한 재화 건수
        /// 예. 10개 일괄 수령 시 로그는 1개지만 실제 받은 재화는 10개
        /// </summary>
        public int realCount;

        protected override void Convert(Context context)
        {
            channelUid = context.uid.ToString();

            accountId = context.oid;
            accountLevel = context.level;
            country = context.country;

            market = context.market;
            deviceId = context.did;
            clientIp = context.clientIp;
            serverIp = context.serverIp;
            gameLanguage = context.gameLanguage;
        }
    }
    public class Score_Payload : Payload
    {
        public override string category => $"{GameName}_score2_log";

        public string channelUid { get; private set; }

        public long accountId { get; private set; }
        public int accountLevel { get; private set; }

        public string country { get; private set; }

        public string market { get; private set; }
        public long deviceId { get; private set; }
        public string clientIp { get; private set; }
        public string serverIp { get; private set; }

        public string game => GameName;
        public long characterId = 0;
        public int characterTypeId;
        public int characterLevel;
        public int ai1TypeId;
        public int ai1Level;
        public int ai2TypeId;
        public int ai2Level;

        public int modeId;
        public string modeName;
        public int submodeId;
        public string submodeName;
        public int gradeId;
        public string gradeName;
        public int score;
        public string battleResult;

        public int playmodeId;

        protected override void Convert(Context context)
        {
            channelUid = context.uid.ToString();

            accountId = context.oid;
            accountLevel = context.level;
            country = context.country;

            market = context.market;
            deviceId = context.did;
            clientIp = context.clientIp;
            serverIp = context.serverIp;
        }
    }
    public class Tutorial_Payload : Payload
    {
        public override string category => $"com2us_tutorial2_log";

        public string uid { get; private set; }
        public string channelUid { get; private set; }
        public string vid { get; private set; }

        public string serveruid { get; private set; }
        public string accountId { get; private set; }
        public string level { get; private set; }
        public string lastresetdate { get; private set; }

        public string market { get; private set; }
        public string did { get; private set; }
        public string clientIp { get; private set; }

        public string game => GameName;
        public int serverIdInt => Logger.HiveConfig.ServerZone.toInt();
        public string wasRegistTimestamp => dateTime;
        public string characterid = "0";
        public string charactertypeid = "0";
        public string characterlevel = "0";

        public string modeid;
        public string modename;
        public string submodeid = "0";
        public string submodename = "";
        public string tutorialid;
        public string tutorialname;
        public string playtime = "0";

        protected override void Convert(Context context)
        {
            uid = context.uid.ToString();
            channelUid = context.uid.ToString();
            vid = context.uid.ToString();

            serveruid = context.oid.ToString();
            accountId = context.oid.ToString();
            level = context.level.ToString();
            lastresetdate = context.created;

            market = context.market;
            did = context.did.ToString();
            clientIp = context.clientIp;
        }
    }

    public class LevelUp_Payload : Payload
    {
        public override string category => $"com2us_levelup2_log";

        public string uid { get; private set; }
        public string channelUid { get; private set; }
        public string vid { get; private set; }

        public string accountId { get; private set; }
        public string level { get; private set; }
        public string lastresetdate { get; private set; }

        public string market { get; private set; }
        public string did { get; private set; }
        public string clientIp { get; private set; }

        public string game => GameName;
        public string wasRegistTimestamp => dateTime;
        public string gubun; // 계정레벨업= 0 / 캐릭터레벨업 =1
        public string characterid = "0";
        public string charactertypeid = "0";
        public string characterlevel = "0";

        protected override void Convert(Context context)
        {
            uid = context.uid.ToString();
            channelUid = context.uid.ToString();
            vid = context.uid.ToString();

            accountId = context.oid.ToString();
            level = context.level.ToString();
            lastresetdate = context.created;

            market = context.market;
            did = context.did.ToString();
            clientIp = context.clientIp;
        }
    }

    public class Custom_Payload : Payload
    {
        public override string category => $"{GameName}_custom_log";
        public Custom_Payload_Log log;
        public List<Custom_Payload_Meta> meta = new List<Custom_Payload_Meta>();

        protected override void Convert(Context context) { }
    }
    public abstract class Custom_Payload_Log
    {
        public string date = DateTime.UtcNow.ToKST().ToString_DateTime();
        public string type;
    }
    //컴투스와 협의후 정해야 한다. 지금은 샘플을 넣음
    public class EquipmentChangeLog : Custom_Payload_Log
    {
        public ulong oidAccount;
        public string reason;
        public uint serial;
        public int itemId;
        public int level;
        public bool isGain;
    }
    public class ConsumeChangeLog : Custom_Payload_Log
    {
        public ulong oidAccount;
        public string reason;
        public int itemId;
        public int amountPrev;
        public int amountVar;
        public int amountCurr;
    }
    public abstract class Custom_Payload_Meta
    {
        public string type;
        public string channel => Payload.Channel;
        public string game => Payload.GameName;
        public string id;
        public string name;
    }
}
#endif