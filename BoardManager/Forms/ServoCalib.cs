using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

namespace ScratchConnection
{
    //--------------------------------------------------------------------------
    // 概要   : サーボモーター校正用ダイアログ
    //        : 入出力ポート設定情報は送信しません。代わりに、サーボモーターの
    //        : 角度情報を受信した時にサーボモーターにピンをアタッチします。
    // Date   : 2014/01/29  kagayama 0.94    新規作成
    //        : 2014/01/30  kagayama 0.94    NumericUpDownのイベントを別コントロールへパスする処理を追加
    //--------------------------------------------------------------------------
    public partial class ServoCalib : Form
    {
        ServoOffset svoff;
        stRobotIOStatus io;
        bool isConnectiong; // 接続中かどうか
/*
        public delegate void NudEventHandler(object sender, EventArgs e);
        [Category("動作")]
        [Description("情報を更新するときに発生するイベント。")]
        public event NudEventHandler NudEvent;

        public delegate void CalibButtonEventHandler(object sender, EventArgs e);
        public event CalibButtonEventHandler CalibButtonEvent;

        public delegate void CalibSliderEventHandler(object sender, EventArgs e);
        public event CalibSliderEventHandler CalibSliderEvent;
*/
        SerialPort targetPort = null;      // シリアル通信ポート

        public ServoCalib()
        {
            InitializeComponent();
        }

        //---------------------------------------------------------------------
        // 概要  : コンストラクタ
        // 引数  : ServoOffset svoff  : 角度オフセット
        //       : stRobotIOStatus io : IOステータス
        // Date  : 2014/02/12 : 0.95  kagayama    新規作成
        //       : 2014/08/04 : 0.973 kagayama    ひらがな対応
        //---------------------------------------------------------------------
        const int ENGLISH = 0;
        const int JAPAN = 1;
        const int CHINESE = 2;
        const int HIRAGANA = 3;
        public ServoCalib(ServoOffset svoff, stRobotIOStatus io, int lang)
            : this()
        {
            switch (lang)
            {
                case ENGLISH:
                    tsmiEnglish_Click();
                    break;
                case JAPAN:
                    tsmiJapanese_Click();
                    break;
                case CHINESE:
                    tsmiChinese_Click();
                    break;
                case HIRAGANA:
                    tsmiJapanese_Click();
                    convToHiragana();
                    break;
                default:
                    break;
            }


            this.svoff = svoff;
            nudD9.Value = svoff.getValue(0);
            nudD10.Value = svoff.getValue(1);
            nudD11.Value = svoff.getValue(2);
            nudD12.Value = svoff.getValue(3);
            nudD2.Value = svoff.getValue(4);
            nudD4.Value = svoff.getValue(5);
            nudD7.Value = svoff.getValue(6);
            nudD8.Value = svoff.getValue(7);

            nudD9.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD10.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD11.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD12.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD2.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD4.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD7.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD8.ValueChanged += new EventHandler(nud_ValueChanged);

            this.io = io;
            nudD9.Enabled = io.fSvMotor1Used;
            nudD10.Enabled = io.fSvMotor2Used;
            nudD11.Enabled = io.fSvMotor3Used;
            nudD12.Enabled = io.fSvMotor4Used;
            nudD2.Enabled = io.fSvMotor5Used;
            nudD4.Enabled = io.fSvMotor6Used;
            nudD7.Enabled = io.fSvMotor7Used;
            nudD8.Enabled = io.fSvMotor8Used;

            // DCモーター校正メニュー初期化
            byte m1Rate = svoff.getDCCalibInfo().calibM1Rate;
            byte m2Rate = svoff.getDCCalibInfo().calibM2Rate;
            Debug.Write("Pin: " + m1Rate);
            tbM1.Value = (int)(255 * m1Rate / 100.0);
            lbDCM1Value.Text = tbM1.Value.ToString();
            tbM2.Value = (int)(255 * m2Rate / 100.0);
            lbDCM2Value.Text = tbM2.Value.ToString();

            // 初期化完了後にイベント登録
            this.tbM1.ValueChanged += new System.EventHandler(this.tbDC_ValueChanged);
            this.tbM2.ValueChanged += new System.EventHandler(this.tbDC_ValueChanged);

            // M1/M2どちらかが未使用の場合は機能をオフにする
            if (!(io.fDCMotor1Used && io.fDCMotor2Used)) gbDC.Enabled = false;
        }

