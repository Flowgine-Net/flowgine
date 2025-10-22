using Flowgine.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowgine.Example.Console.Examples._04_SimpleBot
{
    public class AgentState
    {
        public string Prompt { get; set; } = "Say 'hello from Flowgine'.";
        public string LastAnswer { get; set; } = "";
    }
}
