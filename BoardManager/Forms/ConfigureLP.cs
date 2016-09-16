using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScratchConnection.Forms
{
    public partial class ConfigureLP : ConfigureBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigureLP(bool hiragana)
            : base(hiragana)
        {
            InitializeComponent();

            // センサー用コンボボックスのアイテムの初期化
            ComboBox[] sensors = { cbbSIn1, cbbSIn2, cbbSIn3, cbbSIn4, cbbSIn5, cbbSIn6 };
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
                        if (!(elm == cbbSIn5 || elm == cbbSIn6) && i == (int)OptionPartsID.Accelerometer)
                        {   // Sensor A5またはA6以外で、加速度センサーの場合、登録しない
                        }
                        else
                        {   // 上記以外は登録する
                            elm.Items.Add(name);
                        }
                    }
                    else if (i < (int)OptionPartsID.BuzzerEnd + 1)
                    {   // 電子機器の場合
                        // Sensor A4またはA5で、カラーLEDの場合、登録しない
                        if ((elm == cbbSIn5 || elm == cbbSIn6) && i == (int)OptionPartsID.ColorLED)
                        {
                        }
                        else
                        {   // 上記以外は登録する
                            elm.Items.Add(name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// コンストラクタ(与えられた入出力情報で初期化)
        /// </summary>
        /// <param name="state">入出力情報</param>
        /// <param name="hiragana">ひらがなフラグ</param>
        public ConfigureLP(stRobotIOStatus state, bool hiragana = false)
            : this(hiragana)
        {
            this.ioStatus = state;
            // 引数でモータとセンサのチェックボックスを初期化
            cbDCM1.Checked = state.fDCMotor1Used;
            cbDCM2.Checked = state.fDCMotor2Used;
            cbSVD5.Checked = state.fSvMotor1Used;
            cbSVD6.Checked = state.fSvMotor2Used;
            cbSVD9.Checked = state.fSvMotor3Used;
            cbSVD10.Checked = state.fSvMotor4Used;
            cbSVD11.Checked = state.fSvMotor5Used;
            cbSIn1.Checked = state.fSns1Used;
            cbSIn2.Checked = state.fSns2Used;
            cbSIn3.Checked = state.fSns3Used;
            cbSIn4.Checked = state.fSns4Used;
            cbSIn5.Checked = state.fSns5Used;
            cbSIn6.Checked = state.fSns6Used;
            cbLEDD5.Checked = (state as stRobotIOStatusLP).fLED1Used;
            cbLEDD6.Checked = (state as stRobotIOStatusLP).fLED2Used;
            cbLEDD9.Checked = (state as stRobotIOStatusLP).fLED3Used;
            cbClock.Checked = (state as stRobotIOStatusLP).fClockUsed;

            // コンボボックスの選択を初期化
            cbbSIn1.SelectedItem = si.strOptionParts[state.nSns1Kind];
            cbbSIn2.SelectedItem = si.strOptionParts[state.nSns2Kind];
            cbbSIn3.SelectedItem = si.strOptionParts[state.nSns3Kind];
            cbbSIn4.SelectedItem = si.strOptionParts[state.nSns4Kind];
            cbbSIn5.SelectedItem = si.strOptionParts[state.nSns5Kind];
            cbbSIn6.SelectedItem = si.strOptionParts[state.nSns6Kind];
        }

        
        #region 【イベント】 DCモータ出力No.1のチェックボックスが変更されたときの処理
        /// <summary>
        /// DCモーターM1チェックON/OFF M1/D5,D6,D9,D10,A2,A3の排他処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDCOut1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)
            {
                cbSVD5.Enabled = false;
                cbSVD6.Enabled = false;
                cbSVD9.Enabled = false;
                cbSVD10.Enabled = false;
                cbLEDD5.Enabled = false;
                cbLEDD6.Enabled = false;
                cbLEDD9.Enabled = false;
                cbSIn3.Enabled = false;
                cbSIn4.Enabled = false;
            }
            else
            {
                if (!cbDCM1.Checked && !cbDCM2.Checked)
                {
                    cbSVD5.Enabled = true;
                    cbSVD6.Enabled = true;
                    cbSVD9.Enabled = true;
                    cbSVD10.Enabled = true;
                    cbLEDD5.Enabled = true;
                    cbLEDD6.Enabled = true;
                    cbLEDD9.Enabled = true;
                    cbSIn3.Enabled = true;
                    cbSIn4.Enabled = true;
                }
            }
        }
        #endregion

        #region 【イベント】 DCモータ出力No.2のチェックボックスが変更されたときの処理
        /// <summary>
        /// DCモーターM2チェックON/OFF M2/D6,D10の排他処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDCOut2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)
            {
                cbSVD6.Enabled = false;
                cbSVD10.Enabled = false;
                cbLEDD6.Enabled = false;
            }
            else
            {
                cbSVD6.Enabled = true;
                cbSVD10.Enabled = true;
                cbLEDD6.Enabled = true;
            }
        }
        #endregion

        #region 【イベント】 サーボモータD5, D6, D9, D10のチェックボックスが変更されたときの処理
        private void cbSVOut56910_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                if (target != cbSVD10)
                {
                    cbLEDD5.Enabled = false;
                    cbLEDD6.Enabled = false;
                    cbLEDD9.Enabled = false;
                }
                cbDCM1.Enabled = false;
                cbDCM2.Enabled = false;
            }
            else
            {   // アンチェック状態
                if (target != cbSVD10)
                {
                    if (!cbSVD5.Checked && !cbSVD6.Checked && !cbSVD9.Checked)
                    {
                        cbLEDD5.Enabled = true;
                        cbLEDD6.Enabled = true;
                        cbLEDD9.Enabled = true;
                    }
                }
                if (enableDCCheck())
                {
                    cbDCM1.Enabled = true;
                    cbDCM2.Enabled = true;
                }
            }
        }
        #endregion

        #region 【イベント】 サーボモータD5, D9のチェックボックスが変更されたときの処理
        private void cbSVOut59_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                if (target == cbSVD5)
                {
                    cbLEDD5.Enabled = false;
                }
                else if(target == cbSVD9)
                {
                    cbLEDD9.Enabled = false;
                }
                cbDCM1.Enabled = false;
            }
            else
            {   // アンチェック状態
                if (target == cbSVD5)
                {
                    cbLEDD5.Enabled = true;
                }
                else if (target == cbSVD9)
                {
                    cbLEDD9.Enabled = true;
                }
                if (!cbSVD5.Checked && !cbSVD9.Checked && !cbLEDD5.Checked && !cbLEDD9.Checked)
                {   
                    cbDCM1.Enabled = true;
                }
            }
        }
        #endregion

        #region 【イベント】 サーボモータD6, D10のチェックボックスが変更されたときの処理
        private void cbSVOut610_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                if (target == cbSVD6)
                {
                    cbLEDD6.Enabled = false;
                }
                cbDCM2.Enabled = false;
            }
            else
            {   // アンチェック状態
                if (target == cbSVD6)
                {
                    cbLEDD6.Enabled = true;
                }
                if (!cbSVD6.Checked && !cbSVD10.Checked && !cbLEDD6.Checked)
                {
                    cbDCM2.Enabled = true;
                }
            }
        }
        #endregion

        #region 【イベント】 サーボモータD11のチェックボックスが変更されたときの処理
        private void cbSVOut11_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から液晶時計のチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                cbClock.Enabled = false;
            }
            else
            {   // アンチェック状態
                cbClock.Enabled = true;
            }
        }
        #endregion

        #region 【イベント】 LEDD5, D6, D9のチェックボックスが変更されたときの処理
        private void cbLEDD569_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                cbSVD5.Enabled = false;
                cbSVD6.Enabled = false;
                cbSVD9.Enabled = false;
                cbDCM1.Enabled = false;
                cbDCM2.Enabled = false;
            }
            else
            {   // アンチェック状態
                if (!cbLEDD5.Checked && !cbLEDD6.Checked && !cbLEDD9.Checked)
                {
                    cbSVD5.Enabled = true;
                    cbSVD6.Enabled = true;
                    cbSVD9.Enabled = true;
                }

                if (enableDCCheck())
                {
                    cbDCM1.Enabled = true;
                    cbDCM2.Enabled = true;
                }
            }
        }
        #endregion

        #region 【イベント】 LEDD5, D9のチェックボックスが変更されたときの処理
        private void cbLEDD59_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                if (target == cbLEDD5)
                {
                    cbSVD5.Enabled = false;
                }
                else if (target == cbLEDD9)
                {
                    cbSVD9.Enabled = false;
                }
                cbDCM1.Enabled = false;
            }
            else
            {   // アンチェック状態
                if (target == cbLEDD5)
                {
                    cbSVD5.Enabled = true;
                }
                else if (target == cbLEDD9)
                {
                    cbSVD9.Enabled = true;
                }
                if (!cbSVD5.Checked && !cbSVD9.Checked && !cbLEDD5.Checked && !cbLEDD9.Checked)
                {
                    cbDCM1.Enabled = true;
                }
            }
        }
        #endregion

        #region 【イベント】 LEDD6のチェックボックスが変更されたときの処理
        private void cbLED6_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                cbSVD6.Enabled = false;
                cbDCM2.Enabled = false;
            }
            else
            {   // アンチェック状態
                cbSVD6.Enabled = true;
                if (!cbSVD6.Checked && !cbSVD10.Checked && !cbLEDD6.Checked)
                {
                    cbDCM2.Enabled = true;
                }
            }
        }
        #endregion

        #region 【イベント】 液晶時計のチェックボックスが変更されたときの処理
        private void cbClock_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定からサーボモーターD11のチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                cbSVD11.Enabled = false;
            }
            else
            {   // アンチェック状態
                cbSVD11.Enabled = true;
            }
        }
        #endregion

        #region 【イベント】 センサー入力のチェックボックスが変更されたときの処理
        private void cbSIn_CheckedChanged(object sender, EventArgs e)
        {
            ComboBox target = null;
            if (sender == cbSIn1)
                target = cbbSIn1;
            if (sender == cbSIn2)
                target = cbbSIn2;
            if (sender == cbSIn3)
                target = cbbSIn3;
            if (sender == cbSIn4)
                target = cbbSIn4;

            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if(target != null)
                target.Enabled = (sender as CheckBox).Checked;
        }
        #endregion

        #region 【イベント】 センサーA2, A3入力のチェックボックスが変更されたときの処理
        private void cbSIn34_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox t1 = (CheckBox)sender;
            if (t1.Checked)
            {
                cbDCM1.Enabled = false;
                cbDCM2.Enabled = false;
            }
            else
            {
                if (enableDCCheck())
                {
                    cbDCM1.Enabled = true;
                    cbDCM2.Enabled = true;
                }
            }

            ComboBox target = null;
            if (sender == cbSIn3)
                target = cbbSIn3;
            if (sender == cbSIn4)
                target = cbbSIn4;

            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target != null)
                target.Enabled = (sender as CheckBox).Checked;
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

        bool enableDCCheck()
        {
            return !cbSVD5.Checked && !cbSVD6.Checked && !cbSVD9.Checked && !cbSVD10.Checked
                && !cbLEDD5.Checked && !cbLEDD6.Checked && !cbLEDD9.Checked
                && !cbSIn3.Checked && !cbSIn4.Checked;
        }

        /// <summary>
        /// フォームの内容に従って入出力情報を更新する
        /// </summary>
        protected override void updateIOStatus()
        {
            ioStatus.fDCMotor1Used = cbDCM1.Checked;
            ioStatus.fDCMotor2Used = cbDCM2.Checked;
            ioStatus.fSvMotor1Used = cbSVD5.Checked;
            ioStatus.fSvMotor2Used = cbSVD6.Checked;
            ioStatus.fSvMotor3Used = cbSVD9.Checked;
            ioStatus.fSvMotor4Used = cbSVD10.Checked;
            ioStatus.fSvMotor5Used = cbSVD11.Checked;
            ioStatus.fSns1Used = cbSIn1.Checked;
            ioStatus.fSns2Used = cbSIn2.Checked;
            ioStatus.fSns3Used = cbSIn3.Checked;
            ioStatus.fSns4Used = cbSIn4.Checked;
            ioStatus.fSns5Used = cbSIn5.Checked;
            ioStatus.fSns6Used = cbSIn6.Checked;
            (ioStatus as stRobotIOStatusLP).fLED1Used = cbLEDD5.Checked;
            (ioStatus as stRobotIOStatusLP).fLED2Used = cbLEDD6.Checked;
            (ioStatus as stRobotIOStatusLP).fLED3Used = cbLEDD9.Checked;

            ioStatus.nSns1Kind = indexToId(cbbSIn1, cbbSIn1.SelectedIndex);
            ioStatus.nSns2Kind = indexToId(cbbSIn2, cbbSIn2.SelectedIndex);
            ioStatus.nSns3Kind = indexToId(cbbSIn3, cbbSIn3.SelectedIndex);
            ioStatus.nSns4Kind = indexToId(cbbSIn4, cbbSIn4.SelectedIndex);
            ioStatus.nSns5Kind = indexToId(cbbSIn5, cbbSIn5.SelectedIndex);
            ioStatus.nSns6Kind = indexToId(cbbSIn6, cbbSIn6.SelectedIndex);
            (ioStatus as stRobotIOStatusLP).fClockUsed = cbClock.Checked;
        }

        protected override void convertToHiragana()
        {
            base.convertToHiragana();
            this.cbLEDD5.Text = "D5 [あか]";
            this.cbLEDD6.Text = "D6 [き]";
            this.cbLEDD9.Text = "D9 [みどり]";
            this.cbClock.Text = "えきしょうどけいをつかう";
        }
    }
}
