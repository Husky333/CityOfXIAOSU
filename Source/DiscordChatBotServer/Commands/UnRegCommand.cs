using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace OC.DiscordBotServer.Commands
{
    public sealed class UnRegCommand : ICommand
    {
        public string Execute(SocketCommandContext parameter)
        {
            throw new NotImplementedException();
        }

        #pragma warning disable CS0628
        protected bool CanExecute(SocketCommandContext parameter, out string message)
        {
            throw new NotImplementedException();
        }
    }
}
