using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScratchConnection.Forms
{
    public partial class ServoAngle : UserControl
    {
        public string portName
        {
            get
            {
                return lbPort.Text;
            }
            set
            {
                lbPort.Text = value;
            }
        }

        public ServoAngle()
        {
            InitializeComponent();
        }

        public void convertToHiragana()
        {
            lbDegree.Text = "ど";
        }
    }
}
