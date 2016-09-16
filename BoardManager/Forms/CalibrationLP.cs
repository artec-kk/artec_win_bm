using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Artec.TestModeCommunication;

namespace ScratchConnection.Forms
{
    public partial class CalibrationLP : CalibrationBase
    {
        TestModeCommunication tcom;
        bool canSend = true;

        public CalibrationLP(ServoOffset svoff, stRobotIOStatus io, TestModeCommunication tcom)
            : base(svoff, io)
        {
            InitializeComponent();

            this.tcom = tcom;

            saD5.nudAngle.Value = svoff.getValue(0);
            saD6.nudAngle.Value = svoff.getValue(1);
            saD9.nudAngle.Value = svoff.getValue(2);
            saD10.nudAngle.Value = svoff.getValue(3);
            saD11.nudAngle.Value = svoff.getValue(4);

            saD5.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD6.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD9.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD10.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD11.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);

            saD5.Enabled = io.fSvMotor1Used;
            saD6.Enabled = io.fSvMotor2Used;
            saD9.Enabled = io.fSvMotor3Used;
            saD10.Enabled = io.fSvMotor4Used;
            saD11.Enabled = io.fSvMotor5Used;

            // サーボモーターの初期設定
            if (saD5.Enabled) setServoMotor(PinID.D5, svoff.getValue(0));
            if (saD6.Enabled) setServoMotor(PinID.D6, svoff.getValue(1));
            if (saD9.Enabled) setServoMotor(PinID.D9, svoff.getValue(2));
            if (saD10.Enabled) setServoMotor(PinID.D10, svoff.getValue(3));
            if (saD11.Enabled) setServoMotor(PinID.D11, svoff.getValue(4));

            // DCモーター速度設定(DCモーター2つが有効な時のみ)
            if (io.fDCMotor1Used && io.fDCMotor2Used)
            {
                setDCMotorPower(0, (byte)tbM1.Value);
                setDCMotorPower(1, (byte)tbM2.Value);
            }
        }

        void nudAngle_ValueChanged(object sender, EventArgs e)
        {
            if (canSend)
            {
                canSend = false;
                int offset = (int)(sender as NumericUpDown).Value;
                PinID port = PinID.D5;
                object target = (sender as NumericUpDown).Parent;

                if (target == saD5) port = PinID.D5;
                if (target == saD6) port = PinID.D6;
                if (target == saD9) port = PinID.D9;
                if (target == saD10) port = PinID.D10;
                if (target == saD11) port = PinID.D11;

                // サーボモーターに角度情報を送信
                setServoMotor(port, offset);
                //if (setServoMotor(port, offset) != RET_SUCCESS)
                //{   // 通信が切断された場合
                //    // エラーコードを設定して、キャンセルボタンをクリック
                //    btCancel.PerformClick();
                //    errorCode = (byte)ConnectingCondition.DISCONNECT;
                //    isConnectiong = false;  // 通信切断状態を設定
                //}
                canSend = true;
            }
        }

        void setServoMotor(PinID port, int offset)
        {
            byte[] angle = { (byte)(90 + offset) };
            tcom.sendCommand(CommandID.SV, port, angle);
        }

        protected override void setDCMotorSwitch(byte port, bool onoff)
        {
            base.setDCMotorSwitch(port, onoff);

            byte[] args = new byte[2];
            args[0] = onoff ? (byte)0x01 : (byte)0x02;   // 0x01: 回転 0x02: 停止
            args[1] = 0;                                 // 回転:: 0:正転 1:逆転 | 停止:: 0:ブレーキあり 1:ブレーキなし
            PinID pin = (port == 0) ? PinID.M1 : PinID.M2;
            //setDCMotorPower(port, 100);
            tcom.sendCommand(CommandID.DC, pin, args);
        }

        protected override void setDCMotorPower(byte port, byte power)
        {
            if (canSend)
            {
                canSend = false;
                base.setDCMotorPower(port, power);

                byte[] args = new byte[2];
                args[0] = (byte)0x04;                        // 0x04: 速度
                args[1] = power;
                PinID pin = (port == 0) ? PinID.M1 : PinID.M2;
                tcom.sendCommand(CommandID.DC, pin, args);
                canSend = true;
            }
        }

        protected override void updateCalibInfo()
        {
            base.updateCalibInfo();

            offsetInfo.set(0, (int)saD5.nudAngle.Value);
            offsetInfo.set(1, (int)saD6.nudAngle.Value);
            offsetInfo.set(2, (int)saD9.nudAngle.Value);
            offsetInfo.set(3, (int)saD10.nudAngle.Value);
            offsetInfo.set(4, (int)saD11.nudAngle.Value);
        }

        protected override void resetAngle()
        {
            base.resetAngle();

            if (saD5.Enabled) saD5.nudAngle.Value = 0;
            if (saD6.Enabled) saD6.nudAngle.Value = 0;
            if (saD9.Enabled) saD9.nudAngle.Value = 0;
            if (saD10.Enabled) saD10.nudAngle.Value = 0;
            if (saD11.Enabled) saD11.nudAngle.Value = 0;
        }
    }
}
