using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Artec.BuildConst;
using Artec.TestModeCommunication;
using ScratchConnection.Forms;

namespace ScratchConnection
{
    // 基板タイプ
    public struct BoardType
    {
        public int id;
        public string message;
        public BoardType(int id, string message)
        {
            this.id = id;
            this.message = message;
        }
        public static BoardType STUDUINO = new BoardType(0, "STANDARD");
        public static BoardType STUDUINO_MINI = new BoardType(1, "MINI");
        public static BoardType STUDUINO_AND_MINI = new BoardType(2, "STANDMINI");

        public static BoardType FromId(int id)
        {
            if (id == 0)
                return STUDUINO;
            if (id == 1)
                return STUDUINO_MINI;
            if (id == 2)
                return STUDUINO_AND_MINI;

            throw new ArgumentOutOfRangeException("id", "ID must be 0, 1, or 2.");
        }
    }

    class Utility
    {
        Studuino st;
        BoardType currentBT;
        NetworkStream stream;
        PortManager pm;
        TestModeCommunication tcom = new TestModeCommunication();  // テストモード通信用(Studuino mini専用)

        const int BuildError = 0;           // ビルドエラー
        const int BuildSuccess = 1;         // ビルド成功
        const int BuildFlashOverflow = 2;   // FLASHオーバーフロー
        // 多言語対応
        //const int ENGLISH = 0;
        //const int JAPANESE = 1;
        //const int CHINESE = 2;
        const int HIRAGANA = 3;
        // サーボモーターのオフセット情報
        const string iniFile = @"..\common\sv_offset_ini";   // オフセット用設定ファイル
        const string iniDC = @"..\common\dc_calib_ini";   // DCモーター校正用設定ファイル
        const string BOARD_IO = "Board.cfg";
        ServoOffset svOffset = new ServoOffset(iniDC, iniFile);
        bool hiragana = false;      // ひらがなフラグ
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
        int ErrorNumber = 0;

        public Utility(Studuino st)
        {
            this.st = st;
        }

        public Utility()
        {
            currentBT = BoardType.STUDUINO;
            st = new Studuino();
            pm = new PortManager();    // COMポート管理クラス
        }

        public void setStream(NetworkStream stream)
        {
            this.stream = stream;
        }

