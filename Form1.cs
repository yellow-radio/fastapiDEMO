using System;
using System.Diagnostics;
using System.Windows.Forms;


namespace fastapiDEMO
{
    public partial class Form1 : Form
    {
        bool SensorIdState = false;
        bool RpmState = false;
        string SensorIdinput;
        int RpminputInt;
        private Process pyService;
        private string path = @"D:\fastapi練習\DEMO\fastapipyDEMO.exe"; // 替換為你的 Python 執行檔路徑

        public Form1()
        {
            InitializeComponent();
            this.Load += async (s, e) => await InitializeSystemAsync();// 在 Form1 的 Load 事件中啟動 Python 後端服務
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);// 在 Form1 的 FormClosing 事件中確保 Python 後端服務被正確關閉
            //StartPythonExe(path);
            timer1.Enabled = true;
        }

        private async Task InitializeSystemAsync()// 這是一個非同步方法，用來初始化系統，包括啟動 Python 後端服務和檢查其狀態
        {
            // 1. 鎖定按鍵並顯示訊息
            SetUiState(false);
            listBox1.Items.Add("正在與後端連線...");

            // 2. 獲取相對路徑並啟動 EXE
            string exeName = "fastapipyDEMO.exe";
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);

            if (!File.Exists(fullPath))
            {
                listBox1.Items.Add($"錯誤：找不到檔案 {exeName}");
                listBox1.Items.Add($"錯誤：找不到檔案 {fullPath}");
                return;
            }

            StartPythonExe(fullPath);

            // 3. 輪詢檢查 FastAPI 是否就緒
            bool isReady = false;
            int retryCount = 0;
            while (!isReady && retryCount < 20) // 最多等 20 秒
            {
                isReady = await InferenceService.CheckHealthAsync();
                if (!isReady)
                {
                    await Task.Delay(1000); // 等一秒再試
                    retryCount++;
                }
            }

            // 4. 解鎖介面
            if (isReady)
            {
                listBox1.Items.Add("連線成功");
                SetUiState(true);
                timer1.Enabled = true;
            }
            else
            {
                listBox1.Items.Add("後端啟動超時，請重新開啟程式");
            }
        }

        private void SetUiState(bool enabled)// 這是一個輔助方法，用來根據參數 enabled 的值來鎖定或解鎖 UI 元件
        {
            // 根據你實際的按鈕名稱進行鎖定
            SensorIdKeyIn.Enabled = enabled;
            RpmKeyIn.Enabled = enabled;
            SensorIdtextBox.Enabled = enabled;
            RpmtextBox.Enabled = enabled;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)// 這個事件處理器會在 Form1 關閉時被觸發，用來確保 Python 後端服務被正確關閉
        {
            if (pyService != null && !pyService.HasExited)
            {
                pyService.Kill(true); // 連同子進程一併關閉
                pyService.Dispose();
            }
        }

        // 在 Form1 的 Load 事件中啟動 Python 後端服務
        private void StartPythonExe(string path)
        {
            try
            {
                pyService = new Process();
                pyService.StartInfo.FileName = path;
                pyService.StartInfo.UseShellExecute = false;
                pyService.StartInfo.CreateNoWindow = false; // 測試時可看 CMD
                pyService.Start();
            }
            catch (Exception ex)
            {
                listBox1.Items.Add($"開啟失敗: {ex.Message}");
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (SensorIdState && RpmState)
            {
                timer1.Enabled = false;
                string result = await InferenceService.PostPredictAsync(SensorIdinput, RpminputInt);
                listBox1.Items.Add($"結果: {result}");
                SensorIdState = false;
                RpmState = false;
                timer1.Enabled = true;
            }
        }

        private void SensorIdtextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void SensorIdKeyIn_Click(object sender, EventArgs e)
        {
            SensorIdinput = SensorIdtextBox.Text;
            SensorIdState = true;
        }

        private void RpmtextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void RpmKeyIn_Click(object sender, EventArgs e)
        {
            if (int.TryParse(RpmtextBox.Text, out int val))
            {
                RpminputInt = val;
                RpmState = true;
            }
            else
            {
                listBox1.Items.Add("RPM必須是整數");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        
    }
}
