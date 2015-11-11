using System;
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
        StrongPlayer b2 = new StrongPlayer();
        bool is_searching = false;
        int player_color;
        Random rnd = new Random();
        bool start_flag = false;

        public Form1()
        {
            InitializeComponent();
            initimg();
            int seed = Environment.TickCount;
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
                g.DrawLine(Pens.Black, 0, i * img_height - 1, 30 * BMAX, i * img_height - 1);
                g.DrawLine(Pens.Black, i * img_width - 1, 0, i * img_width - 1, 30 * BMAX);
            }
        }


        private void AIPlace()
        {
            int rx, ry, rt;
            int[] bb = new int[1000];
            int bb_cnt = 0;
            rx = ry = rt = 0;


            Task.Run(() =>
            {
                label1.Text = "コンピュータ思考中...";
                is_searching = true;
                b2.search_place(ref rx, ref ry, ref rt, b2.mycolor);

                b.place(rx, ry, rt, bb, ref bb_cnt);
                refresh_board();
                is_searching = false;
                label1.Text = "あなたの番です";
            });
        }

        private void refresh_board()
        {
            for (int i = 2; i < BMAX + 2; i++)
            {
                for (int j = 2; j < BMAX + 2; j++)
                {
                    if (b.board[i, j] == 0) continue;
                    else
                    {
                        g.DrawImage(img[b.board[i, j]], img_width * (i - 2), img_height * (j - 2));
                    }
                }
            }
            pictureBox7.Refresh();
        }


        private void pictureBox7_Click(object sender, EventArgs e)
        {
            Point formClientCurPos = pictureBox7.PointToClient(Cursor.Position);
            formClientCurPos = new Point((int)formClientCurPos.X / img_width, (int)formClientCurPos.Y / img_height);
            //label1.Text = formClientCurPos.ToString();

            if (selected_tile != 0 && is_searching == false && start_flag == true)
            {
                int[] bb = new int[100];
                int bb_cnt = 0;
                if (is_first)
                {
                    //if (formClientCurPos.X != 13 || formClientCurPos.Y != 11) return;
                    b.first_place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile);
                    is_first = false;
                    b2.first_place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile);
                }
                else if (b.place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile, bb, ref bb_cnt) == -1) return;
                refresh_board();
                if (!is_first) b2.place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile, bb, ref bb_cnt);
                //pictureBox7.Refresh();
                selected_tile = 0;
                pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
                AIPlace();

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


        bool button1_tasking = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (start_flag == false)
            {
                player_color = rnd.Next(1, 3);
                b2.mycolor = 3 - player_color;
                if (player_color == 2)
                {
                    label1.Text = "あなたは \"白\" です。";
                    g.FillRectangle(Brushes.YellowGreen, img_width * 13, img_height * 11, 29, 29);
                    pictureBox7.Refresh();
                }
                else
                {
                    int[] bb = new int[100];
                    int bb_cnt = 0;
                    if ((rnd.Next() & 1) == 0)
                    {
                        b.place(15, 13, 0x0a, bb, ref bb_cnt);
                        b2.place(15, 13, 0x0a, bb, ref bb_cnt);
                    }
                    else
                    {
                        b.place(15, 13, 0x06, bb, ref bb_cnt);
                        b2.place(15, 13, 0x06, bb, ref bb_cnt);
                    }
                    refresh_board();
                    label1.Text = "あなたは \"赤\" です。";
                }
                start_flag = true;
                button1.Text = "終了";
            }
            else
            {
                button1.Text = "開始";
                start_flag = false;
                b2 = new StrongPlayer();
                b = new Board();
                is_first = true;

                refresh_board();
            }
        }
    }
}
