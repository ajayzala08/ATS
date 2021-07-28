using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ATS2019_2.Hubs
{
    public class MyChat :Hub
    {
        static ConcurrentDictionary<string, string> dic = new ConcurrentDictionary<string, string>();

        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }

        public void SendToSpecific(string name, string message, string to)
        {
            //  
            if (to == "All")
            {
                Clients.All.broadcastMessage(name, message);
            }
            else
            {
                Clients.Client(dic[to]).broadcastMessage(name, message);
                Clients.Client(dic[name]).broadcastmessage(name, message);
            }
        }

        public void Notify(string name, string id)
        {
            if (dic.ContainsKey(name))
            {
                Clients.Caller.differentName();
            }
            else
            {
                dic.TryAdd(name, id);
                foreach (KeyValuePair<String, String> entry in dic)
                {
                    Clients.Caller.online(entry.Key);
                }
                Clients.Others.enters(name);
            }
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            var name = dic.FirstOrDefault(x => x.Value == Context.ConnectionId.ToString());
            //string s;

           //uncomment when host on server
           // dic.TryRemove(name.Key, out s);
            return Clients.All.disconnected(name.Key);
        }
        public void sendnewmassage(string un, string msg)
        {
            Clients.All.senddata(un, msg);
        }
    }
}