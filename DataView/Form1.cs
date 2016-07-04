using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataView {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            pictureBox.Paint += pictureBox_Paint;
        }

        void pictureBox_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            Pen pen = new Pen(new SolidBrush(Color.Lime));
            for (int i = 0; i < 13; i++) {
                for (int j = 0; j < 13; j++) {
                    g.DrawRectangle(pen, i * 30 + 1, j * 30 + 1, 28, 28);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
    }
}
