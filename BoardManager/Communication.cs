using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Threading;

namespace Artec
{
    namespace TestModeCommunication
    {
        public enum CommandID
        {
            DC = 0x00,
            SV,
            Buzzer,
            LED,
            SyncSV
        }

        public enum PinID
        {
            M1 = 0x00,
            M2, D5, D6, D9, D10, D11,
            A0 = 0x0a,
            A1, A2, A3, A4, A5, A6
        }

        public enum PartID
        {
            OPEN = 0x00,
            DC, SV, LED, Buzzer,
            LightSensor = 0x10,
            TouchSensor, SoundSensor, IRPhotoreflector, Accelerometer, Button
        }

        class HIDSendException : Exception
        {
            public HIDSendException(string message)
                : base(message)
            {
            }
        }

        class SensorValue
        {
            int[] value;

            public SensorValue(int size)
            {
                value = new int[size];
            }

            public void setValue(int index, int val)
            {
                value[index] = val;
            }

            public int getValue(int index)
            {
                return value[index];
            }

            public byte[] getValueForBPE(int index)
            {
                byte[] msg = new byte[2];
                msg[0] = (byte)(128 + index * 8 + (value[index] >> 7 & 0x07));
                msg[1] = (byte)(value[index] & 0x7f);
                return msg;
            }
        }

        class TestModeCommand
        {
            /// <summary>
            /// アクションコマンドを生成する
            /// </summary>
            /// <param name="cid">コマンドID</param>
            /// <param name="pin">ピン番号[0,1(M1/2)2-9(Servo)10-18(Sensor)19(SPI)</param>
            /// <param name="args">各コマンドに応じた引数の配列</param>
            /// <returns>アクションコマンド(2バイト)</returns>
            public byte[] actCommand(CommandID cid, PinID pin, byte[] args)
            {
                byte[] msg = new byte[2];
                msg[0] = (byte)(128 + ((int)cid << 4 & 0x70) + ((int)pin << 1));
                switch (cid)
                {
                    case CommandID.DC:
                        msg[0] = (byte)(128 + (int)pin * 8 + args[0]);
                        msg[1] = (byte)args[1];
                        break;
                    case CommandID.SV:
                        msg[0] = (byte)(128 + ((int)cid << 4 & 0x70) + ((int)(pin - 2) << 1));
                        msg[0] += (byte)(args[0] >> 7 & 0x01);
                        msg[1] = (byte)(args[0] & 0x7f);
                        break;
                    case CommandID.LED:
                        if (args.Length >= 1)
                            msg[0] += (byte)(args[0] & 0x01);
                        if (args.Length >= 2)
                            msg[1] = (byte)((args[1] & 0x01) << 6);
                        break;
                    case CommandID.SyncSV:
                        msg[1] = (byte)args[0];
                        break;
                }

                return msg;
            }

            public byte[] actCommand(CommandID cid, byte[] args)
            {
                byte[] msg = new byte[2];
                msg[0] = (byte)(1 << 7 | ((int)cid << 4 & 0x70));
                switch (cid)
                {
                    case CommandID.DC:  // args[0]: pin   args[1]: action  args[2]: value
                        msg[0] += (byte)(args[0] << 3 | args[1]);
                        msg[1] = (byte)args[2];
                        break;
                    case CommandID.SV:  // args[0]: pin   args[1]: degree
                        msg[0] += (byte)(args[0] << 1 | args[1] >> 7);
                        msg[1] = (byte)(args[1] & 0x7f);
                        break;
                    case CommandID.LED:
                        if (args.Length >= 1)
                            msg[0] += (byte)(args[0] & 0x01);
                        if (args.Length >= 2)
                            msg[1] = (byte)((args[1] & 0x01) << 6);
                        break;
                    case CommandID.SyncSV:
                        msg[1] = (byte)args[0];
                        break;
                }

                return msg;
            }

