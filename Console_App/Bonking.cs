using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using static Shared.Shared;
using static Console_App.Input;
using static  Console_App.Output;

namespace Console_App
{
    public static class Bonking
    {
        public static void Bonk()
        {
            string name;
            List<Person> people;
            
            // Getting the Person
            while (true)
            {
                name = InputString("Input name to bonk:", 20, 1, "Name is over 20 characters! Try again",
                    "Name needs to have at least one character");
                people = SearchPeople(name, AliveOrDead.Alive);
                if (!people.Count.Equals(0)) break;
                WriteLine("This name does not match any person. Try again!");
            }

            Person person;
            if (people.Count.Equals(1)) { person = people.First(); }
            else
            {
                WriteLine(
                    $"There was muliple people with the name {name}! Please select the users you wanted by the order in the table.\n" + 
                    PeopleToSearchTable(people));
                
                int option = InputInt(max: people.Count + 1, min: 0, errorMax: "This option does not exist. Try again!",
                    errorMin: "This option does not exist. Try again!");

                person = people[option - 1];
            }
            
            // Getting Bonk amount
            WriteLine(
                "How much to Bonk? Bonking removes horniness from a range of 0 to 100. Make sure not bonk under 0!");
            Random rng = new();
            int strength = InputInt() + rng.Next(-10, 10);
            
            while (strength <= 0)
            {
                WriteMessage($"WARNING WARNING: DETECTED NEGATIVE BONK STRENGTH ARE YOU SURE TO PROCEED WITH ANTI-BONK TO {name.ToUpper()}?\nY/N",
                    ConsoleColor.Red, ConsoleColor.Yellow);
                
                if (InputBool())
                {
                    WriteMessage("CONTINUING ANTI-BONKING. MAY RESULT IN DEATH.",
                        ConsoleColor.Red, ConsoleColor.Yellow);
                    break;
                }

                LoadingWriter("RESETTING BONK");
                strength = InputInt() + rng.Next(-10, 10);
            }
            
            // Returning Bonk output
            person.Horny = -strength;
            switch (person.Horny + 50)
            {
                case > 100:
                    WriteLine("They have died due to an overload of horniness!");
                    person.Dead = true;
                    break;
                case < 0:
                    WriteLine("You bonked them so hard they died!");
                    person.Dead = true;
                    break;
                case > 70:
                    WriteLine("Bonk went successfully although the person is still incredibly horny");
                    break;
                case > 50:
                    WriteLine("Bonk went successfully although the person is still very horny");
                    break;
                case > 20:
                    WriteLine("Bonk went successfully although the person is still horny");
                    break;
                case < 20:
                    WriteLine("Bonk went successfully although the person is still a tiny bit  horny");
                    break;
                default:
                    WriteLine("Bonk went successfully! The person is not horny at all!");
                    break;
            }
            if (InputBool("\nDo you wish to save this bonk?")) { SavePerson(person); }
            
            LoadingWriter("Returning to home");
        }
    }
}