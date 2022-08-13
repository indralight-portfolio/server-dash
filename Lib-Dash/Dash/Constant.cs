using System;
using System.Collections.Generic;
using Dash.Types;
using Unity.Mathematics;

namespace Dash
{
    public static class Constant
    {
        #region GamePlay
        public const float TurnFixedDeltaTime = 0.02f;
        public const long TurnFixedDeltaTimeMs = 20;
        public const int TurnPerSecond = 50;
        public static readonly float2[][] CharacterInitialPositions = new[]
        {
            new[] {new float2(0.0f, 0.25f),},
            new[] {new float2(-0.45f, 0.25f), new float2(0.45f, 0.25f)}, // x: 총 0.9 간격
            new[]
            {
                new float2(-0.65f, 0.25f), new float2(0.0f, 0.25f), new float2(0.65f, 0.25f)
            }, // x: 총 1.3 간격
        };

        public static float3 GetAngelPosition(StageSize stageSize, StageWidth stageWidth)
        {
            // tilesize * 5.5 = 2.75
            float x = (TileSize * (float)stageWidth) * 0.5f;
            float y = (TileSize * (float)stageSize) - 2.75f;
            return new float3(x, y, 0f);
        }

        public static float3 GetDevilPosition(StageSize stageSize, StageWidth stageWidth)
        {
            // tilesize * 1.0 = 0.5
            float x = (TileSize * (float)stageWidth) * 0.5f;
            float y = (TileSize * (float)stageSize) - 0.5f;
            return new float3(x, y, 0f);
        }

        public static float3 GetGoldChestPosition(StageSize stageSize, StageWidth stageWidth)
        {
            // tilesize * 1.5 = 0.75
            float x = (TileSize * (float)stageWidth) * 0.5f;
            float y = (TileSize * (float)stageSize) - 0.75f;
            return new float3(x, y, 0f);
        }

        public static float3 GetRewardChestTriggerPosition(StageSize stageSize, StageWidth stageWidth)
        {
            // tilesize * 1.5 = 0.75
            float x = (TileSize * (float)stageWidth) * 0.5f;
            float y = (TileSize * (float)stageSize) + 0.75f;
            return new float3(x, y, 0f);
        }

        public static float3[] GetRewardChestPositions(StageSize stageSize, StageWidth stageWidth, int count)
        {
            float x = (TileSize * (float)stageWidth) * 0.5f;
            float y = (TileSize * (float)stageSize);

            float3[] preset = _rewardChestPositionPreset[count];
            float3[] resultArray = new float3[preset.Length];
            for (int index = 0; index < preset.Length; ++index)
            {
                resultArray[index] = new float3(preset[index].x + x, preset[index].y + y, 0f);
            }

            return resultArray;
        }

        private static readonly float3[][] _rewardChestPositionPreset = new[]
        {
            // tilesize * 3 = 1.5
            new[]
            {
                new float3(0.0f, -1.5f, 0f)
            },
            new[]
            {
                new float3(TileSize * -1.5f, -1.5f, 0f),
                new float3(TileSize * 1.5f, -1.5f, 0f)
            },
            new[]
            {
                new float3(0.0f, -1.5f, 0f),
                new float3(TileSize * -2.5f, -1.5f, 0f),
                new float3(TileSize * 2.5f, -1.5f, 0f),
            },
        };

        public static readonly int DefaultAngle = -90;
        public static readonly int CharacterDefaultDirection = -90;
        public static readonly int MonsterDefaultDirection = 90;
        public const float TileSize = 0.5f;
        public const float TileHalfSize = TileSize * 0.5f;
        public const string PreviewTag = "Preview";

        private static readonly float2 _axis = new float2(-1f, 0f);
        public static ref readonly float2 Axis => ref _axis;
        private static readonly (StaticData.Vector2, int) _defaultTuple = (new StaticData.Vector2(), 0);
        public static ref readonly (StaticData.Vector2, int) DefaultTuple => ref _defaultTuple;
        public const float ReflectSearchRange = 2f;

        public const float TileTriggerTime = 0.5f;
        public const float RaycastOffset = 0.01f;

        public const int SYSTEM_SERIAL = -1;

        public const float CAMERA_Z = -10f;

        public const float MODEL_ROTATE_SPEED = 20f;
        #endregion

        public const string STAT_ATLAS = "atlas:stat";

#if UNITY_EDITOR
        public const string URL_EVENT_ACTION_INFO = "";
        public const string URL_PROJECTILES_TO_NEAR_TARGETS = "";
#endif// UNITY_EDITOR
    }
}