            /// <summary>
            /// テストモードの初期化用データを生成する
            /// </summary>
            /// <param name="pin">ピン番号[0,1(M1/2)2-9(Servo)10-18(Sensor)19(SPI)</param>
            /// <param name="pid">パーツID</param>
            /// <returns></returns>
            public byte[] initCommand(PinID pin, PartID pid)
            {
                byte[] msg = new byte[2];
                msg[0] = (byte)(192 + ((int)pin >> 1 & 0x0f));
                msg[1] = (byte)(((int)pin << 6 & 0x40) | (int)pid);
                Debug.Write("Port initialization\t");
                Debug.Write(pin);
                Debug.Write("\t");
                Debug.Write(pid);
                Debug.Write("\t");
                Debug.Write(msg[0]);
                Debug.Write("\t");
                Debug.WriteLine(msg[1]);
                return msg;
            }

            public byte[] initCommand(byte pin, PartID pid)
            {
                byte[] msg = new byte[2];
                msg[0] = (byte)(3 << 6 | (pin >> 1 & 0x0f));
                msg[1] = (byte)((pin << 6 & 0x40) | (int)pid);
                Debug.Write("Port initialization\tpin: ");
                Debug.Write(pin);
                Debug.Write("\tparts: ");
                Debug.Write(pid);
                Debug.Write("\tdata1: ");
                Debug.Write(msg[0]);
                Debug.Write("\tdata2: ");
                Debug.WriteLine(msg[1]);
                return msg;
            }
        }

        /// <summary>
        /// テストモード中のコマンドのやり取り、センサー値の送信などを担当する
        /// </summary>
        public class TestModeCommunication : ICommandSender
        {
            const string IOFILE = "Board.cfg";
            //const string HIDTOOL = @"D:\works\StuduinoLP\trial\commandline\hidtool.exe";
            const string HIDTOOL = @"..\common\tools\hidtool.exe";
            TestModeCommand comGen;

            MessageCommunicator commandServer;  // BPEからのコマンド送信要求待ち
            MessageCommunicator sensorServer;   // BPEからのデータ要求待ち

            readonly byte[] INIT = { 0xff, 0xff };
            System.Threading.Timer tRead;
            byte retryCounter = 0;
            const byte MAX_RETRY = 10;
            bool fCanRead = true;

            //byte[] sensorValues = new byte[20];
            SensorValue mSensorValue = new SensorValue(10);
            int mPointer = 0;
            int validSensorSize = 0;
            int[] validSensor = new int[10];

            public TestModeCommunication()
            {
                comGen = new TestModeCommand();
                commandServer = new MessageCommunicator();
                commandServer.MessageReceived += new MessageCommunicator.MessageReceivedEventHandler(commandServer_MessageReceived);

                sensorServer = new MessageCommunicator();
                sensorServer.MessageReceived += new MessageCommunicator.MessageReceivedEventHandler(sensorServer_MessageReceived);

            }

            /// <summary>
            /// BPEからのセンサーデータ要求を受けてセンサーデータを送信する。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void sensorServer_MessageReceived(object sender, MessageReceiveEventArgs e)
            {
                string msg = e.ReceivedMessage;
                byte[] data = e.ReceivedData;

                if (msg.StartsWith("REQDATA"))
                {
                    byte[] sendmsg = getSensorValueForBPE();
                    //byte[] sendmsg = getAllSensorValueForBPE();
                    sensorServer.sendBytes(sendmsg);
                }
            }

            /// <summary>
            /// BPEからのコマンド受信時イベント発生時の処理。基板にコマンドを送信する。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void commandServer_MessageReceived(object sender, MessageReceiveEventArgs e)
            {
                string msg = e.ReceivedMessage;
                byte[] data = e.ReceivedData;
                int size = data.Length - 7 -2;
                Debug.WriteLine(string.Format("Data size: {0:d}", size));

                if (msg.StartsWith("REQSEND"))
                {
                    if (size < 10)
                    {
                        byte[] senddata = new byte[size];
                        Array.Copy(data, 7, senddata, 0, size);
                        sendCommand(senddata);
                        commandServer.sendMessage("ACK");
                    }
                }
            }

