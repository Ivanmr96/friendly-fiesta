using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Servidor_Questions.Models;

[assembly: OwinStartup(typeof(Servidor_Questions.Startup))]

namespace Servidor_Questions
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Para obtener más información sobre cómo configurar la aplicación, visite https://go.microsoft.com/fwlink/?LinkID=316888

            GlobalHost.DependencyResolver.Register(typeof(QuestionsHub), () =>  new QuestionsHub(partidas.Instance));

            app.MapSignalR();
        }
    }
}
