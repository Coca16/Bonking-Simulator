using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Newtonsoft.Json;
using static Shared.Shared.Person.DbFields;

namespace Shared
{
    public class Shared
    {
        private static MySqlConnection sqlconnection { get; set; }
        private static readonly HashSet<char> vowels = new() {'a', 'e', 'i', 'o', 'u'};
        
        private static readonly string[] horny_people =
        {
            "djz", "coolio", "blue", "blueviper", "jam", "jambam", "jambon", "bmk", "josua", "gumbo", "taz", "devin",
            "caeser", "spike", "spikeviper", "mellow"
        };

        public static async Task BeginMySQL()
        {
            string json = string.Empty;

            using (FileStream fs = File.OpenRead("secret.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            SecretJson secret = JsonConvert.DeserializeObject<SecretJson>(json);

            string cs =
                @$"server={secret.Server};userid={secret.UserID};password={secret.Password};database={secret.Database}";
            sqlconnection = new MySqlConnection(cs);
            sqlconnection.Open();
        }

        public static async Task Add(string name)
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

            string stm = $"INSERT INTO people({Name}, {Horny}) VALUES(@{Name}, {horniness - 50});";
            MySqlCommand cmd = new(stm, sqlconnection);
            cmd.Parameters.AddWithValue($"@{Name}", name);
            Console.WriteLine($"{cmd.ExecuteNonQuery()} rows where affected in the database!");
        }

        public static List<Person> SearchPeople( string name, AliveOrDead requested, AliveOrDead deathexclusivity = AliveOrDead.Both)
        {
            string deathStm = deathexclusivity switch
            {
                AliveOrDead.Both => "",
                AliveOrDead.Alive => "AND {Dead} = FALSE",
                AliveOrDead.Dead => "AND {Dead} = TRUE",
                _ => throw new ArgumentOutOfRangeException(nameof(deathexclusivity), deathexclusivity, null)
            };

            string stm = $"SELECT * FROM people WHERE {Name} = @{Name} {deathStm};";
            MySqlCommand cmd = new(stm, sqlconnection);
            cmd.Parameters.AddWithValue($"@{Name}", name);
            MySqlDataReader reader = cmd.ExecuteReader();
            
            List<Person> people = new();
            while (reader.Read())
            {
                uint id = reader.GetUInt32(0);
                ulong? discordId;
                try { discordId = reader.GetUInt64(1); }
                catch (System.Data.SqlTypes.SqlNullValueException) { discordId = null; }
                MySqlDateTime added = reader.GetMySqlDateTime(2);
                MySqlDateTime? latest;
                try { latest = reader.GetMySqlDateTime(3); }
                catch (System.Data.SqlTypes.SqlNullValueException) { latest = null; }
                int horny = reader.GetInt16(5);
                int bonked = reader.GetInt16(6);
                bool dead = reader.GetBoolean(7);
                
                people.Add(new Person(id, added, latest, name, horny, bonked, dead, discordId));
            }
            
            reader.Close();
            Console.WriteLine(PeopleToFullTable(people));
            return people;
        }
        
        public enum AliveOrDead
        {
            Both,
            Alive,
            Dead
        }
        
        public static void SavePerson( Person person )
        {
            person.BonkedCount++;
            string save =
                $"UPDATE people SET {Horny} = @{Horny}, {LatestBonk} = CURRENT_TIMESTAMP, {BonkedCount} = @{BonkedCount}, {Dead} = {person.Dead.ToString().ToUpper()} WHERE {Id} = @{Id};";
            MySqlCommand cmdSave = new(save, sqlconnection);
            cmdSave.Parameters.AddWithValue($"@{Horny}", person.Horny);
            cmdSave.Parameters.AddWithValue($"@{BonkedCount}", person.BonkedCount);
            cmdSave.Parameters.AddWithValue($"@{Id}", person.Id);
            Console.WriteLine($"{cmdSave.ExecuteNonQuery()} rows where affected in the database!");
        }
        
        public static string PeopleToSearchTable( List<Person> people )
        {
            return people.ToStringTable(
                new[] {"", "Added At", "Latest Bonk", "Times Bonked", "Id (Count In Table)"},
                p => people.FindIndex(person => person == p) + 1,
                p => p.Added,
                p => p.BonkedCount.Equals(0) ? "Never Bonked" : p.LatestBonk,
                p => p.BonkedCount,
                p => p.Id
            );
        }
        
        public static string PeopleToFullTable( List<Person> people )
        {
            return people.ToStringTable(
                new[] {"", "Name", "Id (Count In Table)", "Discord Id", "Added", "Latest Bonk", "Times Bonked", "Health"},
                p => people.FindIndex(person => person == p) + 1,
                p => p.Name,
                p => p.Id,
                p => p.DiscordId,
                p => p.Added,
                p => p.BonkedCount.Equals(0) ? "Never Bonked" : p.LatestBonk,
                p => p.BonkedCount,
                p => p.Dead ? "Dead" : "Alive"
            );
        }

        public class Person
        {
            public uint Id { get; }
            public string Name { get; }
            public ulong? DiscordId { get; }
            public MySqlDateTime Added { get; }
            public MySqlDateTime? LatestBonk { get; }
            public int Horny { get; set; }
            public int BonkedCount { get; set; }
            public bool Dead { get; set; }
            
            public class DbFields
            {
                public const string Id = "person_id";
                public const string DiscordId = "discord_id";
                public const string Added = "added";
                public const string LatestBonk = "latest_bonk";
                public const string Name = "name";
                public const string Horny = "horniness";
                public const string BonkedCount = "bonked";
                public const string Dead = "dead";
            }
            
            public Person(uint id, MySqlDateTime added, MySqlDateTime? latestBonk, string name, 
                int horny, int bonkedCount, bool dead = false, ulong? discord = null)
            {
            Id = id;
            DiscordId = discord;
            Added = added;
            LatestBonk = latestBonk;
            Name = name;
            Horny = horny;
            BonkedCount = bonkedCount;
            Dead = dead;
            }
        }

        public static void Populate(int amount)
        {
            Random rand = new(); // we need a random variable to select names randomly
            RandomName nameGen = new RandomName(rand); // create a new instance of the RandomName class
            List<string> names = nameGen.RandomNames(amount, 0);
            
            foreach (string name in names)
            {
                Random rnd = new(name.GetHashCode());
                int horniness;
                if (horny_people.Contains(name.ToLower())) { horniness = rnd.Next(70, 100); }
                else
                {
                    int t_vowels = name.Count(c => vowels.Contains(c));
                    horniness = name.Length + t_vowels + rnd.Next(0, 10);
                    horniness *= 2;
                }

                string stm = $"INSERT INTO people(name, horniness) VALUES(@name, {horniness - 50});";
                MySqlCommand cmd = new MySqlCommand(stm, sqlconnection);
                cmd.Parameters.AddWithValue("@name", name);
                Console.WriteLine($"{cmd.ExecuteNonQuery()} rows where affected in the database!");
            }
        }
    }
}