            /// <summary>
            /// Board.cfgの内容を読み取り、基板にIO初期化コマンドを送る。
            /// </summary>
            public void sendPortInit()
            {
                validSensorSize = 0;
                mPointer = 0;
                using (FileStream fs = new FileStream(IOFILE, FileMode.Open, FileAccess.Read))
                {
                    int fileSize = (int)fs.Length;
                    byte[] buf = new byte[fileSize];
                    int readSize;
                    readSize = fs.Read(buf, 0, fileSize);

                    for (byte i = 0; i < readSize; i++)
                    {
                        Debug.Write(buf[i] + " ");
                        if (i >= 10)
                        {
                            if (buf[i] >= (int)PartID.LightSensor)
                            {
                                validSensor[mPointer] = i-10;
                                validSensorSize++;
                                mPointer++;
                            }
                        }
                        sendCommand(comGen.initCommand((PinID)i, (PartID)buf[i]));
                    }
                    // A6, A7は基板光センサー、電源電圧として固定
                    validSensor[mPointer++] = 6;
                    validSensor[mPointer++] = 7;
                    validSensorSize += 2;
                    mPointer = 0;
                }
            }

            /// <summary>
            /// Studuino miniのテストモードがセットアップ済みかどうかを確認する
            /// </summary>
            /// <returns></returns>
            private bool checkSetupDone()
            {
                poolSensorDate(null);
                //return (sensorValues[0] == 0xff) && (sensorValues[1] == 0) && (sensorValues[2] == 0xff) && (sensorValues[3] == 0);
                return (mSensorValue.getValue(0) == 0xff &&
                    mSensorValue.getValue(1) == 0 &&
                    mSensorValue.getValue(2) == 0xff &&
                    mSensorValue.getValue(3) == 0);
            }

