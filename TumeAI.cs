﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTraxPlayer
{
    class TumeAI : StrongPlayer
    {
        Random rnd = new Random();
        int[] killer_x = new int[100];
        int[] killer_y = new int[100];
        int[] killer_t = new int[100];

        public TumeAI()
        {
            for(int i = 0; i < 100; i++)
            {
                killer_x[i] = killer_y[i] = BMAX / 2;
                killer_t[i] = VW;
            }
        }

        public new int search_place(ref int rx, ref int ry, ref int rt, int color)
        {
            int x = 0, y = 0, t = 0, ret = 0;
            int[] bb = new int[10000];
            int bb_cnt = 0;

            sw.Reset();
            sw.Start();
            long tm;
            for (max_depth = 1; max_depth <= 7; max_depth += 2)
            {
                ret = tsume_search(ref x, ref y, ref t, color, 1);
                tm = sw.ElapsedMilliseconds;
                if (ret == color || tm > 1000)
                {
                    break;
                }
            }
            sw.Stop();
            if(ret == color)
            {
                rx = x;
                ry = y;
                rt = t;
                place(x, y, t, bb, ref bb_cnt);
                if (max_depth == 1) return 1;
                return 0;
            }


            hash_cnt = 0;
            sw.Reset();
            sw.Start();
            for(max_depth = 1; max_depth <= 4; max_depth++)
            {
                ret = search(ref x, ref y, ref t, color, 1);
                if(ret == color)
                {
                    rx = x;
                    ry = y;
                    rt = t;
                    place(x, y, t, bb, ref bb_cnt);
                    return 0;
                }
                else if (ret == 3 - color)
                {
                    random_place(ref rx, ref ry, ref rt, color);
                    place(x, y, t, bb, ref bb_cnt);
                }
                
                else if(sw.ElapsedMilliseconds > 2500)
                {
                    rx = x;
                    ry = y;
                    rt = t;
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
            rx = x;
            ry = y;
            rt = t;
            place(x, y, t, bb, ref bb_cnt);
            return 0;
        }

        int yr_tsume_search(ref int rx, ref int ry,ref int rt, int color, int depth)
        {
            int i, j, x, y, t, ret;
            int fin = 0;
            int x_min_backup = x_min;
            int x_max_backup = x_max;
            int y_min_backup = y_min;
            int y_max_backup = y_max;
            int bb_cnt = 0;
            int[] bb = new int[10000];
            ulong hash_backup = hash;
            int p_cnt = 0;
            int myriichi = 0, yrriichi = 0;

            //ハッシュ利用
            if (HASH_TBL[hash & HASHWIDTH] == (hash | ((ulong)color - 1)))
            {
                return WINLOSS[hash & HASHWIDTH]; // ハッシュに登録済
            }

            //キラームーブチェック
            x = killer_x[depth];
            y = killer_y[depth];
            t = killer_t[depth];

            if (board[x,y] == BLANK)
            {
                if ((board[x - 1,y] | board[x + 1,y] | board[x,y - 1] | board[x,y + 1]) != 0)
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
                                    int _rx = 0, _ry = 0, _rt = 0;
                                    tret = tsume_search(ref _rx, ref _ry, ref _rt, 3 - color, depth + 1);
                                    if (tret == color)
                                    {
                                        fin = 1; // 自分が勝つ
                                    }
                                }
                            }
                        }
                        for (j = 0; j < bb_cnt; j++) board[bb[j] >> 10,bb[j] & 0x3ff] = 0;
                        hash = hash_backup;
                        x_min = x_min_backup;
                        x_max = x_max_backup;
                        y_min = y_min_backup;
                        y_max = y_max_backup;
                        if (fin == 1)
                        {
                            // HASH_TBL[hash & HASHWIDTH] = hash | (color - 1);
                            // WINLOSS[hash & HASHWIDTH] = color; //ハッシュ登録
                            // hash_cnt++;
                            return color;
                        }
                    }
                }
            }

            for (y = y_min - 1; y <= y_max + 1; y++)
            {
                for (x = x_min - 1; x <= x_max + 1; x++)
                {
                    if (board[x,y] != 0) continue;
                    if ((board[x - 1,y] | board[x + 1,y] | board[x,y - 1] | board[x,y + 1]) != 0)
                    {
                        /*
                        if( depth==2 ){
                          if( x == x_min - 1 ) fprintf(stderr, " d=%d %d %d @%d", depth, x, y, y - y_min + 1);
                          else fprintf(stderr, " d=%d %d %d %c%d", depth, x, y, x - x_min + 'A', y - y_min + 1);
                        }
                        */
                        for (i = 0; i < 6; i++)
                        {
                            t = TLIST[i];
                            if (place(x, y, t, bb, ref bb_cnt) == 1)
                            {
                                ret = loop_trace(x, y, color, ref myriichi);
                                if (ret == 1)
                                { // 自分のLoopができた
                                    killer_x[depth] = x; killer_y[depth] = y; killer_t[depth] = t;
                                    fin = 1;
                                }
                                else
                                { // 相手のLoopを確認する
                                    int flag = 0;
                                    yrriichi = 0;
                                    for (j = 0; j < bb_cnt; j++)
                                    { // 新しく置いたところを全て確認する
                                        int lx = bb[j] >> 10;
                                        int ly = bb[j] & 0x3ff;
                                        int _yrriichi = 0;
                                        if (loop_trace(lx, ly, 3 - color, ref _yrriichi) == 1)
                                        { // 相手のLoopができた
                                            flag = 1;
                                            break;
                                        }
                                        yrriichi |= _yrriichi;
                                    }
                                    if (flag == 0)
                                    { // 相手のループはできていない
                                        if (depth < max_depth)
                                        {
                                            int tret = 0;
                                            int _rx = 0, _ry = 0, _rt = 0;
                                            tret = tsume_search(ref _rx, ref _ry, ref _rt, 3 - color, depth + 1);
                                            if (ret == color)
                                            {
                                                //                    if( depth==2 ) fprintf(stderr, " %c(K) ", mark[t]);
                                                killer_x[depth] = x; killer_y[depth] = y; killer_t[depth] = t;
                                                fin = 1; // 自分が勝つ
                                            }
                                            else if (tret == 3 - color)
                                            {
                                                //if( depth==2 ) fprintf(stderr, " %c(M) ", mark[t]);
                                                // 何もしない
                                            }
                                            else
                                            { //勝敗付かない
                                              //if( depth==2 ) fprintf(stderr, " %c ", mark[t]);
                                                p_cnt++;
                                            }
                                        }
                                        else
                                        { // 末端(depth == max_depth)
                                          //if( depth==2 ) fprintf(stderr, " %c ", mark[t]);
                                            p_cnt++;
                                        }
                                    }
                                    else
                                    {
                                        //if( depth==2 ) fprintf(stderr, " %c(M) ", mark[t]);
                                    }
                                }
                                for (j = 0; j < bb_cnt; j++) board[bb[j] >> 10,bb[j] & 0x3ff] = 0;
                                hash = hash_backup;
                                x_min = x_min_backup; x_max = x_max_backup;
                                y_min = y_min_backup; y_max = y_max_backup;

                                if (fin == 1)
                                {
                                    //HASH_TBL[hash & HASHWIDTH] = hash | (color - 1);
                                    //WINLOSS[hash & HASHWIDTH] = color; //ハッシュ登録
                                    //hash_cnt++;
                                    rx = x;
                                    ry = y;
                                    rt = t;
                                    return color;
                                }
                            }
                        }
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
            return 0;
        }

        int tsume_search(ref int rx, ref int ry, ref int rt, int color, int depth)
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
            int myriichi = 0, yrriichi = 0;

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

                if (board[x,y] == BLANK)
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
                                        int _rx = 0, _ry = 0, _rt = 0;
                                        tret = yr_tsume_search(ref _rx, ref _ry, ref _rt, 3 - color, depth + 1);
                                        if (tret == color)
                                        {
                                            fin = 1; // 自分が勝つ
                                        }
                                    }
                                }
                            }
                            for (j = 0; j < bb_cnt; j++) board[bb[j] >> 10,bb[j] & 0x3ff] = 0;
                            hash = hash_backup;
                            x_min = x_min_backup;
                            x_max = x_max_backup;
                            y_min = y_min_backup;
                            y_max = y_max_backup;
                            if (fin == 1)
                            {
                                //      HASH_TBL[hash & HASHWIDTH] = hash | (color - 1);
                                //      WINLOSS[hash & HASHWIDTH] = color; //ハッシュ登録
                                //      hash_cnt++;
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
                    if (board[x,y] != 0) continue;
                    if ((board[x - 1,y] | board[x + 1,y] | board[x,y - 1] | board[x,y + 1]) != 0)
                    {
                        /*
                        if( depth==1 ){
                          if( x == x_min - 1 ) fprintf(stderr, "d=%d %d %d @%d", depth, x, y, y - y_min + 1);
                          else fprintf(stderr, "d=%d %d %d %c%d", depth, x, y, x - x_min + 'A', y - y_min + 1);
                        }
                        */
                        for (i = 0; i < 6; i++)
                        {
                            t = TLIST[i];
                            if (place(x, y, t, bb, ref bb_cnt) == 1)
                            {
                                ret = loop_trace(x, y, color, ref myriichi);
                                if (ret == 1)
                                { // 自分のLoopができた
                                  //if( depth==1 ) fprintf(stderr, " %c %s-LoopOrLine ", mark[t], color_s[color]);
                                    killer_x[depth] = x; killer_y[depth] = y; killer_t[depth] = t;
                                    fin = 1;
                                }
                                else
                                { // 相手のLoopを確認する
                                    int flag = 0;
                                    yrriichi = 0;
                                    for (j = 0; j < bb_cnt; j++)
                                    { // 新しく置いたところを全て確認する
                                        int lx = bb[j] >> 10;
                                        int ly = bb[j] & 0x3ff;
                                        int _yrriichi = 0;
                                        if (loop_trace(lx, ly, 3 - color, ref _yrriichi) == 1)
                                        { // 相手のLoopができた
                                            flag = 1;
                                            break;
                                        }
                                        yrriichi |= _yrriichi;
                                    }
                                    if (flag == 0 && myriichi == 0 && yrriichi == 0)
                                    { // 相手のループはできていないしリートもしていないなら読まない

                                    }
                                    else if (flag == 0 && myriichi == 1)
                                    { // 相手のループはできていない
                                        if (depth < max_depth)
                                        {
                                            int tret;
                                            int _rx = 0, _ry = 0, _rt = 0;
                                            tret = yr_tsume_search(ref _rx, ref _ry, ref _rt, 3 - color, depth + 1);
                                            if (ret == color)
                                            {
                                                //if( depth==1 ) fprintf(stderr, " %c(**KACHI**)", mark[t]);
                                                if (depth == 1)
                                                {
                                                    //if (x == x_min - 1) fprintf(stderr, "d=%d %d %d @%d %c(**KACHI**)", depth, x, y, y - y_min + 1, mark[t]);
                                                    //else fprintf(stderr, "d=%d %d %d %c%d %c(**KACHI)", depth, x, y, x - x_min + 'A', y - y_min + 1, mark[t]);
                                                }
                                                killer_x[depth] = x; killer_y[depth] = y; killer_t[depth] = t;
                                                fin = 1; // 自分が勝つ
                                            }
                                            else if (ret == 3 - color)
                                            { // 相手が勝つ
                                              // デバッグ用コード(すでに登録されていることを確認)
                                              /*
                                              if( (HASH_TBL[hash & HASHWIDTH] != (hash | ( 2 - color)) ) || (WINLOSS[hash & HASHWIDTH] != 3-color) ){
                                                fprintf(stderr, " Error\n");
                                                exit(0);
                                              }
                                              */
                                              //if( depth==1 ) fprintf(stderr, " %c(L)", mark[t]);
                                            }
                                            else
                                            { //勝敗付かない
                                                if (depth == 1)
                                                {
                                                    //if( myriichi==1 ) fprintf(stderr, " %c(R)", mark[t]);
                                                    //else fprintf(stderr, " %c", mark[t]);
                                                    if (myriichi == 1)
                                                    {
                                                        px[p_cnt] = x; py[p_cnt] = y; pt[p_cnt] = t;
                                                    }
                                                }
                                                p_cnt++;
                                            }
                                        }
                                        else
                                        { // 末端(depth == max_depth)
                                            if (depth == 1)
                                            { // MAX_DEPTH と depth の両方とも 1 のとき
                                              // if( myriichi==1 ) fprintf(stderr, " %c(R)", mark[t]);
                                              // else fprintf(stderr, " %c", mark[t]);
                                                if (myriichi == 1)
                                                {
                                                    px[p_cnt] = x; py[p_cnt] = y; pt[p_cnt] = t;
                                                }
                                            }
                                            p_cnt++;
                                        }
                                    }
                                    else
                                    { //相手のループができて負け
                                      // if( depth==1 ) fprintf(stderr, " %c(L)", mark[t]);
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
                                    //if( depth==1 ) fprintf(stderr, "\n");
                                    return color;
                                }
                            }
                        }
                        //if( depth==1 ) fprintf(stderr, "\n");
                    }
                }
            }
            if (p_cnt == 0)
            { //防ぐ手がないので自分の負け
              //    HASH_TBL[hash & HASHWIDTH] = hash | (color - 1);
              //    WINLOSS[hash & HASHWIDTH] = 3 - color; //ハッシュ登録
              //    hash_cnt++;
                return 3 - color;
            }
            if (depth == 1)
            {
                //int r = random() % p_cnt;
                int r = rnd.Next() % p_cnt;
                rx = px[r];
                ry = py[r];
                rt = pt[r];
            }
            return 0;
        }
    }
}
