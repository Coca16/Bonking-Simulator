using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Newtonsoft.Json;

namespace Bonking
{
    class Program
    {
        static Timer timer = new();
        private static readonly HashSet<char> vowels = new() { 'a', 'e', 'i', 'o', 'u' };
        private static readonly string[] horny_people = 
        {"djz", "coolio", "blue", "blueviper", "jam", "jambam", "jambon", "bmk", "josua", "gumbo", "taz", "devin", "Caeser"};
        static async Task Main()
        {
            string json = string.Empty;

            using (FileStream fs = File.OpenRead("secret.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            
            SecretJson secret = JsonConvert.DeserializeObject<SecretJson>(json);

            string cs = @$"server={secret.Server};userid={secret.UserID};password={secret.Password};database={secret.Database}";

            using MySqlConnection con = new(cs);
            con.Open();

            Console.WriteLine($"MySQL version : {con.ServerVersion}");

            
            
            
            while (true)
            {
                Console.WriteLine(@"
Please insert whatever number for the option you want.
1. Add a name
2. Bonk someone
3. Quit");
                int option = Convert.ToInt16(Console.ReadLine());
                Console.WriteLine();
                switch (option)
                {
                    case 1:
                        Add(con);
                        break;
                    case 2:
                        StartBonk(con);
                        break;
                    case 3:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Not an expected input. Try again!");
                        break;
                }
            }
        }

        private static void Add(MySqlConnection sqlconnection)
        {
            while (true)
            {
                Console.WriteLine("Input name to add to database:");
                string name = Console.ReadLine();
                if (name.Length <= 20)
                {
                    int horniness;
                    Random rnd = new(name.GetHashCode());
                    if (horny_people.Contains(name.ToLower()))
                    {
                        horniness = rnd.Next(70, 100);
                    }
                    else
                    {
                        int t_vowels = name.Count(c => vowels.Contains(c));
                        horniness = name.Length + t_vowels + rnd.Next(0, 10);
                        horniness *= 2;
                    }
                    
                    string stm = $"INSERT INTO people(name, horniness) VALUES('{name}', {horniness - 50});";
                    var cmd = new MySqlCommand(stm, sqlconnection);
                    Console.WriteLine($"{cmd.ExecuteNonQuery()} rows where affected in the database!");
                }
                else
                {
                    Console.WriteLine($"Name is over 20 characters! Try again");
                    continue;
                }

                break;
            }
        }

        private static void StartBonk(MySqlConnection sqlconnection)
        {
            while (true)
            {
                Console.WriteLine("Input name to bonk:");
                string name = Console.ReadLine();
                Console.WriteLine();
                if (name.Length <= 20)
                {
                    string stmCount = "SELECT COUNT(*) FROM people WHERE name = @name AND dead = FALSE;";
                    MySqlCommand cmd_count = new(stmCount, sqlconnection);
                    cmd_count.Parameters.AddWithValue("@name", name);
                    MySqlDataReader counter = cmd_count.ExecuteReader();
                    counter.Read();
                    int count = counter.GetInt32(0);
                    bool nodupes = count.Equals(1);
                    counter.Close();
                    if (count.Equals(0))
                    {
                        Console.WriteLine("This name does not match any person. Try again!");
                        continue;
                    }
                    
                    const string stm = "SELECT horniness, added, latest_bonk, bonked, person_id FROM people WHERE name = @name AND dead = FALSE;";
                    MySqlCommand cmd = new(stm, sqlconnection);
                    cmd.Parameters.AddWithValue("@name", name);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    Person person;
                    
                    if (nodupes)
                    {
                        reader.Read();
                        int horny = reader.GetInt16(0);
                        MySqlDateTime added = reader.GetMySqlDateTime(1);
                        MySqlDateTime latest = reader.GetMySqlDateTime(2);
                        int bonked = reader.GetInt16(3);
                        int id = reader.GetInt32(4);
                        reader.Close();
                        person = new Person(horny, added, latest, bonked, id);
                    }
                    else
                    {
                        List<Person> people = new();
                        while (reader.Read())
                        {
                            int horny = reader.GetInt16(0); 
                            MySqlDateTime added = reader.GetMySqlDateTime(1);
                            MySqlDateTime latest = reader.GetMySqlDateTime(2).Equals(added)
                                ? new MySqlDateTime(new DateTime(1,1,1))
                                : reader.GetMySqlDateTime(2);
                            int bonked = reader.GetInt16(3);
                            int id = reader.GetInt32(4);
                            people.Add(new Person(horny, added, latest, bonked, id));
                        }
                        reader.Close();

                        Console.WriteLine(
                            $"There was muliple people with the name {name}! Please select the users you wanted by the order in the table.");
                        string peopleTable = people.ToStringTable(new[] {"Added At", "Latest Bonk", "Times Bonked", "Count (Id)"},
                            p => p.Added,
                            p => p.BonkedCount.Equals(0) ? "Never Bonked" : p.LatestBonk,
                            p => p.BonkedCount,
                            p => p.Id
                        );
                        Console.WriteLine(peopleTable);

                        int option;
                        while (true)
                        {
                            option = Convert.ToInt16(Console.ReadLine()) - 1;
                            try { Console.WriteLine(); }
                            catch (Exception e)
                            {
                                if (e.Message != "Input string was not in a correct format.") throw;
                                Console.WriteLine("The input was not what was expected. Please try again.");
                                continue;
                            }
                            
                            if (option >= count)
                            {
                                Console.WriteLine("This option does not exist. Try again!");
                                continue;
                            }
                            break;
                        }
                        person = people[option];
                    }

                    Random rng = new();
                    int strength;

                    Console.WriteLine("How much to Bonk? Bonking removes horniness from a range of 0 to 100. Make sure not bonk under 0!");
                    while (true)
                    {
                        try { strength = Convert.ToInt16(Console.ReadLine()) + rng.Next(-10,10); }
                        catch (Exception e)
                        {
                            if (e.Message != "Input string was not in a correct format.") throw;
                            Console.WriteLine("The input was not what was expected. Please try again.");
                            continue;
                        }
                        if (strength >= 0) break;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            $"WARNING WARNING: DETECTED NEGATIVE BONK STRENGTH ARE YOU SURE TO PROCEED WITH ANTI-BONK TO {name.ToUpper()}?\nY/N");
                        Console.ResetColor();

                        string optionAntiBonk;
                        while (true)
                        {
                            optionAntiBonk = Console.ReadLine();
                            if (optionAntiBonk.ToLower() == "y" || optionAntiBonk.ToLower() == "yes" || optionAntiBonk.ToLower() == "n" || optionAntiBonk.ToLower() == "no")
                            {
                                break;
                            }
                            Console.WriteLine($"Input is neither Y or N!");
                        }
                        if (optionAntiBonk.ToLower() == "n" || optionAntiBonk.ToLower() == "no")
                        {
                            LoadingWriter("RESETTING BONK");
                        }
                        else if (optionAntiBonk.ToLower() == "y" || optionAntiBonk.ToLower() == "yes")
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine($"CONTINUING ANTI-BONKING. MAY RESULT IN DEATH.");
                            Console.ResetColor();
                            break;
                        }
                    }


                    person.Horny =- strength;

                    switch (person.Horny + 50)
                    {
                        case > 100:
                            Console.WriteLine("They have died due to an overload of horniness!");
                            person.Dead = true;
                            break;
                        case < 0:
                            Console.WriteLine("You bonked them so hard they died!");
                            person.Dead = true;
                            break;
                        case > 70:
                            Console.WriteLine("Bonk went successfully although the person is still incredibly horny");
                            break;
                        case > 40:
                            Console.WriteLine("Bonk went successfully although the person is still very horny");
                            break;
                        case > 10:
                            Console.WriteLine("Bonk went successfully although the person is still horny");
                            break;
                        default:
                            Console.WriteLine("Bonk went successfully! The person is not very horny at all!");
                            break;
                    }
                    Console.WriteLine("\nDo you wish to save this bonk?");
                    string optionSave;
                    while (true)
                    {
                        optionSave = Console.ReadLine();
                        if (optionSave.ToLower() == "y" || optionSave.ToLower() == "yes")
                        {
                            person.BonkedCount++;
                            string save = $"UPDATE people SET horniness = @horny, latest_bonk = CURRENT_TIMESTAMP, bonked = @bonked, dead = {person.Dead.ToString().ToUpper()} WHERE person_id = @id;";
                            MySqlCommand cmdSave = new(save, sqlconnection);
                            cmdSave.Parameters.AddWithValue("@horny", person.Horny);
                            cmdSave.Parameters.AddWithValue("@bonked", person.BonkedCount);
                            cmdSave.Parameters.AddWithValue("@id", person.Id);
                            Console.WriteLine($"{cmdSave.ExecuteNonQuery()} rows where affected in the database!");
                            break;
                        }
                        if (optionSave.ToLower() == "n" || optionSave.ToLower() == "no")
                        {
                            break;
                        }
                        Console.WriteLine($"Input is neither Y or N!");
                    }
                    LoadingWriter("Returning to home");
                }
                else
                {
                    Console.WriteLine($"Name is over 20 characters! Try again");
                    continue;
                }
                break;
            }
        }

        private static void LoadingWriter(string message)
        { 
            Console.Write(message);
            int times = 0;
            for (int i = 0; i < 3; i++)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(500);
                if (i != 2) continue;
                Console.Write("\r".PadRight(message.Length) + "\r" + message);
                i = -1;
                System.Threading.Thread.Sleep(500);
                if (times == 2) break;
                times++;
            }
            Console.WriteLine();
        }

        public class Person
        {
            public int Horny { get; set; }
            public MySqlDateTime Added { get; }
            public MySqlDateTime LatestBonk { get; }
            public int BonkedCount { get; set; }
            public int Id { get; }
            public bool Dead { get; set; }

            public Person(int horny,MySqlDateTime added, MySqlDateTime latestBonk, int bonkedCount, int id)
            {
                Horny = horny;
                Added = added;
                LatestBonk = latestBonk;
                BonkedCount = bonkedCount;
                Id = id;
                Dead = false;            
            }
            
            public Person(int horny,MySqlDateTime added, MySqlDateTime latestBonk, int bonkedCount, int id, bool dead)
            {
                Horny = horny;
                Added = added;
                LatestBonk = latestBonk;
                BonkedCount = bonkedCount;
                Id = id;
                Dead = dead;
            }
        }
    }
}