using BasketLeague.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace BasketLeague
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bienvenido a la competición");

            string path = @"Data\Teams.txt";

            List<Team> teams = new List<Team>();

            try
            {
                StreamReader sr = new StreamReader(@path);

                var line = sr.ReadLine();
                while (line != null)
                {
                    Team team = new Team()
                    {
                        Nombre = line.Split(' ')[0],
                        Ataque = int.Parse(line.Split(' ')[1]),
                        Defensa = int.Parse(line.Split(' ')[2]),
                        Tiro = int.Parse(line.Split(' ')[3]),
                        Rebote = int.Parse(line.Split(' ')[4]),

                    };

                    line = sr.ReadLine();

                    teams.Add(team);
                }
                sr.Close();

                Console.WriteLine("Equipos disponibles:");

                for (int i = 0; i < teams.Count; i++)
                {
                    Console.WriteLine(i + " - " + teams[i].ToString());
                }

                Console.WriteLine("----------");

                Console.WriteLine("Equipo que juega en casa:");
                Team home = teams[int.Parse(Console.ReadLine())];

                Console.WriteLine("Equipo contrario:");
                Team rival = teams[int.Parse(Console.ReadLine())];

                Random rnd = new Random();

                for (int i = 0; i < 10; i++)
                {
                    int ha = home.Atacar(rival, rnd);
                    int hd = home.Defender(rival, rnd);

                    int ra = rival.Atacar(home, rnd);
                    int rd = rival.Defender(home, rnd);

                    if (ha < hd && ra < rd)
                    {
                        Console.WriteLine(string.Format("Home: {0}", home.Resultado(hd, ha) / 4 * 3));
                        Console.WriteLine(string.Format("Rival: {0}", rival.Resultado(rd, ra) / 4 * 3));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Home: {0}", home.Resultado(hd, ha)));
                        Console.WriteLine(string.Format("Rival: {0}", rival.Resultado(rd, ra)));
                    }
                    Console.WriteLine("----------");
                }
            }
            finally
            {
                Console.WriteLine("Programa finalizado");
                Console.ReadLine();
            }
        }
    }
}
