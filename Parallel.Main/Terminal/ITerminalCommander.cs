using System.Collections.Generic;
using System.Linq;

namespace Parallel.Main.Terminal
{
    public interface ITerminalCommander
    {
        TerminalCommand RouteCommand { get; }

        IEnumerable<string> CommandList
        {
            get { return RouteCommand.PossibleArguments.Select(x => x.CommandText); }
        }
    }
}