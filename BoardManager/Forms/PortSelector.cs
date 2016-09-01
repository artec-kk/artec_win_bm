//
//    Copyright (C) 2013,2014  ArTec Co., LTD.
//
//    This file is part of ArtecIconProgramming.
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License along
//    with this program; if not, write to the Free Software Foundation, Inc.,
//    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScratchConnection
{
    public partial class PortSelector : Form
    {
        public PortSelector()
        {
            InitializeComponent();
        }

        public PortSelector(String[] list)
            : this()
        {
            foreach (String elm in list)
            {
                listBox1.Items.Add(elm);
            }
            listBox1.SetSelected(0, true);
        }

        public String getPort()
        {
            return (String)listBox1.SelectedItem;
        }
    }
}
