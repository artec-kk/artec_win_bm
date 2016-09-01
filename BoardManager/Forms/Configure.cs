using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace ScratchConnection
{
    public partial class Configure : Form
    {
        // 2つのサーボモータON/OFF状態を表す変数
        // 0(b'00) : sv1, sv2ともにOFF
        // 1(b'01) : sv1:OFF, sv2:ON
        // 2(b'10) : sv1:ON , sv2:OFF
        // 3(b'11) : sv1:ON , sv2:ON
        UInt16 fSV56On;    // サーボモータNo.5, No.6の状態
        UInt16 fSV78On;    // サーボモータNo.7, No.8の状態

        // カラーLED選択状態を表す変数
        // 0(b'00) : 選択
        // 1(b'01) : 非選択
        bool fCS123On;   // センサA0~A2がカラーLED選択状態
        bool fCS234On;   // センサA1~A3がカラーLED選択状態

        public bool FCS123
        {
            get
            {
                return fCS123On;
            }
            set
            {
                fCS123On = value;
            }
        }
        public bool FCS234
        {
            get
            {
                return fCS234On;
            }
            set
            {
                fCS234On = value;
            }
        }
        stRobotIOStatus conf;
        SensorItems si;

        int language;                   // 使用言語保持用
        bool onChangeByOthers = false;  // イベント呼出し後の変更中を示すフラグ

        // ---------------------------------------------------------------------
        // 概要       : コンストラクタ
        // 引数       : stRobotIOStatus state    入出力設定
        //            : int             lang     言語(0:英, 1:日, 2:中)
        // Date       : 2013/07/06    0.01  kawase     新規作成
        //            : 2013/08/02    0.01  kagayama   加速度センサへの対応
        //            : 2013/08/06    0.01  kagayama   カラーLEDに対する処理の追加
        //            : 2013/08/27    0.01  kagayama   加速度センサ定数名変更に伴う変更
        //            : 2013/09/03    0.01  kagayama   カラーLEDに対する処理の追加
        //            : 2013/12/13    0.93  kagayama   カラーLED用処理の無効化
        //            : 2014/08/04    0.973 kagayama   ひらがな対応
        // ---------------------------------------------------------------------        
        const int ENGLISH = 0;
        const int JAPAN = 1;
        const int CHINESE = 2;
        const int HIRAGANA = 3;
        public Configure(stRobotIOStatus state, int lang)
        {
            InitializeComponent();

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
            this.language = lang;

            // フラグの初期化
            fSV56On = 0;
            fSV78On = 0;

            // センサー用コンボボックスのアイテムの初期化
            ComboBox[] sensors = { cbbSIn1, cbbSIn2, cbbSIn3, cbbSIn4, cbbSIn5, cbbSIn6, cbbSIn7, cbbSIn8 };
            si = new SensorItems();
            foreach (ComboBox elm in sensors)
            {   // 各コンボボックスに対する処理
//                int i = 0;
                elm.Items.Clear();
                for(int i = 0;i <= (int)OptionPartsID.BuzzerEnd;i++){
                    string name = si.strOptionParts[i];
                    if (i < (int)OptionPartsID.SensorEnd + 1)
                    {   // センサーの場合
                        if ((elm == cbbSIn7 || elm == cbbSIn8) && name == si.strOptionParts[(int)OptionPartsID.Touch])
                        {   // Sensor A7またはA8で、タッチセンサーの場合、登録しない
                        }
                        else if(!(elm == cbbSIn5 || elm == cbbSIn6) && name == si.strOptionParts[(int)OptionPartsID.Accelerometer])
                        {   // Sensor A5またはA6以外で、加速度センサーの場合、登録しない
                        }
                        else
                        {   // 上記以外は登録する
                            // TODO: ひらがな対応[済]
                            if (lang == HIRAGANA)
                                name = si.strOptionPartsHiragana[i];
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
                                if (lang == HIRAGANA)
                                    name = si.strOptionPartsHiragana[i];
                                elm.Items.Add(name);
                            }
                        }
                    }
                }                
            }


            conf = new stRobotIOStatus();
//            conf = state;
            // 引数でモータとセンサのチェックボックスを初期化
            cbDCOut1.Checked = state.fDCMotor1Used;
            cbDCOut2.Checked = state.fDCMotor2Used;
            cbSVOut1.Checked = state.fSvMotor1Used;
            cbSVOut2.Checked = state.fSvMotor2Used;
            cbSVOut3.Checked = state.fSvMotor3Used;
            cbSVOut4.Checked = state.fSvMotor4Used;
            cbSVOut5.Checked = state.fSvMotor5Used;
            cbSVOut6.Checked = state.fSvMotor6Used;
            cbSVOut7.Checked = state.fSvMotor7Used;
            cbSVOut8.Checked = state.fSvMotor8Used;
            cbSIn1.Checked = state.fSns1Used;
            cbSIn2.Checked = state.fSns2Used;
            cbSIn3.Checked = state.fSns3Used;
            cbSIn4.Checked = state.fSns4Used;
            cbSIn5.Checked = state.fSns5Used;
            cbSIn6.Checked = state.fSns6Used;
            cbSIn7.Checked = state.fSns7Used;
            cbSIn8.Checked = state.fSns8Used;

            /* カラーLED用の処理
            // カラーLEDの選択状態を判定
            if (state.nSns1Kind == (int)OptionPartsID.ColorLED)
                fCS123On = true;
            else if (state.nSns4Kind == (int)OptionPartsID.ColorLED)
                fCS234On = true;
            */

            // TODO: ひらがな対応[済]
            if (lang == HIRAGANA)
            {
                cbbSIn1.SelectedItem = si.strOptionPartsHiragana[state.nSns1Kind];
                cbbSIn2.SelectedItem = si.strOptionPartsHiragana[state.nSns2Kind];
                cbbSIn3.SelectedItem = si.strOptionPartsHiragana[state.nSns3Kind];
                cbbSIn4.SelectedItem = si.strOptionPartsHiragana[state.nSns4Kind];
                cbbSIn5.SelectedItem = si.strOptionPartsHiragana[state.nSns5Kind];
                cbbSIn6.SelectedItem = si.strOptionPartsHiragana[state.nSns6Kind];
                cbbSIn7.SelectedItem = si.strOptionPartsHiragana[state.nSns7Kind];
                cbbSIn8.SelectedItem = si.strOptionPartsHiragana[state.nSns8Kind];
            }
            else
            {
                cbbSIn1.SelectedItem = si.strOptionParts[state.nSns1Kind];
                cbbSIn2.SelectedItem = si.strOptionParts[state.nSns2Kind];
                cbbSIn3.SelectedItem = si.strOptionParts[state.nSns3Kind];
                cbbSIn4.SelectedItem = si.strOptionParts[state.nSns4Kind];
                cbbSIn5.SelectedItem = si.strOptionParts[state.nSns5Kind];
                cbbSIn6.SelectedItem = si.strOptionParts[state.nSns6Kind];
                cbbSIn7.SelectedItem = si.strOptionParts[state.nSns7Kind];
                cbbSIn8.SelectedItem = si.strOptionParts[state.nSns8Kind];
            }
            cbBIn1.Checked = state.fBtn1Used;
            cbBIn2.Checked = state.fBtn2Used;
            cbBIn3.Checked = state.fBtn3Used;
            cbBIn4.Checked = state.fBtn4Used;
        }

        /// <summary>
        /// コンストラクタ(言語指定なし)
        /// </summary>
        /// <param name="state">入出力管理オブジェクト</param>
        public Configure(stRobotIOStatus state)
        {
            InitializeComponent();

            // フラグの初期化
            fSV56On = 0;
            fSV78On = 0;

            // センサー用コンボボックスのアイテムの初期化
            ComboBox[] sensors = { cbbSIn1, cbbSIn2, cbbSIn3, cbbSIn4, cbbSIn5, cbbSIn6, cbbSIn7, cbbSIn8 };
            si = new SensorItems();
            foreach (ComboBox elm in sensors)
            {   // 各コンボボックスに対する処理
                //                int i = 0;
                elm.Items.Clear();
                for (int i = 0; i <= (int)OptionPartsID.BuzzerEnd; i++)
                {
                    string name = si.strOptionParts[i];
                    if (i < (int)OptionPartsID.SensorEnd + 1)
                    {   // センサーの場合
                        if ((elm == cbbSIn7 || elm == cbbSIn8) && name == si.strOptionParts[(int)OptionPartsID.Touch])
                        {   // Sensor A7またはA8で、タッチセンサーの場合、登録しない
                        }
                        else if (!(elm == cbbSIn5 || elm == cbbSIn6) && name == si.strOptionParts[(int)OptionPartsID.Accelerometer])
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
                                elm.Items.Add(name);
                            }
                        }
                    }
                }
            }


            conf = new stRobotIOStatus();
            //            conf = state;
            // 引数でモータとセンサのチェックボックスを初期化
            cbDCOut1.Checked = state.fDCMotor1Used;
            cbDCOut2.Checked = state.fDCMotor2Used;
            cbSVOut1.Checked = state.fSvMotor1Used;
            cbSVOut2.Checked = state.fSvMotor2Used;
            cbSVOut3.Checked = state.fSvMotor3Used;
            cbSVOut4.Checked = state.fSvMotor4Used;
            cbSVOut5.Checked = state.fSvMotor5Used;
            cbSVOut6.Checked = state.fSvMotor6Used;
            cbSVOut7.Checked = state.fSvMotor7Used;
            cbSVOut8.Checked = state.fSvMotor8Used;
            cbSIn1.Checked = state.fSns1Used;
            cbSIn2.Checked = state.fSns2Used;
            cbSIn3.Checked = state.fSns3Used;
            cbSIn4.Checked = state.fSns4Used;
            cbSIn5.Checked = state.fSns5Used;
            cbSIn6.Checked = state.fSns6Used;
            cbSIn7.Checked = state.fSns7Used;
            cbSIn8.Checked = state.fSns8Used;

            /* カラーLED用の処理
            // カラーLEDの選択状態を判定
            if (state.nSns1Kind == (int)OptionPartsID.ColorLED)
                fCS123On = true;
            else if (state.nSns4Kind == (int)OptionPartsID.ColorLED)
                fCS234On = true;
            */

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

        public stRobotIOStatus getConfiguration()
        {
            return conf;
        }


        #region 【イベント】 DCモータ出力No.1のチェックボックスが変更されたときの処理
        private void cbDCOut1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)
            {
                cbSVOut5.Enabled = false;
                cbSVOut6.Enabled = false;
            }
            else
            {
                cbSVOut5.Enabled = true;
                cbSVOut6.Enabled = true;
            }
            // DCモータNo.1の設定情報を更新
            conf.fDCMotor1Used = target.Checked;
        }
        #endregion

        #region 【イベント】 DCモータ出力No.2のチェックボックスが変更されたときの処理
        private void cbDCOut2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)
            {
                cbSVOut7.Enabled = false;
                cbSVOut8.Enabled = false;
            }
            else
            {
                cbSVOut7.Enabled = true;
                cbSVOut8.Enabled = true;
            }
            // DCモータNo.2の設定情報を更新
            conf.fDCMotor2Used = target.Checked;
        }
        #endregion

        #region 【イベント】 サーボモータ出力No.1,No.2,No.3,No.4のチェックボックスが変更されたときの処理
        private void cbSVOut1234_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定情報を更新
            if (target == cbSVOut1)
            {
                conf.fSvMotor1Used = target.Checked;
            }
            else if (target == cbSVOut2)
            {
                conf.fSvMotor2Used = target.Checked;
            }
            else if (target == cbSVOut3)
            {
                conf.fSvMotor3Used = target.Checked;
            }
            else if (target == cbSVOut4)
            {
                conf.fSvMotor4Used = target.Checked;
            }
        }
        #endregion

        #region 【イベント】 サーボモータ出力No.5,No.6のチェックボックスが変更されたときの処理
        private void svOut56_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {   // チェック状態
                if (target == cbSVOut5)
                {   // SVOUT5
                    fSV56On |= 0x02;
                }
                else
                {   // SVOUT6
                    fSV56On |= 0x01;
                }
                cbDCOut1.Enabled = false;
            }
            else
            {   // アンチェック状態
                if (target == cbSVOut5)
                {   // SVOUT5
                    fSV56On &= 0xfd;
                }
                else
                {   // SVOUT6
                    fSV56On &= 0xfe;
                }

                if (fSV56On == 0)
                {   
                    cbDCOut1.Enabled = true;
                }
            }
            // サーボモータの設定情報を更新
            if (target == cbSVOut5)
            {
                conf.fSvMotor5Used = target.Checked;
            }
            else
            {
                conf.fSvMotor6Used = target.Checked;
            }
        }
        #endregion

        #region 【イベント】 サーボモータ出力No.7,No.8のチェックボックスが変更されたときの処理
        private void svOut78_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するDCモータのチェックボックスを変更する
            if (target.Checked)
            {
                if (target == cbSVOut7)
                {
                    fSV78On |= 0x02;
                }
                else
                {
                    fSV78On |= 0x01;
                }
                cbDCOut2.Enabled = false;
            }
            else
            {
                if (target == cbSVOut7)
                {
                    fSV78On &= 0xfd;
                }
                else
                {
                    fSV78On &= 0xfe;
                }

                if (fSV78On == 0)
                {
                    cbDCOut2.Enabled = true;
                }
            }
            // サーボモータの設定情報を更新
            if (target == cbSVOut7)
            {
                conf.fSvMotor7Used = target.Checked;
            }
            else
            {
                conf.fSvMotor8Used = target.Checked;
            }
        }
        #endregion

        #region 【イベント】 センサー入力１のチェックボックスが変更されたときの処理
        // ---------------------------------------------------------------------
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/12/13 kagayama  0.93    カラーLED用の処理を無効化
        // ---------------------------------------------------------------------
        private void cbSIn1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target.Checked)             // センサー入力1：有効
            {
                cbbSIn1.Enabled = true;     // センサー選択可能
                cbBIn1.Enabled = false;     // ボタン入力1不可
            }
            else                            // センサー入力1：無効
            {
                cbbSIn1.Enabled = false;    // センサー選択不可
                cbBIn1.Enabled = true;      // ボタン入力1可能
            }
            /* カラーLED用処理
            if (FCS123)
            {
                onChangeByOthers = true;
                cbbSIn1.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                onChangeByOthers = false;
                FCS123 = false;
            }
            CheckColorLEDComboBox();
            */
            // チェックボックスの設定情報を更新
            conf.fSns1Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力２のチェックボックスが変更されたときの処理
        // ---------------------------------------------------------------------
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/12/13 kagayama  0.93    カラーLED用の処理を無効化
        // ---------------------------------------------------------------------
        private void cbSIn2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target.Checked)             // センサー入力2：有効
            {
                cbbSIn2.Enabled = true;     // センサー選択可能
                cbBIn2.Enabled = false;     // ボタン入力2不可
            }
            else
            {
                cbbSIn2.Enabled = false;    // センサー選択不可
                cbBIn2.Enabled = true;      // ボタン入力2可能
            }
            /* カラーLED用処理
            if (FCS123)
            {
                onChangeByOthers = true;
                cbbSIn1.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                onChangeByOthers = false;
                FCS123 = false;
            }
            if (FCS234)
            {
                onChangeByOthers = true;
                cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn4.SelectedIndex = (int)OptionPartsID.SensorTop;
                onChangeByOthers = false;
                FCS234 = false;
            }
            CheckColorLEDComboBox();
            */
            // チェックボックスの設定情報を更新
            conf.fSns2Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力３のチェックボックスが変更されたときの処理
        // ---------------------------------------------------------------------
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/12/13 kagayama  0.93    カラーLED用の処理を無効化
        // ---------------------------------------------------------------------
        private void cbSIn3_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target.Checked)             // センサー入力3：有効
            {
                cbbSIn3.Enabled = true;     // センサー選択可能
                cbBIn3.Enabled = false;     // ボタン入力3不可
            }
            else
            {
                cbbSIn3.Enabled = false;    // センサー選択不可
                cbBIn3.Enabled = true;      // ボタン入力3可能
            }
            /* カラーLED用処理
            if (FCS123)
            {
                onChangeByOthers = true;
                cbbSIn1.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                onChangeByOthers = false;
                FCS123 = false;
            }
            if (FCS234)
            {
                onChangeByOthers = true;
                cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn4.SelectedIndex = (int)OptionPartsID.SensorTop;
                onChangeByOthers = false;
                FCS234 = false;
            }
            CheckColorLEDComboBox();
            */
            // チェックボックスの設定情報を更新
            conf.fSns3Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力４のチェックボックスが変更されたときの処理
        // ---------------------------------------------------------------------
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/12/13 kagayama  0.93    カラーLED用の処理を無効化
        // ---------------------------------------------------------------------
        private void cbSIn4_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target.Checked)             // センサー入力4：有効
            {
                cbbSIn4.Enabled = true;     // センサー選択可能
                cbBIn4.Enabled = false;     // ボタン入力4不可
            }
            else
            {
                cbbSIn4.Enabled = false;    // センサー選択不可
                cbBIn4.Enabled = true;      // ボタン入力4可能   
            }
            /* カラーLED用処理
            if (FCS234)
            {
                onChangeByOthers = true;
                cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                cbbSIn4.SelectedIndex = (int)OptionPartsID.SensorTop;
                onChangeByOthers = false;
                FCS234 = false;
            }
            CheckColorLEDComboBox();
            */
            // チェックボックスの設定情報を更新
            conf.fSns4Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力５のチェックボックスが変更されたときの処理
        // ---------------------------------------------------------------------
        // 備考       : 
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/07/30 kagayama  0.01    加速度センサ選択時の対応
        //            : 2013/08/27 kagayama  0.01    加速度センサ定数名変更に伴う変更
        //            : 2013/10/25 kagayama  0.01    チェック状態による表示処理を変更
        //            : 2013/11/28 kagayama  0.92    加速度センサー非選択時の不具合修正
        //            : 2014/08/04 kagayama  0.973   ひらがな対応
        // ---------------------------------------------------------------------
        private void cbSIn5_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            int num = -1;
            // TODO: ひらがな対応[済]
            string strAccel = si.strOptionParts[(int)OptionPartsID.Accelerometer];
            if (language == HIRAGANA) strAccel = si.strOptionPartsHiragana[(int)OptionPartsID.Accelerometer];  // ひらがな対応

            if (target.Checked)
            {
                cbbSIn5.Enabled = true;
            }
            else
            {
                cbbSIn5.Enabled = false;
                if (cbbSIn5.Text == strAccel)
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
            // チェックボックスの設定情報を更新
            conf.fSns5Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力６のチェックボックスが変更されたときの処理
        // ---------------------------------------------------------------------
        // 備考       : 
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/07/30 kagayama  0.01    加速度センサ選択時の対応
        //            : 2013/08/27 kagayama  0.01    加速度センサ定数名変更に伴う変更
        //            : 2013/10/25 kagayama  0.01    チェック状態による表示処理を変更
        //            : 2013/11/28 kagayama  0.92    
        //            : 2014/08/04 kagayama  0.973   ひらがな対応
        // ---------------------------------------------------------------------
        private void cbSIn6_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            int num = -1;
            // TODO: ひらがな対応[済]
            string strAccel = si.strOptionParts[(int)OptionPartsID.Accelerometer];
            if (language == HIRAGANA) strAccel = si.strOptionPartsHiragana[(int)OptionPartsID.Accelerometer];

            if (target.Checked)
            {
                cbbSIn6.Enabled = true;
            }
            else
            {
                cbbSIn6.Enabled = false;
                if (cbbSIn6.Text == strAccel)
                {
                    cbbSIn5.SelectedIndex = (int)OptionPartsID.SensorTop;
                    cbbSIn6.SelectedIndex = (int)OptionPartsID.SensorTop;
                }
            }
            // 入力5,6のチェック状態に応じて表示を変更
            if (cbSIn5.Checked ^ cbSIn6.Checked)
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
            // チェックボックスの設定情報を更新
            conf.fSns6Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力７のチェックボックスが変更されたときの処理
        private void cbSIn7_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target.Checked)
            {
                cbbSIn7.Enabled = true;
                
            }
            else
            {
                cbbSIn7.Enabled = false;
            }
            
            // チェックボックスの設定情報を更新
            conf.fSns7Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力８のチェックボックスが変更されたときの処理
        private void cbSIn8_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するセンサーのコンボボックスを変更する
            if (target.Checked)
            {
                cbbSIn8.Enabled = true;
            }
            else
            {
                cbbSIn8.Enabled = false;
            }
            
            // チェックボックスの設定情報を更新
            conf.fSns8Used = target.Checked;
        }
        #endregion

        #region 【イベント】 センサー入力No.1～No.8のコンボボックスの要素が選択されたときの処理
        // ---------------------------------------------------------------------
        // 備考       : 
        // Date       : 2013/07/06 kawase    0.01    新規作成
        //            : 2013/07/30 kagayama  0.01    加速度センサ選択時の対応
        //            : 2013/08/06 kagayama  0.01    カラーLED選択時の対応
        //            : 2013/08/27 kagayama  0.01    加速度センサ定数名変更に伴う変更
        //            : 2013/09/03 kagayama  0.01    カラーLED選択時の対応
        //            : 2013/11/28 kagayama  0.92    加速度センサー非選択時の不具合修正
        //            : 2013/12/13 kagayama  0.93    カラーLED用の処理を無効化
        //            : 2014/08/04 kagayama  0.973   ひらがな対応
        // ---------------------------------------------------------------------
        private void cbbSIn12345678_SelectedIndexChanged(object sender, EventArgs e)
        {
            // センサーIDの取得
            ComboBox target = (ComboBox)sender;
            int selectedSensorKind = 0;
            for (int i = 0;i < si.strOptionParts.Length;i++)
            {
                if ((si.strOptionParts[i]) == target.Items[target.SelectedIndex].ToString())
                {
                    selectedSensorKind = i;
                    break;
                }
            }
            // TODO: ひらがな対応[済]
            for (int i = 0; i < si.strOptionPartsHiragana.Length; i++)
            {
                if ((si.strOptionPartsHiragana[i]) == target.Items[target.SelectedIndex].ToString())
                {
                    selectedSensorKind = i;
                    break;
                }
            }

            if (target == cbbSIn1)
            {
                /* カラーLED用の処理
                // コンボボックス2のカラーLEDのindexを取得
                int destSensorKind = getIndex(target, OptionPartsID.ColorLED);
                if (selectedSensorKind == (int)OptionPartsID.ColorLED)
                {
                    // 入力2,3の選択状態をカラーLEDにする
                    if (!fCS123On)
                    {
                        onChangeByOthers = true;
                        cbbSIn2.SelectedIndex = destSensorKind;
                        cbbSIn3.SelectedIndex = destSensorKind;
                        onChangeByOthers = false;
                        fCS123On = true;
                    }
                }
                else
                {   // 入力2,3がカラーLED選択状態の場合
                    if (!onChangeByOthers)
                    {
                        if (fCS123On)
                        {
                            onChangeByOthers = true;
                            cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                            cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                            onChangeByOthers = false;
                            fCS123On = false;
                        }
                    }
                }
                CheckColorLEDComboBox();
                */
                conf.nSns1Kind = selectedSensorKind;
            }
            else if (target == cbbSIn2)
            {
                /* カラーLED用の処理
                int destSensorKind = getIndex(target, OptionPartsID.ColorLED);
                if (selectedSensorKind == (int)OptionPartsID.ColorLED)
                {
                    if (!onChangeByOthers)
                    {
                        if (cbSIn1.Checked && cbSIn4.Checked && !fCS123On && !fCS234On)
                        {
                            ColorLEDDiag cld = new ColorLEDDiag(this);
                            DialogResult res = cld.ShowDialog();
                            if (res == DialogResult.Cancel)
                            {
                                fCS123On = false;
                                fCS234On = false;
                                // キャンセル時、センサートップ（光センサー）に戻す
                                target.SelectedIndex = (int)OptionPartsID.SensorTop;
                            }
                        }
                        else
                        {
                            if (!cbSIn1.Checked)
                                fCS234On = true;
                            if (!cbSIn4.Checked)
                                fCS123On = true;
                        }
                        // 入力1,3の選択状態をカラーLEDにする
                        if (fCS123On)
                        {
                            if (cbbSIn1.SelectedIndex != destSensorKind)
                            {
                                onChangeByOthers = true;
                                cbbSIn1.SelectedIndex = destSensorKind;
                                cbbSIn3.SelectedIndex = destSensorKind;
                                onChangeByOthers = false;
                            }
                        }
                        if (fCS234On)
                        {
                            if (cbbSIn3.SelectedIndex != destSensorKind)
                            {
                                onChangeByOthers = true;
                                cbbSIn3.SelectedIndex = destSensorKind;
                                cbbSIn4.SelectedIndex = destSensorKind;
                                onChangeByOthers = false;
                            }
                        }
                    }
                }
                else
                {   // 入力2,3がカラーLED選択状態の場合
                    if (!onChangeByOthers)
                    {
                        if (fCS123On)
                        {
                            onChangeByOthers = true;
                            cbbSIn1.SelectedIndex = (int)OptionPartsID.SensorTop;
                            cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                            onChangeByOthers = false;
                            fCS123On = false;
                        }
                        if (fCS234On)
                        {
                            onChangeByOthers = true;
                            cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                            cbbSIn4.SelectedIndex = (int)OptionPartsID.SensorTop;
                            onChangeByOthers = false;
                            fCS234On = false;
                        }
                    }
                }
                CheckColorLEDComboBox();
                */
                conf.nSns2Kind = selectedSensorKind;
            }
            else if (target == cbbSIn3)
            {
                /* カラーLED用の処理
                int destSensorKind = getIndex(target, OptionPartsID.ColorLED);
                if (selectedSensorKind == (int)OptionPartsID.ColorLED)
                {
                    if (!onChangeByOthers)
                    {
                        if (cbSIn1.Checked && cbSIn4.Checked && !fCS123On && !fCS234On)
                        {
                            ColorLEDDiag cld = new ColorLEDDiag(this);
                            DialogResult res = cld.ShowDialog();
                            if (res == DialogResult.Cancel)
                            {
                                fCS123On = false;
                                fCS234On = false;
                                // キャンセル時、センサートップ（光センサー）に戻す
                                target.SelectedIndex = (int)OptionPartsID.SensorTop;
                            }
                        }
                        else
                        {
                            if (!cbSIn1.Checked)
                                fCS234On = true;
                            if (!cbSIn4.Checked)
                                fCS123On = true;
                        }
                        // 入力1,3の選択状態をカラーLEDにする
                        if (fCS123On)
                        {
                            if (cbbSIn1.SelectedIndex != destSensorKind)
                            {
                                onChangeByOthers = true;
                                cbbSIn1.SelectedIndex = destSensorKind;
                                cbbSIn2.SelectedIndex = destSensorKind;
                                onChangeByOthers = false;
                            }
                        }
                        if (fCS234On)
                        {
                            if (cbbSIn2.SelectedIndex != destSensorKind)
                            {
                                onChangeByOthers = true;
                                cbbSIn2.SelectedIndex = destSensorKind;
                                cbbSIn4.SelectedIndex = destSensorKind;
                                onChangeByOthers = false;
                            }
                        }
                    }
                }
                else
                {   // 入力2,3がカラーLED選択状態の場合
                    if (!onChangeByOthers)
                    {
                        if (fCS123On)
                        {
                            onChangeByOthers = true;
                            cbbSIn1.SelectedIndex = (int)OptionPartsID.SensorTop;
                            cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                            onChangeByOthers = false;
                            fCS123On = false;
                        }
                        if (fCS234On)
                        {
                            onChangeByOthers = true;
                            cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                            cbbSIn4.SelectedIndex = (int)OptionPartsID.SensorTop;
                            onChangeByOthers = false;
                            fCS234On = false;
                        }
                    }
                }
                CheckColorLEDComboBox();
                */
                conf.nSns3Kind = selectedSensorKind;
            }
            else if (target == cbbSIn4)
            {
                /* カラーLED用の処理
                // コンボボックス2のカラーLEDのindexを取得
                int destSensorKind = getIndex(target, OptionPartsID.ColorLED);
                if (selectedSensorKind == (int)OptionPartsID.ColorLED)
                {
                    // 入力2,3の選択状態をカラーLEDにする
                    if (!fCS234On)
                    {
                        onChangeByOthers = true;
                        cbbSIn2.SelectedIndex = destSensorKind;
                        cbbSIn3.SelectedIndex = destSensorKind;
                        onChangeByOthers = false;
                        fCS234On = true;
                    }
                }
                else
                {   // 入力2,3がカラーLED選択状態の場合
                    if (!onChangeByOthers)
                    {
                        if (fCS234On)
                        {
                            onChangeByOthers = true;
                            cbbSIn2.SelectedIndex = (int)OptionPartsID.SensorTop;
                            cbbSIn3.SelectedIndex = (int)OptionPartsID.SensorTop;
                            onChangeByOthers = false;
                            fCS234On = false;
                        }
                    }
                }
                CheckColorLEDComboBox();
                */
                conf.nSns4Kind = selectedSensorKind;
            }
            else if (target == cbbSIn5)
            {   // 加速度センサー選択
                if (selectedSensorKind == (int)OptionPartsID.Accelerometer)
                {   // 入力6の選択状態を加速度センサーにする
                    if (cbbSIn6.SelectedIndex != (int)OptionPartsID.Accelerometer)
                    {
                        cbbSIn6.SelectedIndex = selectedSensorKind;
                    }
                }
                else
                {   // 入力6が加速度センサー選択状態の場合
                    if (indexToId(cbbSIn6,cbbSIn6.SelectedIndex) == (int)OptionPartsID.Accelerometer)
                    {
                        cbbSIn6.SelectedIndex = (int)OptionPartsID.SensorTop;
                    }
                }
                conf.nSns5Kind = selectedSensorKind;
            }
            else if (target == cbbSIn6)
            {   // 加速度センサー選択
                if (selectedSensorKind == (int)OptionPartsID.Accelerometer)
                {   //  入力5の選択状態を加速度センサーにする
                    if (cbbSIn5.SelectedIndex != (int)OptionPartsID.Accelerometer)
                    {
                        cbbSIn5.SelectedIndex = selectedSensorKind;
                    }
                }
                else
                {   // 入力5が加速度センサー選択状態の場合
                    if (indexToId(cbbSIn5,cbbSIn5.SelectedIndex) == (int)OptionPartsID.Accelerometer)
                    {
                        cbbSIn5.SelectedIndex = (int)OptionPartsID.SensorTop;
                    }
                }
                conf.nSns6Kind = selectedSensorKind;
            }
            else if (target == cbbSIn7)
            {
                conf.nSns7Kind = selectedSensorKind;
            }
            else if (target == cbbSIn8)
            {
                conf.nSns8Kind = selectedSensorKind;
            }
        }
        #endregion

        #region 【イベント】 ボタン入力１のチェックボックスが変更されたときの処理
        private void cbBIn1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)             // ボタン入力1：有効
            {
                cbSIn1.Enabled = false;     // センサー入力1選択不可
            }
            else
            {
                cbSIn1.Enabled = true;      // センサー入力1選択可能
            }
            
            // チェックボックスの設定情報を更新
            conf.fBtn1Used = target.Checked;
        }
        #endregion

        #region 【イベント】 ボタン入力２のチェックボックスが変更されたときの処理
        private void cbBIn2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)             // ボタン入力2：有効
            {
                cbSIn2.Enabled = false;     // センサー入力2選択不可
            }
            else
            {
                cbSIn2.Enabled = true;      // センサー入力2選択可能
            }
            
            // チェックボックスの設定情報を更新
            conf.fBtn2Used = target.Checked;
        }
        #endregion

        #region 【イベント】 ボタン入力３のチェックボックスが変更されたときの処理
        private void cbBIn3_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)             // ボタン入力3：有効
            {
                cbSIn3.Enabled = false;     // センサー入力3選択不可
            }
            else
            {
                cbSIn3.Enabled = true;      // センサー入力3選択可能
            }
            
            // チェックボックスの設定情報を更新
            conf.fBtn3Used = target.Checked;
        }
        #endregion

        #region 【イベント】 ボタン入力４のチェックボックスが変更されたときの処理
        private void cbBIn4_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox target = (CheckBox)sender;
            // チェックボックスの設定から関連するサーボモータのチェックボックスを変更する
            if (target.Checked)             // ボタン入力4：有効
            {
                cbSIn4.Enabled = false;     // センサー入力4選択不可
            }
            else
            {
                cbSIn4.Enabled = true;      // センサー入力4選択可能
            }
            // チェックボックスの設定情報を更新
            
            conf.fBtn4Used = target.Checked;
        }
        #endregion

        #region 【イベント】 "チェックを全て外す"ボタンが押されたときの処理
        private void button1_Click(object sender, EventArgs e)
        {
            checkOff(this.Controls);
        }
        #endregion

        // ------------------------------------------------------------------------------------
        // 概要       : カラーLEDのコンボボックスに表示・非表示を設定する
        // Date       : 2013/08/06    0.01  kagayama   新規作成
        //            : 2013/08/08    0.01  kagayama   チェックボックスの状態に応じた処理を修正
        //                                             コンボの選択状態に応じた処理を追加
        // ------------------------------------------------------------------------------------
        private void CheckColorLEDComboBox()
        {
            int num;
            ComboBox[] cboxes = { cbbSIn1, cbbSIn2, cbbSIn3 ,cbbSIn4};
            string name = si.strOptionParts[(int)OptionPartsID.ColorLED];

            // 初期化処理
            foreach (ComboBox elm in cboxes)
            {
                num = elm.Items.IndexOf(name);
                if (num == -1) elm.Items.Insert(elm.Items.Count, name);
            }
            // チェックボックスの状態に応じた処理
            if (cbSIn1.Checked && cbSIn2.Checked && cbSIn3.Checked && !cbSIn4.Checked)
            {
                num = cbbSIn4.Items.IndexOf(name);
                if (num != -1) cbbSIn4.Items.RemoveAt(num);
                return;
            }
            else if (!cbSIn1.Checked && cbSIn2.Checked && cbSIn3.Checked && cbSIn4.Checked)
            {
                num = cbbSIn1.Items.IndexOf(name);
                if (num != -1) cbbSIn1.Items.RemoveAt(num);
                return;
            }
            else if (!cbSIn2.Checked || !cbSIn3.Checked)
            {
                foreach (ComboBox elm in cboxes)
                {
                    num = elm.Items.IndexOf(name);
                    if (num != -1) elm.Items.RemoveAt(num);
                }
                return;
            }
            else if (!cbSIn1.Checked || !cbSIn4.Checked)
            {
                foreach (ComboBox elm in cboxes)
                {
                    num = elm.Items.IndexOf(name);
                    if (num != -1) elm.Items.RemoveAt(num);
                }
                return;
            }
            else
            {
            }
            // コンボボックスの選択状態に応じた処理
            if (FCS123)
            {
                num = cbbSIn4.Items.IndexOf(name);
                if (num != -1) cbbSIn4.Items.RemoveAt(num);
            }
            else if (FCS234)
            {
                num = cbbSIn1.Items.IndexOf(name);
                if (num != -1) cbbSIn1.Items.RemoveAt(num);
            }
            else
            {
            }
        }

        // ---------------------------------------------------------------------
        // 概要       : 指定されたセンサーのコンボボックス内でのindexを返す
        // 引数       : ComboBox            cbx        コンボボックス
        //            : OptinoPartsID       sensIdx    センサー文字列
        // Date       : 2013/08/06    0.01  kagayama   新規作成
        // ---------------------------------------------------------------------
        private int getIndex(ComboBox cbx, OptionPartsID sensIdx)
        {
            for (int i = 0; i < cbx.Items.Count; i++)
            {
                // TODO: ひらがな対応。関数未使用なので保留。
                if (cbx.Items[i].ToString() == (si.strOptionParts[(int)sensIdx]))
                {
                    return i;
                }
            }
            // センサが見つからなかった場合
            return -1;
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

        // ---------------------------------------------------------------------
        // 概要       : フォームのテキストをひらがなに変換
        // Date       : 2014/08/04    0.973 kagayama    新規作成
        // ---------------------------------------------------------------------
        private void convToHiragana()
        {
            this.Text = "にゅうしゅつりょくせってい";
            this.btCheckOff.Text = "チェックをすべてはずす";

        }

        // ---------------------------------------------------------------------
        // 概要       : 指定されたアイテム(index)のパーツIDを取得
        // 引数       : ComboBox            cbx        コンボボックス
        //            : int                 index      コンボのインデックス
        // Date       : 2013/11/28    0.92  kagayama   新規作成
        //            : 2014/08/04    0.973 kagayama   ひらがな対応
        // ---------------------------------------------------------------------
        private int indexToId(ComboBox cbx, int index)
        {
            if (index < 0) return -1;

            for (int i = 0; i < si.strOptionParts.Length; i++)
            {
                if ((si.strOptionParts[i]) == cbx.Items[index].ToString())
                {
                    return i;
                }
            }
            // TODO: ひらがな対応[済]
            for (int i = 0; i < si.strOptionPartsHiragana.Length; i++)
            {
                if ((si.strOptionPartsHiragana[i]) == cbx.Items[index].ToString())
                {
                    return i;
                }
            }

            return -1;
        }

        // ---------------------------------------------------------------------
        // 概要       : 再帰的にチェックボックスを外す
        // 引数       : ControlCollection   controls   子要素の集合
        // Date       : 2014/09/26  0.983  kagayama    新規作成
        // ---------------------------------------------------------------------
        void checkOff(System.Windows.Forms.Control.ControlCollection controls)
        {
            foreach (Control elm in controls)
            {
                if (elm.HasChildren)
                {
                    checkOff(elm.Controls);
                }
                else
                {
                    if (elm.GetType() == typeof(CheckBox))
                    {
                        (elm as CheckBox).Checked = false;
                    }
                }
            }
        }

        private void Configure_Load(object sender, EventArgs e)
        {
            TopMost = true;
        }
    }
}
