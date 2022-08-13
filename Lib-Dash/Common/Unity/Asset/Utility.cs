#if Common_Unity
using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Common.Unity.Asset
{
    public static class Utility
    {
        public static string GetOsName()
        {
            RuntimePlatform platform = Application.platform;
            // TODO: 추후 Android에서 그래픽 타겟에 따른 번들 분리를 고려해야 한다.
            switch (platform)
            {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                    return "ios";
                case RuntimePlatform.WindowsPlayer:
                //case RuntimePlatform.WindowsEditor:
                    return "windows";
                default:
                    return "android";
            }
        }

        public static string GetOsNameBuildTarget()
        {
#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.iOS:
                    return "ios";
                case UnityEditor.BuildTarget.StandaloneWindows:
                    return "windows";
                default:
                    return "android";
            }
#else
            return GetOsName();
#endif
        }

        public static string GetIoErrorMessage(Exception ex)
        {
            const int ERROR_HANDLE_DISK_FULL = 0x27;
            const int ERROR_DISK_FULL = 0x70;

            int win32ErrorCode = Marshal.GetHRForException(ex) & 0xFFFF;
            if (win32ErrorCode == ERROR_HANDLE_DISK_FULL || win32ErrorCode == ERROR_DISK_FULL)
            {
                return "Not enough disk space.";
            }

            return $"Unknown Error : {win32ErrorCode.ToString()}";
        }
    }
}
#endif