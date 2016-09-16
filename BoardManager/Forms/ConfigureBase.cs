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
    public partial class ConfigureBase : Form
    {
        protected SensorItems si;
        protected stRobotIOStatus ioStatus;

        public ConfigureBase()
        {
            InitializeComponent();
        }

        public ConfigureBase(bool hiragana = false)
            : this()
        {
            if (hiragana) this.Load += new EventHandler(ConfigureBase_Load);
        }

        // ---------------------------------------------------------------------
        // 概要       : 再帰的にチェックボックスを外す
        // 引数       : ControlCollection   controls   子要素の集合
        // Date       : 2014/09/26  0.983  kagayama    新規作成
        // ---------------------------------------------------------------------
        protected void checkOff(System.Windows.Forms.Control.ControlCollection controls)
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

        // ---------------------------------------------------------------------
        // 概要       : 指定されたアイテム(index)のパーツIDを取得
        // 引数       : ComboBox            cbx        コンボボックス
        //            : int                 index      コンボのインデックス
        // Date       : 2013/11/28    0.92  kagayama   新規作成
        //            : 2014/08/04    0.973 kagayama   ひらがな対応
        // ---------------------------------------------------------------------
        protected int indexToId(ComboBox cbx, int index)
        {
            if (index < 0) return -1;

            for (int i = 0; i < si.strOptionParts.Length; i++)
            {
                if ((si.strOptionParts[i]) == cbx.Items[index].ToString())
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 指定されたパーツの、指定されたコンボボックス内のindexを返す
        /// </summary>
        /// <param name="cbx">パーツ選択コンボボックス</param>
        /// <param name="sensIdx">パーツID</param>
        /// <returns>コンボボックスのインデックス</returns>
        protected int getIndex(ComboBox cbx, OptionPartsID sensIdx)
        {
            for (int i = 0; i < cbx.Items.Count; i++)
            {
                if (cbx.Items[i].ToString() == (si.strOptionParts[(int)sensIdx]))
                {
                    return i;
                }
            }
            return -1;
        }

        protected virtual void updateIOStatus()
        {
        }

        /// <summary>
        /// ひらがな変換処理。
        /// </summary>
        protected virtual void convertToHiragana()
        {
            this.Text = "にゅうしゅつりょくせってい";
            this.btCheckOff.Text = "チェックをすべてはずす";
        }

        private void btCheckOff_Click(object sender, EventArgs e)
        {
            checkOff(this.Controls);
        }

        private void ConfigureBase_Load(object sender, EventArgs e)
        {
            convertToHiragana();
        }

        private void ConfigureBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("Form closing");
            if (this.DialogResult == DialogResult.OK)
                updateIOStatus();
        }
    }
}
