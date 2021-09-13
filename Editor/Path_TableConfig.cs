using SettingKit;
using SettingKit.Editor;
using SFramework.Editor;
using UnityEditor;
using UnityEngine;

namespace Table.Editor
{
    public class Path_TableConfig : IPathConfig, IEditorPrefs
    {
        #region 设置上的字段菜单

        [Setting(FieldType.Folder,"Excel 目录: ")]
        public static string ExcelFolder
        {
            get { return EditorPrefsHelper.GetString("Path_TableConfig_ExcelFolder", Application.dataPath); }
            set => EditorPrefsHelper.SetString("Path_TableConfig_ExcelFolder", value);
        }

        [Setting(FieldType.Folder,"Gen Asset目录: ")]
        public static string GenAssetFolder
        {
            get
            {
                return EditorPrefsHelper.GetString("Path_TableConfig_GenAssetFolder",
                    Application.dataPath + "/Asset/Table");
            }
            set => EditorPrefsHelper.SetString("Path_TableConfig_GenAssetFolder", value);
        }
        
        [Setting(FieldType.Folder,"Gen CSharp目录: ")]
        public static string GenCSharpFolder
        {
            get
            {
                return EditorPrefsHelper.GetString("Path_TableConfig_GenCSharpFolder",
                    Application.dataPath + "/Scripts/Table/Define/");
            }
            set => EditorPrefsHelper.SetString("Path_TableConfig_GenCSharpFolder", value);
        }

        #endregion

        #region IPathConfig,IEditorPrefs
        public string GetModuleName()
        {
            return "导表配置";
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