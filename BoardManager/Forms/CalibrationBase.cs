using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ScratchConnection.Forms
{
    public partial class CalibrationBase : Form
    {
        protected ServoOffset offsetInfo;
        protected stRobotIOStatus io;

        public CalibrationBase()
        {
            InitializeComponent();
        }

        public CalibrationBase(ServoOffset offsetInfo, stRobotIOStatus io, bool hiragana = false)
            : this()
        {
            this.offsetInfo = offsetInfo;
            this.io = io;
            if (hiragana) this.Load += new EventHandler(CalibrationBase_Load);

            // DCモーター校正メニュー初期化
            byte m1Rate = offsetInfo.getDCCalibInfo().calibM1Rate;
            byte m2Rate = offsetInfo.getDCCalibInfo().calibM2Rate;
            Debug.Write("Pin: " + m1Rate);
            tbM1.Value = (int)(255 * m1Rate / 100.0);
            lbDCM1Value.Text = tbM1.Value.ToString();
            tbM2.Value = (int)(255 * m2Rate / 100.0);

            lbDCM2Value.Text = tbM2.Value.ToString();
            // 初期化完了後にイベント登録
            this.tbM1.ValueChanged += new EventHandler(tb_ValueChanged);
            this.tbM2.ValueChanged += new EventHandler(tb_ValueChanged);

            // M1/M2どちらかが未使用の場合は機能をオフにする
            if (!(io.fDCMotor1Used && io.fDCMotor2Used)) gbDC.Enabled = false;
        }

        void CalibrationBase_Load(object sender, EventArgs e)
        {
            convertToHiragana();
        }

        void tb_ValueChanged(object sender, EventArgs e)
        {
            lbDCM1Value.Text = tbM1.Value.ToString();
            lbDCM2Value.Text = tbM2.Value.ToString();

            TrackBar tbPower = sender as TrackBar;
            byte power = (byte)((tbPower.Value / 255.0) * 100);
            if (tbPower.Name == "tbM1")
            {
                setDCMotorPower(0, power);
            }
            else if (tbPower.Name == "tbM2")
            {
                setDCMotorPower(1, power);
            }
        }

        /// <summary>
        /// 指定したDCモーターの速度を設定する
        /// </summary>
        /// <param name="port">0: M1 1:M2</param>
        /// <param name="power">回転速度 0~255</param>
        protected virtual void setDCMotorPower(byte port, byte power)
        {
            Debug.WriteLine(string.Format("DC: {0} power: {1}", port, power));
        }

        /// <summary>
        /// 指定したDCモーターを回転・停止
        /// </summary>
        /// <param name="port">0: M1 1:M2</param>
        /// <param name="onoff">true: 回転 false: 停止</param>
        protected virtual void setDCMotorSwitch(byte port, bool onoff)
        {
            Debug.WriteLine(string.Format("DC: {0} onoff: {1}", port, onoff));
        }

        /// <summary>
        /// 校正情報を更新する
        /// </summary>
        protected virtual void updateCalibInfo()
        {
            byte m1Rate, m2Rate;
            m1Rate = (byte)(Math.Ceiling((double)tbM1.Value * 100 / 255.0));
            m2Rate = (byte)(Math.Ceiling((double)tbM2.Value * 100 / 255.0));
            offsetInfo.setDCCalib(m1Rate, m2Rate);
        }

        /// <summary>
        /// フォームが閉じられる際に情報を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(DialogResult == DialogResult.OK)
                updateCalibInfo();
        }

        /// <summary>
        /// 回転ボタンクリック時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDCCalibStart_Click(object sender, EventArgs e)
        {
            // DCモーター校正の回転ボタンがクリックされた場合
            if ((sender as Button).Name == "btDCCalibStart")
            {
                byte power = (byte)((tbM1.Value / 255.0) * 100);
                setDCMotorPower(0, power);
                power = (byte)((tbM2.Value / 255.0) * 100);
                setDCMotorPower(1, power);

                // M1のDCモーターに回転開始を送信
                setDCMotorSwitch(0, true);
                //if (setDCMotorSwitch(0, true) != RET_SUCCESS)
                //{   // 通信が切断された場合
                //    // エラーコードを設定して、キャンセルボタンをクリック
                //    btCancel.PerformClick();
                //    errorCode = (byte)ConnectingCondition.DISCONNECT;
                //    isConnectiong = false;  // 通信切断状態を設定
                //    return;
                //}
                // M2のDCモーターに回転開始を送信
                setDCMotorSwitch(1, true);
                //if (setDCMotorSwitch(1, true) != RET_SUCCESS)
                //{   // 通信が切断された場合
                //    // エラーコードを設定して、キャンセルボタンをクリック
                //    btCancel.PerformClick();
                //    errorCode = (byte)ConnectingCondition.DISCONNECT;
                //    isConnectiong = false;  // 通信切断状態を設定
                //    return;
                //}

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
            if ((sender as Button).Name == "btDCCalibStop")
            {
                // M1のDCモーターに回転停止を送信
                setDCMotorSwitch(0, false);
                //if (setDCMotorSwitch(0, false) != RET_SUCCESS)
                //{   // 通信が切断された場合
                //    // エラーコードを設定して、キャンセルボタンをクリック
                //    btCancel.PerformClick();
                //    errorCode = (byte)ConnectingCondition.DISCONNECT;
                //    isConnectiong = false;  // 通信切断状態を設定
                //    return;
                //}
                // M2のDCモーターに回転停止を送信
                setDCMotorSwitch(1, false);
                //if (setDCMotorSwitch(1, false) != RET_SUCCESS)
                //{   // 通信が切断された場合
                //    // エラーコードを設定して、キャンセルボタンをクリック
                //    btCancel.PerformClick();
                //    errorCode = (byte)ConnectingCondition.DISCONNECT;
                //    isConnectiong = false;  // 通信切断状態を設定
                //    return;
                //}

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

        private void btReset_Click(object sender, EventArgs e)
        {
            resetAngle();
        }

        /// <summary>
        /// 全てのサーボモーターのオフセットを0にする
        /// </summary>
        protected virtual void resetAngle()
        {
        }

        /// <summary>
        /// ひらがな変換処理。
        /// </summary>
        protected virtual void convertToHiragana()
        {
            this.Text = "モーターこうせい";
            this.gbServo.Text = "サーボモーターこうせい";
            this.gbDC.Text = "DCモーターこうせい";
            this.btDCCalibStart.Text = "かいてん";
            this.btDCCalibStop.Text = "ていし";
        }
    }
}