        /// <summary>
        /// コンストラクタ(言語指定なし)
        /// </summary>
        /// <param name="svoff">オフセット情報</param>
        /// <param name="io">入出力状態</param>
        public ServoCalib(ServoOffset svoff, stRobotIOStatus io)
        {
            InitializeComponent();

            this.svoff = svoff;
            nudD9.Value = svoff.getValue(0);
            nudD10.Value = svoff.getValue(1);
            nudD11.Value = svoff.getValue(2);
            nudD12.Value = svoff.getValue(3);
            nudD2.Value = svoff.getValue(4);
            nudD4.Value = svoff.getValue(5);
            nudD7.Value = svoff.getValue(6);
            nudD8.Value = svoff.getValue(7);

            nudD9.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD10.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD11.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD12.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD2.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD4.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD7.ValueChanged += new EventHandler(nud_ValueChanged);
            nudD8.ValueChanged += new EventHandler(nud_ValueChanged);

            this.io = io;
            nudD9.Enabled = io.fSvMotor1Used;
            nudD10.Enabled = io.fSvMotor2Used;
            nudD11.Enabled = io.fSvMotor3Used;
            nudD12.Enabled = io.fSvMotor4Used;
            nudD2.Enabled = io.fSvMotor5Used;
            nudD4.Enabled = io.fSvMotor6Used;
            nudD7.Enabled = io.fSvMotor7Used;
            nudD8.Enabled = io.fSvMotor8Used;

            // DCモーター校正メニュー初期化
            byte m1Rate = svoff.getDCCalibInfo().calibM1Rate;
            byte m2Rate = svoff.getDCCalibInfo().calibM2Rate;
            Debug.Write("Pin: " + m1Rate);
            tbM1.Value = (int)(255 * m1Rate / 100.0);
            lbDCM1Value.Text = tbM1.Value.ToString();
            tbM2.Value = (int)(255 * m2Rate / 100.0);
            lbDCM2Value.Text = tbM2.Value.ToString();

            // 初期化完了後にイベント登録
            this.tbM1.ValueChanged += new System.EventHandler(this.tbDC_ValueChanged);
            this.tbM2.ValueChanged += new System.EventHandler(this.tbDC_ValueChanged);

            // M1/M2どちらかが未使用の場合は機能をオフにする
            if (!(io.fDCMotor1Used && io.fDCMotor2Used)) gbDC.Enabled = false;
        }

        //---------------------------------------------------------------------
        // 概要   : サーボモーターの角度を送信するCOMポートを開く
        // 引数   : string comPort  : COMポート番号
        // 戻り値 : true->成功 false->失敗
        // Date   : 2014/02/17 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        public Boolean openCOMPort(string comPort)
        {
            // -----------------------------------------------------------------
            // シリアルポートを開く
            // -----------------------------------------------------------------
            try
            {
                targetPort = new SerialPort(comPort, 38400);
                // Arduinoとシリアル通信する場合、DtrEnableをtrueに設定した場合、
                // 基板にソフトウェアリセットがかかる。DtrEnableをfalseに設定す
                // ればソフトウェアリセットはかかりません。
                targetPort.DtrEnable = true;
                targetPort.Open();

                targetPort.DtrEnable = false;
                targetPort.DiscardOutBuffer();
            }
            // ポートが開かれていない場合(物理的に接続が切断された場合)
            catch (UnauthorizedAccessException)
            {   // 例外内容：ポートへのアクセスが拒否されています。
                MessageBox.Show(Properties.Resources.str_msg_err_miscon1 +
                    Environment.NewLine +
                    Properties.Resources.str_msg_err_miscon2);
                return false;
            }
            // 物理的に接続が切断された場合
            catch (IOException)
            {   // 例外内容：ポートが無効状態です。
                MessageBox.Show(Properties.Resources.str_msg_err_miscon1 +
                    Environment.NewLine +
                    Properties.Resources.str_msg_err_miscon3);
                return false;
            }
            // 予期せぬ例外処理
            catch (Exception e)
            {   // ログを取る
            }


//            targetPort.ReadTimeout = 10000;       // リードタイムアウト(100msec)を設定
//            int recv = targetPort.ReadByte();         // ボードからのデータ受信
//            MessageBox.Show(recv.ToString());
//            // ACKが返ってきたら、戻り値を設定し送信処理を抜ける
//            if (recv == 0x80)
//            {
//                fsuccess = RET_ACK;
//                break;
//            }

            // エラーコードを初期化
            errorCode = (byte)ConnectingCondition.CONNECTED;
            isConnectiong = true;
            return true;
        }

