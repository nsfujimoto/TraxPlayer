using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTraxPlayer
{
    class Board
    {
        protected const int BMAX = 27;
        protected const int LIMITTIME = 1000000;
        protected const int TSUME_MAX_DEPTH = 7;
        public int MAX_DEPTH = 1;

        protected const int BLANK = 0x00;
        protected const int RED = 1;
        protected const int WHITE = 2;
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
        protected const int VW = VERTICAL_W;   //定跡入力用
        protected const int HW = HORIZONTAL_W;  //定跡入力用
        protected const int ULW = UPPER_LEFT_W;  //定跡入力用
        protected const int LRW = LOWER_RIGHT_W; //定跡入力用
        protected const int URW = UPPER_RIGHT_W; //定跡入力用
        protected const int LLW = LOWER_LEFT_W;  //定跡入力用
        protected const int TB = VERTICAL_W;    //定跡入力用
        protected const int LR = HORIZONTAL_W;  //定跡入力用
        protected const int BL = LOWER_LEFT_W;   //定跡入力用
        protected const int BR = LOWER_RIGHT_W;  //定跡入力用
        protected const int TL = UPPER_LEFT_W;   //定跡入力用
        protected const int TR = UPPER_RIGHT_W;  //定跡入力用
        protected const int HASHWIDTH = 0xffffff;


        protected int[] TLIST = new int[6] { VW, HW, ULW, LRW, URW, LLW };

        public int[,] board = new int[BMAX, BMAX];
        protected int[,,,] ForceTile = new int[LLW + 1, LLW + 1, LLW + 1, LLW + 1];
        public int[,,,] PlaceableTile = new int[LLW+1, LLW + 1, LLW + 1, LLW + 1];
        protected ulong[,,] random_t = new ulong[BMAX, BMAX, LLW + 1];
        public ulong hash;

        protected ulong[] HASH_TBL = new ulong[HASHWIDTH + 1];
        protected char[] WINLOSS = new char[HASHWIDTH + 1];
        protected uint hash_cnt;

        public int x_min, x_max, y_min, y_max;
        public int last_x_min = BMAX/2, last_y_min = BMAX/2;
        double t1, t2;
        public int max_depth;
        protected int ulimit;
        protected int use_jouseki = 1;
        protected int random_player;
        public int mycolor;

        public System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        protected string[] color_s = new string[3] { "", "RED", "WHITE" };
        protected char[] mark = new char[LLW+1] { '\0', '\0', '\0', '\\', '\0', '+', '/', '\0', '\0', '/', '+', '\0', '\\' };
        protected string[] b_string = new string[LLW + 1] { " ", "", "", "\\", "", "+", "\x1b[31m/\x1b[0m", "", "", "/", "\x1b[31m+\x1b[0m", "", "\x1b[31m\\\x1b[0m" };

        protected int[,] RR = new int[LLW+1, 8]{ {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0},
                            {URW,ULW,LRW,LLW,LLW,LRW,ULW,URW}, {0,0,0,0,0,0,0,0},{ HW, HW, HW, HW, VW, VW, VW, VW},
                            {ULW,URW,LLW,LRW,ULW,URW,LLW,LRW}, {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0},{LRW,LLW,URW,ULW,LRW,LLW,URW,ULW},
                            { VW, VW, VW, VW, HW, HW, HW, HW}, {0,0,0,0,0,0,0,0},{LLW,LRW,ULW,URW,URW,ULW,LRW,LLW}
                            };

        protected int[] JOUSEKI_CNT = new int[2] { 28, 30 };

        Random rr = new Random();


        public Board()
        {
            init();
        }



        protected void init()
        {
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    for (int k = 0; k < 13; k++)
                    {
                        for (int l = 0; l < 13; l++)
                        {
                            ForceTile[i,j,k,l] = 0;
                            PlaceableTile[i,j,k,l] = 0;
                        }
                    }
                }
            }

            initForceTile();

            x_min = y_min = last_x_min = last_y_min = BMAX/2 + 1;
            x_max = y_max = BMAX/2;

            for(int i = 0; i < BMAX; i++)
            {
                for(int j = 0; j < BMAX; j++)
                {
                    board[i, j] = 0;
                }
            }
        }

        protected int popcount(int x)
        {
            int ret = 0;
            while(x != 0)
            {
                x &= x - 1;
                ++ret;
            }
            return ret;
        }

        public void first_place(int x, int y, int tile)
        {
            int[] bb = new int[100];
            int bb_cnt = 0;
            PlaceableTile[0, 0, 0, 0] = 0xffffff;
            place(x, y, tile, bb,ref bb_cnt);
            PlaceableTile[0, 0, 0, 0] = 0;
        }
        protected int loop_trace(int x, int y, int color, ref int riichi) {
            int vec1, vec2, vectmp = 0;
            int is_line_edge = 0;
            int value;
            int xx, yy;
            int xx2, yy2;
            int tile = board[x,y];
            int vect;
            int tmp;
            riichi = 0;

            if (color == WHITE) vect = tile;
            else vect = ~tile & 0x0f;

            xx = xx2 = x;
            yy = yy2 = y;

            //双方向に伸びるか、片方向に伸びるかのチェック
            if (board[x + 1,y] != 0) vectmp |= RIGHT;
            if (board[x - 1,y] != 0) vectmp |= LEFT;
            if (board[x,y + 1] != 0) vectmp |= LOWER;
            if (board[x,y - 1] != 0) vectmp |= UPPER;
            value = vect & vectmp;
            if (value == 0)
            {
                return 0;
            }
            else if ((value & (value - 1)) != 0)
            { //両方向
                if ((vect & UPPER) != 0) vec1 = UPPER;
                else if ((vect & LEFT) != 0) vec1 = LEFT;
                else if ((vect & RIGHT) != 0) vec1 = RIGHT;
                else vec1 = LOWER;
                vec2 = vect ^ vec1;
            }
            else
            { //片方向
                vec1 = value;
                vec2 = vect ^ vec1;
                is_line_edge = 1;
            }

            if (color == WHITE)
            {
                while (tile != 0)
                {
                    if ((vec1 & UPPER) != 0)
                    {
                        yy--;
                        tile = board[xx,yy];
                        vec1 = LOWER ^ tile;
                    }
                    else if ((vec1 & LEFT) != 0)
                    {
                        xx--;
                        tile = board[xx,yy];
                        vec1 = RIGHT ^ tile;
                    }
                    else if ((vec1 & LOWER) != 0)
                    {
                        yy++;
                        tile = board[xx,yy];
                        vec1 = UPPER ^ tile;
                    }
                    else if ((vec1 & RIGHT) != 0)
                    {
                        xx++;
                        tile = board[xx,yy];
                        vec1 = LEFT ^ tile;
                    }
                    if (xx == x && yy == y)
                    {
                        return 1;
                    }
                }
            }
            else
            {
                while (tile != 0)
                {
                    if ((vec1 & UPPER) != 0)
                    {
                        yy--;
                        tile = board[xx,yy];
                        vec1 = LOWER ^ ~tile;
                    }
                    else if ((vec1 & LEFT) != 0)
                    {
                        xx--;
                        tile = board[xx,yy];
                        vec1 = RIGHT ^ ~tile;
                    }
                    else if ((vec1 & LOWER) != 0)
                    {
                        yy++;
                        tile = board[xx,yy];
                        vec1 = UPPER ^ ~tile;
                    }
                    else if ((vec1 & RIGHT) != 0)
                    {
                        xx++;
                        tile = board[xx,yy];
                        vec1 = LEFT ^ ~tile;
                    }
                    if (xx == x && yy == y) return 1;
                }
            }

            if (is_line_edge != 0)
            {
                if ((vec2 & UPPER) != 0) yy2--;
                if ((vec2 & LEFT) != 0) xx2--;
                if ((vec2 & LOWER) != 0) yy2++;
                if ((vec2 & RIGHT) != 0) xx2++;

                if (xx == xx2 && Math.Abs(yy - yy2) < 3)
                {
                    if (Math.Abs(yy - yy2) == 1)
                    {
                        riichi = 1;
                    }
                    else
                    {
                        if (color == WHITE)
                        {
                            tmp = (yy + yy2) >> 1;
                            if (board[xx,tmp] == BLANK)
                                if ((PlaceableTile[board[xx + 1,tmp],board[xx,tmp - 1],board[xx - 1,tmp],board[xx,tmp + 1]] & (1 << VERTICAL_W)) != 0)riichi = 1;
                        }
                        else
                        {
                            tmp = (yy + yy2) >> 1;
                            if (board[xx,tmp] == BLANK)
                                if ((PlaceableTile[board[xx + 1,tmp],board[xx,tmp - 1],board[xx - 1,tmp],board[xx,tmp + 1]] & (1 << HORIZONTAL_W)) != 0)riichi = 1;
                        }
                    }
                }
                else if (yy == yy2 && Math.Abs(yy - yy2) < 3)
                {
                    if (Math.Abs(xx - xx2) == 1)
                    {
                        riichi = 1;
                    }
                    else
                    {
                        if (color == WHITE)
                        {
                            tmp = (xx + xx2) >> 1;
                            if (board[tmp,yy] == BLANK)
                                if ((PlaceableTile[board[tmp + 1,yy],board[tmp,yy - 1],board[tmp - 1,yy],board[tmp,yy + 1]] & (1 << HORIZONTAL_W)) != 0)riichi = 1;
                        }
                        else
                        {
                            tmp = (xx + xx2) >> 1;
                            if (board[tmp,yy] == BLANK)
                                if ((PlaceableTile[board[tmp + 1,yy],board[tmp,yy - 1],board[tmp - 1,yy],board[tmp,yy + 1]] & (1 << VERTICAL_W)) != 0)riichi = 1;
                        }
                    }
                }
            }

            if (color == RED) vec1 = ~vec1;
            if (is_line_edge != 0)
            {
                //片側だけつながったラインのエッジ判断
                if (x_max - x_min >= 7)
                {
                    if (xx == x_max + 1 && (vec1 & LEFT) != 0 && x == x_min && (vec2 & LEFT) != 0) return 1;
                    if (xx == x_min - 1 && (vec1 & RIGHT) != 0 && x == x_max && (vec2 & RIGHT) != 0) return 1;
                }
                if (y_max - y_min >= 7)
                {
                    if (yy == y_max + 1 && (vec1 & UPPER) != 0 && y == y_min && (vec2 & UPPER) != 0) return 1;
                    if (yy == y_min - 1 && (vec1 & LOWER) != 0 && y == y_max && (vec2 & LOWER) != 0) return 1;
                }
                //あとライン勝利１歩手前かもしれなければ深さ１で探索を行う
                return 0;
            }
            else
            {
                //もう片方のもたどる
                xx2 = xx;
                yy2 = yy;
                xx = x;
                yy = y;
                tile = board[x,y];

                if (color == WHITE)
                {
                    while (tile != 0)
                    {
                        if ((vec2 & UPPER) != 0)
                        {
                            yy--;
                            tile = board[xx,yy];
                            vec2 = LOWER ^ tile;
                        }
                        else if ((vec2 & LEFT) != 0)
                        {
                            xx--;
                            tile = board[xx,yy];
                            vec2 = RIGHT ^ tile;
                        }
                        else if ((vec2 & LOWER) != 0)
                        {
                            yy++;
                            tile = board[xx,yy];
                            vec2 = UPPER ^ tile;
                        }
                        else if ((vec2 & RIGHT) != 0)
                        {
                            xx++;
                            tile = board[xx,yy];
                            vec2 = LEFT ^ tile;
                        }
                    }
                }
                else
                {
                    while (tile != 0)
                    {
                        if ((vec2 & UPPER) != 0)
                        {
                            yy--;
                            tile = board[xx,yy];
                            vec2 = LOWER ^ ~tile;
                        }
                        else if ((vec2 & LEFT) != 0)
                        {
                            xx--;
                            tile = board[xx,yy];
                            vec2 = RIGHT ^ ~tile;
                        }
                        else if ((vec2 & LOWER) != 0)
                        {
                            yy++;
                            tile = board[xx,yy];
                            vec2 = UPPER ^ ~tile;
                        }
                        else if ((vec2 & RIGHT) != 0)
                        {
                            xx++;
                            tile = board[xx,yy];
                            vec2 = LEFT ^ ~tile;
                        }
                    }
                }
                if (xx == xx2 && Math.Abs(yy - yy2) < 3)
                {
                    if (Math.Abs(yy - yy2) == 1)
                    {
                        riichi = 1;
                    }
                    else
                    {
                        if (color == WHITE)
                        {
                            tmp = (yy + yy2) >> 1;
                            if (board[xx,tmp] == BLANK)
                                if ((PlaceableTile[board[xx + 1,tmp],board[xx,tmp - 1],board[xx - 1,tmp],board[xx,tmp + 1]] & (1 << VERTICAL_W)) != 0)riichi = 1;
                        }
                        else
                        {
                            tmp = (yy + yy2) >> 1;
                            if (board[xx,tmp] == BLANK)
                                if ((PlaceableTile[board[xx + 1,tmp],board[xx,tmp - 1],board[xx - 1,tmp],board[xx,tmp + 1]] & (1 << HORIZONTAL_W)) != 0)riichi = 1;
                        }
                    }
                }
                else if (yy == yy2 && Math.Abs(yy - yy2) < 3)
                {
                    if (Math.Abs(xx - xx2) == 1)
                    {
                        riichi = 1;
                    }
                    else
                    {
                        if (color == WHITE)
                        {
                            tmp = (xx + xx2) >> 1;
                            if (board[tmp,yy] == BLANK)
                                if ((PlaceableTile[board[tmp + 1,yy],board[tmp,yy - 1],board[tmp - 1,yy],board[tmp,yy + 1]] & (1 << HORIZONTAL_W)) != 0)riichi = 1;
                        }
                        else
                        {
                            tmp = (xx + xx2) >> 1;
                            if (board[tmp,yy] == BLANK)
                                if ((PlaceableTile[board[tmp + 1,yy],board[tmp,yy - 1],board[tmp - 1,yy],board[tmp,yy + 1]] & (1 << VERTICAL_W)) != 0)riichi = 1;
                        }
                    }
                }
                if (color == RED) vec2 = ~vec2;
                if (x_max - x_min >= 7)
                {
                    if (xx2 == x_max + 1 && xx == x_min - 1 && (vec1 & LEFT) != 0 && (vec2 & RIGHT) != 0) return 1;
                    if (xx2 == x_min - 1 && xx == x_max + 1 && (vec1 & RIGHT) != 0 && (vec2 & LEFT) != 0) return 1;
                }
                if (y_max - y_min >= 7)
                {
                    if (yy2 == y_max + 1 && yy == y_min - 1 && (vec1 & UPPER) != 0 && (vec2 & LOWER) != 0) return 1;
                    if (yy2 == y_min - 1 && yy == y_max + 1 && (vec1 & LOWER) != 0 && (vec2 & UPPER) != 0) return 1;
                }
                //ライン勝利１歩手前かもしれなければ深さ１で探索を行う
            }
            return 0;
        }
        protected void initForceTile()
        {
            int i, j, k, l, m;
            int[] t = new int[7]{ 0x00, 0x03, 0x05, 0x06, 0x09, 0x0a, 0x0c };

            for (i = 0; i < 7; i++)
            {
                for (j = 0; j < 7; j++)
                {
                    for (k = 0; k < 7; k++)
                    {
                        for (l = 0; l < 7; l++)
                        {
                            int wt = (Convert.ToInt32(((t[l] & UPPER) != 0)) << 3) | (Convert.ToInt32(((t[k] & RIGHT) != 0)) << 2) 
                                | ((Convert.ToInt32(((t[j] & LOWER) != 0)) << 1) | Convert.ToInt32(((t[i] & LEFT) != 0)));
                            int rt;
                            rt = (t[l] == 0 ? 0 : Convert.ToInt32(((~t[l] & UPPER) != 0)) << 3);
                            rt |= (t[k] == 0 ? 0 : Convert.ToInt32(((~t[k] & RIGHT) != 0)) << 2);
                            rt |= (t[j] == 0 ? 0 : Convert.ToInt32(((~t[j] & LOWER) != 0)) << 1);
                            rt |= (t[i] == 0 ? 0 : Convert.ToInt32((~t[i] & LEFT) != 0));
                            rt &= 0x0f;
                            if (popcount(wt) >= 3 || popcount(rt) >= 3)
                            {
                                ForceTile[t[i],t[j],t[k],t[l]] = -1;
                                //  printf("%d %d %d %d  =  %d\n", t[i], t[j], t[k], t[l], ForceTile[t[i],t[j],t[k],t[l]]);//確認用
                            }
                            else if (popcount(wt) == 2)
                            {
                                ForceTile[t[i],t[j],t[k],t[l]] = wt;
                                //  printf("%d %d %d %d  =  %d\n", t[i], t[j], t[k], t[l], ForceTile[t[i],t[j],t[k],t[l]]);//確認用
                            }
                            else if (popcount(rt) == 2)
                            {
                                ForceTile[t[i],t[j],t[k],t[l]] = ~rt & 0x0f;
                                //printf("%d %d %d %d  =  %d\n", t[i], t[j], t[k], t[l], ForceTile[t[i],t[j],t[k],t[l]]); // 確認用
                            }
                        }
                    }
                }
            }
            for (i = 0; i < 7; i++)
            {
                for (j = 0; j < 7; j++)
                {
                    for (k = 0; k < 7; k++)
                    {
                        for (l = 0; l < 7; l++)
                        {
                            for (m = 1; m < 7; m++)
                            {
                                int ok = 1;
                                if (t[i] != 0 && (t[i] & LEFT) != 0 && ((t[m] & RIGHT) == 0)) ok = 0;
                                if (t[i] != 0 && (~t[i] & LEFT) != 0 && ((~t[m] & RIGHT) == 0)) ok = 0;
                                if (t[j] != 0 && (t[j] & LOWER) != 0 && ((t[m] & UPPER) == 0)) ok = 0;
                                if (t[j] != 0 && (~t[j] & LOWER) != 0 && ((~t[m] & UPPER) == 0)) ok = 0;
                                if (t[k] != 0 && (t[k] & RIGHT) != 0 && ((t[m] & LEFT) == 0)) ok = 0;
                                if (t[k] != 0 && (~t[k] & RIGHT) != 0 && ((~t[m] & LEFT) == 0)) ok = 0;
                                if (t[l] != 0 && (t[l] & UPPER) != 0 && ((t[m] & LOWER) == 0)) ok = 0;
                                if (t[l] != 0 && (~t[l] & UPPER) != 0 && ((~t[m] & LOWER) == 0)) ok = 0;
                                PlaceableTile[t[i],t[j],t[k],t[l]] |= ok << t[m];
                                // printf("%d %d %d %d %d = %d\n", t[i], t[j], t[k], t[l], t[m], ok); //確認用
                            }
                        }
                    }
                }
            }
        }

        public int place(int x, int y, int tile, int[] bb, ref int bb_cnt)
        {
            int i;
            ulong hash_orig = hash;
            last_x_min = x_min;
            last_y_min = y_min;

            if (board[x,y] != BLANK) return -1;
            
            if ((PlaceableTile[board[x + 1,y],board[x,Math.Abs(y - 1)],board[Math.Abs(x - 1),y],board[x,y + 1]] & (1 << tile)) != 0)
            {
                bb_cnt = 0;
                if (force_place(x, y, tile, bb, ref bb_cnt) == -1)
                { // 3 Same Color
                    for (i = 0; i < bb_cnt; i++) board[bb[i] >> 10,bb[i] & 0x3ff] = 0;
                    bb_cnt = 0;
                    hash = hash_orig;
                    return -1; // 3 Same Color
                }
                if (x < x_min) x_min = x;
                else if (x > x_max) x_max = x;
                if (y < y_min) y_min = y;
                else if (y > y_max) y_max = y;
                return 1;
            }
            return -1;
        }

        protected int force_place(int x, int y, int tile, int[] bb, ref int bb_cnt)
        {
            int t;

            board[x,y] = tile;           //配置できる場所ならタイルを配置
            hash ^= random_t[x,y,tile];   //ハッシュ値更新
            bb[(bb_cnt)++] = (x << 10) | y; //配置した場所を記録

            if (board[x + 1,y] == BLANK)
            { // 右強制手処理
                t = ForceTile[board[x + 2,y],board[x + 1,y - 1],board[x,y],board[x + 1,y + 1]];
                if (t > 0) t = force_place(x + 1, y, t, bb, ref bb_cnt);
                if (t == -1) return -1; // 3 Same Color
            }
            if (board[x,y - 1] == BLANK)
            { // 上強制手処理
                t = ForceTile[board[x + 1,y - 1],board[x,y - 2],board[x - 1,y - 1],board[x,y]];
                if (t > 0) t = force_place(x, y - 1, t, bb, ref bb_cnt);
                if (t == -1) return -1; // 3 Same Color
            }
            if (board[x - 1,y] == BLANK)
            { // 左強制手処理
                t = ForceTile[board[x,y],board[x - 1,y - 1],board[x - 2,y],board[x - 1,y + 1]];
                if (t > 0) t = force_place(x - 1, y, t, bb, ref bb_cnt);
                if (t == -1) return -1; // 3 Same Color
            }
            if (board[x,y + 1] == BLANK)
            { // 下強制手処理
                t = ForceTile[board[x + 1,y + 1],board[x,y],board[x - 1,y + 1],board[x,y + 2]];
                if (t > 0) t = force_place(x, y + 1, t, bb, ref bb_cnt);
                if (t == -1) return -1; // 3 Same Color
            }
            return 0;
        }

        public string xxyyt_to_string(int xx, int yy, int t)
        {
            string s;

            if (xx == 0)
            {
                s = "@";
            }
            else if (xx <= 26)
            {
                s = ((char)('A' + xx - last_x_min)).ToString();
            }
            else
            {
                s = ('A' + ((xx - last_x_min) / 26) - 1).ToString();
                s = ('A' + ((xx - last_x_min) % 26)).ToString();
            }
            s += (yy - last_y_min+1).ToString();
            if (t == VERTICAL_W || t == HORIZONTAL_W) s += "+";
            else if (t == UPPER_LEFT_W || t == LOWER_RIGHT_W) s += "/";
            else s += "\\";
            return s;
        }

        public void random_place(ref int xx, ref int yy, ref int tt, int color)
        {
            int i, j, x, y, t;
            int x_min_backup = x_min;
            int x_max_backup = x_max;
            int y_min_backup = y_min;
            int y_max_backup = y_max;
            ulong hash_backup = hash;
            int[] px = new int[10000];
            int[] py = new int[10000];
            int[] pt = new int[10000];
            int p_cnt = 0;
            int[] bb = new int[256];
            int bb_cnt = 0 , riichi = 0;

            for (y = y_min - 1; y <= y_max + 1; y++)
            {
                for (x = x_min - 1; x <= x_max + 1; x++)
                {
                    if (board[x,y] != 0) continue;
                    if ((board[x - 1,y] | board[x + 1,y] | board[x,y - 1] | board[x,y + 1]) != 0)
                    {
                        for (i = 0; i < 6; i++)
                        {
                            t = TLIST[i];
                            if (place(x, y, t, bb, ref bb_cnt) == 1)
                            {
                                if (loop_trace(x, y, 3 - color, ref riichi) == 0)
                                {
                                    px[p_cnt] = x;
                                    py[p_cnt] = y;
                                    pt[p_cnt++] = t;
                                }
                                for (j = 0; j < bb_cnt; j++) board[bb[j] >> 10,bb[j] & 0x3ff] = 0;
                                hash = hash_backup;
                                x_min = x_min_backup;
                                x_max = x_max_backup;
                                y_min = y_min_backup;
                                y_max = y_max_backup;
                            }
                        }
                    }
                }
            }
            int r = rr.Next() % p_cnt;
            xx = px[r];
            yy = py[r];
            tt = pt[r];
        }
    }
}
