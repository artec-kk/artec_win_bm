using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

namespace ScratchConnection
{
    class Program
    {
        #region 【設定】 プログラム作成・転送用
        // 転送コマンドの設定
        const string TestModeFile = "ar.cpp.hex";   // テストモード用プログラム
        const string SVCalibrationFile = "calibration.cpp.hex";   // サーボモーター角度校正用プログラム
        const string TestModePath = @".\etc\";      // テストモード用プログラムのパス

        #region 【設定】 ビルド命令セット
        const string ArduinoSystemPath = @"..\common\tools\";
        const string UserCodePath = @".\user\";
        const string SourceFile = "artecRobot.cpp";
        const string DependencyFile = @"build\" + SourceFile + ".d";

        // コンパイラコマンドの設定
        const string Compiler = @"hardware\tools\avr\bin\avr-g++.exe";
        const string CompilerOption = "-c -g -Os -Wall -fno-exceptions -ffunction-sections -fdata-sections -mmcu=atmega168 -DF_CPU=8000000L -MMD -DUSB_VID=null -DUSB_PID=null -DARDUINO=101";
        static readonly string[] IncludeFiles = {
            @"hardware\arduino\cores\arduino",
            @"hardware\arduino\variants\standard",
            @"libraries\Servo",
            @"libraries\Wire",
            @"libraries\Wire\utility",
            @"libraries\MMA8653",
            @"libraries\Studuino"
        };
        const string ObjectFile = @"build\" + SourceFile + ".o";
        static readonly string[] SystemObjectFiles = {
            @"build\Servo\Servo.cpp.o",
            @"build\MMA8653\MMA8653.cpp.o",
            @"build\Wire\Wire.cpp.o",
            @"build\Wire\utility\twi.c.o",
            @"build\Studuino\Studuino.cpp.o",
            @"build\WInterrupts.c.o", 
            @"build\wiring.c.o",
            @"build\wiring_analog.c.o",
            @"build\wiring_digital.c.o",
            @"build\wiring_pulse.c.o",
            @"build\wiring_shift.c.o",
            @"build\CDC.cpp.o",
            @"build\HardwareSerial.cpp.o",
            @"build\HID.cpp.o",
            @"build\IPAddress.cpp.o",
            @"build\main.cpp.o",
            @"build\new.cpp.o",
            @"build\Print.cpp.o",
            @"build\Stream.cpp.o",
            @"build\Tone.cpp.o",
            @"build\USBCore.cpp.o",
            @"build\WMath.cpp.o",
            @"build\WString.cpp.o",
        };

        // アーカイバコマンドの設定
        const string Archiver = @"hardware\tools\avr\bin\avr-ar.exe";
        const string ArchiverOption = "rcs";
        const string ArchiverFile = @"build\core.a";

        // リンカコマンドの設定
        const string Linker = @"hardware\tools\avr\bin\avr-gcc.exe";
        const string LinkerOption = "-Os -Wl,--gc-sections -mmcu=atmega168";
        const string LinkFile = @"build\Servo\Servo.cpp.o";
        const string ElfFile = @"build\" + SourceFile + ".elf";
        const string LinkDirectory = "build -lm";

        // オブジェクトコピーコマンドの設定
        const string Objcopy = @"hardware\tools\avr\bin\avr-objcopy.exe";
        const string ObjcopyOption1 = "-O ihex -j .eeprom --set-section-flags=.eeprom=alloc,load --no-change-warnings --change-section-lma .eeprom=0";
        const string EepFile = @"build\" + SourceFile + ".eep";
        const string ObjcopyOption2 = "-O ihex -R .eeprom";
        const string HexFile = @"build\" + SourceFile + ".hex";

        // 転送コマンドの設定
        const string Transfer = @"hardware\tools\avr\bin\avrdude.exe";
        const string ConfFile = @"hardware\tools\avr\etc\avrdude.conf";
        const string TransferOption = @"-q -q -patmega168 -carduino -b115200 -D -V"; 
        #endregion

        // ダンプコマンドの設定
        const string ObjDump = @"hardware\tools\avr\bin\avr-objdump.exe";
        const string ObjDumpOption = "-h";

        #endregion

        // プログラムの最大サイズ
        const int MAXPROGRAMSIZE = 15872;   // 15.5kB (Flashサイズ16kB - ブートローダー領域0.5kB)
        // ビルド処理時の結果
        const int BuildError = 0;           // ビルドエラー
        const int BuildSuccess = 1;         // ビルド成功
        const int BuildFlashOverflow = 2;   // FLASHオーバーフロー
        // 多言語対応
        const int ENGLISH = 0;
        const int JAPANESE = 1;
        const int CHINESE = 2;
        const int HIRAGANA = 3;
        // BPEとの通信用ポート情報
        static string portNumber = "portNumber.info";
        // サーボモーターのオフセット情報
        static ServoOffset svOffset;
        const string iniFile = @"..\common\sv_offset_ini";   // オフセット用設定ファイル
        const string iniDC = @"..\common\dc_calib_ini";   // DCモーター校正用設定ファイル
        static bool hiragana = false;      // ひらがなフラグ

        //  0: No Error
        //  1: 【通信】COMポート未検出(Serial port is not found. Check to connect Studuino to a computer)
        //  2: 【通信】COMポートが既に使われている(Serial port'XXX' already in use. Try quiting any programs that may be using it.)
        //  3: 【通信】Studuinoとの同期が取れない()
        //  4: 【通信】書き込みエラー(Communication has been disconnected)
        //  5: 【システム】致命的なエラー(Emergency error)
        //  7: 【リンク】メインルーチンが定義されていない
        //  8: 【ビルド】プログラムサイズオーバーフロー
        //  9: 【アーカイブ】アーカイブエラー
        // 10: 【オブジェクトコピー】オブジェクトコピーエラー1
        // 11: 【オブジェクトコピー】オブジェクトコピーエラー2
        // 17(b'10001): 【コンパイル】サブルーチンが定義されていません
        // 18(b'10010): 【コンパイル】繋がっていないブロックが存在します
        // 20(b'10100): 【コンパイル】０で割ろうとしている処理があります
        static int ErrorNumber;