        //---------------------------------------------------------------------
        // 概要   : サーボモーターの角度を送信するCOMポートを閉じる
        // Date   : 2014/02/17 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        public void closeCOMPort()
        {
            // -----------------------------------------------------------------
            // シリアルポートを閉じる
            // -----------------------------------------------------------------
            targetPort.Close();
            targetPort.Dispose();
        }

        //---------------------------------------------------------------------
        // 概要   : サーボモーターの角度のセットアップ
        // Date   : 2014/02/24 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        public void setupCurrentServoDegrees()
        {
            if (nudD9.Enabled) { setServoMotor(4, svoff.getValue(0)); }
            if (nudD10.Enabled) { setServoMotor(5, svoff.getValue(1)); }
            if (nudD11.Enabled) { setServoMotor(6, svoff.getValue(2)); }
            if (nudD12.Enabled) { setServoMotor(7, svoff.getValue(3)); }
            if (nudD2.Enabled) { setServoMotor(0, svoff.getValue(4)); }
            if (nudD4.Enabled) { setServoMotor(1, svoff.getValue(5)); }
            if (nudD7.Enabled) { setServoMotor(2, svoff.getValue(6)); }
            if (nudD8.Enabled) { setServoMotor(3, svoff.getValue(7)); }
        }

        //---------------------------------------------------------------------
        // Date   : 2014/08/04 : 0.973 kagayama    新規作成
        //        : 2015/09/03 : 1.133 kagayama    変更抜け分追加
        //---------------------------------------------------------------------
        /// <summary>
        /// フォームのテキストをひらがなに変換
        /// </summary>
        private void convToHiragana()
        {
            this.Text = "サーボモーターかくどこうせい";
            gbServo.Text = "";
            lbDegree.Text = "ど";
            label1.Text = "ど";
            label2.Text = "ど";
            label3.Text = "ど";
            label4.Text = "ど";
            label5.Text = "ど";
            label6.Text = "ど";
            label7.Text = "ど";
            gbDC.Text = "";
            btDCCalibStart.Text = "かいてん";
            btDCCalibStop.Text = "ていし";
            btCancel.Text = "キャンセル";
        }

        #region 【イベント】 OKボタン押下時の処理
        //---------------------------------------------------------------------
        // 概要   : オフセットを設定する
        // Date   : 2014/02/17 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        private void btOK_Click(object sender, EventArgs e)
        {
            svoff.set(0, (int)nudD9.Value);
            svoff.set(1, (int)nudD10.Value);
            svoff.set(2, (int)nudD11.Value);
            svoff.set(3, (int)nudD12.Value);
            svoff.set(4, (int)nudD2.Value);
            svoff.set(5, (int)nudD4.Value);
            svoff.set(6, (int)nudD7.Value);
            svoff.set(7, (int)nudD8.Value);

            setDCCalibInfo();
        }
        #endregion

        #region 【イベント】 リセットボタン押下時の処理
        //---------------------------------------------------------------------
        // 概要   : オフセットを設定しない
        // Date   : 2014/02/17 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        private void btReset_Click(object sender, EventArgs e)
        {
            if (nudD9.Enabled) nudD9.Value = 0;
            if (nudD10.Enabled) nudD10.Value = 0;
            if (nudD11.Enabled) nudD11.Value = 0;
            if (nudD12.Enabled) nudD12.Value = 0;
            if (nudD2.Enabled) nudD2.Value = 0;
            if (nudD4.Enabled) nudD4.Value = 0;
            if (nudD7.Enabled) nudD7.Value = 0;
            if (nudD8.Enabled) nudD8.Value = 0;
        }
        #endregion

        #region 【イベント】 オフセット値が変化した時の処理
        //---------------------------------------------------------------------
        // 概要   : 
        // Date   : 2014/02/17 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        const byte Limit = 128;
        private void nud_ValueChanged(object sender, EventArgs e)
        {
            if (targetPort == null || isConnectiong == false) return;

            int offset = (int)(sender as NumericUpDown).Value;
            byte port;
            if ((sender as NumericUpDown).Name == "nudD9") port = 4;
            else if ((sender as NumericUpDown).Name == "nudD10") port = 5;
            else if ((sender as NumericUpDown).Name == "nudD11") port = 6;
            else if ((sender as NumericUpDown).Name == "nudD12") port = 7;
            else if ((sender as NumericUpDown).Name == "nudD2") port = 0;
            else if ((sender as NumericUpDown).Name == "nudD4") port = 1;
            else if ((sender as NumericUpDown).Name == "nudD7") port = 2;
            else port = 3;

            // サーボモーターに角度情報を送信
            if (setServoMotor(port, offset) != RET_SUCCESS)
            {   // 通信が切断された場合
                // エラーコードを設定して、キャンセルボタンをクリック
                btCancel.PerformClick();
                errorCode = (byte)ConnectingCondition.DISCONNECT;
                isConnectiong = false;  // 通信切断状態を設定
            }
        }
        #endregion

