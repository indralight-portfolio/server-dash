using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Common.StaticInfo;
using Dash.Core.Components;
using Dash.StaticData;
using Dash.StaticData.Entity;
using Dash.StaticData.Item;
using Dash.StaticData.Stat;
using ILogger = Common.Log.ILogger;

namespace Dash
{
    public interface IStatProvider
    {
        StatResult GetStat(StatCalcParam param);
    }

    public struct StatPathExpression
    {
        public string Value;

        public bool IsOperator()
        {
            return Value == "*" || Value == "-" || Value == "+";
        }
    }

    /// <summary>
    /// Dictionary(path,Provider) caching 가능할 듯?
    /// </summary>
    public class StatGetter
    {
        private static readonly ILogger _logger = Common.Log.Logger.Instance;

        private readonly StatusComponent _statusComponent;
        private IReadOnlyDictionary<string, object> _paramDic;
        private List<object> _pathValues = new List<object>();

        public StatGetter(StatusComponent statusComponent, IReadOnlyDictionary<string, object> paramDic)
        {
            _statusComponent = statusComponent;
            _paramDic = paramDic;
        }

        public StatResult GetStat(object info, StatData statData, string descKey)
        {
            _pathValues.Clear();
            try
            {
                List<StatPathExpression> expressions = GetExpressions(statData.Path);
                List<StatResult> statResults = new List<StatResult>();
                StatResult lastResult = default;
                string reservedOperator = string.Empty;
                foreach (StatPathExpression expression in expressions)
                {
                    if (expression.IsOperator())
                    {
                        reservedOperator = expression.Value;
                    }
                    else
                    {
                        lastResult = GetStatResult(statData, expression, RetrieveData(info, expression));
                        if (string.IsNullOrEmpty(reservedOperator) == false)
                        {
                            StatResult prevResult = statResults[statResults.Count - 1];
                            if (reservedOperator == "+")
                            {
                                lastResult = prevResult + lastResult;
                            }
                            else if (reservedOperator == "-")
                            {
                                lastResult = prevResult - lastResult;
                            }
                            else if (reservedOperator == "*")
                            {
                                lastResult = prevResult * lastResult;
                            }
                            else if (reservedOperator == "/")
                            {
                                lastResult = prevResult / lastResult;
                            }
                        }

                        statResults.Add(lastResult);
                        if (lastResult.IsValid == false)
                        {
                            _logger.Error($"Unknown Data : {expression.Value}, {statData.Path}, DescKey : {descKey}");
                            return default;
                        }
                    }
                }

                return lastResult;
            }
            catch (Exception e)
            {
                Common.Log.Logger.Fatal(e);
                return default;
            }
        }

        private StatResult GetStatResult(StatData statData, StatPathExpression expression, object targetData)
        {
            if (targetData == null)
            {
                _logger.Error($"Target data not found, expression : {expression.Value}");
                return default;
            }
            
            if (targetData is IStatProvider statProvider)
            {
                return statProvider.GetStat(new StatCalcParam()
                {
                    Type = statData.CalcType,
                    StatusComponent = _statusComponent,
                    Param = statData.Param,
                    LevelParam = statData.LevelParam,
                    PathValues = _pathValues
                });
            }
            if (targetData is int intValue)
            {
                return StatResult.FromInt(intValue);
            }

            if (targetData is float floatValue)
            {
                return StatResult.FromFloat(floatValue);
            }

            return default;
        }

        private static List<StatPathExpression> GetExpressions(string fullPath)
        {
            var result = new List<StatPathExpression>();
            string[] split = fullPath.Split(' ');
            foreach (string s in split)
            {
                result.Add(new StatPathExpression(){ Value = s.Trim()});
            }

            return result;
        }

        private object RetrieveData(object rootInfo, StatPathExpression expression)
        {
            /* [Path 구조]
             * 1. '.' 으로 참조 설정
             * 2. '[]' 으로 ID 설정 or Array Index
             *
             * [핵심]
             * 해당 Keyword의 Field를 찾는다. 타입에 의존하지 않는다.(GeneratorInfo.volcanicEruptionAreaTemplate 이 가능하다.)
             * 
             * ex)
             *  
             *  GeneratorInfo[11].onceAreaTemplate
             *  ActionListInfo[1].ActionInfos[0].AreaTemplate
             *  AbilityInfo[@].LevelValues[@]
             */

            if (string.IsNullOrEmpty(expression.Value) == true || expression.IsOperator() == true)
                return null;

