using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 解析Excel
/// </summary>
public class DecodeExcel
{
    /// <summary>
    /// 解析表格
    /// </summary>
    /// <param name="filePath">表格路径</param>
    /// <param name="_commentList">注释</param>
    /// <param name="_fieldNameList">变量名</param>
    /// <param name="_typeNameList">变量类型</param>
    /// <param name="_valueList">数据</param>
    public ExcelData Decode(string filePath)
    {
        UnityEngine.Debug.Log("----->" + filePath);
        if (File.Exists(filePath))
        {
            FileInfo file = new FileInfo(filePath);
            using (var package = new ExcelPackage(file))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                //解析注释
                List<string> commentList = new List<string>();
                for (int i = 1; i <= sheet.Dimension.End.Column; i++)
                {
                    ExcelRange item = sheet.Cells[1, i];
                    string value = (item.Value ?? "").ToString().Trim();
                    if (!string.IsNullOrEmpty(value))
                    {
                        ExcelComment comment = item.Comment;
                        string str_comment = "";
                        if (comment != null)
                            str_comment = comment.Text.Replace("\n", " ").Replace("\r", " ");
                        str_comment = value + str_comment;
                        commentList.Add(str_comment);
                    }
                    else
                        break;
                }

                //变量名 类型名
                int columCount = commentList.Count;
                List<string> fieldNameList = new List<string>();
                List<string> typeNameList = new List<string>();
                for (int i = 1; i <= columCount; i++)
                {
                    fieldNameList.Add((sheet.Cells[2, i].Value ?? "").ToString().Trim());
                    typeNameList.Add((sheet.Cells[3, i].Value ?? "").ToString().Trim());
                }

                //表中的值
                List<string[]> valueList = new List<string[]>();
                int maxRow = sheet.Dimension.End.Row;
                for (int row = 4; row <= maxRow; row++)
                {
                    var obj = sheet.Cells[row, 1].Value;
                    if (obj != null)
                    {
                        string[] arrayItem = new string[columCount];
                        arrayItem[0] = obj.ToString();
                        for (int col = 2; col <= columCount; col++)
                            arrayItem[col - 1] = sheet.Cells[row, col].Text == null ? "" : sheet.Cells[row, col].Text.ToString();

                        valueList.Add(arrayItem);
                    }
                    else
                        break;
                }

                string name = Path.GetFileNameWithoutExtension(file.Name);
                ExcelData data = new ExcelData(name, commentList, typeNameList, valueList, fieldNameList);
                return data;
            }
        }
        return null;
    }
}
public class ExcelData
{
    public string tableName;
    public List<string> _commentList;       //注释
    public List<string> _typeNameList;      //数据类型
    public List<string[]> _valueList;       //
    public List<string> _fieldNameList;     //变量名

    public ExcelData(string tableName, List<string> _commentList, List<string> _typeNameList, List<string[]> _valueList, List<string> _fieldNameList)
    {
        this.tableName = tableName;
        this._commentList = _commentList;
        this._typeNameList = _typeNameList;
        this._valueList = _valueList;
        this._fieldNameList = _fieldNameList;
    }
}