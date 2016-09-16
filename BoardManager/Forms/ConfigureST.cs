using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScratchConnection.Forms
{
    public partial class ConfigureST : ConfigureBase
    {
        public ConfigureST(bool hiragana)
            : base(hiragana)
        {
            InitializeComponent();

            // センサー用コンボボックスのアイテムの初期化
            ComboBox[] sensors = { cbbSIn1, cbbSIn2, cbbSIn3, cbbSIn4, cbbSIn5, cbbSIn6, cbbSIn7, cbbSIn8 };
            si = new SensorItems(hiragana);
            foreach (ComboBox elm in sensors)
            {   // 各コンボボックスに対する処理
                //                int i = 0;
                elm.Items.Clear();
                for (int i = 0; i <= (int)OptionPartsID.BuzzerEnd; i++)
                {
                    string name = si.strOptionParts[i];
                    if (i < (int)OptionPartsID.SensorEnd + 1)
                    {   // センサーの場合
                        if ((elm == cbbSIn7 || elm == cbbSIn8) && i == (int)OptionPartsID.Touch)
                        {   // Sensor A7またはA8で、タッチセンサーの場合、登録しない
                        }
                        else if (!(elm == cbbSIn5 || elm == cbbSIn6) && i == (int)OptionPartsID.Accelerometer)
                        {   // Sensor A5またはA6以外で、加速度センサーの場合、登録しない
                        }
                        else
                        {   // 上記以外は登録する
                            elm.Items.Add(name);
                        }
                    }
                    else if (i < (int)OptionPartsID.BuzzerEnd + 1)
                    {   // 電子機器の場合
                        if (elm == cbbSIn7 || elm == cbbSIn8)
                        {   // Sensor A7またはA8の場合、登録しない
                        }
                        else
                        {   // 上記以外は登録する
                            // Sensor A4またはA5で、カラーLEDの場合、登録しない
                            if ((elm == cbbSIn5 || elm == cbbSIn6) && name == si.strOptionParts[(int)OptionPartsID.ColorLED])
                            {
                            }
                            else
                            {   // 上記以外は登録する
                                // TODO: ひらがな対応
                                //if (lang == HIRAGANA)
                                //    name = si.strOptionPartsHiragana[i];
                                elm.Items.Add(name);
                            }
                        }
                    }
                }
            }
        }

        public ConfigureST(stRobotIOStatus state, bool hiragana = false)
            : this(hiragana)
        {
            this.ioStatus = state;
            // 引数でモータとセンサのチェックボックスを初期化
            cbDCM1.Checked = state.fDCMotor1Used;
            cbDCM2.Checked = state.fDCMotor2Used;
            cbSVD9.Checked = state.fSvMotor1Used;
            cbSVD10.Checked = state.fSvMotor2Used;
            cbSVD11.Checked = state.fSvMotor3Used;
            cbSVD12.Checked = state.fSvMotor4Used;
            cbSVD2.Checked = state.fSvMotor5Used;
            cbSVD4.Checked = state.fSvMotor6Used;
            cbSVD7.Checked = state.fSvMotor7Used;
            cbSVD8.Checked = state.fSvMotor8Used;
            cbSIn1.Checked = state.fSns1Used;
            cbSIn2.Checked = state.fSns2Used;
            cbSIn3.Checked = state.fSns3Used;
            cbSIn4.Checked = state.fSns4Used;
            cbSIn5.Checked = state.fSns5Used;
            cbSIn6.Checked = state.fSns6Used;
            cbSIn7.Checked = state.fSns7Used;
            cbSIn8.Checked = state.fSns8Used;

            // TODO: ひらがな対応
            cbbSIn1.SelectedItem = si.strOptionParts[state.nSns1Kind];
            cbbSIn2.SelectedItem = si.strOptionParts[state.nSns2Kind];
            cbbSIn3.SelectedItem = si.strOptionParts[state.nSns3Kind];
            cbbSIn4.SelectedItem = si.strOptionParts[state.nSns4Kind];
            cbbSIn5.SelectedItem = si.strOptionParts[state.nSns5Kind];
            cbbSIn6.SelectedItem = si.strOptionParts[state.nSns6Kind];
            cbbSIn7.SelectedItem = si.strOptionParts[state.nSns7Kind];
            cbbSIn8.SelectedItem = si.strOptionParts[state.nSns8Kind];

            cbBIn1.Checked = state.fBtn1Used;
            cbBIn2.Checked = state.fBtn2Used;
            cbBIn3.Checked = state.fBtn3Used;
            cbBIn4.Checked = state.fBtn4Used;
        }

        #region 【イベント】 DCモータ出力No.1のチェックボックスが変更されたときの処理
        private void cbDCOut1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)
            {
                cbSVD2.Enabled = false;
                cbSVD4.Enabled = false;
            }
            else
            {
                cbSVD2.Enabled = true;
                cbSVD4.Enabled = true;
            }
        }
        #endregion

        #region 【イベント】 DCモータ出力No.2のチェックボックスが変更されたときの処理
        private void cbDCOut2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)
            {
                cbSVD7.Enabled = false;
                cbSVD8.Enabled = false;
            }
            else
            {
                cbSVD7.Enabled = true;
                cbSVD8.Enabled = true;
            }
        }
        #endregion

        #region 【イベント】 サーボモータD2, D4のチェックボックスが変更されたときの処理
        private void cbSVD2_4_CheckedChanged(object sender, EventArgs e)
        {
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (!cbSVD2.Checked && !cbSVD4.Checked)
                cbDCM1.Enabled = true;
            else
                cbDCM1.Enabled = false;
        }
        #endregion

        #region 【イベント】 サーボモータD7, D8のチェックボックスが変更されたときの処理
        private void cbSVD7_8_CheckedChanged(object sender, EventArgs e)
        {
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (!cbSVD7.Checked && !cbSVD8.Checked)
                cbDCM2.Enabled = true;
            else
                cbDCM2.Enabled = false;
        }
        #endregion

        #region 【イベント】 センサー入力１のチェックボックスが変更されたときの処理
        // ---------------------------------------------------------------------
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/12/13 kagayama  0.93    カラーLED用の処理を無効化
        // ---------------------------------------------------------------------
        private void cbSIn_CheckedChanged(object sender, EventArgs e)
        {
            ComboBox trgCbx = null;
            CheckBox trgCbBtn = null;
            if (sender == cbSIn1)
            {
                trgCbx = cbbSIn1;
                trgCbBtn = cbBIn1;
            }
            if (sender == cbSIn2)
            {
                trgCbx = cbbSIn2;
                trgCbBtn = cbBIn2;
            }
            if (sender == cbSIn3)
            {
                trgCbx = cbbSIn3;
                trgCbBtn = cbBIn3;
            }
            if (sender == cbSIn4)
            {
                trgCbx = cbbSIn4;
                trgCbBtn = cbBIn4;
            }
            if (sender == cbSIn7)
            {
                trgCbx = cbbSIn7;
            }
            if (sender == cbSIn8)
            {
                trgCbx = cbbSIn8;
            }

            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (trgCbx != null)
                trgCbx.Enabled = (sender as CheckBox).Checked;
            // チェックボックスの設定から関連するボタンのチェックボックスを変更する
            if (trgCbBtn != null)
                trgCbBtn.Enabled = !(sender as CheckBox).Checked;
        }
        #endregion

        #region 【イベント】 センサー入力のチェックボックスが変更されたときの処理
        private void cbSIn56_CheckedChanged(object sender, EventArgs e)
        {
            ComboBox target = null;
            if (sender == cbSIn5)
                target = cbbSIn5;
            if (sender == cbSIn6)
                target = cbbSIn6;

            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target != null)
                target.Enabled = (sender as CheckBox).Checked;

            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            int num = -1;
            string strAccel = si.strOptionParts[(int)OptionPartsID.Accelerometer];
            //if (language == HIRAGANA) strAccel = si.strOptionPartsHiragana[(int)OptionPartsID.Accelerometer];  // ひらがな対応

            if (!(sender as CheckBox).Checked)
            {
                if (target.Text == strAccel)
                {
                    cbbSIn5.SelectedIndex = (int)OptionPartsID.SensorTop;
                    cbbSIn6.SelectedIndex = (int)OptionPartsID.SensorTop;
                }
            }
            // 入力5,6のチェック状態に応じて表示を変更
            if (cbSIn5.Checked ^ cbSIn6.Checked)   // どちらか一方だけがチェックされた状態になった
            {
                num = cbbSIn5.Items.IndexOf(strAccel);
                if (num != -1)
                {
                    cbbSIn5.Items.RemoveAt(num);
                    cbbSIn6.Items.RemoveAt(num);
                }
            }
            else
            {
                num = (int)OptionPartsID.Accelerometer;
                cbbSIn5.Items.Insert(num, strAccel);
                cbbSIn6.Items.Insert(num, strAccel);
            }

        }
        #endregion

        #region 【イベント】 ボタン入力１のチェックボックスが変更されたときの処理
        private void cbBIn_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = null;
            if (sender == cbBIn1)
                target = cbSIn1;
            if (sender == cbBIn2)
                target = cbSIn2;
            if (sender == cbBIn3)
                target = cbSIn3;
            if (sender == cbBIn4)
                target = cbSIn4;

            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target != null)
                target.Enabled = !(sender as CheckBox).Checked;
        }
        #endregion

        private void cbbSIn56_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox target = sender as ComboBox;
            ComboBox changeTarget = (target == cbbSIn5) ? cbbSIn6 : cbbSIn5;
            int selectedSensorKind = indexToId(target, target.SelectedIndex);

            if (selectedSensorKind == (int)OptionPartsID.Accelerometer)
            {   // もう一方の選択状態を加速度センサーにする
                changeTarget.SelectedIndexChanged -= cbbSIn56_SelectedIndexChanged;
                changeTarget.SelectedIndex = getIndex(target, OptionPartsID.Accelerometer);
                changeTarget.SelectedIndexChanged += cbbSIn56_SelectedIndexChanged;
            }
            else
            {   // もう一方の選択状態をトップ(光センサー)にする
                if (indexToId(changeTarget, changeTarget.SelectedIndex) == (int)OptionPartsID.Accelerometer)
                {
                    changeTarget.SelectedIndexChanged -= cbbSIn56_SelectedIndexChanged;
                    changeTarget.SelectedIndex = getIndex(target, OptionPartsID.SensorTop);
                    changeTarget.SelectedIndexChanged += cbbSIn56_SelectedIndexChanged;
                }
            }
        }
 
        /// <summary>
        /// フォームの内容に従って入出力情報を更新する
        /// </summary>
        protected override void updateIOStatus()
        {
            ioStatus.fDCMotor1Used = cbDCM1.Checked;
            ioStatus.fDCMotor2Used = cbDCM2.Checked;
            ioStatus.fSvMotor1Used = cbSVD9.Checked;
            ioStatus.fSvMotor2Used = cbSVD10.Checked;
            ioStatus.fSvMotor3Used = cbSVD11.Checked;
            ioStatus.fSvMotor4Used = cbSVD12.Checked;
            ioStatus.fSvMotor5Used = cbSVD2.Checked;
            ioStatus.fSvMotor6Used = cbSVD4.Checked;
            ioStatus.fSvMotor7Used = cbSVD7.Checked;
            ioStatus.fSvMotor8Used = cbSVD8.Checked;

            ioStatus.fSns1Used = cbSIn1.Checked;
            ioStatus.fSns2Used = cbSIn2.Checked;
            ioStatus.fSns3Used = cbSIn3.Checked;
            ioStatus.fSns4Used = cbSIn4.Checked;
            ioStatus.fSns5Used = cbSIn5.Checked;
            ioStatus.fSns6Used = cbSIn6.Checked;
            ioStatus.fSns7Used = cbSIn7.Checked;
            ioStatus.fSns8Used = cbSIn8.Checked;

            ioStatus.nSns1Kind = indexToId(cbbSIn1, cbbSIn1.SelectedIndex);
            ioStatus.nSns2Kind = indexToId(cbbSIn2, cbbSIn2.SelectedIndex);
            ioStatus.nSns3Kind = indexToId(cbbSIn3, cbbSIn3.SelectedIndex);
            ioStatus.nSns4Kind = indexToId(cbbSIn4, cbbSIn4.SelectedIndex);
            ioStatus.nSns5Kind = indexToId(cbbSIn5, cbbSIn5.SelectedIndex);
            ioStatus.nSns6Kind = indexToId(cbbSIn6, cbbSIn6.SelectedIndex);
            ioStatus.nSns7Kind = indexToId(cbbSIn7, cbbSIn7.SelectedIndex);
            ioStatus.nSns8Kind = indexToId(cbbSIn8, cbbSIn8.SelectedIndex);

            ioStatus.fBtn1Used = cbBIn1.Checked;
            ioStatus.fBtn2Used = cbBIn2.Checked;
            ioStatus.fBtn3Used = cbBIn3.Checked;
            ioStatus.fBtn4Used = cbBIn4.Checked;
        }
    }
}