        //---------------------------------------------------------------------
        // 概要   : サーボモーターの設定
        // Date   : 2014/02/24 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        private byte setServoMotor(byte port, int offset)
        {
            int angle = Math.Min(Math.Max(0, (int)(90 + offset)), 180);

            byte carry = 0;     // 桁上がり:0
            byte low = 0x00;    // 下位側データ
            byte high = 0x90;   // 上位側データ
            // 下位側データの作成
            if (angle >= Limit) // 角度が128度以上の場合
            {
                // 桁上げして128度引いた値を下位側に設定
                carry = 1;
                low = (byte)(angle - Limit);
            }
            else
            {
                // 下位側に設定
                low = (byte)angle;
            }

            // 上位側データの作成
            high = (byte)(high | (port << 1) | carry);

            byte[] send = new byte[2];          // 送信データ
            send[0] = high; // 上位側
            send[1] = low;  // 下位側

            return sendPackage(send, port);
        }

        //---------------------------------------------------------------------
        // 概要   : サーボモーターの角度情報の送信データを作成する
        // Date   : 2014/02/24 : 0.95 kawase    新規作成
        //---------------------------------------------------------------------
        const byte RET_SUCCESS = 0;
        const byte RET_NORESPONSE = 1;
        const byte RET_DISCONNECT = 2;
        const byte RET_ERROR = 3;

        const int RETRYNUM = 10;
        const byte ACK = 0x01;
        private byte sendPackage(byte[] package, byte ack)
        {
            byte fsuccess = RET_SUCCESS;

            targetPort.ReadTimeout = 100;       // リードタイムアウト(100msec)を設定
            targetPort.WriteTimeout = 10;      // ライトタイムアウト(100msec)を設定
            try
            {
                targetPort.DiscardOutBuffer();                 // 送信バッファのクリア
                targetPort.DiscardInBuffer();                  // 受信バッファのクリア
                targetPort.Write(package, 0, package.Length);  // ボードへデータ送信
                targetPort.ReadByte();                         // Ack受信
            }
            // Ack Readタイムアウトが発生した場合
            catch (TimeoutException e)
            {
                targetPort.Close();
                targetPort.Dispose();
                fsuccess = RET_DISCONNECT;
            }
            // 物理的に接続が切断された場合
            catch (IOException e)
            {
                targetPort.Close();
                targetPort.Dispose();
                fsuccess = RET_DISCONNECT;
            }
            // ポートが開かれていない場合(物理的に接続が切断された場合)
            catch (InvalidOperationException e)
            {
                fsuccess = RET_DISCONNECT;
            }
            // 予期せぬ例外処理
            catch (Exception e)
            {   // ログを出力する
                fsuccess = RET_ERROR;
            }
            return fsuccess;
        }

        byte errorCode = 0;
        public byte getErrorCode()
        {
            return errorCode;
        }

        #region 【イベント】 言語切り替えメニュー 日本語
        // ---------------------------------------------------------------------
        // 備考       : 
        // Date       : 2013/09/10 kagayama  0.01    新規作成
        // ---------------------------------------------------------------------
        public void tsmiJapanese_Click()
        {
            // メニューバー 言語 - 日本語
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "jp" ||     // 日本
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "iv")       // 既定(invariant)
            {
                return;
            }

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("");
            changeLanguage(this, null, Thread.CurrentThread.CurrentUICulture);
        }
        #endregion

        #region 【イベント】 言語切り替えメニュー 英語
        // ---------------------------------------------------------------------
        // 備考       : 
        // Date       : 2013/09/10 kagayama  0.01    新規作成
        // ---------------------------------------------------------------------
        public void tsmiEnglish_Click()
        {
            // メニューバー 言語 - 英語
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en") return;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            changeLanguage(this, null, Thread.CurrentThread.CurrentUICulture);
        }
        #endregion