            public bool startTestMode()
            {
                try
                {
                    if (checkSetupDone())
                        sendData(INIT);
                    else
                        return false;
                }
                catch (HIDSendException)
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// HIDで基板にデータ送信
            /// </summary>
            /// <param name="data"></param>
            void sendData(byte[] data)
            {
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = HIDTOOL;                         // 実行するファイル
                psInfo.Arguments = "write " + convertData(data);   // 引数設定
                psInfo.CreateNoWindow = true;                      // コンソール・ウィンドウを開かない
                psInfo.UseShellExecute = false;                    // シェル機能を使用しない
                psInfo.RedirectStandardError = true;
                psInfo.RedirectStandardOutput = false;
                Process p = Process.Start(psInfo);
                string error = p.StandardError.ReadToEnd();        // 標準出力の読み取り
                p.WaitForExit();
                // Studuino mini用
                //if (error != "")
                //{
                //    throw new HIDSendException(error);
                //}
                // Studuino mini転送基板用
                if (error.Contains("finding"))
                {
                    throw new HIDSendException(error);
                }
            }

            /// <summary>
            /// HIDでセンサーデータを要求
            /// </summary>
            /// <returns></returns>
            string readData()
            {
                ProcessStartInfo psInfo = new ProcessStartInfo();
                psInfo.FileName = HIDTOOL;                         // 実行するファイル
                psInfo.Arguments = "read";                         // 引数設定
                psInfo.CreateNoWindow = true;                      // コンソール・ウィンドウを開かない
                psInfo.UseShellExecute = false;                    // シェル機能を使用しない
                psInfo.RedirectStandardError = true;
                psInfo.RedirectStandardOutput = true;
                Process p = Process.Start(psInfo);
                string output = p.StandardOutput.ReadToEnd();      // 標準出力の読み取り
                p.WaitForExit();
                //Debug.WriteLine(output);
                return output;
            }

            string convertData(byte[] data)
            {
                string msg = "";
                for (int i = 0; i < data.Length; i++)
                {
                    msg += string.Format("0x{0,0:X2}", data[i]);
                    msg += (i == data.Length - 1) ? "" : ",";
                }
                return msg;
            }

            public bool sendCommand(PinID pin, PartID part)
            {
                return sendCommand(comGen.initCommand(pin, part));
            }

            public bool sendInitCommand(byte pin, PartID part)
            {
                return sendCommand(comGen.initCommand(pin, part));
            }

            public bool sendCommand(CommandID cid, PinID pin, byte[] args)
            {
                return sendCommand(comGen.actCommand(cid, pin, args));
            }

            public bool sendCommand(byte[] data)
            {
                if (retryCounter > MAX_RETRY) return false;

                bool result = false;
                try
                {
                    sendData(data);
                    result = true;
                }
                catch (HIDSendException ex)
                {
                    Debug.Write(ex.Message);
                    Debug.WriteLine("Retry: " + retryCounter++);
                    Thread.Sleep(20);
                    result = sendCommand(data);
                }

                Debug.Write(result + " ");
                for (byte i = 0; i < data.Length; i++)
                {
                    Debug.Write(String.Format("D{0:d}: {1:d} ", i, data[i]));
                }
                Debug.WriteLine("");
                retryCounter = 0;
                return result;
            }

            public int startSensorRead()
            {
                int ret = -1;
                int pcom = commandServer.startLister();        // コマンド送信用のtcpサーバーをスタートする
                int psns = sensorServer.startLister(pcom + 1); // センサーデータ用のtcpサーバーをスタートする

                if (pcom > 0 && psns > 0)
                {
                    TimerCallback tc = new TimerCallback(poolSensorDate);
                    tRead = new System.Threading.Timer(tc, null, 500, 100);
                    //tRead.Change(0, 100);
                    ret = (psns - pcom) << 16 | pcom;
                }

                return ret;
            }

            public void stopSensorRead()
            {
                tRead.Change(Timeout.Infinite, Timeout.Infinite);
                commandServer.disconnect();
                sensorServer.disconnect();
            }

            public void suspendSensorRead()
            {
                fCanRead = false;
            }

            public void restartSensorRead()
            {
                fCanRead = true;
            }

            private void poolSensorDate(object o)
            {
                if (!fCanRead) return;  // suspend時は処理を行わない

                string msg = readData();
                if (msg != "")
                {
                    string[] rcv = msg.Split(' ');
                    for (int i = 0; i < rcv.Length; i++)
                    {
                        if (i < 8)
                        {
                            try
                            {
                                byte tmp = Byte.Parse(rcv[i].Substring(2), NumberStyles.AllowHexSpecifier);
                                //sensorValues[i] = tmp;
                                mSensorValue.setValue(i, tmp);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    Debug.WriteLine(msg);
                }
                else
                {
                    Debug.WriteLine("No reply. Reading failure.");
                }
            }

            public byte[] getSensorValueForBPE()
            {
                if (!(mPointer < validSensorSize)) mPointer = 0;
                return mSensorValue.getValueForBPE(validSensor[mPointer++]);
                //return mSensorValue.getValueForBPE(index);
            }

            public byte[] getAllSensorValueForBPE()
            {
                mPointer = 0;
                List<byte> lst = new List<byte>();
                while (mPointer < validSensorSize)
                {
                    lst.AddRange(mSensorValue.getValueForBPE(validSensor[mPointer++]));
                }
                return lst.ToArray();
            }

            //public byte[] getSensorValues()
            //{
            //    return sensorValues;
            //}

            void ICommandSender.sendCommand(byte[] data)
            {
                this.sendCommand(data);
            }
        }

        /// <summary>
        /// テストモード中のBPEからのリクエスト受付
        /// </summary>
        class MessageCommunicator
        {
            TcpListener server;
            TcpClient client;
            const int PORT_START = 49212; // 自由に使用できるポート番号(49212～)
            const int PORT_END = 65535;   // 自由に使用できるポート番号(～65535)

            public delegate void MessageReceivedEventHandler(object sender, MessageReceiveEventArgs e);
            public event MessageReceivedEventHandler MessageReceived;

            public MessageCommunicator()
            {
            }

            /// <summary>
            /// デフォルトの開始ポートからリッスン開始する
            /// </summary>
            /// <returns></returns>
            public int startLister()
            {
                return startLister(PORT_START);
            }

            /// <summary>
            /// 開始ポートを指定してリッスン開始する
            /// </summary>
            /// <param name="startPort">開始ポート</param>
            /// <returns></returns>
            public int startLister(int startPort)
            {
                //-----------------------------------------------------------------
                // ブロックプログラミング環境(BPE)との通信ポートの開設
                //-----------------------------------------------------------------
                string host = "127.0.0.1";  // ローカルホストIP
                System.Net.IPAddress localAddr = System.Net.IPAddress.Parse(host);
                //int port = 49212;           
                // 使用できるポート番号の検索
                for (int port = startPort; port < PORT_END; port++)
                {
                    try
                    {
                        server = new TcpListener(localAddr, port);
                        server.Start();

                        Thread waitConnection = new Thread(new ThreadStart(startWaiting));
                        waitConnection.Start();

                        return port;
                    }
                    catch (SocketException)
                    {
                        // ポート番号がかぶっていた場合の対応
                    }
                }
                return -1;
            }



            /// <summary>
            /// クライアントからの接続要求を待つ
            /// </summary>
            private void startWaiting()
            {
                Byte[] msg = new Byte[256];
                // 接続要求が送信されるまでブロック
                client = server.AcceptTcpClient();
                Debug.WriteLine("Client connected!");

                if (client.Connected)
                {
                    server.Stop();
                    NetworkStream stream = client.GetStream();

                    int i;
                    string data;
                    // BPEからのデータを受信(バイトデータ)
                    try
                    {
                        while ((i = stream.Read(msg, 0, msg.Length)) != 0)
                        {
                            byte[] rcv = new byte[i];
                            Array.Copy(msg, rcv, i);
                            // バイトデータをASCII文字に変換
                            data = System.Text.Encoding.ASCII.GetString(msg, 0, i);
                            //Debug.WriteLine("Received: {0}", data);
                            // 受信データから改行(\r\n)を削除
                            data = data.Replace("\r", "").Replace("\n", "");

                            if (data == "BREAK") break;

                            MessageReceiveEventArgs e = new MessageReceiveEventArgs();
                            e.ReceivedData = rcv;
                            e.ReceivedMessage = data;
                            OnMessageReceived(e);
                        }
                    }
                    catch (IOException)
                    {
                    }
                }

            }

            /// <summary>
            /// BPEに指定されたメッセージを送信する
            /// </summary>
            /// <param name="message">送信するメッセージ</param>
            public void sendMessage(string message)
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                sendBytes(data);
            }

            /// <summary>
            /// BPEに指定されたバイト配列を送信する
            /// </summary>
            /// <param name="message">送信するバイト配列</param>
            public void sendBytes(byte[] message)
            {
                if (client.Connected)
                {
                    List<byte> msg = new List<byte>(message);
                    msg.AddRange(System.Text.Encoding.ASCII.GetBytes(".\r\n"));
                    byte[] smsg = msg.ToArray();
                    // BPEとRead/Writeに使用するストリームオブジェクトを作成
                    NetworkStream stream = client.GetStream();
                    stream.Write(smsg, 0, smsg.Length);
                }
            }

            /// <summary>
            /// クライアントとの接続を切断する
            /// </summary>
            public void disconnect()
            {
                if (client.Connected)
                    client.Close();

                client = null;
                server = null;
            }

            protected virtual void OnMessageReceived(MessageReceiveEventArgs e)
            {
                if (MessageReceived != null) MessageReceived(this, e);
            }
        }

        public class MessageReceiveEventArgs : EventArgs
        {
            public string ReceivedMessage;
            public byte[] ReceivedData;
        }

        public interface ICommandSender
        {
            void sendCommand(byte[] data);
        }
    }

    namespace StuduinoMini
    {
        class BoardStatus
        {
            const int TIMEOUT = 10;
            public bool isReady()
            {
                DateTime start = DateTime.Now;
                // リセット押されるのを待つ
                while (boardStatus())
                {
                    if ((DateTime.Now - start).Seconds > TIMEOUT)
                    {
                        Console.WriteLine("Time out 1");
                        return false;
                    }
                }

                start = DateTime.Now;
                // リセット離されるのを待つ
                while (!boardStatus())
                {
                    if ((DateTime.Now - start).Seconds > TIMEOUT)
                    {
                        Console.WriteLine("Time out 2");
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Studuino mini基板の状態を返す
            /// </summary>
            /// <returns><br>true: Studuino mini基板として認識されている</br><br>false: Studuino mini基板として認識されていない</br></returns>
            public bool boardStatus()
            {
                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Service='HidUsb'");
                    ManagementObjectCollection mc = searcher.Get();
                    foreach (ManagementObject queryObj in mc)
                    {
                        string deviceID = queryObj["DeviceID"].ToString();
                        if (deviceID.Contains("VID_20A0&PID_4268"))     // 本番用
                        //if (deviceID.Contains("VID_16C0&PID_05DF"))   // T1サンプル用
                        {
                            return true;
                        }
                    }
                }
                catch (ManagementException ex)
                {
                    Debug.WriteLine("An error occurred while querying for WMI data: " + ex.Message);
                    return false;
                }
                return false;
            }
        }
    }
}
