#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GmChs
{
    /// <summary>
    /// Gesture Manager 汉化补丁的一键安装脚本（兜底方案）。
    /// 放到工程的 Assets/Editor 下，菜单：Tools → Gesture Manager 汉化 → 安装/还原
    /// </summary>
    public static class GmChsInstaller
    {
        private const string PkgRoot = "Packages/vrchat.blackstartx.gesture-manager";
        private const string AstRoot = "Assets/GestureManager";

        private static readonly string[] Exts = { ".cs", ".meta", ".asmdef" };

        [MenuItem("Tools/Gesture Manager 汉化/安装汉化（选择 Scripts 文件夹）", false, 100)]
        public static void Install()
        {
            var src = EditorUtility.OpenFolderPanel("选择解压出来的汉化 Scripts 文件夹", "", "Scripts");
            if (string.IsNullOrEmpty(src)) return;

            var hasEditor = Directory.Exists(Path.Combine(src, "Editor"));
            var hasRuntime = Directory.Exists(Path.Combine(src, "Runtime"));
            if (!hasEditor && !hasRuntime)
            {
                EditorUtility.DisplayDialog("路径不对",
                    "所选文件夹里没有 Editor / Runtime 子目录。\n\n请选中解压出来的那个 Scripts 文件夹本身。", "知道了");
                return;
            }

            var dst = FindScriptsDir();
            if (dst == null)
            {
                EditorUtility.DisplayDialog("没找到 Gesture Manager",
                    "工程里既没有：\n" + PkgRoot + "\n也没有：\n" + AstRoot +
                    "\n\n请先安装官方 Gesture Manager 3.9.9。", "知道了");
                return;
            }

            var backup = BackupPath();
            if (!EditorUtility.DisplayDialog("确认覆盖",
                    "汉化源：\n" + src + "\n\n目标：\n" + dst + "\n\n" +
                    "覆盖前会先把原文件备份到：\n" + backup + "\n\n是否继续？", "开始安装", "取消"))
                return;

            try
            {
                CopyTree(dst, backup);
                CopyTree(src, dst);
            }
            catch (Exception e)
            {
                Debug.LogError("[GM汉化] 安装失败：" + e.Message);
                EditorUtility.DisplayDialog("安装失败", e.Message, "知道了");
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log("[GM汉化] 安装完成，等待 Unity 重新编译。备份位置：" + backup);
            EditorUtility.DisplayDialog("完成",
                "汉化文件已覆盖，Unity 正在重新编译。\n\n备份位置：\n" + backup, "好的");
        }

        [MenuItem("Tools/Gesture Manager 汉化/还原英文（选择备份或原版 Scripts 文件夹）", false, 101)]
        public static void Restore()
        {
            var src = EditorUtility.OpenFolderPanel("选择英文原版 Scripts 文件夹（zip 里的 _英文原版备份/Scripts）", "", "Scripts");
            if (string.IsNullOrEmpty(src)) return;
            if (!Directory.Exists(Path.Combine(src, "Editor")) && !Directory.Exists(Path.Combine(src, "Runtime")))
            {
                EditorUtility.DisplayDialog("路径不对", "所选文件夹里没有 Editor / Runtime 子目录。", "知道了");
                return;
            }

            var dst = FindScriptsDir();
            if (dst == null)
            {
                EditorUtility.DisplayDialog("没找到 Gesture Manager", "工程里没有找到 Gesture Manager。", "知道了");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认还原", "把\n" + src + "\n覆盖到\n" + dst + "\n\n是否继续？", "开始还原", "取消"))
                return;

            try
            {
                CopyTree(src, dst);
            }
            catch (Exception e)
            {
                Debug.LogError("[GM汉化] 还原失败：" + e.Message);
                EditorUtility.DisplayDialog("还原失败", e.Message, "知道了");
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log("[GM汉化] 已还原为英文原版。");
            EditorUtility.DisplayDialog("完成", "已还原为英文原版。", "好的");
        }

        [MenuItem("Tools/Gesture Manager 汉化/当前插件位置", false, 200)]
        public static void Where()
        {
            var dst = FindScriptsDir();
            EditorUtility.DisplayDialog("Gesture Manager 位置",
                dst ?? "未找到 Gesture Manager。", "知道了");
        }

        private static string FindScriptsDir()
        {
            if (Directory.Exists(PkgRoot)) return Path.Combine(PkgRoot, "Scripts");
            if (Directory.Exists(AstRoot)) return Path.Combine(AstRoot, "Scripts");
            return null;
        }

        private static string BackupPath()
        {
            // 备份放到工程目录之外，避免 Unity 编译两份同名脚本
            var root = Directory.GetParent(Application.dataPath);
            var basePath = root != null ? root.Parent?.FullName ?? root.FullName : Application.dataPath;
            return Path.Combine(basePath, "GestureManager-Scripts备份-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        }

        private static void CopyTree(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var file in Directory.GetFiles(src))
            {
                if (Array.IndexOf(Exts, Path.GetExtension(file).ToLower()) < 0) continue;
                Directory.CreateDirectory(dst);
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), true);
            }
            foreach (var dir in Directory.GetDirectories(src))
                CopyTree(dir, Path.Combine(dst, Path.GetFileName(dir)));
        }
    }
}
#endif
