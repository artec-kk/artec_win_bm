using ScratchConnection;
using System;

namespace Artec
{
    namespace BuildConst
    {
        /// <summary>
        /// ビルド時のオプションを定義するクラス
        /// </summary>
        public class Studuino
        {
            // テストモード・モーター校正用設定
            public virtual string TestModeFile       // テストモード用プログラム
            {
                get {
                    if(boardType.Equals(BoardType.STUDUINO_MINI))
                    {
                        return "testmode_mini.hex";
                    }
                    return "ar.cpp.hex";
                }
            }
            //public readonly  string SVCalibrationFile = "calibration.cpp.hex";   // サーボモーター角度校正用プログラム
            public virtual string SVCalibrationFile  // モーター角度校正用プログラム
            {
                get {
                    if (boardType.Equals(BoardType.STUDUINO_MINI))
                    {
                        return "testmode_mini.hex";
                    }
                    if(boardType.Equals(BoardType.STUDUINO_AND_MINI))
                    {
                        return "ss38400.hex";
                    }
                    return "calibration.cpp.hex";
                }
            }
            public readonly  string TestModePath = @".\etc\";      // テストモード用プログラムのパス

            #region 【設定】 ビルド命令セット
            public readonly  string ArduinoSystemPath = @"..\common\tools\";
            public readonly  string UserCodePath = @".\user\";
            public readonly  string SourceFile = "artecRobot.cpp";
            //public readonly  string DependencyFile = @"build\" + SourceFile + ".d";