        #region 【イベント】 言語切り替えメニュー 中国語
        // ---------------------------------------------------------------------
        // 備考       : 
        // Date       : 2013/09/10 kagayama  0.01    新規作成
        // ---------------------------------------------------------------------
        public void tsmiChinese_Click()
        {
            // メニューバー 言語 - 中国語
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh") return;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            changeLanguage(this, null, Thread.CurrentThread.CurrentUICulture);
        }
        #endregion    

        #region 【共通】 ローカライゼーション動的変更
        // -----------------------------------------------------------------------------------
        // 概要       : ローカリゼーションの動的変更
        // 引数       : object						obj			:対象のオブジェクト
        //            : ComponentResourceManager	resources	:オブジェクトのリソース列挙用
        //            : CaltureInfo					calture     :変更するカルチャ   
        // 備考       : 
        // Date       : 2013/09/10 kagayama  0.01    新規作成
        //            : 2013/09/17 kagayama  0.01    右クリックメニューへの対応
        // -----------------------------------------------------------------------------------
        private void changeLanguage(object obj, ComponentResourceManager resources, CultureInfo culture)
        {
            if (obj is Form)
            {
                Form form = obj as Form;
                ComponentResourceManager form_resources = new ComponentResourceManager(form.GetType());
                form_resources.ApplyResources(form, "$this", culture);
                foreach (Form fm in form.OwnedForms) { changeLanguage(fm, form_resources, culture); }
                foreach (Form fm in form.MdiChildren) { changeLanguage(fm, form_resources, culture); }
                foreach (Control c in form.Controls) { changeLanguage(c, form_resources, culture); }
            }
            else if (obj is ToolStripItem)
            {
                //                Debug.Write("tsi: " + obj + "\r\n");
                ToolStripItem item = obj as ToolStripItem;
                resources.ApplyResources(item, item.Name, culture);
                if (item is ToolStripDropDownItem)
                {
                    ToolStripDropDownItem dditem = item as ToolStripDropDownItem;
                    foreach (ToolStripItem tsi in dditem.DropDownItems) { changeLanguage(tsi, resources, culture); }
                }
            }
            else if (obj is UserControl)
            {
                UserControl uc = obj as UserControl;
                ComponentResourceManager uc_resources = new ComponentResourceManager(uc.GetType());
                uc_resources.ApplyResources(uc, uc.Name, culture);
                foreach (Control c in uc.Controls) { changeLanguage(c, uc_resources, culture); }
            }
            else if (obj is Control)
            {
                Control ctrl = obj as Control;
                resources.ApplyResources(ctrl, ctrl.Name, culture);
                foreach (Control c in ctrl.Controls) { changeLanguage(c, resources, culture); }
                if (ctrl.ContextMenuStrip != null)
                {
                    foreach (object tsmi in ctrl.ContextMenuStrip.Items)
                    {
                        //                        Debug.Write("tsmi**********************************" + "\r\n");

                        changeLanguage(tsmi, resources, culture);
                    }
                }
                if (ctrl is ToolStrip)
                {
                    ToolStrip ts = ctrl as ToolStrip;
                    foreach (ToolStripItem tsi in ts.Items) { changeLanguage(tsi, resources, culture); }
                }
            }
        }
        #endregion

