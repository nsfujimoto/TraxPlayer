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
        protected const int RIGHT = 0x01;
        protected const int UPPER = 0x02;
        protected const int LEFT = 0x04;
        protected const int LOWER = 0x08;
        protected const int VERTICAL_W = (UPPER | LOWER);    // "+" 1010 0x0a
        protected const int HORIZONTAL_W = (RIGHT | LEFT);   // "+" 0101 0x05
        protected const int UPPER_LEFT_W = (UPPER | LEFT);  // "/" 0110 0x06
        protected const int LOWER_RIGHT_W = (RIGHT | LOWER); // "/" 1001 0x09
        protected const int UPPER_RIGHT_W = (RIGHT | UPPER); // "\" 0011 0x03
        protected const int LOWER_LEFT_W = (LEFT | LOWER);   // "\" 1100 0x0c



        int selected_tile = 0;
        Bitmap canvas;
        Graphics g;
        Image[] img = new Image[33];
        const int img_width = 30;
        const int img_height = 30;
        const int BMAX = 23;
        bool is_first = true;
        ViewBoard b = new ViewBoard();
        StrongPlayer b2 = new StrongPlayer();
        TumeAI b3 = new TumeAI();
        bool is_searching = false;
        int player_color;
        Random rnd = new Random();
        bool start_flag = false;
        int turn = 0;
        bool winflag = false;
        bool loseflag = false;
        string[] color_s = new string[3] {"", "RED   ","WHITE"};
        int[] convert_emphasis = new int[13];
        string[] tmp_s = new string[400];
        Image[,] btnimg = new Image[2, 6];

        public Form1()
        {
            InitializeComponent();
            initimg();
            initce();
            int seed = Environment.TickCount;
            ai1ToolStripMenuItem.Checked = true;
            b2.MAX_DEPTH = 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void initce()
        {
            convert_emphasis[VERTICAL_W] = 0;
            convert_emphasis[HORIZONTAL_W] = 1;
            convert_emphasis[UPPER_LEFT_W] = 2;
            convert_emphasis[LOWER_LEFT_W] = 4;
            convert_emphasis[UPPER_RIGHT_W] = 7;
            convert_emphasis[LOWER_RIGHT_W] = 8;
        }

        void initimg()
        {
            img[0x0a] = new Bitmap(GetType(), "vertical_w.png");
            img[0x05] = new Bitmap(GetType(), "horizontal_w.png");
            img[0x06] = new Bitmap(GetType(), "upper_left_w.png");
            img[0x09] = new Bitmap(GetType(), "lower_right_w.png");
            img[0x03] = new Bitmap(GetType(), "upper_right_w.png");
            img[0x0c] = new Bitmap(GetType(), "lower_left_w.png");
            img[0] = new Bitmap(GetType(), "bvertical_w.png");
            img[1] = new Bitmap(GetType(), "bhorizontal_w.png");
            img[2] = new Bitmap(GetType(), "bupper_left_w.png");
            img[4] = new Bitmap(GetType(), "blower_left_w.png");
            img[7] = new Bitmap(GetType(), "bupper_right_w.png");
            img[8] = new Bitmap(GetType(), "blower_right_w.png");


            btnimg[0, 0] = new Bitmap(GetType(), "VW.png");
            btnimg[0, 1] = new Bitmap(GetType(), "HW.png");
            btnimg[0, 2] = new Bitmap(GetType(), "BLW.png");
            btnimg[0, 3] = new Bitmap(GetType(), "BRW.png");
            btnimg[0, 4] = new Bitmap(GetType(), "TLW.png");
            btnimg[0, 5] = new Bitmap(GetType(), "TRW.png");

            btnimg[1, 0] = new Bitmap(GetType(), "bVW.png");
            btnimg[1, 1] = new Bitmap(GetType(), "bHW.png");
            btnimg[1, 2] = new Bitmap(GetType(), "bBLW.png");
            btnimg[1, 3] = new Bitmap(GetType(), "bBRW.png");
            btnimg[1, 4] = new Bitmap(GetType(), "bTLW.png");
            btnimg[1, 5] = new Bitmap(GetType(), "bTRW.png");




            canvas = new Bitmap(pictureBox7.Width, pictureBox7.Height);
            g = Graphics.FromImage(canvas);
            pictureBox7.Image = canvas;

            for (int i = 0; i < BMAX; i++)
            {
                g.DrawLine(Pens.Black, 0, i * img_height - 1, 30 * BMAX, i * img_height - 1);
                g.DrawLine(Pens.Black, i * img_width - 1, 0, i * img_width - 1, 30 * BMAX);
            }
        }


        private async void AIPlace()
        {
            int rx, ry, rt,ret;
            int[] bb = new int[1000];
            int bb_cnt = 0;
            rx = ry = rt = 0;
            label1.Text = "コンピュータ思考中...";

            await Task.Run(() =>
            {
                is_searching = true;
                if (ai3ToolStripMenuItem.Checked == true)
                {
                    b3.search_place(ref rx, ref ry, ref rt, b3.mycolor);
                    b2.place(rx, ry, rt, bb, ref bb_cnt);
                }
                else
                {
                    b2.search_place(ref rx, ref ry, ref rt, b2.mycolor);
                    b3.place(rx, ry, rt, bb, ref bb_cnt);
                }
            });
            ret = b.view_place(rx, ry, rt);
            richTextBox1.Text += color_s[3 - player_color] + " ： " + b.xxyyt_to_string(rx, ry, rt) + "\n";
            
            refresh_board();
            for (int i = 0; i < b.gh.hbb_cnt; i++) g.DrawImage(img[convert_emphasis[b.board[b.gh.hbb[i] >> 10, b.gh.hbb[i] & 0x3ff]]], 
                img_width * ((b.gh.hbb[i] >> 10) - 2), img_height * ((b.gh.hbb[i] & 0x3ff) - 2));
            //g.DrawImage(img[b.board[i, j]], img_width * (i - 2), img_height * (j - 2));
            pictureBox7.Refresh();

            is_searching = false;
            turn++;
            label1.Text = "あなたの番です";
            if (ret == 10)
            {
                winflag = true;
                richTextBox1.Text += "You Win!\n";
                MessageBox.Show("You Win!");
            }
            else if (ret == 11)
            {
                loseflag = true;
                richTextBox1.Text += "You Lose...\n";
                MessageBox.Show("You Lose...");
            }
        }

        private void undo() //2回呼ぶ必要あり(相手ターンと自分ターン)
        {
            history h;
            h = b.undo();
            b2.x_min = h.hx_min;
            b2.x_max = h.hx_max;
            b2.y_min = h.hy_min;
            b2.y_max = h.hy_max;
            b2.hash = h.hhash;
            for (int i = 0; i < h.hbb_cnt; i++) b2.board[h.hbb[i] >> 10, h.hbb[i] & 0x3ff] = 0;


            b3.x_min = h.hx_min;
            b3.x_max = h.hx_max;
            b3.y_min = h.hy_min;
            b3.y_max = h.hy_max;
            b3.hash = h.hhash;
            for (int i = 0; i < h.hbb_cnt; i++) b3.board[h.hbb[i] >> 10, h.hbb[i] & 0x3ff] = 0;

            int l = richTextBox1.Lines.Length;
            int ll = richTextBox1.Lines[l - 2].Length;
            richTextBox1.Text = richTextBox1.Text.Substring(0, richTextBox1.Text.Length - ll-1);
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
            int ret = 0;
            Point formClientCurPos = pictureBox7.PointToClient(Cursor.Position);
            formClientCurPos = new Point((int)formClientCurPos.X / img_width, (int)formClientCurPos.Y / img_height);
            //label1.Text = formClientCurPos.ToString();

            if (selected_tile != 0 && is_searching == false && start_flag == true)
            {
                int[] bb = new int[100];
                int bb_cnt = 0;
                if (is_first)
                {
                    int xx = formClientCurPos.X + 2;
                    int yy = formClientCurPos.Y + 2;
                    if (turn == 0 && (formClientCurPos.X != (int)BMAX/2 || formClientCurPos.Y != (int)BMAX/2)) return;
                    if (turn != 0 && (b.board[xx - 1, yy] | b.board[xx + 1, yy] |
                        b.board[xx, yy - 1] |  b.board[xx, yy + 1])  == 0) return;
                    if(b.view_first_place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile) == -1) return;
                    is_first = false;
                    b2.first_place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile);
                    b3.first_place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile);
                }
                else if ((ret = b.view_place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile)) == -1) return;
                //TODO 勝敗が決したなら終了処理へ
                
                refresh_board();
                if (!is_first)
                {
                    b2.place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile, bb, ref bb_cnt);
                    b3.place(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile, bb, ref bb_cnt);
                }
                //pictureBox7.Refresh();
                turn++;
                richTextBox1.Text += color_s[player_color] + " ： " + b.xxyyt_to_string(formClientCurPos.X + 2, formClientCurPos.Y + 2, selected_tile) + "\n";
                selected_tile = 0;
                pictureBox1.Image = btnimg[0, 0];
                pictureBox2.Image = btnimg[0, 1];
                pictureBox3.Image = btnimg[0, 2];
                pictureBox4.Image = btnimg[0, 3];
                pictureBox5.Image = btnimg[0, 4];
                pictureBox6.Image = btnimg[0, 5];
                /*
                pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
                pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
                */

                if (ret == 10)
                {
                    winflag = true;
                    richTextBox1.Text += "You Win!\n";
                    MessageBox.Show("You Win");
                }
                else if (ret == 11)
                {
                    loseflag = true;
                    richTextBox1.Text += "You Lose...\n";
                    MessageBox.Show("You Lose...");
                }
                else AIPlace();

            }
        }






        private void pictureBox1_Click(object sender, EventArgs e)
        {
            selected_tile = 0x0a;

            pictureBox1.Image = btnimg[1, 0];
            pictureBox2.Image = btnimg[0, 1];
            pictureBox3.Image = btnimg[0, 2];
            pictureBox4.Image = btnimg[0, 3];
            pictureBox5.Image = btnimg[0, 4];
            pictureBox6.Image = btnimg[0, 5];
            /*
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            */
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            selected_tile = 0x05;
            pictureBox1.Image = btnimg[0, 0];
            pictureBox2.Image = btnimg[1, 1];
            pictureBox3.Image = btnimg[0, 2];
            pictureBox4.Image = btnimg[0, 3];
            pictureBox5.Image = btnimg[0, 4];
            pictureBox6.Image = btnimg[0, 5];
            /*
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            */
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            selected_tile = 0x0c;
            pictureBox1.Image = btnimg[0, 0];
            pictureBox2.Image = btnimg[0, 1];
            pictureBox3.Image = btnimg[1, 2];
            pictureBox4.Image = btnimg[0, 3];
            pictureBox5.Image = btnimg[0, 4];
            pictureBox6.Image = btnimg[0, 5];
            /*
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            */
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            selected_tile = 0x09;
            pictureBox1.Image = btnimg[0, 0];
            pictureBox2.Image = btnimg[0, 1];
            pictureBox3.Image = btnimg[0, 2];
            pictureBox4.Image = btnimg[1, 3];
            pictureBox5.Image = btnimg[0, 4];
            pictureBox6.Image = btnimg[0, 5];
            /*
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            */
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            selected_tile = 0x06;
            pictureBox1.Image = btnimg[0, 0];
            pictureBox2.Image = btnimg[0, 1];
            pictureBox3.Image = btnimg[0, 2];
            pictureBox4.Image = btnimg[0, 3];
            pictureBox5.Image = btnimg[1, 4];
            pictureBox6.Image = btnimg[0, 5];
            /*
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            */
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            selected_tile = 0x03;
            pictureBox1.Image = btnimg[0, 0];
            pictureBox2.Image = btnimg[0, 1];
            pictureBox3.Image = btnimg[0, 2];
            pictureBox4.Image = btnimg[0, 3];
            pictureBox5.Image = btnimg[0, 4];
            pictureBox6.Image = btnimg[1, 5];
            /*
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            */
        }


        bool button1_tasking = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (start_flag == false)
            {
                player_color = rnd.Next(1, 3);
                b.mycolor = player_color;
                b2.mycolor = 3 - player_color;
                b3.mycolor = 3 - player_color;
                //b2.MAX_DEPTH = 2;
                //ai1ToolStripMenuItem.Checked = true;
                //ai2ToolStripMenuItem.Checked = false;
                //ai3ToolStripMenuItem.Checked = false;

                if (player_color == 2)
                {
                    label1.Text = "あなたは \"白\" です。";
                    richTextBox1.Text = "Player：白,  CPU：赤  \n";
                    g.FillRectangle(Brushes.YellowGreen, img_width * (BMAX/2), img_height * (BMAX/2), 29, 29);
                    pictureBox7.Refresh();
                }
                else
                {
                    turn++;
                    int[] bb = new int[100];
                    int bb_cnt = 0;
                    if ((rnd.Next() & 1) == 0)
                    {
                        b.view_place(BMAX / 2 + 2, BMAX / 2 + 2, 0x0a);
                        b.PlaceableTile[0,0,0,0] = 0;
                        b2.place(BMAX / 2 + 2, BMAX / 2 + 2, 0x0a, bb, ref bb_cnt);
                        b3.place(BMAX / 2 + 2, BMAX / 2 + 2, 0x0a, bb, ref bb_cnt);
                    }
                    else
                    {
                        b.view_place(BMAX / 2 + 2, BMAX / 2 + 2, 0x06);
                        b2.place(BMAX / 2 + 2, BMAX / 2 + 2, 0x06, bb, ref bb_cnt);
                        b3.place(BMAX / 2 + 2, BMAX / 2 + 2, 0x06, bb, ref bb_cnt);
                    }
                    refresh_board();
                    label1.Text = "あなたは \"赤\" です。";
                    richTextBox1.Text = "Player：赤,  CPU：白   \n";
                }
                start_flag = true;
                button1.Text = "終了";
            }
            else
            {
                turn = 0;
                button1.Text = "開始";
                start_flag = false;
                b2 = new StrongPlayer();
                if (ai1ToolStripMenuItem.Checked == true) b2.MAX_DEPTH = 1;
                else if (ai2ToolStripMenuItem.Checked == true) b2.MAX_DEPTH = 4;
                else if (ai5ToolStripMenuItem.Checked == true) b2.MAX_DEPTH = 2;


                b3 = new TumeAI();
                b = new ViewBoard();
                is_first = true;
                g.FillRectangle(Brushes.WhiteSmoke, 0, 0, pictureBox7.Height, pictureBox7.Width);
                for (int i = 0; i < BMAX; i++)
                {
                    g.DrawLine(Pens.Black, 0, i * img_height - 1, 30 * BMAX, i * img_height - 1);
                    g.DrawLine(Pens.Black, i * img_width - 1, 0, i * img_width - 1, 30 * BMAX);
                }
                pictureBox7.Refresh();
                richTextBox1.Clear();
                refresh_board();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (turn <= 2) return;
            undo();
            undo();
            g.FillRectangle(Brushes.WhiteSmoke, 0, 0, pictureBox7.Height, pictureBox7.Width);
            for (int i = 0; i < BMAX; i++)
            {
                g.DrawLine(Pens.Black, 0, i * img_height - 1, 30 * BMAX, i * img_height - 1);
                g.DrawLine(Pens.Black, i * img_width - 1, 0, i * img_width - 1, 30 * BMAX);
            }
            refresh_board();
            turn -= 2;
        }

        private void 終了ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 弱いToolStripMenuItem_Click(object sender, EventArgs e)
        {
            b2.MAX_DEPTH = 1;
            ai1ToolStripMenuItem.Checked = true;
            ai2ToolStripMenuItem.Checked = false;
            ai3ToolStripMenuItem.Checked = false;
            ai5ToolStripMenuItem.Checked = false;
        }

        private void 強いToolStripMenuItem_Click(object sender, EventArgs e)
        {
            b2.MAX_DEPTH = 4;
            ai1ToolStripMenuItem.Checked = false;
            ai2ToolStripMenuItem.Checked = true;
            ai3ToolStripMenuItem.Checked = false;
            ai5ToolStripMenuItem.Checked = false;
        }

        private void ai3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ai1ToolStripMenuItem.Checked = false;
            ai2ToolStripMenuItem.Checked = false;
            ai3ToolStripMenuItem.Checked = true;
            ai5ToolStripMenuItem.Checked = false;
        }

        private void ai5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            b2.MAX_DEPTH = 2;
            ai1ToolStripMenuItem.Checked = false;
            ai2ToolStripMenuItem.Checked = false;
            ai3ToolStripMenuItem.Checked = false;
            ai5ToolStripMenuItem.Checked = true;
        }
    }

    /*
    public Image CreateColorCorrectedImage(Image img, int x, int y, float rScale, float gScale, float bScale)
    {
        Bitmap newImg = new Bitmap(img.Width, img.Height);
        Graphics g = Graphics.FromImage(newImg);

        System.Drawing.Imaging.ColorMatrix cm =
            new System.Drawing.Imaging.ColorMatrix(
                new float[][] {
                    new float[] { rScale, 0, 0, 0, 0},
                    new float[] { 0, gScale, 0, 0, 0},
                    new float[] { 0, 0, bScale, 0, 0},
                    new float[] { 0, 0, 0, 1, 0},
                    new float[] { 0, 0, 0, 0, 1}
                });

        System.Drawing.Imaging.ImageAttributes ia =
            new System.Drawing.Imaging.ImageAttributes();

        ia.SetColorMatrix(cm);

        g.DrawImage(img,
            new Rectangle(),
            0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

    }
    */



}
