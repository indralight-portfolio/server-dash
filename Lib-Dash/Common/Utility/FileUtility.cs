using System.IO;
using System.Linq;
using System.Collections.Generic;
using Common.Log;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Common.Utility
{
    public static class FileUtility
    {
        public static bool CopyFolder(string sourcePath, string targetPath)
        {
            if (Directory.Exists(targetPath) == false)
            {
                Directory.CreateDirectory(targetPath);
            }

            if (!Directory.Exists(sourcePath))
                return false;

            string[] files = Directory.GetFiles(sourcePath);
            foreach (string i in files)
            {
                string fileName = Path.GetFileName(i);
                string destFile = Path.Combine(targetPath, fileName);
                //Logger.Info($"Copy File. From : {i}, To : {destFile}");
                File.Copy(i, destFile, true);
            }

            return true;
        }

        public static bool CopyFolderRecursive(string sourcePath, string targetPath, bool setNoBackupInIos = false)
        {
            if (Directory.Exists(targetPath) == false)
            {
                Directory.CreateDirectory(targetPath);
            }

            if (!Directory.Exists(sourcePath))
                return false;

            string[] files = Directory.GetFiles(sourcePath);
            foreach (string i in files)
            {
                string fileName = Path.GetFileName(i);
                string destFile = Path.Combine(targetPath, fileName);
                //Logger.Info($"Copy File. From : {i}, To : {destFile}");
                File.Copy(i, destFile, true);

                if (setNoBackupInIos)
                {
#if UNITY_IOS
                    UnityEngine.iOS.Device.SetNoBackupFlag(destFile);
#endif
                }
            }

            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                CopyFolderRecursive(dir.FullName, Path.Combine(targetPath, dir.Name), setNoBackupInIos);
            }

            return true;
        }

        public static string ReadAllText(string path)
        {
            return StaticInfo.Reader.TextReader<string>.ReadContents(path);
        }

#if Common_Unity

        public static T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            return UnityEngine.Resources.Load<T>(path);
        }

        private static List<string> GetFilePathsFromPatternClient(string rootPath, string path, string[] exts)
        {
            List<string> paths = new List<string>();
            if (path.Contains("*") == false && exts.Length == 1)
            {
                paths.AddRange(exts.Select(ext => $"{rootPath}{path}{ext}"));
                return paths;
            }

            paths.AddRange(Unity.Asset.AssetManager.Instance.GetMatchPaths(rootPath, path, exts));

            return paths;
        }
#endif

        public static List<string> GetFilePathsFromPattern(string rootPath, string path, string[] exts)
        {
            // 패턴으로 할 경우 *? 등은 반드시 마지막 경로에 있어야 한다.
#if Common_Unity
            return GetFilePathsFromPatternClient(rootPath, path, exts);
#endif
            List<string> paths = new List<string>();
            if (path.Contains("*") == false && exts.Length == 1)
            {
                paths.AddRange(exts.Select(ext => $"{rootPath}{path}{ext}"));
                return paths;
            }

            string dirName = Path.GetDirectoryName(rootPath + "/");
            int idx = path.IndexOf('/');
            string subpath = idx >= 0 ? path.Substring(0, path.LastIndexOf('/')) + "/" : "/";
            string pattern = idx == 0 ? path.Remove(0, 1) : path;
            pattern = exts.Length == 1 ? pattern + exts[0] : pattern + ".*";

            DirectoryInfo directory = new DirectoryInfo(dirName);
            FileInfo[] files = directory.GetFiles(pattern);
            files = files.Where(f => f.Extension != ".meta").Where(f => exts.Length == 0 || exts.Contains(f.Extension)).ToArray();
            paths.AddRange(files.Select(f => $"{rootPath}{subpath}{f.Name}"));
            return paths;
        }

        public static bool FitsMask(string fileName, string fileMask)
        {
            Regex mask = new Regex(
                "^.*" +
                fileMask
                    .Replace(".", "[.]")
                    .Replace("*", ".*")
                    .Replace("?", ".")
                + '$',
                RegexOptions.IgnoreCase);
            return mask.IsMatch(fileName);
        }

        public static bool FilterExtensions(string path, string[] extensions)
        {
            if (extensions.Length == 0) return true;
            bool result = false;
            foreach (var extension in extensions)
            {
                result |= path.EndsWith(extension, System.StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }
    }
}