using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDataReader;


public class ExcelReader : MonoBehaviour
{
    public struct ExcelData
    {
        public string speaker;
        public string content;
        public string avatorImageFileName;
        public string vocalAudioFileName;
        public string backgroundImageFileName;
        public string bgmFileName;
        public string cha1Action;
        public string cha2Action;
        public string cha1Image;
        public string cha2Image;
        public string cha1CoorX;
        public string cha2CoorX;
    }

    public static List<ExcelData> ReadExcel(string filePath)
    {
        List<ExcelData> excelData = new();
        //提供编码以读取中文、日文等数据
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using(var reader = ExcelReaderFactory.CreateReader(stream))     
                do
                {
                    while (reader.Read())
                    {
                        ExcelData data = new ExcelData();
                        data.speaker = reader.IsDBNull(0) ? string.Empty : reader.GetValue(0).ToString();
                        data.content = reader.IsDBNull(1) ? string.Empty : reader.GetValue(1).ToString();
                        data.avatorImageFileName = reader.IsDBNull(2) ? string.Empty : reader.GetValue(2).ToString();
                        data.vocalAudioFileName = reader.IsDBNull(3) ? string.Empty : reader.GetValue(3).ToString();
                        data.backgroundImageFileName = reader.IsDBNull(4) ? string.Empty : reader.GetValue(4).ToString();
                        data.bgmFileName = reader.IsDBNull(5) ? string.Empty : reader.GetValue(5).ToString();
                        data.cha1Action = reader.IsDBNull(6) ? string.Empty : reader.GetValue(6).ToString();
                        data.cha1CoorX = reader.IsDBNull(7) ? string.Empty : reader.GetValue(7).ToString();
                        data.cha1Image = reader.IsDBNull(8) ? string.Empty : reader.GetValue(8).ToString();
                        data.cha2Action = reader.IsDBNull(9) ? string.Empty : reader.GetValue(9).ToString();
                        data.cha2CoorX = reader.IsDBNull(10) ? string.Empty : reader.GetValue(10).ToString();
                        data.cha2Image = reader.IsDBNull(11) ? string.Empty : reader.GetValue(11).ToString();
                        excelData.Add(data);
                    }
                }while(reader.NextResult());
        }
        return excelData;
    }
}


