using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlackStartX.GestureManager.Editor.Library;
using BlackStartX.GestureManager.Editor.Modules;
using UnityEditor;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Data
{
    public static class GestureManagerSettings
    {
        private const string LocalFolder = "LocalAvatarData";

        private static IEnumerable<string> Users(string directory) => Directory.Exists(directory) ? Directory.GetDirectories(directory).Select(Path.GetFileName) : new string[] { };
        private static string UserPath(string folder, string user) => user == null ? null : PathCombine(VrcDirectory, folder, user);
        private static string UserID(int index, IReadOnlyList<string> users) => index < users.Count ? users[index] : null;
        public static string VrcDirectory => PathCombine(ModuleHelper.LocalLowPath, "VRChat", "VRChat");
        private static string[] Choose => new[] { "[未设置]" }.Concat(Users(DataDirectory)).ToArray();
        public static string UserID(int index) => index < 1 ? null : UserID(index, Choose);
        public static string UserPath(int index) => UserPath(LocalFolder, UserID(index));
        private static string DataDirectory => PathCombine(VrcDirectory, LocalFolder);

        private static string PathCombine(string path1, string path2, string path3) => path1 == null ? null : Path.Combine(path1, path2, path3);
        private static string PathCombine(string path1, string path2) => path1 == null ? null : Path.Combine(path1, path2);

        private static bool _generalSettings;
        private static bool _simulationSettings;

        public static void SettingGui(GestureManager manager)
        {
            GUILayout.Label("在这里你可以自定义本工具的设置！", GestureManagerStyles.MiddleStyle);
            GUILayout.Space(10);

            using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError)) GeneralSettings(manager);
            using (new GUILayout.VerticalScope(GestureManagerStyles.EmoteError)) SimulationSettings(manager);
        }

        private static void GeneralSettings(GestureManager manager)
        {
            if (!GmgLayoutHelper.FoldoutSection("常规设置", ref _generalSettings)) return;
            GUILayout.Label("进入播放模式时自动选择哪个模型？", GestureManagerStyles.SettingsText);
            manager.settings.favourite = GmgLayoutHelper.ComponentField("常用模型: ", manager.settings.favourite, manager);
            GUILayout.Label("用户数据", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("该文件夹将用于读取本地的模型信息！", GestureManagerStyles.SettingsText);
            manager.settings.userIndex = GmgLayoutHelper.Popup("用户 ID: ", manager.settings.userIndex, Choose, manager);
            GUILayout.Label("编辑器设置", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("Unity 编辑器设置的快捷开关！", GestureManagerStyles.SettingsText);
            using (new GUILayout.HorizontalScope()) BlendShapeSettings();
        }

        private static void SimulationSettings(GestureManager manager)
        {
            if (!GmgLayoutHelper.FoldoutSection("模拟设置", ref _simulationSettings)) return;
            GUILayout.Label("是否加载本地已存储的参数值？", GestureManagerStyles.SettingsText);
            using (new GUILayout.HorizontalScope()) LoadParametersSettings(manager);
            GUILayout.Label("初始姿势", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("设置模型的初始姿势！", GestureManagerStyles.SettingsText);
            manager.settings.initialPose = GmgLayoutHelper.EnumPopup("初始姿势: ", manager.settings.initialPose, manager);
            GUILayout.Label("默认参数", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("设置默认参数的初始状态！", GestureManagerStyles.SettingsText);
            using (new GUILayout.HorizontalScope()) DefaultParametersSettings(manager);
            manager.settings.isOnFriendsList = GmgLayoutHelper.Toggle("好友列表内: ", manager.settings.isOnFriendsList, manager);
            GUILayout.Label("模型剔除", GestureManagerStyles.ToolSubHeader);
            GUILayout.Label("根据摄像机距离模拟 VRChat 的模型剔除行为。", GestureManagerStyles.SettingsText);
            using (new GUILayout.HorizontalScope()) AvatarCullingSettings(manager);
        }

        private static void AvatarCullingSettings(GestureManager manager, string label = null, float leftValue = 1f, float rightValue = 10f)
        {
            manager.settings.simulateCulling = GmgLayoutHelper.Toggle("模拟剔除: ", manager.settings.simulateCulling, manager);
            using (new GmgLayoutHelper.GuiEnabled(manager.settings.simulateCulling)) manager.settings.cullingDistance = GmgLayoutHelper.Slider(label, manager.settings.cullingDistance, leftValue, rightValue, manager);
        }

        private static void BlendShapeSettings(string label = "混合形状钳制: ")
        {
            PlayerSettings.legacyClampBlendShapeWeights = EditorGUILayout.Toggle(label, PlayerSettings.legacyClampBlendShapeWeights);
            if (PlayerSettings.legacyClampBlendShapeWeights) return;
            GUILayout.FlexibleSpace();
            using (new GmgLayoutHelper.GuiContent(Color.yellow)) GUILayout.Label("[建议保持开启]");
        }

        private static void LoadParametersSettings(GestureManager manager)
        {
            using (new GmgLayoutHelper.GuiEnabled(manager.settings.userIndex != 0))
            {
                var buttonString = GUI.enabled ? "打开缓存文件夹" : "未选择用户！";

                if (!GUI.enabled) GmgLayoutHelper.Toggle("加载已存储参数: ", GUI.enabled, manager);
                else manager.settings.loadStored = GmgLayoutHelper.Toggle("加载已存储参数: ", manager.settings.loadStored, manager);

                if (GUILayout.Button(buttonString)) EditorUtility.RevealInFinder(UserPath(manager.settings.userIndex));
            }
        }

        private static void DefaultParametersSettings(GestureManager manager)
        {
            manager.settings.isRemote = !GmgLayoutHelper.Toggle("本地模式: ", !manager.settings.isRemote, manager);
            GUILayout.FlexibleSpace();
            manager.settings.vrMode = GmgLayoutHelper.Toggle("VR 模式: ", manager.settings.vrMode, manager);
        }
    }
}