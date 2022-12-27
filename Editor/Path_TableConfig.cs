using NCore.Editor;
using UnityEditor;
using UnityEngine;

namespace Table.Editor
{
    public class Path_TableConfig : IPathConfig, IEditorPrefs
    {
        private static string default_excel_folder => System.IO.Path.GetFullPath(Application.dataPath + "/../../Table");
        private static string default_gen_asset_folder => $"{Application.dataPath}/BuildBundle/Asset/Table";

        private static string default_gen_csharp_folder => $"{Application.dataPath}/Scripts/Table/Generate";

        private static string default_gen_csharp_assembly_name = "Assembly-CSharp";

        #region 设置上的字段菜单

        [SettingProperty(FieldType.Folder,"Excel 目录: ")]
        public static string ExcelFolder
        {
            get { return EditorPrefsHelper.GetString("Path_TableConfig_ExcelFolder", default_excel_folder); }
            set => EditorPrefsHelper.SetString("Path_TableConfig_ExcelFolder", value);
        }

        [SettingProperty(FieldType.Folder,"Gen Asset目录: ")]
        public static string GenAssetFolder
        {
            get
            {
                return EditorPrefsHelper.GetString("Path_TableConfig_GenAssetFolder", default_gen_asset_folder);
            }
            set => EditorPrefsHelper.SetString("Path_TableConfig_GenAssetFolder", value);
        }
        
        [SettingProperty(FieldType.Folder,"Gen C#目录: ")]
        public static string GenCSharpFolder
        {
            get
            {
                return EditorPrefsHelper.GetString("Path_TableConfig_GenCSharpFolder", default_gen_csharp_folder);
            }
            set => EditorPrefsHelper.SetString("Path_TableConfig_GenCSharpFolder", value);
        }

        [SettingProperty(FieldType.EditField,"Gen C#所在程序集：")]
        public static string TableSharpAssemblyName
        {
            get
            {
                return EditorPrefsHelper.GetString("Path_TableConfig_GenCSharpAssemblyName", default_gen_csharp_assembly_name);
            }
            set => EditorPrefsHelper.SetString("Path_TableConfig_GenCSharpAssemblyName", value);
        }

        [SettingMethod("", "打开导表工具")]
        public static void OpenConvertTableTool()
        {
            ConvertSettingWnd.SettingConvertExcel();
        }
        #endregion

        #region IPathConfig,IEditorPrefs
        public const string TAG = "导表配置";

        public string GetModuleName()
        {
            return TAG;
        }

        public void ReleaseEditorPrefs()
        {
            EditorPrefs.DeleteKey("Path_TableConfig_ExcelFolder");
            EditorPrefs.DeleteKey("Path_TableConfig_GenAssetFolder");
            EditorPrefs.DeleteKey("Path_TableConfig_GenCSharpFolder");
        }

        #endregion
    }
}