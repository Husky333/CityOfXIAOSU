using System;
using System.Linq;
using OCUnion;
using OCUnion.Transfer.Model;
using ServerOnlineCity.Model;
using Transfer;
using Transfer.ModelMails;
using Model;
using System.Collections.Generic;

namespace ServerOnlineCity.Services
{
    internal sealed class GetShipGoodsList : IGenerateResponseContainer
    {
        public int RequestTypePackage => (int)PackageType.Request202GetTradeship;

        public int ResponseTypePackage => (int)PackageType.Response203GetTradeship;

        public ModelContainer GenerateModelContainer(ModelContainer request, ServiceContext context)
        {
            if (context.Player == null) return null;
            var result = new ModelContainer() { TypePacket = ResponseTypePackage };
            result.Packet = GetShipGoods((ModelMailTrade)request.Packet, context);
            return result;
        }

        public Dictionary<string, ThingEntry> GetShipGoods(ModelMailTrade packet, ServiceContext context)
        {
            /*
            PlayerServer toPlayer;

            lock (context.Player)
            {
                var data = Repository.GetData;

                toPlayer = data.PlayersAll.FirstOrDefault(p => p.Public.Login == packet.To.Login);
                if (toPlayer == null)
                {
                    return new ModelStatus()
                    {
                        Status = 1,
                        Message = "Destination not found"
                    };
                }

                packet.From = data.PlayersAll.Select(p => p.Public).FirstOrDefault(p => p.Login == packet.From.Login);
                packet.To = toPlayer.Public;
                packet.Created = DateTime.UtcNow;
                packet.NeedSaveGame = true;
            }
            lock (toPlayer)
            {
                toPlayer.Mails.Add(packet);
            }
            Loger.Log($"Mail SendThings {packet.From.Login}->{packet.To.Login} {packet.ContentString()}");
            */

            return Repository.GetShipGoodsData.GetGoodsDict();
        }
    }
}