        public int compile()
        {
            // コンパイル実行
            Debug.Write("---------- コンパイル ----------\n");
            int errorNumber = 0;
            string exe = st.ArduinoSystemPath + st.Compiler;
            string arg = st.CompilerOption + " ";
            // インクルードファイル指定
            for (int i = 0; i < st.IncludeFiles.Length; i++)
            {
                arg += "-I" + st.ArduinoSystemPath + st.IncludeFiles[i] + " ";
            }

            arg += st.UserCodePath + st.SourceFile + " ";             // ソースファイル
            arg += "-o " + st.ArduinoSystemPath + st.ObjectFile;      // オブジェクトファイル指定

            Debug.Write("[Command]\n");
            Debug.Write(exe);
            Debug.Write(" ");
            Debug.Write(arg);
            Debug.Write("\n");

            Process p = Process.Start(createProcessInfo(exe,arg));
            string error = p.StandardError.ReadToEnd();               // 標準出力の読み取り
            p.WaitForExit();

            if (error != "")
            {
                string line = "";
                error = error.Replace("\r\r\n", "\n");                // 改行コードの修正
                writeCompileLog("CompileError", error);

                StringReader sr = new StringReader(error);
                int errorFlag = 0;  // 0x01:サブルーチン、 0x02:ブロック未接続、0x04:0による割り算
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("was not declared in this scope"))
                    {   // 関数未定義が検出された場合
                        if ((errorFlag & 0x1) == 0)
                        {
                            errorFlag |= 0x1;   // フラグを立てる
                            errorNumber = 17;
                        }
                    }
                    else if (line.Contains("error:"))
                    {   // その他のエラー
                        if ((errorFlag & 0x2) == 0)
                        {
                            errorFlag |= 0x2;
                            errorNumber = 18;
                        }
                    }
                    else if (line.Contains("warning:"))
                    {
                        // Warningは無視する
                    }
                }
            }
            return errorNumber;
        }

        public int link()
        {                // リンカ実行
            Debug.Write("---------- リンク ----------\n");
            int errorNumber = 0;
            string exe = st.ArduinoSystemPath + st.Linker;                       // リンカの設定
            string arg = st.LinkerOption + " ";
            arg += "-o " + st.ArduinoSystemPath + st.ElfFile + " ";
            arg += st.ArduinoSystemPath + st.ObjectFile + " ";
            //foreach (string elm in st.SystemObjectFiles)
            //{
            //    arg += st.ArduinoSystemPath + elm + " ";
            //}
            arg += st.ArduinoSystemPath + st.ArchiverFile + " ";
            arg += "-L" + st.ArduinoSystemPath + st.LinkDirectory;

            Debug.Write("[Command]\n");
            Debug.Write(exe);
            Debug.Write(" ");
            Debug.Write(arg);
            Debug.Write("\n");

            Process p = Process.Start(createProcessInfo(exe,arg));
            string error = p.StandardError.ReadToEnd();               // 標準出力の読み取り

            p.WaitForExit();

            if (error != "")
            {
                writeCompileLog("LinkError", error);
                errorNumber = 7;
            }
            return errorNumber;
        }

        public int objDump()
        {
            // オブジェクトダンプ実行
            Debug.Write("---------- オブジェクトダンプ ----------\n");
            int errorNumber = 0;
            string exe = st.ArduinoSystemPath + st.ObjDump;                       // リンカの設定
            string arg = "-h " + st.ArduinoSystemPath + st.ElfFile;

            Debug.Write("[Command]\n");
            Debug.Write(exe);
            Debug.Write(" ");
            Debug.Write(arg);
            Debug.Write("\n");

            ProcessStartInfo psInfo = createProcessInfo(exe, arg);
            psInfo.RedirectStandardOutput = true;                       // 標準出力をリダイレクト
            Process p = Process.Start(psInfo);

            string dumpResult = p.StandardOutput.ReadToEnd();               // 標準出力の読み取り
            p.WaitForExit();

            Debug.Write(dumpResult);
            // オブジェダンプ結果から、.data .text .bssのサイズを取得する
            string[] token = dumpResult.Split(' ');        // 空白で分割する
            string dataSize = "";
            string textSize = "";
            string bssSize = "";
            for (int i = 0; i < token.Length; i++)
            {
                if (token[i] == ".data")
                {   // .dataサイズを取得
                    for (int j = i + 1; ; j++)
                    {
                        // .dataの次にSizeが来るので、その値を取得する
                        if (token[j] == "")
                        {   // 空白は読み飛ばす
                            continue;
                        }
                        else
                        {   // .dataの次の文字列
                            dataSize = token[j];
                            i = j;
                            break;
                        }
                    }
                }
                else if (token[i] == ".text")
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
            int ds = Convert.ToInt32(dataSize, 16);
            int ts = Convert.ToInt32(textSize, 16);
            int bs = Convert.ToInt32(bssSize, 16);
            int sizeFLASH = ds + ts;      // FLASHサイズ
            int sizeSRAM = ds + bs;       // SRAMサイズ
            Debug.Write("Flash：" + sizeFLASH + "\n");
            Debug.Write("SRAM ：" + sizeSRAM + "\n");

            // プログラムサイズが規定値以上になった場合、オーバーフローを返す
            if (sizeFLASH > st.MAXPROGRAMSIZE)
            {
                writeCompileLog("Program size overflow.", sizeFLASH.ToString());
                errorNumber = 8;
            }

            return errorNumber;
        }

        public int objCopy()
        {
            // オブジェクトコピー実行
            Debug.Write("---------- オブジェクトコピー実行 ----------\n");
            int errorNumber = 0;
            string exe = st.ArduinoSystemPath + st.Objcopy;
            string arg = st.ObjcopyOption2 + " ";
            arg += st.ArduinoSystemPath + st.ElfFile + " ";
            arg += st.ArduinoSystemPath + st.HexFile;

            Debug.Write("[Command]\n");
            Debug.Write(exe);
            Debug.Write(" ");
            Debug.Write(arg);
            Debug.Write("\n");

            Process p = Process.Start(createProcessInfo(exe, arg));
            string error = p.StandardError.ReadToEnd();               // 標準出力の読み取り
            p.WaitForExit();
            if (error != "")
            {
                error = error.Replace("\r\r\n", "\n");         // 改行コードの修正
                errorNumber = 11;
            }

            return errorNumber;
        }

        public void mergeHex()
        {
            using (StreamWriter sw = new StreamWriter("test", false, System.Text.Encoding.Default))
            {
                using (StreamReader sr = new StreamReader(st.ArduinoSystemPath + st.HexFile, System.Text.Encoding.Default))
                {
                    while (sr.Peek() >= 0)
                    {
                        // ファイルを 1 行ずつ読み込む
                        string stBuffer = sr.ReadLine();
                        // 最終行は飛ばす
                        if (sr.Peek() >= 0)
                            sw.WriteLine(stBuffer);
                    }
                    sr.Close();
                }
                using (StreamReader sr = new StreamReader(@"..\common\optiboot_pro_8MHz.hex", System.Text.Encoding.Default))
                {
                    while (sr.Peek() >= 0)
                    {
                        sw.WriteLine(sr.ReadLine());
                    }
                    sr.Close();
                }
                sw.Close();
            }
            System.IO.File.Copy("test", st.ArduinoSystemPath + st.HexFile, true);
        }

        /// <summary>
        /// Avrdudeによる転送
        /// </summary>
        /// <param name="port">COMポート</param>
        /// <param name="hex">転送するファイル(hex)</param>
        /// <returns>エラーコード</returns>
        public int transferAvrdude(string port, string hex)
        {
            int result = 0;
            string exe = st.ArduinoSystemPath + st.Transfer;                           // ファイル転送コマンド
            string arg = "-C" + st.ArduinoSystemPath + st.ConfFile + " " + st.TransferOption + " "
                + @"-P\\.\" + port + " " + "-Uflash:w:" + hex + ":i";
            StringBuilder errorAvrdude = new StringBuilder();

            Process p = new Process();
            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = arg;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            try
            {
                p.Start();
                p.ErrorDataReceived += (sender, e) => { if (e.Data != null) { errorAvrdude.AppendLine(e.Data); } }; // 標準エラー出力に書き込まれた文字列を取り出す
                p.BeginErrorReadLine();
            }
            catch
            {
                result = 5;    // avrdudeの起動でエラー
            }

            if (!p.WaitForExit(7000))
            {// 環境やタイミングによって、avrdude実行後の通信失敗時に余計なリトライが入るためタイムアウトを設定する。
                Debug.WriteLine("avrdude: TIMEOUT");
                p.Kill();
                result = 4;
                return result;
            }

            Thread.Sleep(500); // StringBuilderへの書き込み完了を待つ
            string erro = errorAvrdude.ToString();

            if (erro.Contains("can't open device"))
            {
                result = 2;    // 既にポートがオープンされている
            }
            if (erro.Contains("not in sync: resp="))
            {
                result = 3;    // Studuinoと同期が取れない
            }
            if (erro.Contains("write error:"))
            {
                result = 4;    // 書き込みエラー
            }
            if (erro.Contains("programmer is out of sync"))
            {
                result = 4;    // 書き込みエラー
            }

            return result;
        }

        //---------------------------------------------------------------------
        // Date  : 2016/**/** :    新規作成
        //       : 2016/08/04 :    エラーの内容をより細かく返すよう修正
        //---------------------------------------------------------------------
        /// <summary>
        /// bootloadHIDによる転送
        /// </summary>
        /// <param name="hex">転送するファイル(hex)</param>
        /// <returns>エラーコード</returns>
        public int transferBootloadHID(string hex)
        {
            int result = 0;
            Process p1 = new Process();
            p1.StartInfo.FileName = st.ArduinoSystemPath + "bootloadHID.exe";
            p1.StartInfo.Arguments = "-r " + hex;
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.RedirectStandardError = true;
            try
            {
                p1.Start();

                string erro = p1.StandardError.ReadToEnd();
                Debug.Write(erro);
                p1.WaitForExit();

                if (erro.Contains("The specified device was not found"))
                {
                    result = 1;    // 基板が接続されていない
                }
                if (erro.Contains("Error opening HIDBoot device: Communication error with device"))
                {
                    result = 2;    // 接続されているが、HIDデバイスとして動作していない（ブートローダー実行中でない）
                }
                if (erro.Contains("Error uploading data block: Communication error with device"))
                {
                    result = 4;    // 書き込みエラー
                }
            }
            catch
            {
                result = 5;    // その他のエラー
            }

            return result;
        }

        /// <summary>
        /// bootloadHIDによる転送
        /// </summary>
        /// <param name="hex">転送するファイル(hex)</param>
        /// <returns>エラーコード</returns>
        public int transferHidaspx(string hex)
        {
            int result = 0;
            string exe = st.ArduinoSystemPath + "hidspx-gcc.exe";                           // ファイル転送コマンド
            string arg = hex + " -ph";

            Process p = new Process();
            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = arg;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            try
            {
                p.Start();

                string erro = p.StandardError.ReadToEnd();
                Debug.Write(erro);
                p.WaitForExit();

                if (erro.Contains("error"))
                {
                    result = 4;    // 基板が接続されていない
                }
            }
            catch
            {
                result = 5;    // avrdudeの起動でエラー
            }

            return result;
        }

        /// <summary>
        /// BMで標準的に使用するProcessStartInfoを返す。
        /// </summary>
        /// <param name="exe">実行する外部プログラム名</param>
        /// <param name="arg">引数</param>
        /// <returns></returns>
        private ProcessStartInfo createProcessInfo(string exe, string arg)
        {
            ProcessStartInfo psInfo = new ProcessStartInfo();
            psInfo.FileName = exe;                                      // 実行するファイル
            psInfo.Arguments = arg;                                     // 引数設定
            psInfo.CreateNoWindow = true;                               // コンソール・ウィンドウを開かない
            psInfo.UseShellExecute = false;                             // シェル機能を使用しない
            psInfo.RedirectStandardError = true;
            psInfo.RedirectStandardOutput = false;

            return psInfo;
        }

        public static int ExecCommand(string exe, string arg)
        {
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
                throw new ExecException("実行エラー", 0, error);
            }

            return 0;
        }

        public class ExecException : Exception
        {
            public UInt32 errorCode { get; set; }
            public string errorMessage { get; set; }


            public ExecException(string message, UInt32 errorCode, string errorMessage)
                : base(message)
            {
                this.errorCode = errorCode;
                this.errorMessage = errorMessage;
            }
        }

        #region 【共通】 ログ出力
        // ---------------------------------------------------------------------
        // Date       : 2013/07/11 kawase  0.01    新規作成
        // ---------------------------------------------------------------------
        /// <summary>
        /// ビルド時のエラーログ
        /// </summary>
        /// <param name="title">見出し</param>
        /// <param name="message">エラー内容</param>
        private void writeCompileLog(string title, string message)
        {
            DateTime dt = DateTime.Now;
            // ファイル名の作成 (年_月_日_時_分_秒.log)
            string logFileName = dt.Year + "_" + dt.Month + "_" + dt.Day + "_" + dt.Hour + "_" + dt.Minute + "_" + dt.Second;
            string log = "[" + title + "]\r\n";
            log += "[message]\t" + message + "\r\n";

            File.WriteAllText(logFileName + ".log", log);
        }
        #endregion

        //---------------------------------------------------------------------
        // Date  : 2015/10/26 : kagayama     新規作成
        //       : 2015/10/30 : kagayama     COMポートマネージャ適用
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        // Date  : 2016/**/** :    新規作成
        //       : 2016/08/04 :    Studuino mini リセット待ちの削除
        //---------------------------------------------------------------------
        /// <summary>
        /// 基板への転送処理
        /// </summary>
        /// <param name="hexFile">転送するhexファイル</param>
        /// <returns>true:成功 false:失敗</returns>
        private bool transferHexFile(string hexFile)
        {
            if (currentBT.Equals(BoardType.STUDUINO))
            {
                ////////////////////////////////////////////////////////////////////////
                //                          Studuino
                ////////////////////////////////////////////////////////////////////////
                string comPortName = string.Empty;
                try
                {
                    comPortName = pm.getStuduinoPort();

                    // COMポートが見つからない、またはエラーが発生した場合は終了
                    if (comPortName == null)
                        ErrorNumber = 1;
                    else
                        ErrorNumber = transferAvrdude(comPortName, hexFile);
                }
                catch (ComPortException)
                {
                    ErrorNumber = 6;
                }
            }
            else if (currentBT.Equals(BoardType.STUDUINO_MINI))
            {
                ////////////////////////////////////////////////////////////////////////
                //                          Studuino mini
                ////////////////////////////////////////////////////////////////////////
                int err = 0;
                for (int i = 0; i < 10; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    err = transferBootloadHID(hexFile);
                    if (err == 0)
                    {
                        ErrorNumber = err;
                        break;
                    }

                    // リトライ回数を越えてエラーが出る場合
                    if (err == 1)
                    {
                        ErrorNumber = err;  // 接続エラー
                    }
                    else if (err == 2)
                    {
                        ErrorNumber = 101;  // タイムアウトエラー
                    }
                    else //if (err == 4)
                    {
                        ErrorNumber = 100;  // 転送エラー
                    }
                }
            }
            else  // if (currentBT.Equals(BT_STUDUINO_AND_MINI))
            {
                ////////////////////////////////////////////////////////////////////////
                //                          Studuino mini転送基板
                ////////////////////////////////////////////////////////////////////////
                int err = 0;
                for (int i = 0; i < 10; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    err = transferBootloadHID(st.TestModePath + "hidaspx.hex");
                    if (err == 0)
                    {
                        ErrorNumber = err;
                        break;
                    }

                    // リトライ回数を越えてエラーが出る場合
                    if (err == 1)
                    {
                        ErrorNumber = err;  // 接続エラー
                    }
                    else if (err == 2)
                    {
                        ErrorNumber = 101;  // タイムアウトエラー
                    }
                    else //if (err == 4)
                    {
                        ErrorNumber = 100;  // 転送エラー
                    }
                }
                if (ErrorNumber != 0) return false;
                System.Threading.Thread.Sleep(1000);
                ErrorNumber = transferHidaspx(hexFile);
            }

            return (ErrorNumber == 0);
        }

        /// <summary>
        /// Studuino基板のタイプを変更する
        /// <param name="id">id: 基板タイプID</param>
        /// </summary>
        public void changeBoardType(int id)
        {
            currentBT = BoardType.FromId(id);
            if (id == BoardType.STUDUINO.id)
            {
                st = new Studuino();
                //st = new Studuino10609();
            }
            else if (id == BoardType.STUDUINO_MINI.id)
            {
                st = new StuduinoLP();
                //st = new StuduinoLP10609();
            }
            else if (id == BoardType.STUDUINO_AND_MINI.id)
            {
                st = new StuduinoAndMini();
            }

            Debug.WriteLine("BoardType changed: " + currentBT.id);
            sendMessageToBPE(currentBT.id.ToString());
        }

        #region 【処理】 テストモード
        //---------------------------------------------------------------------
        // 概要  :【テストモード時】基板へのテストモード用ファイル(.hex)の転送処理
        // 引数  : NetworkStream socket : 4byteの値
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        // Date  : 2016/**/** :     新規作成
        //       : 2016/08/04 :     Studuino mini リセット待ちの削除
        //---------------------------------------------------------------------
        /// <summary>
        /// テストモード移行処理
        /// </summary>
        /// <param name="socket"></param>
        public void transferBoardSide()
        {
            if (currentBT.Equals(BoardType.STUDUINO))
            {
                bool isTransfered = transferHexFile(@st.TestModePath + st.TestModeFile);
                if (isTransfered)   // 基板側プログラムの転送成功時
                {
                    // 接続ポート情報を送信
                    Debug.WriteLine("Port Info");
                    string msg = pm.getStuduinoPort();
                    sendMessageToBPE(msg);
                    sendMessageToBPE("FINISH");
                }
                else                // 基板側プログラムの転送失敗時
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("ERR");
                    // エラー情報を送信
                    sendMessageToBPE(ErrorNumber.ToString());
                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH");
                }
            }
            else if (currentBT.Equals(BoardType.STUDUINO_MINI))
            {
                bool isTransfered = transferHexFile(@st.TestModePath + st.TestModeFile);
                if (isTransfered)   // 基板側プログラムの転送成功時
                {
                    // 接続ポート情報を送信
                    Debug.WriteLine("Port Info");
                    string msg = "OK";
                    sendMessageToBPE(msg);

                    int retryCount = 0;
                    while (retryCount < 10)
                    {
                        if (tcom.startTestMode())
                        {
                            // 初期化完了通知を送信
                            Debug.WriteLine("Initialization succeeded.");
                            sendMessageToBPE("INITOK");
                            break;
                        }
                        retryCount++;
                        System.Threading.Thread.Sleep(500);
                    }
                    // 初期化失敗
                    if (retryCount == 10)
                    {
                        // 初期化失敗通知を送信
                        Debug.WriteLine("Initialization failed.");
                        sendMessageToBPE("INITFAIL");
                    }
                }
                else                // 基板側プログラムの転送失敗時
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("ERR");
                    // エラー情報を送信
                    sendMessageToBPE(ErrorNumber.ToString());
                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH");
                }
            }
            else
            {
                ////////////////////////////////////////////////////
                // Studuino mini 転送基板
                ////////////////////////////////////////////////////

                // Studuino miniにhidaspx転送後、Studuinoにテストモード転送
                bool isTransfered = transferHexFile(@st.TestModePath + "ss38400.hex");
                if (!isTransfered)
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("ERR");
                    // エラー情報を送信
                    sendMessageToBPE(ErrorNumber.ToString());
                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH");
                    return;
                }

                // 転送完了を送信
                Debug.WriteLine("Port Info");
                string msg = "OK";
                sendMessageToBPE(msg);

                // 再びStuduino miniにデータパススルーモードを転送
                int err = 0;
                for (int i = 0; i < 10; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    err = transferBootloadHID(@st.TestModePath + "testmode_mini2.hex");
                    if (err == 0)
                    {
                        ErrorNumber = err;
                        break;
                    }

                    // リトライ回数を越えてエラーが出る場合
                    if (err == 1)
                    {
                        ErrorNumber = err;  // 接続エラー
                    }
                    else if (err == 2)
                    {
                        ErrorNumber = 101;  // タイムアウトエラー
                    }
                    else //if (err == 4)
                    {
                        ErrorNumber = 100;  // 転送エラー
                    }
                }
                if (ErrorNumber != 0)
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("ERR");
                    // エラー情報を送信
                    sendMessageToBPE(ErrorNumber.ToString());
                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH");
                    return;
                }

                // 転送完了を尊信
                Debug.WriteLine("Port Info");
                msg = "OK";
                sendMessageToBPE(msg);

                // Studuino miniに初期化コードを送信
                int retryCount = 0;
                while (retryCount < 10)
                {
                    if (tcom.startTestMode())
                    {
                        // 初期化完了通知を送信
                        Debug.WriteLine("Initialization succeeded.");
                        sendMessageToBPE("INITOK");
                        break;
                    }
                    retryCount++;
                    System.Threading.Thread.Sleep(500);
                }
                // 初期化失敗
                if (retryCount == 10)
                {
                    // 初期化失敗通知を送信
                    Debug.WriteLine("Initialization failed.");
                    sendMessageToBPE("INITFAIL");
                }
            }
        }
        #endregion

        #region 【処理】 プログラム作成・転送
        //---------------------------------------------------------------------
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //       : 2016/08/04 :     Studuino mini リセット待ちの削除
        //---------------------------------------------------------------------
        /// <summary>
        /// ビルド＆転送処理
        /// </summary>
        /// <param name="socket"></param>
        public void makeAndTransferProgram()
        {
            //-----------------------------------------------------------------
            // ユーザプログラムのビルド処理
            //-----------------------------------------------------------------
            //int ret = bulidUserProgram();
            int ret = buildUserProgram();
            if (ret == BuildSuccess)
            {   // make成功の場合は、転送処理を実行
                #region BPE側でリセット指示を出すため、ビルド成功を送信 (基板タイプ依存)
                if (currentBT.Equals(BoardType.STUDUINO_MINI) | currentBT.Equals(BoardType.STUDUINO_AND_MINI))
                {
                    sendMessageToBPE("OK");
                    sendMessageToBPE("0");
                }
                #endregion

                //-------------------------------------------------------------
                // ユーザプログラム(.hex)の転送処理
                //-------------------------------------------------------------
                bool isTransfered = transferHexFile(st.ArduinoSystemPath + st.HexFile);
                if (isTransfered)   // 基板側プログラムの転送成功時
                {
                    // 接続ポート情報を送信
                    Debug.WriteLine("Port info");
                    if (currentBT.Equals(BoardType.STUDUINO))
                    {
                        sendMessageToBPE(pm.getStuduinoPort());
                    }
                    else
                    {
                        sendMessageToBPE("1");
                    }

                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH");
                }
                else                // 基板側プログラムの転送失敗時
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("ERR");
                    // エラー情報を送信
                    sendMessageToBPE(ErrorNumber.ToString());
                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH");
                }
            }
            else                // 基板側プログラムの転送失敗時
            {
                // 接続ポート情報を送信
                sendMessageToBPE("ERR");
                // エラー情報を送信
                sendMessageToBPE(ErrorNumber.ToString());
                // 転送完了通知を送信
                sendMessageToBPE("FINISH");
            }
        }
        #endregion
        #region 【処理】 サーボモータの角度校正
        //---------------------------------------------------------------------
        // 概要  : 【サーボモーター角度校正】サーボモーター角度校正処理
        // 引数  : NetworkStream socket : 4byteの値
        //       : int lang : 言語
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        // サーボモーターのオフセット情報を読み込む
        public void setServomotorOffset(int lang = 0)
        {
            // Studuino miniの場合、リセットが押されるのを待つ
            //if (currentBT.Equals(BoardType.STUDUINO_MINI))
            //{
            //    BoardStatus bs = new BoardStatus();
            //    if (bs.isReady())
            //    {
            //        Debug.WriteLine("Reset pushed");
            //        sendMessageToBPE("OK");
            //    }
            //    else
            //    {
            //        Debug.WriteLine("Time out");
            //        // タイムアウト通知を送信
            //        sendMessageToBPE("TIMEOUT");
            //        return;
            //    }
            //}

            //// -----------------------------------------------------------------
            //// 入出力情報を読み込む
            //// -----------------------------------------------------------------
            stRobotIOStatus io = getIOStatusFromFile();

            // -----------------------------------------------------------------
            // 基板側プログラム(.hex)をアップロード
            // -----------------------------------------------------------------
            bool isTransfered = transferHexFile(@st.TestModePath + st.SVCalibrationFile);
            if (isTransfered)   // 基板側プログラムの転送成功時
            {
                // -------------------------------------------------------------
                // COMポートをオープンする
                // -------------------------------------------------------------
                // ひらがなの場合のみ言語指定してフォームを作成する
                using (ServoCalib fmCalib = hiragana ? new ServoCalib(svOffset, io, HIRAGANA) : new ServoCalib(svOffset, io))
                {
                    bool isOpen = fmCalib.openCOMPort(pm.getStuduinoPort());
                    if (!isOpen)
                    {   // COMポートのオープンに失敗
                        // 接続ポート情報を送信
                        sendMessageToBPE("ERR");
                        // エラー情報を送信
                        sendMessageToBPE(ErrorNumber.ToString());
                        // 転送完了通知を送信
                        sendMessageToBPE("FINISH");

                        return; // 処理終了
                    }

                    System.Threading.Thread.Sleep(2000);	// １秒停止

                    // -------------------------------------------------------------
                    // サーボモータ校正状態であることをBPEに送信
                    // -------------------------------------------------------------
                    // 入出力ポート設定情報が終了したら、サーボモーター校正処理に入るので
                    // その旨をブロックプログラミング環境に伝える
                    sendMessageToBPE("CALIBRATE");
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
                        sendMessageToBPE("FINISH");
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        if (fmCalib.getErrorCode() == (byte)ConnectingCondition.DISCONNECT)
                        {
                            fmCalib.closeCOMPort(); // COMポートをクローズ

                            // 転送完了通知を送信
                            sendMessageToBPE("ERR");
                        }
                        else
                        {
                            fmCalib.closeCOMPort(); // COMポートをクローズ

                            // 転送完了通知を送信
                            sendMessageToBPE("FINISH");
                        }
                    }
                }
            }
            else                // 基板側プログラムの転送失敗時
            {
                // 接続ポート情報を送信
                sendMessageToBPE("ERR");
                // エラー情報を送信
                sendMessageToBPE(ErrorNumber.ToString());
                // 転送完了通知を送信
                sendMessageToBPE("FINISH");
            }
        }

        #region 【共通】 ユーザプログラムのビルド
        // ---------------------------------------------------------------------
        // 概要       : ユーザプログラムのビルド
        // 戻り値     : int    0: ビルド失敗
        //            :        1: ビルド成功
        //            :        2: FLASHサイズオーバーフロー
        // Date       : 2013/07/06 kawase  0.01    新規作成
        // ---------------------------------------------------------------------
        public int buildUserProgram()
        {
            try
            {
                // コンパイル実行
                ErrorNumber = compile();
                if (ErrorNumber != 0)
                {
                    return BuildError;
                }

                // リンク実行
                ErrorNumber = link();
                if (ErrorNumber != 0)
                {
                    return BuildError;
                }

                // オブジェクトダンプ実行
                ErrorNumber = objDump();
                if (ErrorNumber != 0)
                {
                    return BuildFlashOverflow;
                }

                // オブジェクトコピー実行
                ErrorNumber = objCopy();
                if (ErrorNumber != 0)
                {
                    return BuildError;
                }

                // Studuino miniから転送する場合、hexファイルをbootloaderとマージする
                if (currentBT.Equals(BoardType.STUDUINO_AND_MINI))
                    mergeHex();
                return BuildSuccess;
            }
            catch
            {
                ErrorNumber = 5;
                return BuildError;
            }
        }
        #endregion

        /// <summary>
        /// ブロックプログラミングにメッセージを送信
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="stream">データストリーム</param>
        /// <param name="delim">区切り文字</param>
        private void sendMessageToBPE(String message, String delim = ".")
        {
            String sendMessage = message + delim + "\r\n";   // 末尾の改行(\r\n)は必須, delimはスクラッチ側で文字検出を容易にするため
            byte[] data = System.Text.Encoding.ASCII.GetBytes(sendMessage);
            stream.Write(data, 0, data.Length);
            //Debug.WriteLine("Sent: {0}", message);
        }

        //---------------------------------------------------------------------
        // Date  : 2016/02/25 : 新規作成
        //       : 2016/08/04 : リセット待ち削除。その他コードの整理。
        //---------------------------------------------------------------------
        /// <summary>
        /// モーター校正
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="lang"></param>
        public void calibMotor(int lang = 0)
        {
            // Studuinoへの対応が完了するまで以前のメソッドを使用する
            if (currentBT.Equals(BoardType.STUDUINO))
            {
                setServomotorOffset(lang);
                return;
            }
            //// -----------------------------------------------------------------
            //// 入出力情報を読み込む
            //// -----------------------------------------------------------------
            stRobotIOStatus io = getIOStatusFromFile();

            // -----------------------------------------------------------------
            // 基板側プログラム(.hex)をアップロード
            // -----------------------------------------------------------------
            bool isTransfered = transferHexFile(@st.TestModePath + st.SVCalibrationFile);
            if (!isTransfered)
            {
                // 接続ポート情報を送信
                sendMessageToBPE("ERR");
                // エラー情報を送信
                sendMessageToBPE(ErrorNumber.ToString());
                // 転送完了通知を送信
                sendMessageToBPE("FINISH");
                return;
            }

            sendMessageToBPE("OK");

            if (currentBT.Equals(BoardType.STUDUINO_AND_MINI))
            {
                // 再びStuduino miniにデータパススルーモードを転送
                int err = 0;
                for (int i = 0; i < 10; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    err = transferBootloadHID(@st.TestModePath + "testmode_mini2.hex");
                    if (err == 0)
                    {
                        ErrorNumber = err;
                        break;
                    }

                    // リトライ回数を越えてエラーが出る場合
                    if (err == 1)
                    {
                        ErrorNumber = err;  // 接続エラー
                    }
                    else if (err == 2)
                    {
                        ErrorNumber = 101;  // タイムアウトエラー
                    }
                    else //if (err == 4)
                    {
                        ErrorNumber = 100;  // 転送エラー
                    }
                }
                if (ErrorNumber != 0)
                {
                    // 接続ポート情報を送信
                    sendMessageToBPE("ERR");
                    // エラー情報を送信
                    sendMessageToBPE(ErrorNumber.ToString());
                    // 転送完了通知を送信
                    sendMessageToBPE("FINISH");
                    return;
                }

                // 転送完了を尊信
                Debug.WriteLine("Port Info");
                sendMessageToBPE("OK");
            }

            // -------------------------------------------------------------
            // COMポートをオープンする
            // -------------------------------------------------------------
            // ひらがなの場合のみ言語指定してフォームを作成する
            //using (CalibrationBase calib = hiragana ? new CalibrationLP(svOffset, io, tcom) : new CalibrationLP(svOffset, io, tcom))
            using (CalibrationBase calib = getCalib(svOffset, io, hiragana))
            {
                // Studuino miniの場合、基板に通信開始の合図を送る
                if (currentBT.Equals(BoardType.STUDUINO_MINI) || currentBT.Equals(BoardType.STUDUINO_AND_MINI))
                {
                    int retryCount = 0;
                    while (retryCount < 10)
                    {
                        if (tcom.startTestMode())
                        {
                            // 初期化完了通知を送信
                            Debug.WriteLine("Initialization succeeded.");
                            sendMessageToBPE("INITOK");
                            break;
                        }
                        retryCount++;
                        System.Threading.Thread.Sleep(500);
                    }
                    // 初期化失敗
                    if (retryCount == 10)
                    {
                        // 初期化失敗通知を送信
                        Debug.WriteLine("Initialization failed.");
                        sendMessageToBPE("INITFAIL");
                        return;
                    }
                }

                //System.Threading.Thread.Sleep(2000);	// １秒停止
                //tcom.sendPortInit();

                using (FileStream fs = new FileStream("Board.cfg", FileMode.Open, FileAccess.Read))
                {
                    byte[] buf = new byte[10];
                    int readSize;
                    readSize = fs.Read(buf, 0, 10);

                    for (byte i = 0; i < 10; i++)
                    {
                        Debug.Write(buf[i] + " ");
                        tcom.sendInitCommand(i, (PartID)buf[i]);
                    }
                }


                // -------------------------------------------------------------
                // サーボモータ校正ダイアログを表示する
                // -------------------------------------------------------------
                DialogResult res = calib.ShowDialog();
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
                }
                sendMessageToBPE("FINISH");
            }
        }

        #region 【処理】 入出力I/O設定
        //---------------------------------------------------------------------
        // 概要  : 【入出力設定】入出力設定処理
        // 引数  : NetworkStream socket : 4byteの値
        //       : int lang : 言語
        // Date  : 2014/02/25 : 0.95 kawase    新規作成
        //       : 2015/02/08        kagayama  Studuino miniの対応
        //---------------------------------------------------------------------
        public void setBoardIOConfiguration(int lang = 0)
        {
            //-----------------------------------------------------------------
            // I/O設定ファイルを読み込む
            //-----------------------------------------------------------------
            stRobotIOStatus io = getIOStatusFromFile();

            //-----------------------------------------------------------------
            // I/O設定ファイル作成
            //-----------------------------------------------------------------
            using (ConfigureBase setting = getConfigure(io, hiragana))
            {
                if (setting.ShowDialog() == DialogResult.OK)
                {
                    File.Delete(BOARD_IO); // ファイルを削除し、新しくファイルを作成する
                    BinaryWriter bw = new BinaryWriter(File.OpenWrite(BOARD_IO));
                    List<byte> lst = io.getStatusByte();
                    foreach (byte elm in lst)
                    {
                        Debug.WriteLine(elm.ToString());
                        bw.Write(elm);
                    }
                    bw.Close();
                    // IOが更新されたことを送信
                    sendMessageToBPE("UPDATE");
                }
                else
                {
                    // キャンセルされたことを送信
                    sendMessageToBPE("CANCEL");
                }
            }

            // 転送完了通知を送信
            sendMessageToBPE("FINISH");
        }

        /// <summary>
        /// ボードタイプに応じて入出力フォームを返す。
        /// </summary>
        /// <param name="io">入出力情報</param>
        /// <param name="hiragana">ひらがなフラグ</param>
        /// <returns>入出力フォーム</returns>
        private ConfigureBase getConfigure(stRobotIOStatus io, bool hiragana)
        {
            ConfigureBase config;
            if (currentBT.Equals(BoardType.STUDUINO) || currentBT.Equals(BoardType.STUDUINO_AND_MINI))
            {
                config = new ConfigureST(io, hiragana);
            }
            else
            {
                config = new ConfigureLP(io, hiragana);
            }
            return config;
        }
        #endregion

        /// <summary>
        /// "Board.cfg"から現状のIO設定を読み込み、stRobotIOStatusを返す。
        /// </summary>
        /// <returns>入出力オブジェクト(stRobotIOStatus)</returns>
        private stRobotIOStatus getIOStatusFromFile()
        {
            //stRobotIOStatus io = (currentBT.Equals(BT_STUDUINO)) ? new stRobotIOStatus() : new stRobotIOStatusLP();
            //int numIO = (currentBT.Equals(BT_STUDUINO)) ? 18 : 19;
            stRobotIOStatus io = (currentBT.Equals(BoardType.STUDUINO_MINI)) ? new stRobotIOStatusLP() : new stRobotIOStatus();
            int numIO = (currentBT.Equals(BoardType.STUDUINO_MINI)) ? 19 : 18;

            try
            {
                BinaryReader br = new BinaryReader(File.OpenRead(BOARD_IO));   // ファイルオープン
                byte[] scratchIO = new byte[numIO];                  // I/O設定を保存する配列の確保
                for (int i = 0; i < numIO; i++)
                {
                    // 配列にI/O設定を読み込む
                    scratchIO[i] = br.ReadByte();
                }
                io.setStatusByte(scratchIO);
                br.Close();
            }
            catch (FileNotFoundException)
            {
                io.initRobotIOConfiguration();
            }

            return io;
        }

        private CalibrationBase getCalib(ServoOffset offset, stRobotIOStatus io, bool hiragana)
        {
            CalibrationBase calib;
            if (currentBT.Equals(BoardType.STUDUINO_MINI))
            {
                calib = new CalibrationLP(offset, io, tcom);
            }
            else
            {
                calib = new CalibrationST(offset, io, tcom);
            }
            return calib;
        }
        #endregion

        public void startTestModeTransfer()
        {
            Debug.WriteLine("Port Init");
            tcom.sendPortInit();
            int ret = tcom.startSensorRead();
            sendMessageToBPE(ret.ToString());
        }

        public void stopTestModeTransfer()
        {
            tcom.stopSensorRead();
            sendMessageToBPE("ACK");
        }

        /// <summary>
        /// 指定された言語に変更する
        /// </summary>
        /// <param name="lang">言語コード(ISO 639-1)</param>
        /// <param name="socket">BPEとの通信用ストリーム</param>
        public void changeLanguage(String lang)
        {
            if (lang == "ja_HIRA")
            {
                hiragana = true;
                lang = "ja";
            }
            if (lang == "zh_CN")
            {
                lang = "zh";
            }

            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
            }
            catch
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            }

            if (stream != null)
            {
                // 言語を変更した旨を送信
                sendMessageToBPE("UPDATE");

                // 転送完了通知を送信
                sendMessageToBPE("FINISH");
            }
        }

        /// <summary>
        /// ブロックプログラミング環境を終了する
        /// </summary>
        public void finishBPE()
        {
            sendMessageToBPE("ACK");

            System.Diagnostics.Process[] ps =
                System.Diagnostics.Process.GetProcessesByName("block");

            foreach (System.Diagnostics.Process p in ps)
            {
                //IDとメインウィンドウのキャプションを出力する
                Debug.WriteLine(string.Format("{0}/{1}", p.Id, p.MainWindowTitle));
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
}