            // コンパイラコマンドの設定
            public readonly  string Compiler = @"hardware\tools\avr\bin\avr-g++.exe";
            //public readonly string CompilerOption = "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=atmega168 -DF_CPU=8000000L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
            //public readonly string CompilerOption = "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=atmega168 -DF_CPU=12000000L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
            public virtual string CompilerOption
            {
                get {
                    //if (boardType.Equals(BoardType.STUDUINO_MINI))
                    //{
                    //    return "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=atmega168 -DF_CPU=12000000L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
                    //}
                    //return "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=atmega168 -DF_CPU=8000000L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
                    return "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=" + boardType.mcu.optionMCU + " -DF_CPU=" + boardType.frequency + "L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
                }
            }
            public virtual string[] IncludeFiles
            {
                get
                {
                    return new string[]{
                        @"hardware\arduino\cores\arduino",
                        @"hardware\arduino\variants\standard",
                        @"libraries\Servo",
                        @"libraries\Wire",
                        @"libraries\Wire\utility",
                        @"libraries\MMA8653",
                        @"libraries\MPU6050",
                        @"libraries\Clock",
                        @"libraries\Studuino"};
                }
            }
            //public readonly string[] IncludeFiles = {
            //    @"hardware\arduino\cores\arduino",
            //    @"hardware\arduino\variants\standard",
            //    @"libraries\Servo",
            //    @"libraries\Wire",
            //    @"libraries\Wire\utility",
            //    @"libraries\MMA8653",
            //    @"libraries\Clock",
            //    @"libraries\Studuino"};
            //public readonly  string ObjectFile = @"build\" + SourceFile + ".o";
            public string ObjectFile
            {
                get { return @"build\" + SourceFile + ".o"; }
            }

            public virtual string[] SystemObjectFilesV1
            {
                get
                {
                    return new string[]{
                        @"build\Servo\Servo.cpp.o",
                        @"build\MMA8653\MMA8653.cpp.o",
                        @"build\Wire\Wire.cpp.o",
                        @"build\Wire\utility\twi.c.o",
                        @"build\Studuino\Studuino.cppforV1.o",};
                }
            }

            public virtual string[] SystemObjectFilesGyro
            {
                get
                {
                    return new string[]{
                        @"build\Servo\Servo.cpp.o",
                        @"build\MPU6050\I2Cdev.cpp.o",
                        @"build\MPU6050\MPU6050.cpp.o",
                        @"build\Wire\Wire.cpp.o",
                        @"build\Wire\utility\twi.c.o",
                        @"build\Studuino\Studuino.cppforGyro.o",};
                }
            }

            //public readonly  string[] SystemObjectFiles = {
            //    @"build\Servo\Servo.cpp.o",
            //    @"build\MMA8653\MMA8653.cpp.o",
            //    @"build\Wire\Wire.cpp.o",
            //    @"build\Wire\utility\twi.c.o",
            //    @"build\Studuino\Studuino.cpp.o",
            //    @"build\WInterrupts.c.o", 
            //    @"build\wiring.c.o",
            //    @"build\wiring_analog.c.o",
            //    @"build\wiring_digital.c.o",
            //    @"build\wiring_pulse.c.o",
            //    @"build\wiring_shift.c.o",
            //    @"build\CDC.cpp.o",
            //    @"build\HardwareSerial.cpp.o",
            //    @"build\HID.cpp.o",
            //    @"build\IPAddress.cpp.o",
            //    @"build\main.cpp.o",
            //    @"build\new.cpp.o",
            //    @"build\Print.cpp.o",
            //    @"build\Stream.cpp.o",
            //    @"build\Tone.cpp.o",
            //    @"build\USBCore.cpp.o",
            //    @"build\WMath.cpp.o",
            //    @"build\WString.cpp.o",};

            // アーカイバコマンドの設定
            public readonly  string Archiver = @"hardware\tools\avr\bin\avr-ar.exe";
            public readonly string ArchiverOption = "rcs";
            //public readonly string ArchiverFile = @"build\core.a";
            public virtual string ArchiverFile
            {
                get {
                    if (boardType.Equals(BoardType.STUDUINO_MINI))
                    {
                        return @"build\core_mini.a";
                    }
                    return @"build\core.a";
                }
            }

            // リンカコマンドの設定
            public readonly  string Linker = @"hardware\tools\avr\bin\avr-gcc.exe";
            //public readonly  string LinkerOption = "-Os -Wl,--gc-sections -mmcu=atmega168";
            public string LinkerOption
            {
                get
                {
                    return "-Os -Wl,--gc-sections -mmcu=" + boardType.mcu.optionMCU;
                }
            }
            public readonly  string LinkFile = @"build\Servo\Servo.cpp.o";
            //public readonly  string ElfFile = @"build\" + SourceFile + ".elf";
            public string ElfFile
            {
                get { return @"build\" + SourceFile + ".elf"; }
            }
            public readonly string LinkDirectory = "build -lm";

            // オブジェクトコピーコマンドの設定
            public readonly  string Objcopy = @"hardware\tools\avr\bin\avr-objcopy.exe";
            public readonly  string ObjcopyOption1 = "-O ihex -j .eeprom --set-section-flags=.eeprom=alloc,load --no-change-warnings --change-section-lma .eeprom=0";
            //public readonly  string EepFile = @"build\" + SourceFile + ".eep";
            public string EepFile
            {
                get { return @"build\" + SourceFile + ".eep"; }
            }
            public readonly string ObjcopyOption2 = "-O ihex -R .eeprom";
            //public readonly  string HexFile = @"build\" + SourceFile + ".hex";
            public string HexFile
            {
                get { return @"build\" + SourceFile + ".hex"; }
            }

            // 転送コマンドの設定
            //public readonly string Transfer = @"hardware\tools\avr\bin\avrdude.exe";
            //public readonly string Transfer = @"bootloadHID.exe";
            public virtual string Transfer
            {
                get
                {
                    if (boardType.Equals(BoardType.STUDUINO_MINI))
                    {
                        return @"bootloadHID.exe";
                    }
                    if (boardType.Equals(BoardType.STUDUINO_AND_MINI))
                    {
                        return @"hidspx-gcc.exe";
                    }
                    return @"hardware\tools\avr\bin\avrdude.exe";
                }
            }
            public readonly string ConfFile = @"hardware\tools\avr\etc\avrdude.conf";
            //public readonly string TransferOption = @"-v -v -v -v -patmega168 -carduino -b115200 -D -V";
            //public readonly string TransferOption = @"-r";
            public virtual string TransferOption
            {
                get
                {
                    if (boardType.Equals(BoardType.STUDUINO_MINI))
                    {
                        return @"-r";
                    }
                    //return @"-q -q -p" + boardType.mcu.optionMCU +" - carduino -b115200 -D -V";
                    return @"-q -q -p" + boardType.mcu.optionMCU + " -carduino -b115200 -D -V";
                }
            }

            // ダンプコマンドの設定
            public readonly  string ObjDump = @"hardware\tools\avr\bin\avr-objdump.exe";
            public readonly  string ObjDumpOption = "-h";
            #endregion

            // プログラムの最大サイズ
            //public readonly  int MAXPROGRAMSIZE = 15872;   // 15.5kB (Flashサイズ16kB - ブートローダー領域0.5kB)
            public virtual int MAXPROGRAMSIZE        // 15.5kB (Flashサイズ16kB - ブートローダー領域0.5kB)
            {
                get {
                    //if (boardType.Equals(BoardType.STUDUINO_MINI))
                    //{
                    //    return 14336;
                    //}
                    //return 15872;
                    return boardType.MaxProgramSize;
                }
            }

            private BoardType boardType;

            public Studuino() { }

            public Studuino(BoardType type)
            {
                this.boardType = type;
            }
        }
#if False
        /// <summary>
        /// ArduinoIDE1.6.9に含まれるavr-toolに合わせたビルドオプション
        /// </summary>
        [Obsolete]
        public class Studuino10609 : Studuino
        {
            public override string CompilerOption
            {
                get { return "-c -g -Os -Wall -Wextra -std=gnu++11 -fno-exceptions -ffunction-sections -fdata-sections -fno-threadsafe-statics -MMD -mmcu=atmega168 -DF_CPU=8000000L -DARDUINO=10609 -DARDUINO_AVR_PRO -DARDUINO_ARCH_AVR"; }
            }

            public override string[] IncludeFiles
            {
                get
                {
                    return new string[]{
                        @"\hardware\arduino\avr\cores\arduino",
                        @"\hardware\arduino\avr\variants\standard",
                        @"\libraries\Servo\src",
                        @"\hardware\arduino\avr\libraries\Wire\src",
                        @"\libraries\MMA8653",
                        @"\libraries\Studuino",
                        @"\libraries\Clock"};
                }
            }

            public override string[] SystemObjectFiles
            {
                get
                {
                    return new string[]{
                        @"build\10609\Servo\avr\Servo.cpp.o",
                        @"build\10609\Servo\sam\Servo.cpp.o",
                        @"build\10609\Servo\samd\Servo.cpp.o",
                        @"build\10609\Wire\Wire.cpp.o",
                        @"build\10609\Wire\utility\twi.c.o",
                        @"build\10609\MMA8653\MMA8653.cpp.o",
                        @"build\10609\Studuino\Studuino.cpp.o",
                        @"build\10609\Clock\Clock.cpp.o",};
                }
            }

            public override string ArchiverFile
            {
                get { return @"build\10609\core.a"; }
            }
        }

        [Obsolete]
        public class StuduinoLP : Studuino
        {
            public override string TestModeFile      // テストモード用プログラム
            {
                get { return "testmode_mini.hex"; }
            }
            public override string SVCalibrationFile // モーター校正用プログラム
            {
                get { return "testmode_mini.hex"; }
            }
            // コンパイラコマンドの設定
            //public readonly new string CompilerOption = "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=atmega168 -DF_CPU=12000000L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
            public override string CompilerOption
            {
                get
                {
                    return "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=atmega168 -DF_CPU=12000000L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
                }
            }
            // アーカイバコマンドの設定
            //public readonly new string ArchiverFile = @"build\core_mini.a";
            public override string ArchiverFile
            {
                get { return @"build\core_mini.a"; }
            }
            // 転送コマンドの設定
            //public readonly new string Transfer = @"bootloadHID.exe";
            //public readonly new string TransferOption = @"-r";
            public override string Transfer
            {
                get
                {
                    return @"bootloadHID.exe"; ;
                }
            }
            public override string TransferOption
            {
                get
                {
                    return @"-r";
                }
            }
            public override int MAXPROGRAMSIZE      // 14kB (Flashサイズ16kB - ブートローダー領域2kB)
            {
                get
                {
                    return 14336;
                }
            }
        }

        [Obsolete]
        public class StuduinoLP10609 : StuduinoLP
        {
            public override string CompilerOption
            {
                get
                {
                    return "-c -g -Os -Wall -Wextra -std=gnu++11 -fno-exceptions -ffunction-sections -fdata-sections -fno-threadsafe-statics -MMD -mmcu=atmega168 -DF_CPU=12000000L -DARDUINO=10609 -DARDUINO_AVR_PRO -DARDUINO_ARCH_AVR";
                }
            }
            public override string[] IncludeFiles
            {
                get
                {
                    return new string[]{
                        @"\hardware\arduino\avr\cores\arduino",
                        @"\hardware\arduino\avr\variants\standard",
                        @"\libraries\Servo\src",
                        @"\hardware\arduino\avr\libraries\Wire\src",
                        @"\libraries\MMA8653",
                        @"\libraries\Studuino",
                        @"\libraries\Clock"};
                }
            }
        }

        [Obsolete]
        public class StuduinoAndMini : Studuino
        {
            public override string SVCalibrationFile // モーター校正用プログラム
            {
                get { return "ss38400.hex"; }
            }
            // 転送コマンドの設定
            //public readonly new string Transfer = @"bootloadHID.exe";
            //public readonly new string TransferOption = @"-r";
            public override string Transfer
            {
                get
                {
                    return @"hidspx-gcc.exe"; ;
                }
            }
        }
#endif
    }
}
