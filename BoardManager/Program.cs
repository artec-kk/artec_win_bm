using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ScratchConnection
{
    class Program
    {
        // BPEとの通信用ポート情報
        static string portNumber = "portNumber.info";

        static Utility util = new Utility();

        //---------------------------------------------------------------------
        // 概要  : Main
        //---------------------------------------------------------------------
        static void Main(string[] args)
        {
            // 引数で指定された言語に切替える
            if (args.Length > 0)
            {
                util.changeLanguage(args[0]);
            }
            if (args.Length > 1)
            {
                util.changeBoardType(int.Parse(args[1]));
            }

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
            for (; port < 65535; port++)
            {
                try
                {
                    server = new TcpListener(localAddr, port);
                    server.Start();
                    break;
                }
                catch (SocketException)
                {
                    // ポート番号がかぶっていた場合の対応
                }
            }
            // BPEに通知するポート番号をエンディアン変換してファイル出力
            Debug.WriteLine(port);
            BinaryWriter bw = new BinaryWriter(File.Create(portNumber));
            byte[] portbin = BitConverter.GetBytes(port);
            Array.Reverse(portbin);
            bw.Write(portbin);
            bw.Close();


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

                // BPEとRead/Writeに使用するストリームオブジェクトを作成
                NetworkStream stream = client.GetStream();
                util.setStream(stream);

                int i;
                // BPEからのデータを受信(バイトデータ)
                try
                {
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
                            util.setBoardIOConfiguration();
                        }
                        #endregion
                        #region モーター校正フォームを開く
                        if (data == "CALIB")
                        { // オフセット設定
                            util.calibMotor();
                        }
                        #endregion
                        #region 言語切り替え
                        if (data == "LANGE")
                        { // 言語切り替え(英語)
                            util.changeLanguage("en");
                        }
                        if (data == "LANGJ")
                        { // 言語切り替え(日本語)
                            util.changeLanguage("ja");
                        }
                        if (data == "LANGZ")
                        { // 言語切り替え(中国語)
                            util.changeLanguage("zh");
                        }
                        if (data == "LANGZT")
                        { // 言語切り替え(中国語(台湾))
                            util.changeLanguage("zh-TW");
                        }
                        if (data == "LANGH")
                        { // 言語切り替え(にほんご)
                            util.changeLanguage("ja_HIRA");
                        }
                        if (data == "LANGK")
                        { // 言語切り替え(韓国語)
                            util.changeLanguage("ko");
                        }
                        if (data == "LANGES")
                        { // 言語切り替え(スペイン語)
                            util.changeLanguage("es");
                        }
                        if (data == "LANGFR")
                        { // 言語切り替え(フランス語)
                            util.changeLanguage("fr");
                        }
                        #endregion
                        #region その他
                        if (data == "RUN")
                        { // プログラム作成・転送命令
                            util.makeAndTransferProgram();
                        }
                        if (data.StartsWith("BOARD"))
                        { // 基板タイプ変更命令
                            Debug.WriteLine(data.Substring(5, 1));
                            int id = int.Parse(data.Substring(5, 1));
                            util.changeBoardType(id);
                        }
                        if (data == "TEST")
                        { // テストモード命令
                            Debug.Write("communication test TEST\r\n");
                            util.transferBoardSide();
                        }
                        if (data == "TEST2S")
                        { // テストモード開始(Studuino mini)
                            Debug.Write("communication test START\r\n");
                            // コマンド受付用TCPリスナーのポートを返す
                            util.startTestModeTransfer();
                        }
                        if (data == "TEST2E")
                        { // テストモード終了
                            Debug.Write("communication test END\r\n");
                            // 転送完了通知を送信
                            util.stopTestModeTransfer();
                        }
                        if (data == "SHOWKEYPAD")
                        {
                            util.showKeypad(false);
                        }
                        if (data == "HIDEKEYPAD")
                        {
                            util.hideKeypad();
                        }
                        if (data == "BREAK")
                        { // 通信切断通知
                            break;
                        }
                        if (data == "FINISH")
                        {
                            util.finishBPE();

                            clientFinish = true;
                            break;
                        }
                        #endregion
                        // Process the data sent by the client.
                    }
                }
                catch (IOException)
                {
                    // 切断された場合は接続終了として、再度接続待ちに入る
                }

                // 通信を切断し、再び接続処理へ
                client.Close();
                if (clientFinish) break;
            }
            // 停止処理
            server.Stop();
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
