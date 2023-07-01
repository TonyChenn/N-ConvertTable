using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Text;

public class ConvertTable
{

    public static void GenCSharp(List<SettingItem> list)
    {
        if (Directory.Exists(Path_TableConfig.ExcelFolder))
        {
            if (!Directory.Exists(Path_TableConfig.GenCSharpFolder))
                Directory.CreateDirectory(Path_TableConfig.GenCSharpFolder);
            
            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach (var item in list)
            {
                GenCSharp(item);
            }
            AssetDatabase.Refresh();
            watch.Stop();
            int seconds = (int)watch.ElapsedMilliseconds / 1000;
            EditorUtility.DisplayDialog("提示", $"生成CSharp,耗时{seconds}秒", "好的");
            AssetDatabase.Refresh();
        }
        else
        {
            if (EditorUtility.DisplayDialog("提示", "配表路径有问题，请检查！！", "这就去"))
            {
                PackageWnd.ShowWnd("2.设置");
            }
        }
    }

    public static void GenCSharp(SettingItem item)
    {
        if (Directory.Exists(Path_TableConfig.ExcelFolder))
        {
            ExcelData data = GetTableData(item);
            string str_csharp = GetCSharpString(data);
            string path = $"{Path_TableConfig.GenCSharpFolder}/Config_{data.tableName}.cs";
            if (!Directory.Exists(Path_TableConfig.GenCSharpFolder))
                Directory.CreateDirectory(Path_TableConfig.GenCSharpFolder);
                    
            File.WriteAllText(path, str_csharp);
        }
    }
    
    /// <summary>
    /// 获取表格数据
    /// </summary>
    public static ExcelData GetTableData(SettingItem item)
    {
        return GetTableData(item.Name);
    }
    public static ExcelData GetTableData(string itemName)
    {
        var decoder = new DecodeExcel();
        return decoder.Decode($"{Path_TableConfig.ExcelFolder}/{itemName}");
    }
    static string GetCSharpString(ExcelData excelData)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < excelData._fieldNameList.Count; i++)
        {
            sb.AppendLine("\t/// <summary>");
            sb.Append("\t/// " + excelData._fieldNameList[i]);
            if (!string.IsNullOrEmpty(excelData._commentList[i]))
                sb.AppendFormat("({0})", excelData._commentList[i]);
            sb.AppendLine();
            sb.AppendLine("\t/// <summary>");
            sb.Append("\tpublic ");
            switch (excelData._typeNameList[i].ToLower())
            {
                case "byte"     :sb.Append("short ");  break;
                case "string"   : sb.Append("string "); break;
                case "bool"     : sb.Append("bool ");   break;
                case "int"      :
                case "int32"    : sb.Append("int ");    break;
				case "uint"		:
                case "uint32"   : sb.Append("uint ");   break;
                case "int64"    :
                case "long"     : sb.Append("long ");   break;
                case "datetime" : sb.Append("DateTime ");break;
            }
            sb.Append(excelData._fieldNameList[i]);
            sb.AppendLine(";");
        }

        var builder = new StringBuilder();
        builder.Append(String_Config_Template);
        builder.Replace("{FILE_NAME}", excelData.tableName);
        builder.Replace("{ITEM_CLASS_VARIABLE}", sb.ToString());
        builder.Replace("{ASSET_PATH}", Path_TableConfig.GenAssetFolder.Replace(Application.dataPath, ""));
        string result = builder.ToString();
        
        return result;
    }
    
    /// <summary>
    /// Excel转CSharp脚本模板
    /// </summary>
    public const string String_Config_Template = @"/// <summary>
/// 本文件中的代码为生成的代码，不允许手动修改
/// Generate by TonyChenn @
/// </summary>

using System;
using UnityEngine;

[Serializable]
public partial class Item_{FILE_NAME}
{
{ITEM_CLASS_VARIABLE}
}

public partial class Config_{FILE_NAME} : ScriptableObject
{
    private static Config_{FILE_NAME} _instence = null;
    public Item_{FILE_NAME}[] Array;
    public static Config_{FILE_NAME} Singleton
    {
        get
        {
            if (_instence == null) Init();
            return _instence;
        }
    }

    private static void Init()
    {
#if UNITY_EDITOR
        if (GameConfig.UseLocalScript)
            LoadFromLocal();
        else
            LoadFromBundle();
#else
            LoadFromBundle();
#endif
    }

    private static void LoadFromBundle()
    {
		string path = $""{Application.streamingAssetsPath}/asset/table/{FILE_NAME}.u"";
		if(GameConfig.PlayMode == PlayMode.HostMode)
		{
			string temp = $""{Application.persistentDataPath}/asset/table/{FILE_NAME}.u"";
			if (System.IO.File.Exists(temp))
			{
				path = temp;
			}
		}
		AssetBundle bundle = AssetBundle.LoadFromFile(path);
		_instence = bundle.LoadAsset<Config_{FILE_NAME}>(""asset/table/{FILE_NAME}.u"");
    }

#if UNITY_EDITOR
    private static void LoadFromLocal()
	{
		string path = ""Assets{ASSET_PATH}/{FILE_NAME}.asset"";
		var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Config_{FILE_NAME}>(path);
		_instence = obj;
	}
#endif
}";
}
