using Model;
using OCUnion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Transfer;
using System.Linq;
using ServerOnlineCity.Services;

namespace ServerOnlineCity.Model
{
    [Serializable]
    public class ShipGoodsContainer
    {
        public List<ThingEntry> goods { get; set; }
        private  Dictionary<string, ThingEntry> goods_dict { get; set; }

        private static object Updating = new object();

        public ShipGoodsContainer()
        {
            goods_dict = new Dictionary<string, ThingEntry>();
        }

        public Dictionary<string, ThingEntry> GetGoodsDict()
        {
            lock (Updating)
            {
                return goods_dict;
            }
        }

        public void UpdateGoodsDict(List<ThingEntry> deltas)
        {
            lock (Updating) {
                goods_dict.Remove("Silver");
                foreach (ThingEntry te in deltas)
                {
                    if (te.DefNameTE == "Silver")
                        continue;
                    if (goods_dict.ContainsKey(te.DefNameTE))
                    {
                        int new_count = goods_dict[te.DefNameTE].Count + te.Count;
                        if (new_count > 0)
                        {
                            goods_dict[te.DefNameTE].Count = new_count;
                        }
                        else
                        {
                            goods_dict.Remove(te.DefNameTE);
                        }
                    }
                    else if (te.Count > 0)
                    {
                        goods_dict.Add(te.DefNameTE, te);
                    }
                }
                if (deltas.Count > 0) Repository.ShipChangeData = true;
            }
        }
    }
}
