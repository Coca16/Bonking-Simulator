using System;
using static Shared.Shared;
using static System.Console;
using static Console_App.Bonking;
using static Console_App.Input;
using static Console_App.Output;

namespace Console_App
{
    class Program
    {
        static void Main()
        {
            BeginMySQL();
            while (true)
            {
                int option = InputInt(@"
Please insert whatever number for the option you want.
1. Add a name
2. Find someone
3. Bonk someone
4. Populate
5. Quit");
                WriteLine();
                switch (option)
                {
                    case 1:
                        Add(InputString("Input name to add:",
                            20, 1,
                            "Name is over 20 characters! Try again",
                            "Name needs to have at least one character"));
                        break;
                    case 2:
                        
                        break;
                    case 3:
                        Bonk();
                        break;
                    case 4:
                        Populate(InputInt("Input amount of people you want to populate the database with:", 
                            1000, 
                            errorMax: "I will not let you crash my computer."));
                        break;
                    case 5:
                        LoadingWriter("Exiting Bonk");
                        Environment.Exit(0);
                        break;
                    default:
                        WriteLine("Not an expected input. Try again!");
                        break;
                }
            }
        }
    }
}