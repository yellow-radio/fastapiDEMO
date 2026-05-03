using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fastapiDEMO
{
    public class InferenceService
    {
        private static readonly HttpClient client = new HttpClient();// 定義一個靜態 HttpClient 實例，供整個類別使用

        // 定義一個給 Form1 呼叫的「預測」功能
        public static async Task<string> PostPredictAsync(string id, double rpm)//這是一個非同步方法，回傳 Task<string>，接受兩個參數：id 和 rpm
        {
            var data = new { sensor_id = id, rpm = rpm }; // 匿名物件轉 JSON
            string json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // 修正 Class1.cs 中的讀取邏輯
                var response = await client.PostAsync("http://127.0.0.1:8000/predict", content);

                // 不論狀態碼為何，皆讀取 Body
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using (JsonDocument doc = JsonDocument.Parse(result))
                    {
                        // 直接提取 message 欄位的內容
                        return doc.RootElement.GetProperty("message").GetString();
                    }
                    return result; // 回傳 OK 結果
                }
                else
                {
                    using (JsonDocument doc = JsonDocument.Parse(result))
                    {
                        // 直接提取 detail 陣列中第一個項目的 msg 內容
                        string errorMessage = doc.RootElement.GetProperty("detail")[0].GetProperty("msg").GetString();
                        return $"驗證失敗: {errorMessage}";
                    }
                }
            }
            catch (HttpRequestException)
            {
                return "連線失敗：請檢查 Python API 是否已啟動";
            }
        }
        public static async Task<bool> CheckHealthAsync()//這是一個非同步方法，回傳 Task<bool>，用來檢查 FastAPI 服務是否運行
        {
            try
            {
                // 嘗試存取根目錄，只要不噴 HttpRequestException 就代表埠口已開
                var response = await client.GetAsync("http://127.0.0.1:8000/");
                return true;
            }
            catch
            {
                return false; // 連線失敗
            }
        }
    }


}