        static PortManager pm;        // COMポート管理クラス

        //---------------------------------------------------------------------
        // 概要  : Main
        //---------------------------------------------------------------------
        static void Main(string[] args)
        {
            // 引数で指定された言語に切替える
            if (args.Length > 0)
            {
                string lang = args[0];
                if (lang == "ja_HIRA")
                {
                    hiragana = true;
                    lang = "ja";
                }
                if (lang == "zh_CN")
                {
                    lang = "zh";
                }
                changeLanguage(lang);
            }

            ErrorNumber = 0;    // エラーNo.の初期化
            pm = new PortManager();    // COMポート管理クラス

            // 入出力設定、モーター校正フォームが初回表示時にアクティブにならないことを回避するための処理。
            // ダミーでフォームを表示させてすぐに消す。
            // 原因不明だが、コンソールアプリからフォームを正しく呼び出す際に何か処理が必要？
            Application.Run(new Dummy());

            //-----------------------------------------------------------------
            // ブロックプログラミング環境(BPE)との通信ポートの開設
            //-----------------------------------------------------------------
            string host = "127.0.0.1";  // ローカルホストIP
            TcpListener server = null;   
            System.Net.IPAddress localAddr = System.Net.IPAddress.Parse(host);
            int port = 49212;           // 自由に使用できるポート番号(49212～)
            // 使用できるポート番号の検索
            for (; port < 65535; port++) {
                try {
                    server = new TcpListener(localAddr, port);
                    server.Start();
                    break;
                }
                catch (SocketException) {
                    // ポート番号がかぶっていた場合の対応
                }
            }
            // BPEに通知するポート番号をエンディアン変換してファイル出力
            Debug.WriteLine(port);
            BinaryWriter bw = new BinaryWriter(File.Create(portNumber));
            bw.Write(convertEndianUINT(port));
            bw.Close();

            //-----------------------------------------------------------------
            // サーボモーターのオフセット情報を読み込む
            //-----------------------------------------------------------------
            svOffset = new ServoOffset();          // サーボオフセット
            int index = 0;                         // 配列用インデックス
            int pos = 0;                           // 読み込み位置
            try {
                using (FileStream fs = new FileStream(iniFile, FileMode.Open, FileAccess.Read)) {
                    int fileSize = (int)fs.Length;
                    byte[] buf = new byte[fileSize];
                    int readSize;
                    readSize = fs.Read(buf, 0, fileSize);

                    while (index < 8) {
                        int offset = ((sbyte)buf[index]);   // 符号なしバイト整数を符号付き整数に変更
                        if (offset < -15 || offset > 15) offset = 0;
                        svOffset.set(index, offset);
                        index++;
                    }
                }
                using (FileStream fs = new FileStream(iniDC, FileMode.Open, FileAccess.Read))
                {
                    index = 0;
                    int fileSize = (int)fs.Length;
                    byte[] buf = new byte[fileSize];
                    int readSize;
                    readSize = fs.Read(buf, 0, fileSize);
                    // DCモーター校正情報
                    if (readSize >= 2)
                    {
                        byte m1Rate = buf[index];
                        byte m2Rate = buf[index + 1];
                        if (!(m1Rate > 50 && m1Rate <= 100)) m1Rate = 100; // 1-100以外の場合は100とする
                        if (!(m2Rate > 50 && m2Rate <= 100)) m2Rate = 100; // 1-100以外の場合は100とする

                        Debug.Write("Read Rate: " + m1Rate + ", " + m2Rate);
                        svOffset.setDCCalib(m1Rate, m2Rate);
                    }
                    else
                    {
                        // ファイルに情報が無い場合は何もしない
                    }
                }
            }
            catch (Exception e) {
                // ファイルが見つからない場合は何も読み込まない
                Debug.Write("File Not Found.");
            }


            //-----------------------------------------------------------------
            // BPEとの通信処理
            //-----------------------------------------------------------------
            Byte[] bytes = new Byte[256];
            String data = null;
            bool clientFinish = false; // クライアント終了を表すフラグ
            // BPEからの接続要求対応
            while (true)
            {
                Debug.Write("Waiting for a connection... ");

                // BPEから接続要求が送信されるまでブロック
                TcpClient client = server.AcceptTcpClient();
                Debug.WriteLine("Connected!");

                data = null;      // データ初期化
                ErrorNumber = 0;  // エラーコード初期化

                // BPEとRead/Writeに使用するストリームオブジェクトを作成
                NetworkStream stream = client.GetStream();

                int i;
                // BPEからのデータを受信(バイトデータ)
                try {
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // バイトデータをASCII文字に変換
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Debug.WriteLine("Received: {0}", data);
                        // 受信データから改行(\r\n)を削除
                        data = data.Replace("\r", "").Replace("\n", "");

                        // メッセージ別に処理を実行
                        #region 入出力設定フォームを開く
                        if (data == "CONFIG")
                        { // ピン設定
                            if (hiragana)
                            {
                                setBoardIOConfiguration(stream, HIRAGANA);
                            }
                            else
                            {
                                setBoardIOConfiguration(stream);
                            }
                        }
                        if (data == "CONFIGE")
                        { // ピン設定
                            setBoardIOConfiguration(stream, ENGLISH);
                        }
                        if (data == "CONFIGJ")
                        { // ピン設定
                            setBoardIOConfiguration(stream, JAPANESE);
                        }
                        if (data == "CONFIGZ")
                        { // ピン設定
                            setBoardIOConfiguration(stream, CHINESE);
                        }
                        if (data == "CONFIGH")
                        { // ピン設定
                            setBoardIOConfiguration(stream, HIRAGANA);
                        }
                        #endregion
                        #region モーター校正フォームを開く
                        if (data == "CALIB")
                        { // オフセット設定
                            if (hiragana)
                            {
                                setServomotorOffset(stream, HIRAGANA);
                            }
                            else
                            {
                                setServomotorOffset(stream);
                            }
                        }
                        if (data == "CALIBE")
                        { // オフセット設定
                            setServomotorOffset(stream, ENGLISH);
                        }
                        if (data == "CALIBJ")
                        { // オフセット設定
                            setServomotorOffset(stream, JAPANESE);
                        }
                        if (data == "CALIBZ")
                        { // オフセット設定
                            setServomotorOffset(stream, CHINESE);
                        }
                        if (data == "CALIBH")
                        { // オフセット設定
                            setServomotorOffset(stream, HIRAGANA);
                        }
                        #endregion
                        #region 言語切り替え
                        if (data == "LANGE")
                        { // 言語切り替え(英語)
                            changeLanguage("en", stream);
                            hiragana = false;
                        }
                        if (data == "LANGJ")
                        { // 言語切り替え(日本語)
                            changeLanguage("ja", stream);
                            hiragana = false;
                        }
                        if (data == "LANGZ")
                        { // 言語切り替え(中国語)
                            changeLanguage("zh", stream);
                            hiragana = false;
                        }
                        if (data == "LANGH")
                        { // 言語切り替え(にほんご)
                            changeLanguage("ja", stream);
                            hiragana = true;
                        }
                        if (data == "LANGK")
                        { // 言語切り替え(韓国語)
                            changeLanguage("ko", stream);
                            hiragana = false;
                        }
                        if (data == "LANGES")
                        { // 言語切り替え(スペイン語)
                            changeLanguage("es", stream);
                            hiragana = false;
                        }
                        #endregion
                        #region その他
                        if (data == "RUN")
                        { // プログラム作成・転送命令
                            makeAndTransferProgram(stream);
                        }
                        if (data == "TEST")
                        { // テストモード命令
                            transferBoardSide(stream);
                        }
                        if (data == "BREAK")
                        { // 通信切断通知
                            break;
                        }
                        if (data == "FINISH")
                        {
                            // 転送完了通知を送信
                            sendMessageToBPE("ACK", stream);
                            finishBPE();

                            clientFinish = true;
                            break;
                        }
                        #endregion
                        // Process the data sent by the client.
                    }
                }
                catch (IOException) {
                    // 切断された場合は接続終了として、再度接続待ちに入る
                }

                // 通信を切断し、再び接続処理へ
                client.Close();
                if (clientFinish) break;
            }
            // 停止処理
            server.Stop();
        }

