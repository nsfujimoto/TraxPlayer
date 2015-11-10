﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTraxPlayer
{
    class Board
    {
        const int BMAX = 25;
        const int LIMITTIME = 1000000;
        const int TSUME_MAX_DEPTH = 7;
        int MAX_DEPTH = 4;

        const int BLANK = 0x00;
        const int RED = 1;
        const int WHITE = 2;
        const int RIGHT = 0x01;
        const int UPPER = 0x02;
        const int LEFT = 0x04;
        const int LOWER = 0x08;
        const int VERTICAL_W = (UPPER | LOWER);    // "+" 1010 0x0a
        const int HORIZONTAL_W = (RIGHT | LEFT);   // "+" 0101 0x05
        const int UPPER_LEFT_W = (UPPER | LEFT);  // "/" 0110 0x06
        const int LOWER_RIGHT_W = (RIGHT | LOWER); // "/" 1001 0x09
        const int UPPER_RIGHT_W = (RIGHT | UPPER); // "\" 0011 0x03
        const int LOWER_LEFT_W = (LEFT | LOWER);   // "\" 1100 0x0c
        const int VW = VERTICAL_W;   //定跡入力用
        const int HW = HORIZONTAL_W;  //定跡入力用
        const int ULW = UPPER_LEFT_W;  //定跡入力用
        const int LRW = LOWER_RIGHT_W; //定跡入力用
        const int URW = UPPER_RIGHT_W; //定跡入力用
        const int LLW = LOWER_LEFT_W;  //定跡入力用
        const int TB = VERTICAL_W;    //定跡入力用
        const int LR = HORIZONTAL_W;  //定跡入力用
        const int BL = LOWER_LEFT_W;   //定跡入力用
        const int BR = LOWER_RIGHT_W;  //定跡入力用
        const int TL = UPPER_LEFT_W;   //定跡入力用
        const int TR = UPPER_RIGHT_W;  //定跡入力用
        const int HASHWIDTH = 0xffffff;


        int[] TLIST = new int[6] { VW, HW, ULW, LRW, URW, LLW };

        int[,] board = new int[BMAX, BMAX];
        int[,,,] ForceTile = new int[LLW + 1, LLW + 1, LLW + 1, LLW + 1];
        int[,,,] PlaceableTile = new int[LLW+1, LLW + 1, LLW + 1, LLW + 1];
        ulong[,,] random_t = new ulong[BMAX, BMAX, LLW + 1];
        ulong hash;

        ulong[] HASH_TBL = new ulong[HASHWIDTH + 1];
        char[] WINLOSS = new char[HASHWIDTH + 1];
        uint hash_cnt;

        int x_min, x_max, y_min, y_max;
        double t1, t2;
        int max_depth;
        int ulimit;
        int use_jouseki = 1;
        int random_player;
        int riichi = 0;

        string[] color_s = new string[3] { "", "RED", "WHITE" };
        char[] mark = new char[LLW+1] { '\0', '\0', '\0', '\\', '\0', '+', '/', '\0', '\0', '/', '+', '\0', '\\' };
        string[] b_string = new string[LLW + 1] { " ", "", "", "\\", "", "+", "\x1b[31m/\x1b[0m", "", "", "/", "\x1b[31m+\x1b[0m", "", "\x1b[31m\\\x1b[0m" };

        int[,] RR = new int[LLW+1, 8]{ {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0},
                            {URW,ULW,LRW,LLW,LLW,LRW,ULW,URW}, {0,0,0,0,0,0,0,0},{ HW, HW, HW, HW, VW, VW, VW, VW},
                            {ULW,URW,LLW,LRW,ULW,URW,LLW,LRW}, {0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0},{LRW,LLW,URW,ULW,LRW,LLW,URW,ULW},
                            { VW, VW, VW, VW, HW, HW, HW, HW}, {0,0,0,0,0,0,0,0},{LLW,LRW,ULW,URW,URW,ULW,LRW,LLW}
                            };

        int[] JOUSEKI_CNT = new int[2] { 28, 30 };


        public Board()
        {
            init();
        }



        void init()
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

            x_min = y_min = BMAX/2 + 1;
            x_max = y_max = BMAX/2;

            for(int i = 0; i < BMAX; i++)
            {
                for(int j = 0; j < BMAX; j++)
                {
                    board[i, j] = 0;
                }
            }

        }

        int popcount(int x)
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
        int loop_trace(int x, int y, int color) {
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
        void initForceTile()
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

            if (board[x,y] != BLANK) return -1;
            if ((PlaceableTile[board[x + 1,y],board[x,y - 1],board[x - 1,y],board[x,y + 1]] & (1 << tile)) != 0)
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

        int force_place(int x, int y, int tile, int[] bb, ref int bb_cnt)
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



    }
}
