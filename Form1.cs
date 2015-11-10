﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSTraxPlayer
{
    public partial class Form1 : Form
    {
        int selected_tile = 0;
        Bitmap canvas;
        Graphics g;
        Image[] img = new Image[33];
        const int img_width = 30;
        const int img_height = 30;
        const int BMAX = 25;
        bool is_first = true;
        Board b = new Board();

        public Form1()
        {
            InitializeComponent();
            initimg();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void initimg()
        {
            img[0x0a] = new Bitmap(GetType(), "vertical_w.png");
            img[0x05] = new Bitmap(GetType(), "horizontal_w.png");
            img[0x06] = new Bitmap(GetType(), "upper_left_w.png");
            img[0x09] = new Bitmap(GetType(), "lower_right_w.png");
            img[0x03] = new Bitmap(GetType(), "upper_right_w.png");
            img[0x0c] = new Bitmap(GetType(), "lower_left_w.png");
            canvas = new Bitmap(pictureBox7.Width, pictureBox7.Height);
            g = Graphics.FromImage(canvas);
            pictureBox7.Image = canvas;

            for (int i = 0; i < BMAX; i++)
            {
                g.DrawLine(Pens.Gray, 0, i * img_height-1, 30 * BMAX, i * img_height-1);
                g.DrawLine(Pens.Black, i * img_width-1, 0, i * img_width-1, 30 * BMAX);
            }
        }




        private void pictureBox7_Click(object sender, EventArgs e)
        {
            Point formClientCurPos = pictureBox7.PointToClient(Cursor.Position);
            formClientCurPos = new Point((int)formClientCurPos.X / img_width, (int)formClientCurPos.Y / img_height);
            label1.Text = formClientCurPos.ToString();

            if(selected_tile != 0)
            {
                int[] bb = new int[100];
                int bb_cnt = 0;
                if (is_first)
                {
                    b.first_place(formClientCurPos.X, formClientCurPos.Y, selected_tile);
                    is_first = false;
                }
                else if (b.place(formClientCurPos.X, formClientCurPos.Y, selected_tile, bb, ref bb_cnt) == -1) return;
                g.DrawImage(img[selected_tile], img_width * formClientCurPos.X, img_height * formClientCurPos.Y);
                pictureBox7.Refresh();
                selected_tile = 0;
                pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            }


        }






        private void pictureBox1_Click(object sender, EventArgs e)
        {
            selected_tile = 0x0a;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            selected_tile = 0x05;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            selected_tile = 0x0c;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            selected_tile = 0x09;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            selected_tile = 0x06;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            selected_tile = 0x03;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        }
    }
}
