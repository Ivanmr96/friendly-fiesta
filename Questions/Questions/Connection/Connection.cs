using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Connection
{
    public class Connection
    {
        public static HubConnection conn;
        public static IHubProxy proxy;
    }
}
