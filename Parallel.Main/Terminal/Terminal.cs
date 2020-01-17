using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Parallel.Main.Terminal
{
    public class Terminal
    {
        private readonly IDictionary<string, ITerminalCommander> _commands;
        private readonly StringBuilder _typedCommands = new StringBuilder();
        private readonly LinkedList<string> _enteredCommands = new LinkedList<string>();
        private LinkedListNode<string> _scrollNode = null;

        public Terminal(IEnumerable<ITerminalCommander> terminalCommanders)
        {
            _commands = terminalCommanders.ToDictionary(x => x.RouteCommand.CommandText, v => v);
        }

        public void StartTerminal()
        {
            Console.Write(">:");
            while (true)
            {
                var pressedKey = Console.ReadKey(true);
                switch (pressedKey.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        ProcessCommands();
                        Console.Write(">:");
                        break;
                    case ConsoleKey.Backspace:
                        DeleteChar(true);
                        break;
                    case ConsoleKey.Delete:
                        DeleteChar(false);
                        break;
                    case ConsoleKey.LeftArrow:
                        MoveCursor(-1);
                        break;
                    case ConsoleKey.RightArrow:
                        MoveCursor(1);
                        break;
                
                    case ConsoleKey.UpArrow:
                        ScrollEnteredCommands(true);
                        break;
                    case ConsoleKey.DownArrow:
                        ScrollEnteredCommands(false);
                        break;
                    default:
                        Console.Write(pressedKey.KeyChar);
                        _typedCommands.Append(pressedKey.KeyChar);
                        break;
                }
            }
        }

        private void ScrollEnteredCommands(bool up)
        {
            if (up)
            {
                _scrollNode = _scrollNode == null ? _enteredCommands.First : _scrollNode.Next;
            }
            else
            {
                _scrollNode = _scrollNode == null ? _enteredCommands.Last : _scrollNode.Previous;
            }

            if (_scrollNode != null)
            {
                Console.Write("\r>:" + _scrollNode.Value + string.Join("",
                                  Enumerable.Repeat(' ', Console.WindowWidth - _scrollNode.Value.Length - 3)));
                Console.SetCursorPosition(_scrollNode.Value.Length+2, Console.CursorTop);
                _typedCommands.Clear();
                _typedCommands.Append(_scrollNode.Value);
            }
        }

        private void MoveCursor(int i)
        {
            var x = Console.CursorLeft;
            if (x >= 3 && i < 0)
            {
                Console.SetCursorPosition(x + i, Console.CursorTop);
            }
            else if (i > 0)
            {
                Console.SetCursorPosition(x + i, Console.CursorTop);
            }
        }

        private void DeleteChar(bool left)
        {
            int x = Console.CursorLeft;
            if (left)
            {
                _typedCommands.Remove((x - 3), 1);
                Console.Write("\r>:" + _typedCommands + " ");
                Console.SetCursorPosition(x - 1, Console.CursorTop);
            }
            else
            {
                if (_typedCommands.Length + 2 > x)
                {
                    _typedCommands.Remove((x - 2), 1);
                    Console.Write("\r>:" + _typedCommands + " ");
                    Console.SetCursorPosition(x, Console.CursorTop);
                }
            }
        }

        private void ProcessCommands()
        {
            var textCommand = _typedCommands.ToString();
            var args = textCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                _enteredCommands.AddFirst(textCommand);
                _typedCommands.Clear();
                 if (_commands.ContainsKey(args[0]))
                {
                    ExecuteTerminalCommand(args[1..], _commands[args[0]].RouteCommand.PossibleArguments);
                }
            }
        }

        private void ExecuteTerminalCommand(string[] args, IEnumerable<ITerminalCommand> commands)
        {
            var argsCounter = 0;
            ITerminalCommand lastFound = null;
            ITerminalCommand[] terminalCommands = commands as ITerminalCommand[] ?? commands.ToArray();
            for (int i = 0; i < args.Length; i++)
            {
                for (var j = 0; j < terminalCommands.Length; j++)
                {
                    var terminalCommand = terminalCommands[j];
                    if (argsCounter < args.Length && terminalCommand.CommandText == args[argsCounter])
                    {
                        lastFound = terminalCommand;
                        argsCounter++;
                        terminalCommands = lastFound.PossibleArguments.ToArray();
                        j = 0;
                    }
                }
            }

            if (lastFound != null)
            {
                if (lastFound.Execute != null)
                {
                    lastFound.Execute.Invoke(args[argsCounter..]);
                }
                else
                {
                    Console.WriteLine($"{lastFound.CommandText} is not directly executable.");
                }
            }
        }
    }
}