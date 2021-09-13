using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AssetUtil
{
    public static void ConvertToAsset(string dbClassName,string folderPath,string assetName)
    {
        Assembly assembly = Assembly.Load("Assembly-CSharp");
        Type dbType = assembly.GetType(dbClassName);
        if (dbType == null)
            throw new InvalidOperationException(string.Format("c# class {0} is not exists.", dbClassName));

        string assetPath = string.Format("{0}{1}.asset", folderPath, assetName);
        var asset = AssetDatabase.LoadAssetAtPath(assetPath, dbType);
        if (asset == null)
        {
            var obj = ScriptableObject.CreateInstance(dbClassName);
            AssetDatabase.CreateAsset(obj, assetPath);
            asset = AssetDatabase.LoadAssetAtPath(assetPath, dbType);
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    public static void LoadData()
    {

    }
}
