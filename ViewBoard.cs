using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSTraxPlayer
{
    class ViewBoard : Board
    {
        public history gh;
        public Stack<history> UpdateLog = new Stack<history>();

        public int view_place(int x, int y, int tile)
        {
            bool loseflag = false;
            int riichi = 0;
            gh = new history();
            gh.hx_min = x_min;
            gh.hy_min = y_min;
            gh.hx_max = x_max;
            gh.hy_max = y_max;
            gh.hhash = hash;
            if(place(x, y, tile, gh.hbb, ref gh.hbb_cnt) == -1)return -1;
            UpdateLog.Push(gh);
            for(int i = 0; i < gh.hbb_cnt; i++)
            {
                if(loop_trace(gh.hbb[i] >> 10, gh.hbb[i] & 0x3ff, mycolor,ref riichi) == 1)return 10;
                if(loop_trace(gh.hbb[i] >> 10, gh.hbb[i] & 0x3ff, 3 - mycolor, ref riichi) == 1)loseflag = true;
            }
            if (loseflag == true) return 11;
            return 1;
        }

        public history undo()
        {
            history h;
            h = UpdateLog.Pop();
            x_min = h.hx_min;
            x_max = h.hx_max;
            y_min = h.hy_min;
            y_max = h.hy_max;
            hash = h.hhash;
            for (int i = 0; i < h.hbb_cnt; i++) board[h.hbb[i] >> 10, h.hbb[i] & 0x3ff] = 0;
            return h;
        }

        public int view_first_place(int x, int y, int tile)
        {
            int ret;
            PlaceableTile[0, 0, 0, 0] = 0xffffff;
            ret = view_place(x, y, tile);
            PlaceableTile[0, 0, 0, 0] = 0;
            return ret;
        }
    }
}
