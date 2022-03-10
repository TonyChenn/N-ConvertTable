using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Table.Editor;
using UnityEditor;
using UnityEngine;

public class ConvertSettingWnd : EditorWindow
{
    public const string SettingFilePath = "/NextFramework/Modules/Table/Editor/setting.csv";

    List<SettingItem> settingItemList;
    Vector2 scrollPos;
    string csvConfigPath;
    GUIStyle TitleStyle;
    GUIStyle GreenButtonStyle;

    [MenuItem("Tools/转表工具 &c")]
    public static void SettingConvertExcel()
    {
        GetWindow<ConvertSettingWnd>(false, "转换配表设置", true);
    }

    private void Awake()
    {
        csvConfigPath = Application.dataPath + SettingFilePath;
        
        TitleStyle = new GUIStyle();
        TitleStyle.normal.textColor=Color.red;
        TitleStyle.fontStyle = FontStyle.Bold;

        GreenButtonStyle = new GUIStyle();
        GreenButtonStyle.normal.textColor=Color.green;
    }
    private void OnGUI()
    {
        // 配置目录
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新目录", GUILayout.Width(90)))
        {
            Init();
        }

        if (GUILayout.Button("打开设置",GUILayout.Width(90)))
        {
            // TODO 这里传递字符不合理，但是Path_TableConfig的GetModuleName() 不是静态方法 待解决
            // 实例化个对象也不科学
            
            SettingWnd.ShowWindow("导表配置");
        }
        EditorGUILayout.EndHorizontal();

        bool allToAsset = false;

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("选择", TitleStyle, GUILayout.Width(40));
        GUILayout.Label("配表名称", TitleStyle, GUILayout.Width(249));
        GUILayout.Label("To Asset", TitleStyle, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (settingItemList == null) Init();

        for (int i = 0; i < settingItemList.Count; i++)
        {
            SettingItem item = settingItemList[i];

            GUILayout.BeginHorizontal();
            item.Selected = GUILayout.Toggle(item.Selected, "", GUILayout.Width(40));
            GUILayout.Label(item.Name, GUILayout.Width(249));
            item.ToAsset = GUILayout.Toggle(item.ToAsset, "", GUILayout.Width(80));
            if (allToAsset) item.ToAsset = true;
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("转换选中", GUILayout.Height(50)))
        {
            var list = new List<SettingItem>();
            foreach (var item in settingItemList)
            {
                if (item.Selected)
                    list.Add(item);
            }
            if (list.Count > 0)
                ConvertTable.GenCSharp(list);
            else
                EditorUtility.DisplayDialog("提示", "请至少勾选一项", "好吧");
        }
        if (GUILayout.Button("转换全部",GUILayout.Height(50)))
        {
            if (settingItemList.Count > 0)
                ConvertTable.GenCSharp(settingItemList);

            //WriteSetting();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("导入选中数据", GUILayout.Height(50)))
        {
            var list = new List<SettingItem>();
            foreach (var item in settingItemList)
            {
                if (item.Selected && item.ToAsset)
                    list.Add(item);
            }
            if (list.Count > 0)
            {
                ImportData(list);
            }
            else
                EditorUtility.DisplayDialog("提示", "请至少勾选一项", "好吧");
        }
        if (GUILayout.Button("导入所有数据", GUILayout.Height(50)))
        {
            var list = new List<SettingItem>();
            foreach (var item in settingItemList)
            {
                if (item.ToAsset)
                    list.Add(item);
            }
            ImportData(list);
        }
        EditorGUILayout.EndHorizontal();
    }

    public void Init()
    {
        settingItemList = new List<SettingItem>();
        if (Directory.Exists(Path_TableConfig.ExcelFolder))
        {
            DirectoryInfo folderInfo = new DirectoryInfo(Path_TableConfig.ExcelFolder);
            FileInfo[] fileInfoArray = folderInfo.GetFiles("*.xlsx");
            foreach (var file in fileInfoArray)
            {
                var item = new SettingItem(Path_TableConfig.ExcelFolder, file.Name);
                settingItemList.Add(item);
            }
        }
    }

    #region Write ScriptableObject and import Data
    void ImportData(List<SettingItem> list)
    {
        if (list == null || list.Count == 0) return;

        ExcelData data = null;
        for (int i = 0; i < list.Count; i++)
        {
            data = ConvertTable.GetTableData(list[i]);
            ConvertToAsset(list[i], data);
            //Type type = Type.GetType("Item_" + data.tableName);
            //var config = ScriptableObject.CreateInstance(type);
            //config.
        }
    }
    void ConvertToAsset(SettingItem item, ExcelData excelData)
    {
        Assembly assembly = Assembly.Load("Assembly-CSharp");
        Type dbType = assembly.GetType(item.ConfigClassName);
        if (dbType == null)
        {
            Debug.LogError("找不到" + item.ConfigClassName + "类");
            return;
        }

        if (!Directory.Exists(Path_TableConfig.GenCSharpFolder))
            Directory.CreateDirectory(Path_TableConfig.GenCSharpFolder);
        
        StringBuilder builder = new StringBuilder("Assets");
        builder.Append(Path_TableConfig.GenAssetFolder.Substring(Application.dataPath.Length));
        builder.Append("/");
        builder.Append(item.NameWithoutExtension);
        builder.Append(".asset");
        string assetPath = builder.ToString();
        var asset = AssetDatabase.LoadAssetAtPath(assetPath, dbType);
        if (asset == null)
        {
            var obj = ScriptableObject.CreateInstance(item.ConfigClassName);

            AssetDatabase.CreateAsset(obj, assetPath);
            asset = AssetDatabase.LoadAssetAtPath(assetPath, dbType);
        }
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        //写数据
        //变量名
        List<string> fieldList = excelData._fieldNameList;

        //数据类型
        List<string> typeList = excelData._typeNameList;

        Type itemType = assembly.GetType(item.ItemClassName);
        ConstructorInfo constructor = itemType.GetConstructor(Type.EmptyTypes);
        ArrayList dataObj = new ArrayList();
        for (int i = 0, iMax = excelData._valueList.Count; i < iMax; i++)
        {
            string[] line = excelData._valueList[i];
            var itemObj = constructor.Invoke(null);

            for (int j = 0, jMax = typeList.Count; j < jMax; j++)
            {
                var field = itemType.GetField(fieldList[j]);
                if (field == null)
                {
                    Debug.LogError(string.Format("找不到{0}类中{1}属性", item.ItemClassName, fieldList[j]));
                    return;
                }
                object valueObj = GetCSharpValue(line[j], field.FieldType);
                field.SetValue(itemObj, valueObj);
            }
            dataObj.Add(itemObj);
        }
        FieldInfo info = dbType.GetField("Array");
        info.SetValue(asset, dataObj.ToArray(itemType));
    }

    protected static object GetCSharpValue(string srcValue, Type type)
    {
        object target;

        switch (type.ToString())
        {
            // ulong
            case "System.UInt64":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(ulong);
                else
                {
                    ulong ul;
                    if (!ulong.TryParse(srcValue, out ul))
                        throw new InvalidOperationException(srcValue + " is not a ulong.");
                    target = ul;
                }
                break;

            // int
            case "System.Int32":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(int);
                else
                {
                    int i;
                    if (!int.TryParse(srcValue, out i))
                        throw new InvalidOperationException(srcValue + " is not a int.");
                    target = i;
                }
                break;

            // uint
            case "System.UInt32":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(uint);
                else
                {
                    uint i;
                    if (!uint.TryParse(srcValue, out i))
                        throw new InvalidOperationException(srcValue + " is not a uint.");
                    target = i;
                }
                break;

            // ushort
            case "System.UInt16":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(ushort);
                else
                {
                    ushort b;
                    if (!ushort.TryParse(srcValue, out b))
                        throw new InvalidOperationException(string.Format("{0} is not a ushort", srcValue));
                    target = b;
                }
                break;

            // byte
            case "System.Byte":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(byte);
                else
                {
                    byte b;
                    if (!byte.TryParse(srcValue, out b))
                        throw new InvalidOperationException(srcValue + " is not a byte.");
                    target = b;
                }
                break;

            // float
            case "System.Single":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(float);
                else
                {
                    float f;
                    if (!float.TryParse(srcValue, out f))
                        throw new InvalidOperationException(srcValue + " is not a float.");
                    target = f;
                }
                break;

            // string
            case "System.String":
                target = srcValue.TrimStart('"').TrimEnd('"');
                break;

            // bool
            case "System.Boolean":
                if (string.IsNullOrEmpty(srcValue))
                    target = default(bool);
                else
                {
                    bool b;
                    if (!bool.TryParse(srcValue.ToLower(), out b))
                        throw new InvalidOperationException(srcValue + " is not a boolean.");
                    target = b;
                }
                break;

            default:
                Debug.Log(srcValue);
                target = srcValue;
                throw new InvalidOperationException("Unexpected c# type: " + type.ToString());
        }

        return target;
    }

    #endregion
}
public class SettingItem
{
    public string Name;
    public bool ToAsset;
    public bool Selected = false;   //only for single ExcelToSharp or load single Excel Data; (default:false)


    public string NameWithoutExtension { get { return Path.GetFileNameWithoutExtension(Name); } }

    public string ConfigClassName { get { return "Config_" + NameWithoutExtension; } }
    public string ItemClassName { get { return "Item_" + NameWithoutExtension; } }

    public SettingItem(string folder, string name)
    {
        if (string.IsNullOrEmpty(folder))
            throw new ArgumentException("folder is null");
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("name is null");

        this.Name = name;
    }
    public SettingItem(string folder, string name, bool toAsset) : this(folder, name)
    {
        ToAsset = toAsset;
    }
}
