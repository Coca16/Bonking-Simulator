using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Bonking
{
    class Program
    {
        private static readonly HashSet<char> vowels = new() { 'a', 'e', 'i', 'o', 'u' };
        private static readonly string[] horny_people = {"djz", "coolio", "blue", "blueviper", "jam", "jambam", "jambon", "bmk", "josua", "gumbo", "taz", "devin"};
        static async Task Main()
        {
            string json = string.Empty;

            using (FileStream fs = File.OpenRead("secret.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            
            SecretJson secret = JsonConvert.DeserializeObject<SecretJson>(json);

            string cs = @$"server={secret.Server};userid={secret.UserID};password={secret.Password};database={secret.Database}";

            using MySqlConnection con = new MySqlConnection(cs);
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
                        Bonk(con);
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
                    
                    string stm = $"INSERT INTO people(name, horniness) VALUES('{name}', {horniness});";
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

        private static void Bonk(MySqlConnection sqlconnection)
        {
            while (true)
            {
                Console.WriteLine("Input name to bonk:");
                string name = Console.ReadLine();
                Console.WriteLine();
                if (name.Length <= 20)
                {
                    string stm_count = "SELECT COUNT(*) FROM people WHERE name = @name AND dead = FALSE;";
                    var cmd_count = new MySqlCommand(stm_count, sqlconnection);
                    cmd_count.Parameters.AddWithValue("@name", name);
                    MySqlDataReader counter = cmd_count.ExecuteReader();
                    counter.Read();
                    int count = counter.GetInt32(0);
                    bool nodupes = count.Equals(1);
                    counter.Close();
                    
                    string stm = "SELECT horniness, added, latest_bonk, bonked FROM people WHERE name = @name AND dead = FALSE;";
                    var cmd = new MySqlCommand(stm, sqlconnection);
                    cmd.Parameters.AddWithValue("@name", name);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    Person person;
                    
                    if (nodupes)
                    {
                        reader.Read();
                        int horny = reader.GetInt16(0);
                        string added = reader.GetMySqlDateTime(1).ToString();
                        string latest = reader.GetMySqlDateTime(2).ToString().Equals(added) ? reader.GetMySqlDateTime(2).ToString() : string.Empty;
                        int bonked = reader.GetInt16(3);
                        reader.Close();
                        new Person(horny, added, latest, bonked);
                    }
                    else
                    {
                        List<Person> people = new();
                        while (reader.Read())
                        {
                            int hornyList = reader.GetInt16(0);
                            string addedList = reader.GetMySqlDateTime(1).ToString();
                            string latestList = reader.GetMySqlDateTime(2).ToString().Equals(addedList)
                                ? reader.GetMySqlDateTime(2).ToString()
                                : string.Empty;
                            int bonkedList = reader.GetInt16(3);
                            people.Add(new Person(hornyList, addedList, latestList, bonkedList));
                        }
                        reader.Close();

                        Console.WriteLine(
                            $"There was muliple people with the name {name}! Please select the users you wanted by the order in the table.");
                        string peopleTable = people.ToStringTable(new[] {"Added At", "Latest Bonk", "Times Bonked"},
                            p => p.Added,
                            p => p.LatestBonk,
                            p => p.BonkedCount
                        );
                        Console.WriteLine(peopleTable);

                        int option;
                        while (true)
                        {
                            option = Convert.ToInt16(Console.ReadLine()) - 1;
                            Console.WriteLine();
                            if (option >= count)
                            {
                                Console.WriteLine("This option does not exist. Try again!");
                                continue;
                            }
                            break;
                        }
                        person = people[option];
                    }

                    Console.WriteLine("How much to Bonk? Bonking removes horniness from a range of 0 to 100. Make sure not bonk under 0!");
                    Random rng = new();
                    int strength = Console.Read() + rng.Next(-10,10); 
                    
                    
                    /*
if (human.Horny != 0 && !human.Dead)
{
Console.WriteLine("How much to Bonk? ");
int strength = Console.Read(); 
human.Bonk(strength);
Console.WriteLine(human.Dead);
}
else if (human.Dead) Console.WriteLine(human.Name + " is dead!");
else Console.WriteLine(human.Name + " is just not horny");
*/
                }
                else
                {
                    Console.WriteLine($"Name is over 20 characters! Try again");
                    continue;
                }

                break;
            }
        }

        public class Person
        {
            public int Horny { get; }
            public string Added { get;}
            public string LatestBonk { get; set; }
            public int BonkedCount { get; set; }

            public Person(int horny,string added, string latestBonk, int bonkedCount)
            {
                Horny = horny;
                Added = added;
                LatestBonk = latestBonk;
                BonkedCount = bonkedCount;
            }
        }
    }
}