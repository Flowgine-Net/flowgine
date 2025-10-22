using Flowgine.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowgine.Example.Console.Examples._02_ConditionalFlow
{
    public class AdderNode : INode<AgentState>
    {
        public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
        {
            return Update.Of<AgentState>()
                .Set(s => s.Result, state.Number1 + state.Number2);
        }
    }
}