        private void btDCCalib_Click(object sender, EventArgs e)
        {
            // DCモーター校正の回転ボタンがクリックされた場合
            if ((sender as Button).Name == "btDCCalibStart")
            {
                byte power = (byte)((tbM1.Value / 255.0) * 100);
                setDCMotorPower(0, power);
                power = (byte)((tbM2.Value / 255.0) * 100);
                setDCMotorPower(1, power);

                // M1のDCモーターに回転開始を送信
                if (setDCMotorSwitch(0, true) != RET_SUCCESS)
                {   // 通信が切断された場合
                    // エラーコードを設定して、キャンセルボタンをクリック
                    btCancel.PerformClick();
                    errorCode = (byte)ConnectingCondition.DISCONNECT;
                    isConnectiong = false;  // 通信切断状態を設定
                    return;
                }
                // M2のDCモーターに回転開始を送信
                if (setDCMotorSwitch(1, true) != RET_SUCCESS)
                {   // 通信が切断された場合
                    // エラーコードを設定して、キャンセルボタンをクリック
                    btCancel.PerformClick();
                    errorCode = (byte)ConnectingCondition.DISCONNECT;
                    isConnectiong = false;  // 通信切断状態を設定
                    return;
                }

                // スライダの有効表示
                tbM1.Enabled = true;
                tbM2.Enabled = true;
                btDCCalibStart.Enabled = false; // 回転ボタンを無効化
                btDCCalibStop.Enabled = true;   // 停止ボタンを有効化
                // 校正設定用ボタンを無効化
                btOK.Enabled = false;
                btCancel.Enabled = false;
            }
            // DCモーター校正の停止ボタンがクリックされた場合
            if ((sender as Button).Name == "btDCCalibStop") {
                // M1のDCモーターに回転停止を送信
                if (setDCMotorSwitch(0, false) != RET_SUCCESS)
                {   // 通信が切断された場合
                    // エラーコードを設定して、キャンセルボタンをクリック
                    btCancel.PerformClick();
                    errorCode = (byte)ConnectingCondition.DISCONNECT;
                    isConnectiong = false;  // 通信切断状態を設定
                    return;
                }
                // M2のDCモーターに回転停止を送信
                if (setDCMotorSwitch(1, false) != RET_SUCCESS)
                {   // 通信が切断された場合
                    // エラーコードを設定して、キャンセルボタンをクリック
                    btCancel.PerformClick();
                    errorCode = (byte)ConnectingCondition.DISCONNECT;
                    isConnectiong = false;  // 通信切断状態を設定
                    return;
                }

                // スライダの無効表示
                tbM1.Enabled = false;
                tbM2.Enabled = false;
                btDCCalibStart.Enabled = true; // 回転ボタンを有効化
                btDCCalibStop.Enabled = false; // 停止ボタンを無効化
                // 校正設定用ボタンを有効化
                btOK.Enabled = true;
                btCancel.Enabled = true;
            }

        }

        private void tbDC_ValueChanged(object sender, EventArgs e)
        {
            lbDCM1Value.Text = tbM1.Value.ToString();
            lbDCM2Value.Text = tbM2.Value.ToString();

            TrackBar tbPower = sender as TrackBar;
            byte power = (byte)((tbPower.Value/255.0)*100);
            if (tbPower.Name == "tbM1") {
                setDCMotorPower(0, power);
            }
            else if (tbPower.Name == "tbM2") {
                setDCMotorPower(1, power);
            }
        }

        private void setDCCalibInfo()
        {
            byte m1Rate, m2Rate;
            m1Rate = (byte)(Math.Ceiling((double)tbM1.Value * 100 / 255.0));
            m2Rate = (byte)(Math.Ceiling((double)tbM2.Value * 100 / 255.0));
            svoff.setDCCalib(m1Rate, m2Rate);
        }

        private DCCalibInfo getCalibInfo()
        {
            DCCalibInfo calibInfo;
            calibInfo.calibM1Rate = (byte)(Math.Ceiling((double)tbM1.Value * 100 / 255.0));
            calibInfo.calibM2Rate = (byte)(Math.Ceiling((double)tbM2.Value * 100 / 255.0));

            return calibInfo;
        }

        //---------------------------------------------------------------------
        // 概要   : DCモーターのパワーの設定
        // Date   : 2015/05/11 : 1.xx kawase    新規作成
        //---------------------------------------------------------------------
        private byte setDCMotorPower(byte port, int rate)
        {
            byte low = 0x00;    // 下位側データ
            byte high = 0x80;   // 上位側データ
            // 下位側に設定
            low = (byte)rate;   // set power at 'setting' area

            // 上位側データの作成
            high = (byte)(high | (port << 3));  // set M1 or M2 at 'pin' area
            high = (byte)(high | 0x04);         // set power at 'action' area

            byte[] send = new byte[2];          // 送信データ
            send[0] = high; // 上位側
            send[1] = low;  // 下位側

            return sendPackage(send, 0);
        }

        //---------------------------------------------------------------------
        // 概要   : DCモーターのON/OFFの設定
        // Date   : 2015/05/11 : 1.xx kawase    新規作成
        //---------------------------------------------------------------------
        private byte setDCMotorSwitch(byte port, bool startRolling)
        {
            byte low = 0x00;    // 下位側データ
            byte high = 0x80;   // 上位側データ

            // 下位側に設定
            low = 0;   // If isOn arg is true, set cw. If isOn arg is false, set breke.

            // 上位側データの作成
            high = (byte)(high | (port << 3));  // set M1 or M2 at 'pin' area
            if (startRolling)
            {    // Switch on
                high = (byte)(high | 0x01); 
            } else {        // Switch off
                high = (byte)(high | 0x02); 
            }

            byte[] send = new byte[2];          // 送信データ
            send[0] = high; // 上位側
            send[1] = low;  // 下位側

            return sendPackage(send, 0);
        }

        private void ServoCalib_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}