        //---------------------------------------------------------------------
        // Date  : 2015/--/-- : kagayama 新規作成
        //         2015/08/24 : kagayama 変更後にBPEにメッセージを返すよう変更
        //---------------------------------------------------------------------
        /// <summary>
        /// 指定された言語に変更する
        /// </summary>
        /// <param name="lang">言語コード(ISO 639-1)</param>
        /// <param name="socket">BPEとの通信用ストリーム</param>
        static void changeLanguage(String lang, NetworkStream socket = null)
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
            }
            catch
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            }

            if (socket != null)
            {
                // 言語を変更した旨を送信
                sendMessageToBPE("UPDATE", socket);

                // 転送完了通知を送信
                sendMessageToBPE("FINISH", socket);
            }
        }

        #region 【処理】 テストモード
        //---------------------------------------------------------------------
        // 概要  :【テストモード時】基板へのテストモード用ファイル(.hex)の転送処理
        // 引数  : NetworkStream socket : 4byteの値
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        static void transferBoardSide(NetworkStream socket)
        {
            bool isTransfered = transferHexFile(@TestModePath + TestModeFile);
            if (isTransfered)   // 基板側プログラムの転送成功時
            {
                // 接続ポート情報を送信
                sendMessageToBPE(pm.getStuduinoPort(), socket);

                // 転送完了通知を送信
                sendMessageToBPE("FINISH", socket);
            }
            else                // 基板側プログラムの転送失敗時
            {
                // 接続ポート情報を送信
                sendMessageToBPE("ERR", socket);

                // エラー情報を送信
                sendMessageToBPE(ErrorNumber.ToString(), socket);

                // 転送完了通知を送信
                sendMessageToBPE("FINISH", socket);
            }
        }
        #endregion

        #region 【処理】 プログラム作成・転送
        //---------------------------------------------------------------------
        // 概要  : 【プログラム作成・転送時】ビルド＆転送処理
        // 引数  : NetworkStream socket : 4byteの値
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        static void makeAndTransferProgram(NetworkStream socket)
        {
            //-----------------------------------------------------------------
            // ユーザプログラムのビルド処理
            //-----------------------------------------------------------------
            int ret = bulidUserProgram();
            if (ret == BuildSuccess)
            {   // make成功の場合は、転送処理を実行
                //-------------------------------------------------------------
                // ユーザプログラム(.hex)の転送処理
                //-------------------------------------------------------------
                bool isTransfered = transferHexFile(ArduinoSystemPath + HexFile);
                if (isTransfered)   // 基板側プログラムの転送成功時
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE(pm.getStuduinoPort(), socket);

                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH", socket);
                }
                else                // 基板側プログラムの転送失敗時
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("ERR", socket);

                    // エラー情報を送信
                    sendMessageToBPE(ErrorNumber.ToString(), socket);

                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH", socket);
                }
            }
            else                // 基板側プログラムの転送失敗時
            {
                // 接続ポート情報を送信
                sendMessageToBPE("ERR", socket);

                // エラー情報を送信
                sendMessageToBPE(ErrorNumber.ToString(), socket);

                // 転送完了通知を送信
                sendMessageToBPE("FINISH", socket);
            }
        }
        #endregion

        #region 【処理】 入出力I/O設定
        //---------------------------------------------------------------------
        // 概要  : 【入出力設定】入出力設定処理
        // 引数  : NetworkStream socket : 4byteの値
        //       : int lang : 言語
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        static stRobotIOStatus stRobotIOConf = new stRobotIOStatus();
        static string fname = "Board.cfg";
        const int ScratchIONum = 18;
        static void setBoardIOConfiguration(NetworkStream socket, int lang = 0)
        {
            //-----------------------------------------------------------------
            // I/O設定ファイルを読み込む
            //-----------------------------------------------------------------
            try {
                BinaryReader br = new BinaryReader(File.OpenRead(fname));   // ファイルオープン
                byte[] scratchIO = new byte[ScratchIONum];                  // I/O設定を保存する配列の確保
                for (int i = 0; i < ScratchIONum; i++)
                {
                    // 配列にI/O設定を読み込む
                    scratchIO[i] = br.ReadByte();
                }
                // I/O設定GUIに値を反映
                stRobotIOConf.setStatusByte(scratchIO);

                br.Close();
            }
            catch (FileNotFoundException) {
                // ファイルがない場合、初期設定をI/O設定GUIに設定
                stRobotIOConf.initRobotIOConfiguration();
            }

            string data = "";
            byte[] msg;
            //-----------------------------------------------------------------
            // I/O設定ファイル作成
            //-----------------------------------------------------------------
            // ひらがなの場合のみ言語指定してフォームを作成する
            using (Configure f = hiragana ? new Configure(stRobotIOConf, HIRAGANA) : new Configure(stRobotIOConf))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    stRobotIOConf = f.getConfiguration();
                    File.Delete(fname); // ファイルを削除し、新しくファイルを作成する
                    BinaryWriter bw = new BinaryWriter(File.OpenWrite(fname));
                    List<byte> lst = stRobotIOConf.getStatusByte();
                    foreach (byte elm in lst)
                    {
                        bw.Write(elm);
                    }
                    bw.Close();

                    // 接続ポート情報を送信
                    sendMessageToBPE("UPDATE", socket);
                }
                else
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("CANCEL", socket);
                }
            }

            // 転送完了通知を送信
            sendMessageToBPE("FINISH", socket);

        }
        #endregion

        #region 【処理】 サーボモータの角度校正
        //---------------------------------------------------------------------
        // 概要  : 【サーボモーター角度校正】サーボモーター角度校正処理
        // 引数  : NetworkStream socket : 4byteの値
        //       : int lang : 言語
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        static string servoInfo = "Servo.cfg";
        // サーボモーターのオフセット情報を読み込む
        static void setServomotorOffset(NetworkStream socket, int lang = 0)
        {
            string data = "";   // ブロックプログラミング環境との通信用データ1
            byte[] msg;         // ブロックプログラミング環境との通信用データ2

            // -----------------------------------------------------------------
            // 入出力情報を読み込む
            // -----------------------------------------------------------------
            try {
                BinaryReader br = new BinaryReader(File.OpenRead(fname));   // ファイルオープン
                byte[] scratchIO = new byte[ScratchIONum];                  // I/O設定を保存する配列の確保
                for (int i = 0; i < ScratchIONum; i++)
                {
                    // 配列にI/O設定を読み込む
                    scratchIO[i] = br.ReadByte();
                }
                // I/O設定GUIに値を反映
                stRobotIOConf.setStatusByte(scratchIO);

                br.Close();
            } catch (FileNotFoundException) {
                // ファイルがない場合、初期設定をI/O設定GUIに設定
                stRobotIOConf.initRobotIOConfiguration();
            }

            // -----------------------------------------------------------------
            // 基板側プログラム(.hex)をアップロード
            // -----------------------------------------------------------------
            bool isTransfered = transferHexFile(@TestModePath + SVCalibrationFile);
            if (isTransfered)   // 基板側プログラムの転送成功時
            {
                // -------------------------------------------------------------
                // COMポートをオープンする
                // -------------------------------------------------------------
                // ひらがなの場合のみ言語指定してフォームを作成する
                using (ServoCalib fmCalib = hiragana ? new ServoCalib(svOffset, stRobotIOConf, HIRAGANA) : new ServoCalib(svOffset, stRobotIOConf))
                {
                    bool isOpen = fmCalib.openCOMPort(pm.getStuduinoPort());
                    if (!isOpen)
                    {   // COMポートのオープンに失敗
                        // 接続ポート情報を送信
                        sendMessageToBPE("ERR", socket);


                        // エラー情報を送信
                        sendMessageToBPE(ErrorNumber.ToString(), socket);


                        // 転送完了通知を送信
                        sendMessageToBPE("FINISH", socket);
                        
                        return; // 処理終了
                    }

                    System.Threading.Thread.Sleep(2000);	// １秒停止

                    // -------------------------------------------------------------
                    // サーボモータ校正状態であることをBPEに送信
                    // -------------------------------------------------------------
                    // 入出力ポート設定情報が終了したら、サーボモーター校正処理に入るので
                    // その旨をブロックプログラミング環境に伝える
                    sendMessageToBPE("CALIBRATE", socket);


                    // サーボモータの角度を初期化
                    fmCalib.setupCurrentServoDegrees();

                    // -------------------------------------------------------------
                    // サーボモータ校正ダイアログを表示する
                    // -------------------------------------------------------------
                    DialogResult res = fmCalib.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        // アイコンプログラミング環境との共有ファイルの更新
                        using (FileStream fs = new FileStream(iniFile, FileMode.Create, FileAccess.Write))
                        {
                            // 符号なしバイト型でなければファイルに書き込めないため、byte型にキャストする
                            foreach (byte val in svOffset.getValues())
                            {
                                fs.WriteByte(val);
                            }
                        }
                        using (FileStream fs = new FileStream(iniDC, FileMode.Create, FileAccess.Write))
                        {
                            fs.WriteByte(svOffset.getDCCalibInfo().calibM1Rate);
                            fs.WriteByte(svOffset.getDCCalibInfo().calibM2Rate);
                        }

                        fmCalib.closeCOMPort(); // COMポートをクローズ

                        // 転送完了通知を送信
                        sendMessageToBPE("FINISH", socket);
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        if (fmCalib.getErrorCode() == (byte)ConnectingCondition.DISCONNECT)
                        {
                            fmCalib.closeCOMPort(); // COMポートをクローズ

                            // 転送完了通知を送信
                            sendMessageToBPE("ERR", socket);
                        }
                        else
                        {
                            fmCalib.closeCOMPort(); // COMポートをクローズ

                            // 転送完了通知を送信
                            sendMessageToBPE("FINISH", socket);
                        }
                    }
                }
            }
            else                // 基板側プログラムの転送失敗時
            {
                // 接続ポート情報を送信
                sendMessageToBPE("ERR", socket);

                // エラー情報を送信
                sendMessageToBPE(ErrorNumber.ToString(), socket);

                // 転送完了通知を送信
                sendMessageToBPE("FINISH", socket);
            }
        }
        #endregion

        /// <summary>
        /// ブロックプログラミングにメッセージを送信
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="stream">データストリーム</param>
        /// <param name="delim">区切り文字</param>
        static void sendMessageToBPE(String message, NetworkStream stream, String delim = ".")
        {
            String sendMessage = message + delim + "\r\n";   // 末尾の改行(\r\n)は必須, delimはスクラッチ側で文字検出を容易にするため
            byte[] data = System.Text.Encoding.ASCII.GetBytes(sendMessage);
            stream.Write(data, 0, data.Length);
            //Debug.WriteLine("Sent: {0}", message);
        }

        //---------------------------------------------------------------------
        // Date  : 2015/10/26 : kagayama     新規作成
        //       : 2015/10/30 : kagayama     COMポートマネージャ適用
        //       : 2016/03/31 : kagayama     avrdude実行時のフリーズ対策
        //---------------------------------------------------------------------
        /// <summary>
        /// 指定されたhexファイルを送信する
        /// </summary>
        /// <param name="hexFile">Intel HEXファイル</param>
        /// <returns><br>true:成功</br><br>false:失敗</br></returns>
        static StringBuilder sbError;
        static bool transferHexFile(string hexFile)
        {
            string comPortName = null;
            try
            {
                comPortName = pm.getStuduinoPort();
            }
            catch (ComPortException)
            {
                ErrorNumber = 6;
            }

            // COMポートが見つからない、またはエラーが発生した場合は終了
            if(comPortName == null)
                ErrorNumber = 1;
            if(ErrorNumber != 0)
                return false;

            Debug.WriteLine("COM Port: " + comPortName);


            // 転送実行
            Debug.Write("---------- 転送処理実行 ----------\n");
            string exe = ArduinoSystemPath + Transfer;                           // ファイル転送コマンド
            string arg = "-C" + ArduinoSystemPath + ConfFile + " " + TransferOption + " " 
                + @"-P\\.\" + comPortName + " " + "-Uflash:w:" + hexFile + ":i";

            Debug.Write("[Command]\n");
            Debug.Write(exe);
            Debug.Write(" ");
            Debug.Write(arg);
            Debug.Write("\n");
            Process p = new Process();
            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = arg;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            try
            {
                p.Start();
                sbError = new StringBuilder();
                p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
                p.BeginErrorReadLine();
            }
            catch
            {
                ErrorNumber = 5;    // avrdudeの起動でエラー
                return false;
            }

            if (!p.WaitForExit(7000))
            {// 環境やタイミングによって、avrdude実行後の通信失敗時に余計なリトライが入るためタイムアウトを設定する。
                Debug.WriteLine("avrdude: TIMEOUT");
                p.Kill();
                ErrorNumber = 4;
                return false;
            }

            Thread.Sleep(500); // StringBuilderへの書き込み完了を待つ
            string erro = sbError.ToString();

            if (erro.Contains("can't open device"))
            {
                ErrorNumber = 2;    // 既にポートがオープンされている
                return false;
            }
            if (erro.Contains("not in sync: resp="))
            {
                ErrorNumber = 3;    // Studuinoと同期が取れない
                return false;
            }
            if (erro.Contains("write error:"))
            {
                ErrorNumber = 4;    // 書き込みエラー
                return false;
            }
            if (erro.Contains("programmer is out of sync"))
            {
                ErrorNumber = 4;    // 書き込みエラー
                return false;
            }


            return true;
        }

        /// <summary>
        /// avrdude実行時の出力を随時StringBuilderに追加する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            sbError.Append(e.Data);
            Debug.WriteLine(e.Data);
        }

        //---------------------------------------------------------------------
        // Date  : 2015/10/26 : kagayama     新規作成
        //---------------------------------------------------------------------
        /// <summary>
        /// Studuinoが接続されているCOMポートを検索する。
        /// </summary>
        /// <returns>COMポート名(COMxx)</returns>
        static string searchStuduinoBoard()
        {
            string comPortName = null;
            ManagementClass mcW32SerPort = new ManagementClass("Win32_PnPEntity");  // プラグアンドプレイのデバイスを全て取得
            foreach (ManagementObject aSerialPort in mcW32SerPort.GetInstances())   // プラグアンドプレイのデバイスからArtec Rocotに接続されているUSBシリアルのCOMポートを取得する
            {
                // "Prolific"の文字を検索する
                String id = (String)aSerialPort.GetPropertyValue("DeviceID");
                String name = (String)aSerialPort.GetPropertyValue("Name");
                String service = (String)aSerialPort.GetPropertyValue("Service");
                String manufacturer = (String)aSerialPort.GetPropertyValue("Manufacturer");
                UInt32 errorCode = (UInt32)aSerialPort.GetPropertyValue("ConfigManagerErrorCode");

                if (manufacturer == "Prolific" && service == "Ser2pl")
                {
                    // "COM"の文字を検索する Prolific USB-to-Serial Comm Port (COM4)
                    int n = name.IndexOf("COM");
                    int m = name.IndexOf(")");
                    comPortName = name.Substring(n, m - n);
                    if(errorCode != 0)
                        ErrorNumber = 6;    // USB接続が開始できない(COMポートエラー)
                    break;
                }
            }
            return comPortName;
        }

        #region 【共通】 ユーザプログラムのビルド
        // ---------------------------------------------------------------------
        // Date  : 2013/07/06 kawase  0.01    新規作成
        //---------------------------------------------------------------------
        //       : 2015/10/26 : kagayama     コンパイル時、warningのみの場合でも処理が終了してしまう不具合修正
        // ---------------------------------------------------------------------
        /// <summary>
        /// ユーザプログラムのビルド
        /// </summary>
        /// <returns></returns>
        static public int bulidUserProgram()
        {
            try
            {
                string arg = null;
                string exe = null;

                // コンパイル実行
                Debug.Write("---------- コンパイル ----------\n");
                exe = ArduinoSystemPath + Compiler;                           // コンパイラの設定
                arg = CompilerOption + " ";                                   // オプション
                for (int i = 0; i < IncludeFiles.Length; i++)
                {
                    arg += "-I" + ArduinoSystemPath + IncludeFiles[i] + " ";    // インクルードファイル指定
                }

                arg += UserCodePath + SourceFile + " ";             // ソースファイル
                arg += "-o " + ArduinoSystemPath + ObjectFile;                               // オブジェクトファイル指定

                Debug.Write("[Command]\n");
                Debug.Write(exe);
                Debug.Write(" ");
                Debug.Write(arg);
                Debug.Write("\n");

                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = exe;                                      // 実行するファイル
                psInfo.Arguments = arg;                                     // 引数設定
                psInfo.CreateNoWindow = true;                               // コンソール・ウィンドウを開かない
                psInfo.UseShellExecute = false;                             // シェル機能を使用しない
                psInfo.RedirectStandardError = true;
                psInfo.RedirectStandardOutput = false;
                Process p = Process.Start(psInfo);
                string error = p.StandardError.ReadToEnd();               // 標準出力の読み取り
                p.WaitForExit();
                if (error != "")
                {
                    string line = "";
                    error = error.Replace("\r\r\n", "\n");                    // 改行コードの修正

                    StringReader sr = new StringReader(error);
                    int errorFlag = 0;  // 0x01:サブルーチン、 0x02:ブロック未接続、0x04:0による割り算
                    //ErrorNumber = 16;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains("was not declared in this scope"))
                        {   // 関数未定義が検出された場合
                            if ((errorFlag & 0x1) == 0)
                            {
                                errorFlag |= 0x1;   // フラグを立てる
                                //ErrorNumber += 0x1;
                            }
                        }
                        else if (line.Contains("error:"))
                        {   // その他のエラー
                            if ((errorFlag & 0x2) == 0)
                            {
                                errorFlag |= 0x2;
                                //ErrorNumber += 0x2;
                            }
                        }
                        else if (line.Contains("warning:"))
                        {
                            // Warningは無視する
                        }
                    }
                    if (errorFlag != 0)
                    {
                        writeCompileLog("CompileError", error);
                        ErrorNumber = 16 + errorFlag;
                        return BuildError;
                    }
                }

                // アーカイバ実行
                Debug.Write("---------- アーカイブ ----------\n");
                exe = ArduinoSystemPath + Archiver;                         // アーカイバの設定
                psInfo.FileName = exe;                                      // 実行するファイル
                for (int i = 0; i < SystemObjectFiles.Length; i++)
                {
                    arg = ArchiverOption + " ";
                    arg += ArduinoSystemPath + ArchiverFile + " " + ArduinoSystemPath + SystemObjectFiles[i];

                    Debug.Write("[Command]\n");
                    Debug.Write(exe);
                    Debug.Write(" ");
                    Debug.Write(arg);
                    Debug.Write("\n");

                    psInfo.Arguments = arg;                                 // 引数設定
                    p = Process.Start(psInfo);
                    error = p.StandardError.ReadToEnd();               // 標準出力の読み取り
                    p.WaitForExit();
                    if (error != "")
                    {
                        ErrorNumber = 9;
                        return BuildError;
                    }
                }

                // リンカ実行
                Debug.Write("---------- リンク ----------\n");
                exe = ArduinoSystemPath + Linker;                       // リンカの設定
                arg = LinkerOption + " ";
                arg += "-o " + ArduinoSystemPath + ElfFile + " ";
                arg += ArduinoSystemPath + ObjectFile + " ";
                arg += ArduinoSystemPath + LinkFile + " ";
                arg += ArduinoSystemPath + ArchiverFile + " ";
                arg += "-L" + ArduinoSystemPath + LinkDirectory;

                Debug.Write("[Command]\n");
                Debug.Write(exe);
                Debug.Write(" ");
                Debug.Write(arg);
                Debug.Write("\n");

                psInfo.FileName = exe;                                      // 実行するファイル
                psInfo.Arguments = arg;                                     // 引数設定
                p = Process.Start(psInfo);
                error = p.StandardError.ReadToEnd();               // 標準出力の読み取り

                p.WaitForExit();
                if (error != "")
                {
                    writeCompileLog("LinkError", error);
                    ErrorNumber = 7;
                    return BuildError;
                }

                // オブジェクトダンプ実行
                Debug.Write("---------- オブジェクトダンプ ----------\n");
                exe = ArduinoSystemPath + ObjDump;                       // リンカの設定
                arg = "-h " + ArduinoSystemPath + ElfFile;

                Debug.Write("[Command]\n");
                Debug.Write(exe);
                Debug.Write(" ");
                Debug.Write(arg);
                Debug.Write("\n");

                psInfo.FileName = exe;                                      // 実行するファイル
                psInfo.Arguments = arg;                                     // 引数設定
                psInfo.RedirectStandardOutput = true;                       // 標準出力をリダイレクト
                p = Process.Start(psInfo);
                string message = p.StandardOutput.ReadToEnd();               // 標準出力の読み取り
                p.WaitForExit();

                Debug.Write(message);
                // オブジェダンプ結果から、.textと.bssのサイズを取得する
                string[] token = message.Split(' ');        // 空白で分割する
                string textSize = "";
                string bssSize = "";
                for (int i = 0; i < token.Length; i++)
                {
                    if (token[i] == ".text")
                    {   // .textサイズを取得
                        for (int j = i + 1; ; j++)
                        {
                            // .textの次にSizeが来るので、その値を取得する
                            if (token[j] == "")
                            {   // 空白は読み飛ばす
                                continue;
                            }
                            else
                            {   // .textの次の文字列
                                textSize = token[j];
                                i = j;
                                break;
                            }
                        }
                    }
                    else if (token[i] == ".bss")
                    {   // .bssサイズを取得
                        for (int j = i + 1; ; j++)
                        {
                            // .bssの次にSizeが来るので、その値を取得する
                            if (token[j] == "")
                            {   // 空白は読み飛ばす
                                continue;
                            }
                            else
                            {   // .bssの次の文字列
                                bssSize = token[j];
                                break;
                            }
                        }
                        break;  // トークンループを抜ける
                    }
                }
                int ts = Convert.ToInt32(textSize, 16);
                int bs = Convert.ToInt32(bssSize, 16);
                int programSize = ts + bs;      // プログラムサイズ
                Debug.Write("プログラムサイズ：" + programSize + "\n");

                // プログラムサイズが規定値以上になった場合、オーバーフローを返す
                if (programSize > MAXPROGRAMSIZE)
                {
                    writeCompileLog("Program size overflow.", programSize.ToString());
                    ErrorNumber = 8;
                    return BuildFlashOverflow;
                }

                // オブジェクトコピー実行
                Debug.Write("---------- オブジェクトコピー実行 ----------\n");
                exe = ArduinoSystemPath + Objcopy;
                arg = ObjcopyOption1 + " ";
                arg += ArduinoSystemPath + ElfFile + " ";
                arg += ArduinoSystemPath + EepFile;

                Debug.Write("[Command]\n");
                Debug.Write(exe);
                Debug.Write(" ");
                Debug.Write(arg);
                Debug.Write("\n");

                psInfo.FileName = exe;                            // 実行するファイル
                psInfo.Arguments = arg;                           // 引数設定
                p = Process.Start(psInfo);
                error = p.StandardError.ReadToEnd();              // 標準出力の読み取り
                p.WaitForExit();
                if (error != "")
                {
                    error = error.Replace("\r\r\n", "\n");        // 改行コードの修正
                    ErrorNumber = 10;
                    return BuildError;
                }

                // オブジェクトコピー実行
                Debug.Write("---------- オブジェクトコピー実行 ----------\n");
                exe = ArduinoSystemPath + Objcopy;
                arg = ObjcopyOption2 + " ";
                arg += ArduinoSystemPath + ElfFile + " ";
                arg += ArduinoSystemPath + HexFile;

                Debug.Write("[Command]\n");
                Debug.Write(exe);
                Debug.Write(" ");
                Debug.Write(arg);
                Debug.Write("\n");

                psInfo.FileName = exe;                             // 実行するファイル
                psInfo.Arguments = arg;                            // 引数設定
                p = Process.Start(psInfo);
                error = p.StandardError.ReadToEnd();               // 標準出力の読み取り
                p.WaitForExit();
                if (error != "")
                {
                    error = error.Replace("\r\r\n", "\n");         // 改行コードの修正
                    ErrorNumber = 11;
                    return BuildError;
                }
                return BuildSuccess;
            }
            catch
            {
                ErrorNumber = 5;
                return BuildError;
            }
        }
        #endregion

        #region 【共通】 ログ出力
        // ---------------------------------------------------------------------
        // 概要	      : 予期せぬ通信例外時のログを取得
        // 引数       : string message  ログメッセージ
        // Date       : 2013/07/11 kawase  0.01    新規作成
        // ---------------------------------------------------------------------
        static void writeCompileLog(string title, string message)
        {
            DateTime dt = DateTime.Now;
            // ファイル名の作成 (年_月_日_時_分_秒.log)
            string logFileName = dt.Year + "_" + dt.Month + "_" + dt.Day + "_" + dt.Hour + "_" + dt.Minute + "_" + dt.Second;
            string log = "[" + title + "]\r\n";
            log += "[message]\t" + message + "\r\n";

            File.WriteAllText(logFileName + ".log", log);
        }
        #endregion

        #region 【共通】 エンディアン変換
        //---------------------------------------------------------------------
        // 概要  : リトルエンディアン→ビッグエンディアン変換
        // 引数  : int num  : 4byteの値
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        static UInt32 convertEndianUINT(int num)
        {
            UInt32 high_h = (UInt32)((num << 24) & 0xFF000000);
            UInt32 high_l = (UInt32)((num << 8) & 0x00FF0000);
            UInt32 low_h = (UInt32)((num >> 8) & 0x0000FF00);
            UInt32 low_l = (UInt32)((num >> 24) & 0x000000FF);

            return (high_h | high_l | low_h | low_l);
        }
        #endregion

        /// <summary>
        /// ブロックプログラミング環境を終了する
        /// </summary>
        static void finishBPE()
        {
            System.Diagnostics.Process[] ps =
                System.Diagnostics.Process.GetProcessesByName("block");

            foreach (System.Diagnostics.Process p in ps)
            {
                //IDとメインウィンドウのキャプションを出力する
                Debug.WriteLine("{0}/{1}", p.Id, p.MainWindowTitle);
                p.CloseMainWindow();
                p.Kill();
            }
        }
    }

    // ---------------------------------------------------------------------
    // Date       : 2015/10/30 kagayama    新規作成
    // Date       : 2015/11/11 kagayama    searchStuduino更新
    // ---------------------------------------------------------------------
    /// <summary>
    /// Studuinoが接続されているCOMポートの管理、検索等を行う。
    /// </summary>
    class PortManager
    {
        private string lastOpenedPort;

        /// <summary>
        /// 最後に接続したポートが有効か確認し、有効であればそのポート名を返す。無効ならWMIで検索。
        /// </summary>
        /// <returns>ポート名</returns>
        public string getStuduinoPort()
        {
            string comPortName = null;

            // 前回使用したポートがあれば、有効かどうかチェックする
            if (lastOpenedPort != null)
            {
                foreach (String elm in SerialPort.GetPortNames())
                {
                    // 前回のCOMポートが有効か確認
                    if (elm == lastOpenedPort)
                    {
                        comPortName = elm;
                        Debug.WriteLine("Last opend " + elm + " is available.");
                        break;
                    }
                }
            }
            if (comPortName == null)
            {
                try
                {
                    comPortName = searchStuduino();     // Studuinoが接続されているポートを検索する。
                }
                catch (Exception e)
                {
                    if (e is ManagementException || e is UnauthorizedAccessException)
                    {
                        // 前回使用したポートが見つからなければダイアログを表示し、ユーザーに選択を促す
                        using (PortSelector diag = new PortSelector(SerialPort.GetPortNames()))
                        {
                            // WMIのエラーは警告を表示する
                            if (e is ManagementException)
                                MessageBox.Show(Properties.Resources.str_msg_err_wmi, "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            // COMポート選択ダイアログを出す
                            diag.ShowDialog();
                            if (diag.DialogResult == DialogResult.OK)
                            {
                                comPortName = diag.getPort();
                            }
                        }
                    }
                    if (e is ComPortException) throw;
                }
            }
            lastOpenedPort = comPortName;

            return comPortName;
        }

        // ---------------------------------------------------------------------
        // Date       : 2015/10/30 kagayama    新規作成
        // Date       : 2015/11/11 kagayama    検索方法の修正
        //            : 2015/12/21 kagayama    nameがNull参照にならないよう修正
        // ---------------------------------------------------------------------
        /// <summary>
        /// WMIでStuduinoが接続されているCOMポートを検索する。
        /// </summary>
        /// <returns>ポート名</returns>
        private string searchStuduino()
        {
            string comPortName = null;
            ManagementClass mcW32SerPort = new ManagementClass("Win32_PnPEntity");  // プラグアンドプレイのデバイスを全て取得
            foreach (ManagementObject aSerialPort in mcW32SerPort.GetInstances())   // プラグアンドプレイのデバイスからArtec Rocotに接続されているUSBシリアルのCOMポートを取得する
            {
                // "Prolific"の文字を検索する
                String id = (String)aSerialPort.GetPropertyValue("DeviceID");
                String name = (String)aSerialPort.GetPropertyValue("Name");
                if (id == null || name == null) continue;

                if (id != null && name.Contains("Prolific"))
                {
                    // "COM"の文字を検索する Prolific USB-to-Serial Comm Port (COM4)
                    int n = name.IndexOf("COM");
                    int m = name.IndexOf(")");
                    comPortName = name.Substring(n, m - n);

                    // COMポートエラーチェック
                    UInt32 errorCode = (UInt32)aSerialPort.GetPropertyValue("ConfigManagerErrorCode");
                    if (errorCode != 0)
                        //ErrorNumber = 6;    // USB接続が開始できない(COMポートエラー)
                        throw new ComPortException("COM Por Error", errorCode);
                    break;
                }
            }
            return comPortName;
        }
    }

    // ---------------------------------------------------------------------
    // Date       : 2015/10/30 kagayama    新規作成
    // ---------------------------------------------------------------------
    /// <summary>
    /// COMポートエラー発生時にWMIエラーコードを送る
    /// </summary>
    class ComPortException : Exception
    {
        public UInt32 errorCode { get; set; }
        public ComPortException(string message, UInt32 errorCode)
            : base(message)
        {
            this.errorCode = errorCode;
        }
    }

    // ---------------------------------------------------------------------
    // Date       : 2015/11/13 kagayama    新規作成
    // ---------------------------------------------------------------------
    /// <summary>
    /// ダミーフォーム。起動時に表示され、すぐに消える。
    /// </summary>
    class Dummy : Form
    {
        public Dummy()
            : base()
        {
            this.Size = new System.Drawing.Size(0, 0);
            this.Shown += new EventHandler(Dummy_Shown);
        }

        void Dummy_Shown(object sender, EventArgs e)
        {
            Close();
        }
    }
}