            string[] references = expression.Value.Split('.');
            int infoId = 0;
            int referenceIndex = 0;
            object suspect = null;
            foreach (string reference in references)
            {
                int openBracketIndex = reference.IndexOf('[');
                string key = null;
                string keyword = string.Empty;
                if (openBracketIndex >= 0)
                {
                    keyword = reference.Substring(0, openBracketIndex);
                    key = reference.Substring(openBracketIndex + 1, reference.Length - openBracketIndex - 2);
                }
                else
                {
                    keyword = reference;
                }

                if (referenceIndex == 0) // 첫번째 참조는 Info.
                {
                    if (key == "@") // Root Info의 Id 참조.
                    {
                        if (rootInfo is IdKeyData integerIdKeyData)
                        {
                            key = integerIdKeyData.Id.ToString();
                        }
                    }
                    suspect = GetInfo(keyword, key);
                }
                else
                {
                    if (suspect == null)
                    {
                        return null;
                    }

                    suspect = GetMemberValue(suspect, keyword, key);
                }

                if (suspect == null)
                {
                    return null;
                }

                _pathValues.Add(suspect);
                ++referenceIndex;
            }

            return suspect;
        }

        private static object GetInfo(string keyword, string key)
        {
            IKeyValueInfo keyValueInfo = null;
            switch (keyword)
            {
                case nameof(SkillInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.SkillInfo;
                    break;
                }
                case nameof(GeneratorInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.GeneratorInfo;
                    break;
                }
                case nameof(AreaInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.AreaInfo;
                    break;
                }
                case nameof(ProjectileInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.ProjectileInfo;
                    break;
                }
                case nameof(BuffInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.BuffInfo;
                    break;
                }
                case nameof(RuneInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.RuneInfo;
                    break;
                }
                case nameof(CharacterRankAbilityGroupInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.CharacterRankAbilityGroupInfo;
                    break;
                }
                case nameof(WeaponReforgeInfo):
                {
                    keyValueInfo = StaticInfo.StaticInfo.Instance.WeaponReforgeInfo;
                    break;
                }
                case nameof(ArmorSetInfo):
                    {
                        keyValueInfo = StaticInfo.StaticInfo.Instance.ArmorSetInfo;
                        break;
                    }
                case nameof(ActionGroupInfo):
                    {
                        keyValueInfo = StaticInfo.StaticInfo.Instance.ActionGroupInfo;
                        break;
                    }
                case nameof(ActionContainerListInfo):
                    {
                        keyValueInfo = StaticInfo.StaticInfo.Instance.ActionContainerListInfo;
                        break;
                    }
                case nameof(BarrierInfo):
                    {
                        keyValueInfo = StaticInfo.StaticInfo.Instance.BarrierInfo;
                        break;
                    }
                case nameof(EpisodeRuleInfo):
                    {
                        keyValueInfo = StaticInfo.StaticInfo.Instance.EpisodeRuleInfo;
                        break;
                    }
                default:
                {
                    _logger.Error($"Unknown Stat path keyword : {keyword}");
                    break;
                }
            }

            if (keyValueInfo != null && string.IsNullOrEmpty(key) == false)
            {
                return keyValueInfo.GetRaw(int.Parse(key));
            }

            return null;
        }

        private object GetMemberValue(object suspect, string keyword, string key)
        {
            if (suspect == null)
                return null;

            Type suspectType = suspect.GetType();
            var fieldInfo = suspectType.GetField(keyword, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                object fieldValue = fieldInfo.GetValue(suspect);
                if (fieldValue == null)
                {
                    _logger.Error($"Keyword Target is null : {keyword}");
                    return null;
                }

                return GetMemberValueWithKey(fieldValue, keyword, key);
            }

            var propertyInfo = suspectType.GetProperty(keyword, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null)
            {
                object propertyValue = propertyInfo.GetValue(suspect);
                if (propertyValue == null)
                {
                    _logger.Error($"Keyword Target is null : {keyword}");
                    return null;
                }

                return GetMemberValueWithKey(propertyValue, keyword, key);
            }

            _logger.Error($"Unknown Stat path keyword : {keyword}");
            return null;
        }

        private object GetMemberValueWithKey(object value, string keyword, string key)
        {
            if (value is IList listFieldValue)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return value;
                }

                if (int.TryParse(key, out int index) == false)
                {
                    _logger.Error($"List index parse failed, {keyword} : {key}");
                    return null;
                }

                if (index < 0 || index >= listFieldValue.Count)
                {
                    _logger.Error($"List Index error : {keyword}, {index}");
                    return null;
                }

                return listFieldValue[index];
            }

            if (value is IDictionary dictionaryValue)
            {
                // 현재 int key 만 지원.

                int intKey = 0;
                if (key == "Level")
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        return value;
                    }

                    if (_paramDic.TryGetValue(key, out object levelValue) == false)
                    {
                        _logger.Error($"[Level] param not provided, {keyword} : {key}");
                        return null;
                    }

                    intKey = (int) levelValue;
                }
                else if (int.TryParse(key, out intKey) == false)
                {
                    _logger.Error($"Dictionary key parse failed, {keyword} : {key}");
                    return null;
                }

                if (dictionaryValue.Contains(intKey) == false)
                {
                    _logger.Error($"Dictionary key not found : {keyword}, {key}");
                    return null;
                }

                return dictionaryValue[intKey];
            }

            return value;
        }
    }
}