// 【修改】引用舊版 SDK 的命名空間
using LedSDK;
using System;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

namespace LedDispatchService
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string controllerIp = "192.168.1.126";
            ushort controllerPort = 5005;

            while (true)
            {
                try
                {
                    // ===================【修改】===================
                    string workOrderText = "尚有5件工單未指派";
                    // =============================================

                    Console.WriteLine($"\n[{DateTime.Now:T}] 接收到參數，準備發送到 LED: '{workOrderText}'");

                    SendProgramToLed(controllerIp, controllerPort, workOrderText);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("成功發送。");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"發送時發生嚴重錯誤: {ex.ToString()}");
                    Console.ResetColor();
                }

                Console.WriteLine("\n等待 30 秒...");
                Thread.Sleep(30000);
            }
        }

        /// <summary>
        /// 【完美克隆版】此函式在所有細節上，均與舊版 SendProgramToLed 完全一致。
        /// </summary>
        public static void SendProgramToLed(string ip, ushort port, string text)
        {
            // 宣告 programFile 變數於函式頂部，以確保 finally 區塊可以存取
            var programFile = new bxdualsdk.EQprogram_G6();
            bool programIntegrated = false;

            try
            {
                int err = bxdualsdk.bxDual_InitSdk();
                if (err != 0) throw new Exception($"[錯誤] SDK 初始化失敗: {err}");
                Console.WriteLine("[進度] SDK 初始化成功。");

                var controllerInfo = new bxdualsdk.Ping_data();
                byte[] ipBytes = Encoding.GetEncoding("GBK").GetBytes(ip + "\0");

                err = bxdualsdk.bxDual_cmd_tcpPing(ipBytes, port, ref controllerInfo);
                if (err != 0) throw new Exception($"[錯誤] 無法連接到控制器: {err}");
                Console.WriteLine($"[進度] 連接控制器成功，尺寸: {controllerInfo.ScreenWidth}x{controllerInfo.ScreenHeight}");

                bxdualsdk.E_ScreenColor_G56 screenColor;
                if (controllerInfo.Color == 1) { screenColor = bxdualsdk.E_ScreenColor_G56.eSCREEN_COLOR_SINGLE; }
                else if (controllerInfo.Color == 3) { screenColor = bxdualsdk.E_ScreenColor_G56.eSCREEN_COLOR_DOUBLE; }
                else if (controllerInfo.Color == 7) { screenColor = bxdualsdk.E_ScreenColor_G56.eSCREEN_COLOR_THREE; }
                else { screenColor = bxdualsdk.E_ScreenColor_G56.eSCREEN_COLOR_FULLCOLOR; }

                err = bxdualsdk.bxDual_program_setScreenParams_G56(screenColor, controllerInfo.ControllerType, bxdualsdk.E_DoubleColorPixel_G56.eDOUBLE_COLOR_PIXTYPE_2);
                if (err != 0) throw new Exception($"[錯誤] 設定屏幕參數失敗: {err}");

                var programHeader = new bxdualsdk.EQprogramHeader_G6 { ProgramWeek = 0xff, ProgramLifeSpan_sy = 0xffff, ProgramPlayTimes = 1 };
                err = bxdualsdk.bxDual_program_addProgram_G6(ref programHeader);
                if (err != 0) throw new Exception($"[錯誤] 新增節目失敗: {err}");

                var areaHeader = new bxdualsdk.EQareaHeader_G6 { AreaType = 0, AreaX = 0, AreaY = 0, AreaWidth = controllerInfo.ScreenWidth, AreaHeight = controllerInfo.ScreenHeight };
                err = bxdualsdk.bxDual_program_addArea_G6(0, ref areaHeader);
                if (err != 0) throw new Exception($"[錯誤] 新增區域失敗: {err}");

                var pageHeader = new bxdualsdk.EQpageHeader_G6
                {
                    PageStyle = 0x00,
                    DisplayMode = 0x04,
                    ClearMode = 0x01,
                    Speed = 25,
                    StayTime = 0,
                    RepeatTime = 1,
                    ValidLen = controllerInfo.ScreenWidth,
                    CartoonFrameRate = 0x00,
                    BackNotValidFlag = 0x00,
                    arrMode = bxdualsdk.E_arrMode.eSINGLELINE,
                    fontSize = 20,
                    color = (uint)0x01,
                    fontBold = 0,
                    fontItalic = 0,
                    tdirection = bxdualsdk.E_txtDirection.pNORMAL,
                    txtSpace = 0,
                    Valign = 2,
                    Halign = 0
                };

                byte[] textBytes = Encoding.GetEncoding("GBK").GetBytes(text + "\0");
                byte[] fontBytes = Encoding.GetEncoding("GBK").GetBytes("微軟正黑體\0");
                err = bxdualsdk.bxDual_program_picturesAreaAddTxt_G6(0, textBytes, fontBytes, ref pageHeader);
                if (err != 0) throw new Exception($"[錯誤] 新增文字失敗: {err}");
                Console.WriteLine("[進度] 節目內容建立完成。");

                err = bxdualsdk.bxDual_program_IntegrateProgramFile_G6(ref programFile);
                if (err != 0) throw new Exception($"[錯誤] 整合節目失敗: {err}");
                programIntegrated = true; // 標記節目已成功整合

                // ===================【增加步驟 1】===================
                // 清除 SDK 內部的節目建構狀態，為下一次呼叫做準備
                err = bxdualsdk.bxDual_program_deleteProgram_G6();
                if (err != 0) throw new Exception($"[錯誤] 清除節目編輯狀態失敗: {err}");
                Console.WriteLine("[進度] SDK 節目編輯狀態已清除。");
                // ====================================================

                Console.WriteLine("[進度] 開始傳送檔案...");
                err = bxdualsdk.bxDual_cmd_ofsStartFileTransf(ipBytes, port);
                if (err != 0) throw new Exception($"[錯誤] 開始傳輸失敗: {err}");

                err = bxdualsdk.bxDual_cmd_ofsWriteFile(ipBytes, port, programFile.dfileName, programFile.dfileType, programFile.dfileLen, 1, programFile.dfileAddre);
                if (err != 0) throw new Exception($"[錯誤] 寫入資料檔案失敗: {err}");

                err = bxdualsdk.bxDual_cmd_ofsWriteFile(ipBytes, port, programFile.fileName, programFile.fileType, programFile.fileLen, 1, programFile.fileAddre);
                if (err != 0) throw new Exception($"[錯誤] 寫入參數檔案失敗: {err}");

                err = bxdualsdk.bxDual_cmd_ofsEndFileTransf(ipBytes, port);
                if (err != 0) throw new Exception($"[錯誤] 結束傳輸失敗: {err}");
                Console.WriteLine("[進度] 檔案傳送完畢。");
            }
            finally
            {
                // ===================【增加步驟 2】===================
                // 只有在節目成功整合後，才需要釋放緩衝區
                if (programIntegrated)
                {
                    bxdualsdk.bxDual_program_freeBuffer_G6(ref programFile);
                    Console.WriteLine("[進度] 節目檔案緩衝區已釋放。");
                }
                // ====================================================

                bxdualsdk.bxDual_ReleaseSdk();
                Console.WriteLine("[進度] SDK 資源已釋放。");
            }
        }
    }
}