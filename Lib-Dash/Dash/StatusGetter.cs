using Common.Utility;
using Dash.Core;
using Dash.Core.Components;
using Dash.Core.Entities;
using Dash.Model.Rdb;
using Dash.StaticData;
using Dash.Types;
using System.Collections.Generic;

namespace Dash
{
    public class StatusGetter
    {
        public IReadOnlyCollection<StatusType> Types => _resultValues.Keys;
        private Dictionary<StatusType, float> _resultValues = new Dictionary<StatusType, float>();
        private Dictionary<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)> _rawValues = new Dictionary<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)>();

        private static void ApplyAdd(Dictionary<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)> dic,
            StatusType type, float value)
        {
            (float add, float mul, float postadd, float postmul, float scale) = dic[type];
            dic[type] = (add + value, mul, postadd, postmul, scale);
        }
        private static void ApplyPostAdd(Dictionary<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)> dic,
            StatusType type, float value)
        {
            (float add, float mul, float postadd, float postmul, float scale) = dic[type];
            dic[type] = (add, mul, postadd + value, postmul, scale);
        }
        private static void ApplyMul(Dictionary<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)> dic,
            StatusType type, float value)
        {
            (float add, float mul, float postadd, float postmul, float scale) = dic[type];
            dic[type] = (add, mul + (value - 1f), postadd, postmul, scale);
        }
        private static void ApplyPostMul(Dictionary<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)> dic,
            StatusType type, float value)
        {
            (float add, float mul, float postadd, float postmul, float scale) = dic[type];
            dic[type] = (add, mul, postadd, postmul + (value - 1f), scale);
        }

        private static void ApplyStatusDelta(Dictionary<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)> dic,
            StatusType type, StatusDelta statusDelta)
        {
            if (statusDelta.ModifyType == StatusModifyType.Add)
            {
                ApplyAdd(dic, statusDelta.Type, statusDelta.Value);
            }

            if (statusDelta.ModifyType == StatusModifyType.Minus)
            {
                ApplyAdd(dic, statusDelta.Type, -statusDelta.Value);
            }

            if (statusDelta.ModifyType == StatusModifyType.Multiply)
            {
                ApplyMul(dic, statusDelta.Type, statusDelta.Value);
            }

            if (statusDelta.ModifyType == StatusModifyType.PostAdd)
            {
                ApplyPostAdd(dic, statusDelta.Type, statusDelta.Value);
            }

            if (statusDelta.ModifyType == StatusModifyType.PostMinus)
            {
                ApplyPostAdd(dic, statusDelta.Type, -statusDelta.Value);
            }

            if (statusDelta.ModifyType == StatusModifyType.PostMultiply)
            {
                ApplyPostMul(dic, statusDelta.Type, statusDelta.Value);
            }
        }

        public void Initialize(Character character, List<Equipment> equipments, List<Collection> collections,
                               List<EpisodeRuleInfo> episodeRuleInfos = null, PlayerType playerType = PlayerType.User)
        {
            _resultValues.Clear();
            _rawValues.Clear();

            foreach (StatusType statusType in EnumInfo<StatusType>.GetValues())
            {
                if (statusType == StatusType.Undefined)
                {
                    continue;
                }

                _rawValues.Add(statusType, (0f, 1f, 0f, 1f, 1f));
            }

            if (character != null)
            {
                var statusComponent = new StatusComponent();
                CharacterEntity.InitStatusComponent(statusComponent, character.Id, character.Level, character.Overcome);
                foreach (KeyValuePair<StatusType, Status> pair in statusComponent.FloatStatuses)
                {
                    _rawValues[pair.Key] = (pair.Value.BaseValue, 1f, 0f, 1f, pair.Value.Scale);
                }

                foreach (KeyValuePair<StatusType, Status> pair in statusComponent.IntStatuses)
                {
                    _rawValues[pair.Key] = (pair.Value.BaseValue, 1f, 0f, 1f, pair.Value.Scale);
                }
                if (StaticInfo.StaticInfo.Instance.CharacterRankAbilityGroupInfo.TryGet(character.Info.CharacterRankAbilityGroupId, out var characterRankAbilityGroupInfo))
                {
                    var statusDeltas = characterRankAbilityGroupInfo.GetStatusDeltas(character.Rank);
                    foreach (StatusDelta statusDelta in statusDeltas)
                    {
                        ApplyStatusDelta(_rawValues, statusDelta.Type, statusDelta);
                    }
                }
                if (character.Info.PassiveAbilityInfo != null)
                {
                    var statusDeltas = character.Info.PassiveAbilityInfo.StatusDeltas;
                    foreach (StatusDelta statusDelta in statusDeltas)
                    {
                        ApplyStatusDelta(_rawValues, statusDelta.Type, statusDelta);
                    }
                }
            }

            if (equipments != null)
            {
                Dictionary<int /*setId*/, int /*count*/> armorSets = new Dictionary<int, int>();
                foreach (Equipment equipment in equipments)
                {
                    List<StatusDelta> statusDeltas = new List<StatusDelta>();
                    statusDeltas.AddRange(equipment.Info.GetStatusDeltas(equipment));
                    foreach (StatusDelta statusDelta in statusDeltas)
                    {
                        ApplyStatusDelta(_rawValues, statusDelta.Type, statusDelta);
                    }

                    if (equipment.Info is StaticData.Item.ArmorInfo armorInfo)
                    {
                        if (armorSets.ContainsKey(armorInfo.ArmorSetId) == false)
                        {
                            armorSets.Add(armorInfo.ArmorSetId, 0);
                        }
                        armorSets[armorInfo.ArmorSetId] += 1;
                    }
                }
                foreach (var pair in armorSets)
                {
                    if (StaticInfo.StaticInfo.Instance.ArmorSetInfo.TryGet(pair.Key, out var armorSetInfo) == false)
                    {
                        continue;
                    }
                    var statusDeltas = armorSetInfo.GetStatusDeltas(pair.Value);
                    foreach (StatusDelta statusDelta in statusDeltas)
                    {
                        ApplyStatusDelta(_rawValues, statusDelta.Type, statusDelta);
                    }
                }

            }

            collections?.ForEach((collection) =>
                {
                    if (collection == null || collection.Info == null)
                    {
                        return;
                    }
                    var statBonusInfos = collection.Info.GetAppliedStatBonusList(collection.MinimumRank);
                    statBonusInfos?.ForEach((info) =>
                        info.StatusBonusList.ForEach(statusDelta =>
                            ApplyStatusDelta(_rawValues, statusDelta.Type, statusDelta)));
                });

            if (episodeRuleInfos != null && episodeRuleInfos.Count > 0)
            {
                foreach (var towerRuleInfo in episodeRuleInfos)
                {
                    if (towerRuleInfo.CharacterStatusDeltas == null)
                    {
                        continue;
                    }
                    foreach (StatusDelta statusDelta in towerRuleInfo.CharacterStatusDeltas)
                    {
                        ApplyStatusDelta(_rawValues, statusDelta.Type, statusDelta);
                    }
                }
            }
            foreach (KeyValuePair<StatusType, (float addValue, float multiplyValue, float postAdd, float postMul, float scaleValue)> pair in _rawValues)
            {
                _resultValues.Add(pair.Key,
                    ((pair.Value.addValue * pair.Value.multiplyValue) + (pair.Value.postAdd * pair.Value.postMul)) * pair.Value.scaleValue);
            }
        }

        public float GetValue(StatusType statusType)
        {
            if (_resultValues.ContainsKey(statusType) == false)
            {
                return 0.0f;
            }
            return _resultValues[statusType];
        }

        public string GetValueString(StatusType statusType)
        {
            var value = GetValue(statusType);
            return statusType.GetValueString(value);
        }

        public (float add, float multiply, float postAdd, float postMul, float scale) GetRawValue(StatusType statusType)
        {
            if (_rawValues.ContainsKey(statusType) == false)
            {
                return (0f, 1f, 0f, 1f, 1f);
            }
            return _rawValues[statusType];
        }
        public float GetValueWidthScale(StatusType statusType)
        {
            if (_rawValues.ContainsKey(statusType) == false)
            {
                return 0.0f;
            }
            var rawValue = _rawValues[statusType];
            return (rawValue.addValue * rawValue.multiplyValue) + (rawValue.postAdd * rawValue.postMul);
        }
    }
}
