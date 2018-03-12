//using ScratchConnection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Artec.Studuino.Command
{
    public class Common
    {
        public static byte[] GET_SYNC = new byte[] { (byte)0x30, (byte)0x20 };
        public static byte[] READ_SIGNATURE = new byte[] { (byte)0x75, (byte)0x20 };
    }
}

namespace Artec.Studuino
{
    public enum BoardType
    {
        UNDEFINED, ATMEGA168PA, ATMEGA328P,
    };
}

namespace Artec.Studuino.Utils
{
    /// <summary>
    /// USBタイプのシリアルポートを抜くとアプリケーションが異常終了する現象を
    /// 回避するための SerialPort 継承クラス
    /// </summary>
    public class SafeSerialPort : SerialPort
    {
        bool userOpen = false;
        System.IO.Stream SafeBaseStream = null;

        public new void Open()
        {
            base.Open();
            userOpen = true;
            SafeBaseStream = this.BaseStream;
        }

        public SafeSerialPort(string port, int baudrate) : base(port, baudrate)
        {
        }

        /// <summary>
        /// タイムアウトの期間内で、指定したcount数のデータを受信するまで待つ。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="timeout">タイムアウト時間</param>
        /// <returns>受信したデータ数</returns>
        public int Read(byte[] buffer, int offset, int count, int timeout=500)
        {
            int num = 0;
            System.DateTime start = System.DateTime.Now;
            
            try
            {
                while (BytesToRead < count && (System.DateTime.Now.CompareTo(start.AddMilliseconds(timeout)) < 0)) ;
                num = base.Read(buffer, offset, count);
            }
            catch(TimeoutException)
            {
            }
            return num;
        }

        protected override void Dispose(bool disposing)
        {
            if (userOpen && base.IsOpen == false)
            {
                // 予期せず COM ポートが閉じている
                try
                {
                    userOpen = false;
                    if (disposing)
                    {
                        SafeBaseStream.Close();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // 例外が発生するが 無視する
                }
                catch (IOException)
                {
                    // 例外が発生するが 無視する
                }
            }
            try
            {
                base.Dispose(disposing);
            }
            catch (IOException)
            {
                // 例外が発生するが 無視する
            }
        }
    }

    /*==============================================================================*/
    /* クラス名		：RobotConnector                                                */
    /* 概要			：ロボットとのデータ通信を担当                                  */
    /* 備考			：                                                              */
    /* Date			：2010/-/-	0.01	新規作成			                        */
    /*==============================================================================*/
    public class RobotConnector
    {
        const int ACK = 0x01;      // ACK
        const int NACK = 0xff;     // NACK
        const UInt16 RET_ACK = 1;
        const UInt16 RET_NACK = 2;
        const UInt16 NO_RESPONSE = 0;
        const UInt16 NOT_CONNECT = 3;
        const UInt16 RECV_SUCCESS = 4;

        // --------------------------------------------
        // 送信情報
        // --------------------------------------------
        const byte HEADER = 0xed;   // データヘッダ
        const byte FOOTER = 0xde;   // データフッタ

        // ---------------------------------------------------------------------
        // シリアル通信セットアップ処理
        // ---------------------------------------------------------------------
        SafeSerialPort targetPort;      // シリアル通信ポート

