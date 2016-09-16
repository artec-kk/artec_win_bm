using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScratchConnection;
using System.Diagnostics;
using System.IO;
using ScratchConnection.Forms;
using Artec.TestModeCommunication;

namespace BoardManagerTest
{
    public partial class Form1 : Form
    {
        const string IOFILE = "Board.cfg";
        const string IOFILEMINI = "Board_mini.cfg";
        const string DCINI = @"..\common\dc_calib_ini";
        const string SVINI = @"..\common\sv_offset_ini";
        const byte IONUM = 19;

        public Form1()
        {
            InitializeComponent();

            testConfigure(1, true);
            /*
            int boardtype = 1;  // 0: Studuino 1: Studuino mini
            int ionum = (boardtype == 0) ? 18 : 19;
            Boolean hiragana = false;

            //stRobotIOStatusLP io = new stRobotIOStatusLP();
            stRobotIOStatus io = (boardtype == 0) ? new stRobotIOStatus() : new stRobotIOStatusLP();
            ServoOffset offset = new ServoOffset();
            //offset.setDCCalib(100, 100);
            offset.readDCInfo(DCINI);
            offset.readServoInfo(SVINI);

            TestModeCommunication tcom = new TestModeCommunication();
            int retryCount = 0;
            while (retryCount < 10)
            {
                if (tcom.startTestMode())
                {
                    // 初期化完了通知を送信
                    Debug.WriteLine("Initialization succeeded.");
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
            }

            tcom.sendPortInit();

            try
            {
                BinaryReader br = new BinaryReader(File.OpenRead(IOFILE));   // ファイルオープン
                byte[] scratchIO = new byte[ionum];                  // I/O設定を保存する配列の確保
                for (int i = 0; i < ionum; i++)
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

            if (boardtype == 0)
            {
            }
            else
            {
                using (CalibrationBase calib = new CalibrationLP(offset, io, tcom))
                {
                    if (calib.ShowDialog() == DialogResult.OK)
                    {
                        offset.writeCalibInfo();
                    }
                }
                //using (CalibrationBase calib = new CalibrationBase(offset, io))
                //{
                //    if (calib.ShowDialog() == DialogResult.OK)
                //    {
                //    }
                //}
            }
            */
        }

        // 0: Studuino 1: Studuino mini
        void testConfigure(int boardtype = 0, bool hiragana = false)
        {
            int ionum = (boardtype == 0) ? 18 : 19;

            //stRobotIOStatusLP io = new stRobotIOStatusLP();
            stRobotIOStatus io = (boardtype == 0) ? new stRobotIOStatus() : new stRobotIOStatusLP();

            try
            {
                BinaryReader br = (boardtype == 0) ? new BinaryReader(File.OpenRead(IOFILE)) : new BinaryReader(File.OpenRead(IOFILEMINI));   // ファイルオープン
                byte[] scratchIO = new byte[ionum];                  // I/O設定を保存する配列の確保
                for (int i = 0; i < ionum; i++)
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

            if (boardtype == 0)
            {
                using (ConfigureBase setting = new ConfigureST(io, hiragana))
                {
                    if (setting.ShowDialog() == DialogResult.OK)
                    {
                        File.Delete(IOFILE); // ファイルを削除し、新しくファイルを作成する
                        BinaryWriter bw = new BinaryWriter(File.OpenWrite(IOFILE));
                        List<byte> lst = io.getStatusByte();
                        foreach (byte elm in lst)
                        {
                            Debug.WriteLine(elm.ToString());
                            bw.Write(elm);
                        }
                        bw.Close();
                    }
                }
                //using (Configure setting = hiragana ? new Configure(io, 3) : new Configure(io))
                //{
                //    if (setting.ShowDialog() == DialogResult.OK)
                //    {
                //        io = setting.getConfiguration();
                //        File.Delete(IOFILE); // ファイルを削除し、新しくファイルを作成する
                //        BinaryWriter bw = new BinaryWriter(File.OpenWrite(IOFILE));
                //        List<byte> lst = io.getStatusByte();
                //        foreach (byte elm in lst)
                //        {
                //            bw.Write(elm);
                //        }
                //        bw.Close();
                //    }
                //}
            }
            else
            {
                using (ConfigureBase setting = new ConfigureLP(io, hiragana))
                {
                    if (setting.ShowDialog() == DialogResult.OK)
                    {
                        File.Delete(IOFILE); // ファイルを削除し、新しくファイルを作成する
                        BinaryWriter bw = new BinaryWriter(File.OpenWrite(IOFILE));
                        List<byte> lst = io.getStatusByte();
                        foreach (byte elm in lst)
                        {
                            Debug.WriteLine(elm.ToString());
                            bw.Write(elm);
                        }
                        bw.Close();
                    }
                }
            }
        }
    }
}
