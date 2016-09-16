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
    public partial class CalibrationST : CalibrationBase
    {
        ICommandSender com;
        TestModeCommand comgen;
        bool canSend = true;

        public CalibrationST(ServoOffset offset, stRobotIOStatus io, ICommandSender com)
            : base(offset, io)
        {
            InitializeComponent();

            this.com = com;
            comgen = new TestModeCommand();

            saD9.nudAngle.Value = offset.getValue(0);
            saD10.nudAngle.Value = offset.getValue(1);
            saD11.nudAngle.Value = offset.getValue(2);
            saD12.nudAngle.Value = offset.getValue(3);
            saD2.nudAngle.Value = offset.getValue(4);
            saD4.nudAngle.Value = offset.getValue(5);
            saD7.nudAngle.Value = offset.getValue(6);
            saD8.nudAngle.Value = offset.getValue(7);

            saD9.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD10.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD11.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD12.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD2.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD4.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD7.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);
            saD8.nudAngle.ValueChanged += new EventHandler(nudAngle_ValueChanged);

            saD9.Enabled = io.fSvMotor1Used;
            saD10.Enabled = io.fSvMotor2Used;
            saD11.Enabled = io.fSvMotor3Used;
            saD12.Enabled = io.fSvMotor4Used;
            saD2.Enabled = io.fSvMotor5Used;
            saD4.Enabled = io.fSvMotor6Used;
            saD7.Enabled = io.fSvMotor7Used;
            saD8.Enabled = io.fSvMotor8Used;
        }

        void nudAngle_ValueChanged(object sender, EventArgs e)
        {
            if (canSend)
            {
                canSend = false;
                int offset = (int)(sender as NumericUpDown).Value;
                byte port = 0;
                object target = (sender as NumericUpDown).Parent;

                if (target == saD2) port = 0;
                if (target == saD4) port = 1;
                if (target == saD7) port = 2;
                if (target == saD8) port = 3;
                if (target == saD9) port = 4;
                if (target == saD10) port = 5;
                if (target == saD11) port = 6;
                if (target == saD12) port = 7;

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

        void setServoMotor(byte port, int offset)
        {
            byte[] args = { port, (byte)(90 + offset) };
            //tcom.sendCommand(CommandID.SV, port, angle);
            //com.sendCommand(data);
            com.sendCommand(comgen.actCommand(CommandID.SV, args));
        }

        protected override void setDCMotorSwitch(byte port, bool onoff)
        {
            base.setDCMotorSwitch(port, onoff);

            byte[] args = new byte[3];
            args[0] = port;
            args[1] = onoff ? (byte)0x01 : (byte)0x02;   // 0x01: 回転 0x02: 停止
            args[2] = 0;                                 // 回転:: 0:正転 1:逆転 | 停止:: 0:ブレーキあり 1:ブレーキなし
            //PinID pin = (port == 0) ? PinID.M1 : PinID.M2;
            //setDCMotorPower(port, 100);
            //tcom.sendCommand(CommandID.DC, pin, args);
            com.sendCommand(comgen.actCommand(CommandID.DC, args));
        }

        protected override void setDCMotorPower(byte port, byte power)
        {
            if (canSend)
            {
                canSend = false;
                base.setDCMotorPower(port, power);

                byte[] args = new byte[3];
                args[0] = port;
                args[1] = (byte)0x04;                        // 0x04: 速度
                args[2] = power;
                PinID pin = (port == 0) ? PinID.M1 : PinID.M2;
                //tcom.sendCommand(CommandID.DC, pin, args);
                com.sendCommand(comgen.actCommand(CommandID.DC, args));
                canSend = true;
            }
        }

        protected override void updateCalibInfo()
        {
            base.updateCalibInfo();

            offsetInfo.set(0, (int)saD9.nudAngle.Value);
            offsetInfo.set(1, (int)saD10.nudAngle.Value);
            offsetInfo.set(2, (int)saD11.nudAngle.Value);
            offsetInfo.set(3, (int)saD12.nudAngle.Value);
            offsetInfo.set(4, (int)saD2.nudAngle.Value);
            offsetInfo.set(5, (int)saD4.nudAngle.Value);
            offsetInfo.set(6, (int)saD7.nudAngle.Value);
            offsetInfo.set(7, (int)saD8.nudAngle.Value);
        }

        protected override void resetAngle()
        {
            base.resetAngle();

            if (saD9.Enabled) saD9.nudAngle.Value = 0;
            if (saD10.Enabled) saD10.nudAngle.Value = 0;
            if (saD11.Enabled) saD11.nudAngle.Value = 0;
            if (saD12.Enabled) saD12.nudAngle.Value = 0;
            if (saD2.Enabled) saD2.nudAngle.Value = 0;
            if (saD4.Enabled) saD4.nudAngle.Value = 0;
            if (saD7.Enabled) saD7.nudAngle.Value = 0;
            if (saD8.Enabled) saD8.nudAngle.Value = 0;
        }
    }
}
