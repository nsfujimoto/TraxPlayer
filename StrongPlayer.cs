using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTraxPlayer
{
    class StrongPlayer : Board
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        int[] killer_x = new int[TSUME_MAX_DEPTH + 1];
        int[] killer_y = new int[TSUME_MAX_DEPTH + 1];
        int[] killer_t = new int[TSUME_MAX_DEPTH + 1];

        Random rnd = new Random();

        public StrongPlayer()
        {
            init_hash();
            for (int i = 0; i < TSUME_MAX_DEPTH + 1; i++)
            {
                killer_x[i] = 12;
                killer_y[i] = 12;
                killer_t[i] = 2;
            }
        }

        void init_hash()
        {
            int seed = Environment.TickCount;
            Random rand = new System.Random(seed);
            for(int i = 0; i < BMAX; i++)
            {
                for(int j = 0; j < BMAX; j++)
                {
                    random_t[i,j,VW] = ((ulong)rand.Next() << 63) | ((ulong)rand.Next() << 32) | ((ulong)rand.Next() << 1);
                    random_t[i,j,HW] = ((ulong)rand.Next() << 63) | ((ulong)rand.Next() << 32) | ((ulong)rand.Next() << 1);
                    random_t[i,j,LRW] = ((ulong)rand.Next() << 63) | ((ulong)rand.Next() << 32) | ((ulong)rand.Next() << 1);
                    random_t[i,j,LLW] = ((ulong)rand.Next() << 63) | ((ulong)rand.Next() << 32) | ((ulong)rand.Next() << 1);
                    random_t[i,j,URW] = ((ulong)rand.Next() << 63) | ((ulong)rand.Next() << 32) | ((ulong)rand.Next() << 1);
                    random_t[i,j,ULW] = ((ulong)rand.Next() << 63) | ((ulong)rand.Next() << 32) | ((ulong)rand.Next() << 1);
                }
            }
        }

        public int search_place(ref int x, ref int y, ref int t, int color)
        {
            int[] bb = new int[256];
            int bb_cnt = 0;
            max_depth = 1;
            int time;
            int ret;
            if (search(ref x, ref y, ref t, color, 1) == color)
            {
                place(x, y, t, bb, ref bb_cnt);
                return 1;
            }

            hash_cnt = 0;
            sw.Reset();
            sw.Start();
            for (max_depth = 1; max_depth <= MAX_DEPTH; max_depth++)
            {
                ret = search(ref x, ref y, ref t, color, 1);
                time = (int)sw.ElapsedMilliseconds;
                if (ret == color)
                {
                    place(x, y, t, bb, ref bb_cnt);
                    return 0;
                }
                else if (ret == 3 - color)
                {
                    random_place(ref x, ref y, ref t, color);
                    place(x, y, t, bb, ref bb_cnt);
                }

                else if (time > 3000)
                {
                    place(x, y, t, bb, ref bb_cnt);
                    return 0;
                }

            }

            /*
            else
            {
                random_place(ref rx, ref ry, ref rt, color);
                place(x, y, t, bb, ref bb_cnt);
            }
            */
            sw.Stop();
            place(x, y, t, bb, ref bb_cnt);
            return 0;
        }

        public int search(ref int rx, ref int ry, ref int rt, int color, int depth)
        {
            int i, j, x, y, t, ret;
            int fin = 0;
            int x_min_backup = x_min;
            int x_max_backup = x_max;
            int y_min_backup = y_min;
            int y_max_backup = y_max;
            int[] bb = new int[10000];
            int bb_cnt = 0;
            ulong hash_backup = hash;
            int[] px = new int[10000];
            int[] py = new int[10000];
            int[] pt = new int[10000]; // depth=1 のときしか使わない
            int p_cnt = 0;
            int myriichi = 0;
            int yrriichi = 0;

            //ハッシュ利用
            if (depth > 1)
            {
                if (HASH_TBL[hash & HASHWIDTH] == (hash | ((ulong)color - 1)))
                {
                    return WINLOSS[hash & HASHWIDTH]; // ハッシュに登録済
                }
            }

            //キラームーブチェック
            if (depth > 1)
            {
                x = killer_x[depth];
                y = killer_y[depth];
                t = killer_t[depth];
                if (board[x, y] == BLANK)
                {
                    if ((board[x - 1, y] | board[x + 1, y] | board[x, y - 1] | board[x, y + 1]) != 0)
                    {
                        if (place(x, y, t, bb, ref bb_cnt) == 1)
                        {
                            if (loop_trace(x, y, color, ref myriichi) == 1)
                            { // 自分のLoopができた
                                fin = 1;
                            }
                            else
                            {
                                if (depth < max_depth)
                                {
                                    int flag = 0;
                                    for (j = 0; j < bb_cnt; j++)
                                    { // 新しく置いたところを全て確認する
                                        int lx = bb[j] >> 10;
                                        int ly = bb[j] & 0x3ff;
                                        if (loop_trace(lx, ly, 3 - color, ref yrriichi) == 1)
                                        { // 相手のLoopができた
                                            flag = 1;
                                            break;
                                        }
                                    }
                                    if (flag == 0)
                                    { // 相手のループはできていない
                                        int tret;
                                        int _rx, _ry, _rt;
                                        tret = search(ref rx, ref ry, ref rt, 3 - color, depth + 1);
                                        if (tret == color)
                                        {
                                            fin = 1; // 自分が勝つ
                                        }
                                    }
                                }
                            }
                            for (j = 0; j < bb_cnt; j++) board[bb[j] >> 10, bb[j] & 0x3ff] = 0;
                            hash = hash_backup;
                            x_min = x_min_backup;
                            x_max = x_max_backup;
                            y_min = y_min_backup;
                            y_max = y_max_backup;
                            if (fin == 1)
                            {
                                HASH_TBL[hash & HASHWIDTH] = hash | ((ulong)color - 1);
                                WINLOSS[hash & HASHWIDTH] = (char)color; //ハッシュ登録
                                hash_cnt++;
                                return color;
                            }
                        }
                    }
                }
            }
          

            for (y = y_min - 1; y <= y_max + 1; y++)
            {
                for (x = x_min - 1; x <= x_max + 1; x++)
                {
                    if (x < 2 || x > 24) continue;
                    if (y < 2 || y > 24) continue;
                    if (board[x,y] != 0) continue;
                    if ((board[x - 1,y] | board[x + 1,y] | board[x,y - 1] | board[x,y + 1]) != 0)
                    {
                        if (depth == 1)
                        {
                            //if (x == x_min - 1) fprintf(stderr, "d=%d %d %d @%d", depth, x, y, y - y_min + 1);
                            //else fprintf(stderr, "d=%d %d %d %c%d", depth, x, y, x - x_min + 'A', y - y_min + 1);
                        }
                        for (i = 0; i < 6; i++)
                        {
                            t = TLIST[i];
                            if (place(x, y, t, bb, ref bb_cnt) == 1)
                            {
                                ret = loop_trace(x, y, color, ref myriichi);
                                if (ret == 1)
                                { // 自分のLoopができた
                                    //if (depth == 1) fprintf(stderr, " %c %s-LoopOrLine ", mark[t], color_s[color]);
                                    killer_x[depth] = x; killer_y[depth] = y; killer_t[depth] = t;
                                    fin = 1;
                                }
                                else
                                { // 相手のLoopを確認する
                                    int flag = 0;
                                    for (j = 0; j < bb_cnt; j++)
                                    { // 新しく置いたところを全て確認する
                                        int lx = bb[j] >> 10;
                                        int ly = bb[j] & 0x3ff;
                                        if (loop_trace(lx, ly, 3 - color, ref yrriichi) == 1)
                                        { // 相手のLoopができた
                                            flag = 1;
                                            break;
                                        }
                                    }
                                    if (flag == 0)
                                    { // 相手のループはできていない
                                        if (depth < max_depth)
                                        {
                                            int tret;
                                            int _rx, _ry, _rt;
                                            _rx = _ry = _rt = 0;
                                            tret = search(ref _rx, ref _ry, ref _rt, 3 - color, depth + 1);
                                            if (tret == color)
                                            {
                                                if (depth == 1)
                                                {
                                                    //fprintf(stderr, " %c(**KACHI**)", mark[t]);
                                                }
                                                killer_x[depth] = x; killer_y[depth] = y; killer_t[depth] = t;
                                                fin = 1; // 自分が勝つ
                                            }
                                            else if (tret == 3 - color)
                                            { // 相手が勝つ
                                              // デバッグ用コード(すでに登録されていることを確認)
                                              /*
                                              if( (HASH_TBL[hash & HASHWIDTH] != (hash | ( 2 - color)) ) || (WINLOSS[hash & HASHWIDTH] != 3-color) ){
                                                fprintf(stderr, " Error\n");
                                                exit(0);
                                              }
                                              */
                                                //if (depth == 1) fprintf(stderr, " %c(L)", mark[t]);
                                            }
                                            else
                                            { //勝敗付かない
                                                if (depth == 1)
                                                {
                                                   // if (myriichi == 1) fprintf(stderr, " %c(R)", mark[t]);
                                                    //else fprintf(stderr, " %c", mark[t]);
                                                    px[p_cnt] = x; py[p_cnt] = y; pt[p_cnt] = t;
                                                }
                                                p_cnt++;
                                            }
                                        }
                                        else
                                        { // 末端(depth == max_depth)
                                            if (depth == 1)
                                            { // MAX_DEPTH と depth の両方とも 1 のとき
                                                //if (myriichi == 1) fprintf(stderr, " %c(R)", mark[t]);
                                                //else fprintf(stderr, " %c", mark[t]);
                                                px[p_cnt] = x; py[p_cnt] = y; pt[p_cnt] = t;
                                            }
                                            p_cnt++;
                                        }
                                    }
                                    else
                                    { //相手のループができて負け
                                        //if (depth == 1) fprintf(stderr, " %c(L)", mark[t]);
                                    }
                                }
                                for (j = 0; j < bb_cnt; j++) board[bb[j] >> 10,bb[j] & 0x3ff] = 0;
                                hash = hash_backup;
                                x_min = x_min_backup; x_max = x_max_backup;
                                y_min = y_min_backup; y_max = y_max_backup;

                                if (fin == 1)
                                {
                                    HASH_TBL[hash & HASHWIDTH] = hash | ((ulong)color - 1);
                                    WINLOSS[hash & HASHWIDTH] = (char)color; //ハッシュ登録
                                    hash_cnt++;
                                    rx = x;
                                    ry = y;
                                    rt = t;
                                    //if (depth == 1) fprintf(stderr, "\n");
                                    return color;
                                }
                            }
                        }
                        //if (depth == 1) fprintf(stderr, "\n");
                    }
                }
            }
            if (p_cnt == 0)
            { //防ぐ手がないので自分の負け
                HASH_TBL[hash & HASHWIDTH] = hash | ((ulong)color - 1);
                WINLOSS[hash & HASHWIDTH] = (char)(3 - color); //ハッシュ登録
                hash_cnt++;
                return 3 - color;
            }
            if (depth == 1)
            {
                int r = rnd.Next(0, p_cnt);
                rx = px[r];
                ry = py[r];
                rt = pt[r];
            }
            return 0;
        }
    }
}
