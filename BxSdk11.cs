using System;
using System.Runtime.InteropServices;

namespace LedSDK3
{
    // =================================================================
    // 最終正確的 SDK 定義檔 (完全基於您的成功範例)
    // =================================================================
    public static class bxdualsdk
    {
        private const string DllPath = "bx_sdk_dual.dll";

        #region 枚舉 (Enums) - 根據您的成功範例，全部設為 int
        public enum E_ScreenColor_G56
        {
            eSCREEN_COLOR_SINGLE = 1,
            eSCREEN_COLOR_DOUBLE,
            eSCREEN_COLOR_THREE,
            eSCREEN_COLOR_FULLCOLOR,
        }

        public enum E_DoubleColorPixel_G56
        {
            eDOUBLE_COLOR_PIXTYPE_1 = 1,
            eDOUBLE_COLOR_PIXTYPE_2,
        }

        public enum E_arrMode
        {
            eSINGLELINE,
            eMULTILINE,
        }

        public enum E_txtDirection
        {
            pNORMAL,
            pROTATERIGHT,
            pMIRROR,
            pROTATELEFT,
        }
        #endregion

        #region 結構體 (Structs) - 根據您的成功範例，全部使用 Pack = 1
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Ping_data
        {
            public ushort ControllerType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] FirmwareVersion;
            public byte ScreenParaStatus;
            public ushort uAddress;
            public byte Baudrate;
            public ushort ScreenWidth;
            public ushort ScreenHeight;
            public byte Color;
            public byte CurrentBrigtness;
            public byte CurrentOnOffStatus;
            public ushort ScanConfNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] reversed;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] ipAdder;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EQprogramHeader_G6
        {
            public byte FileType;
            public uint ProgramID;
            public byte ProgramStyle;
            public byte ProgramPriority;
            public byte ProgramPlayTimes;
            public ushort ProgramTimeSpan;
            public byte SpecialFlag;
            public byte CommExtendParaLen;
            public ushort ScheduNum;
            public ushort LoopValue;
            public byte Intergrate;
            public byte TimeAttributeNum;
            public ushort TimeAttribute0Offset;
            public byte ProgramWeek;
            public ushort ProgramLifeSpan_sy;
            public byte ProgramLifeSpan_sm;
            public byte ProgramLifeSpan_sd;
            public ushort ProgramLifeSpan_ey;
            public byte ProgramLifeSpan_em;
            public byte ProgramLifeSpan_ed;
            public byte PlayPeriodGrpNum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EQareaHeader_G6
        {
            public byte AreaType;
            public ushort AreaX;
            public ushort AreaY;
            public ushort AreaWidth;
            public ushort AreaHeight;
            public byte BackGroundFlag;
            public byte Transparency;
            public byte AreaEqual;
            // EQSound_6G stSoundData; // 移除語音以簡化
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EQpageHeader_G6
        {
            public byte PageStyle;
            public byte DisplayMode;
            public byte ClearMode;
            public byte Speed;
            public ushort StayTime;
            public byte RepeatTime;
            public ushort ValidLen;
            public byte CartoonFrameRate;
            public byte BackNotValidFlag;
            public E_arrMode arrMode;
            public ushort fontSize;
            public uint color;
            public byte fontBold;
            public byte fontItalic;
            public E_txtDirection tdirection;
            public ushort txtSpace;
            public byte Valign;
            public byte Halign;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EQprogram_G6
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] fileName;
            public byte fileType;
            public uint fileLen;
            public IntPtr fileAddre;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] dfileName;
            public byte dfileType;
            public uint dfileLen;
            public IntPtr dfileAddre;
        }
        #endregion

        #region 函數 (Functions) - 根據您的成功範例定義
        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_InitSdk();

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern void bxDual_ReleaseSdk();

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_cmd_tcpPing(byte[] ip, ushort port, ref Ping_data retData);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_program_setScreenParams_G56(E_ScreenColor_G56 color, ushort ControllerType, E_DoubleColorPixel_G56 doubleColor);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_program_addProgram_G6(ref EQprogramHeader_G6 programH);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_program_addArea_G6(ushort areaID, ref EQareaHeader_G6 aheader);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_program_picturesAreaAddTxt_G6(ushort areaID, byte[] str, byte[] fontName, ref EQpageHeader_G6 pheader);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_program_IntegrateProgramFile_G6(ref EQprogram_G6 program);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_cmd_ofsStartFileTransf(byte[] ip, ushort port);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_cmd_ofsWriteFile(byte[] ip, ushort port, byte[] fileName, byte fileType, uint fileLen, byte overwrite, IntPtr fileAddre);

        [DllImport(DllPath, CharSet = CharSet.Ansi)]
        public static extern int bxDual_cmd_ofsEndFileTransf(byte[] ip, ushort port);
        #endregion
    }
}

