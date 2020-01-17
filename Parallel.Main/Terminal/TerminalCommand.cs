using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parallel.Main.Terminal
{
    public interface ITerminalCommand : IEnumerable<ITerminalCommand>
    {
        string CommandText { get; }
        ICollection<ITerminalCommand> PossibleArguments { get; }

        string SearchPartial(string partialCommandText)
        {
            if (!string.IsNullOrWhiteSpace(partialCommandText) && PossibleArguments != null &&
                PossibleArguments.Count > 0)
            {
                foreach (ITerminalCommand possibleArgument in PossibleArguments)
                {
                    if (possibleArgument.CommandText.StartsWith(partialCommandText))
                    {
                        return PossibleArguments.ToString();
                    }
                }
            }

            return null;
        }

        string Description { get; }

        Action<string[]> Execute { get; }
    }

    public abstract class BaseTerminalCommand : ITerminalCommand
    {
        public BaseTerminalCommand(string commandText, Action<string[]> execute, string description)
        {
            CommandText = commandText;
            Execute = execute;
            Description = description;
            PossibleArguments = new List<ITerminalCommand>();
            if (commandText != "help")
            {
                PossibleArguments.Add(new TerminalCommand("help", DisplayHelp, "Possible Arguments"));
            }
        }
        
        private void DisplayHelp(string[] obj)
        {
            foreach (ITerminalCommand terminalCommand in PossibleArguments)
            {
                Console.WriteLine(terminalCommand);
            }
        }

        public IEnumerator<ITerminalCommand> GetEnumerator()
        {
            return PossibleArguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string CommandText { get; }
        public ICollection<ITerminalCommand> PossibleArguments { get; }
        public string Description { get; }
        public Action<string[]> Execute { get; }

        public override string ToString()
        {
            return $"{nameof(CommandText)}: {CommandText},\t {nameof(Description)}: {Description}";
        }
    }

    public class TerminalCommand : BaseTerminalCommand
    {
        public TerminalCommand(string commandText, Action<string[]> execute, string description) : base(commandText,
            execute, description)
        {
        }
    }
}