        /*==============================================================================*/
        /* 関数名		：openPort                                                      */
        /* 概要			：ロボットとのポートをオープン                                  */
        /* 引数			：string comPort : ロボットとの接続に使用しているCOMポート      */
        /* 戻り値		：なし                                                          */
        /* 備考			：                                                              */
        /* Date			：2010/-/-	0.01	新規作成			                        */
        /* 				：2014/10/03	0.984	COMポート割出処理を共通化               */
        /* 				：2014/10/17	0.984	エラーメッセージの表示方法変更          */
        /*==============================================================================*/
        public bool openPort(string comPort)
        {
            // -----------------------------------------------------------------
            // シリアルポートを開く
            // -----------------------------------------------------------------
            try
            {
                targetPort = new SafeSerialPort(comPort, 115200);
                // Arduinoとシリアル通信する場合、DtrEnableをtrueに設定した場合、
                // 基板にソフトウェアリセットがかかる。DtrEnableをfalseに設定す
                // ればソフトウェアリセットはかかりません。
                targetPort.DtrEnable = true;
                //              targetPort.RtsEnable = true;
                targetPort.Open();
                targetPort.ReadTimeout = 500;
                targetPort.DtrEnable = false;
                targetPort.DiscardOutBuffer();
            }
            // ポートが開かれていない場合(物理的に接続が切断された場合)
            catch (UnauthorizedAccessException e)
            {   // 例外内容：ポートへのアクセスが拒否されています。
                //string msg1 = Properties.Resources.str_msg_err_miscon1 + Environment.NewLine + Properties.Resources.str_msg_err_miscon2;
                //ErrorMessageBox msg = new ErrorMessageBox(msg1, e.Message);
                //msg.Dispose();
                return false;
            }
            // 物理的に接続が切断された場合
            catch (IOException e)
            {   // 例外内容：ポートが無効状態です。
                //string msg1 = Properties.Resources.str_msg_err_miscon1 + Environment.NewLine + Properties.Resources.str_msg_err_miscon3;
                //ErrorMessageBox msg = new ErrorMessageBox(msg1, e.Message);
                //msg.Dispose();
                return false;
            }
            // 予期せぬ例外処理
            catch (Exception e)
            {   // ログを取る
                //writeConnectionLog("Exception", e.Message, e.Source, e.StackTrace);
            }
            return true;
        }

        /*==============================================================================*/
        /* 関数名		：closePort                                                     */
        /* 概要			：ロボットとのポートをクローズ                                  */
        /* 引数			：なし                                                          */
        /* 戻り値		：なし                                                          */
        /* 備考			：                                                              */
        /* Date			：2010/-/-	0.01	新規作成			                        */
        /*==============================================================================*/
        public void closePort()
        {
            // -----------------------------------------------------------------
            // シリアルポートを閉じる
            // -----------------------------------------------------------------
            targetPort.Close();
            targetPort.Dispose();
        }


        public Boolean getSync()
        {
            byte[] buf = new byte[2];
            if (targetPort == null || !targetPort.IsOpen) return false;
            for (int i = 0; i < 3; i++)
            {
                targetPort.Write(Command.Common.GET_SYNC, 0, 2);
                try
                {
                    int num = targetPort.Read(buf, 0, 2);
                    Debug.WriteLine("try " + i + ": " + num + " @ " + buf[0] + " | " + buf[1]);
                    if (buf[0] == 0x14 && buf[1] == 0x10)
                    {
                        Debug.WriteLine("try " + i + ": " + buf[0] + "Got sync");
                        return true;
                    }
                }
                catch(TimeoutException)
                {
                    continue;
                }
            }
            return false;
        }

        /// <summary>
        /// 接続されている基板の種類を判定する
        /// </summary>
        /// <returns></returns>
        public BoardType checkBoardType(string comPort)
        {
            bool isOpen = false;
            if (targetPort != null && targetPort.IsOpen)
                closePort();
            isOpen = openPort(comPort);
            int numRead = 0;

            if (!getSync())
            {
                if (isOpen) closePort();
                return BoardType.UNDEFINED;
            }

            byte[] buf = new byte[5];
            try
            {
                targetPort.Write(Command.Common.READ_SIGNATURE, 0, 2);
                numRead = targetPort.Read(buf, 0, 5);
                Debug.Write("numRead: " + numRead);
                Debug.Write("buf[0]: " + buf[0]);
                Debug.Write("buf[1]: " + buf[1]);
            }
            catch (TimeoutException e)
            {
                Debug.WriteLine("READ_SIG Timeout");
            }
            if (isOpen) closePort();

            if (numRead == 5)
            {
                if (buf[2] == 0x94) return BoardType.ATMEGA168PA;
                if (buf[2] == 0x95) return BoardType.ATMEGA328P;
            }
            return BoardType.UNDEFINED;
        }
    }
}
