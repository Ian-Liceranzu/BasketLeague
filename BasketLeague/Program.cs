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

            Console.WriteLine("Introduce la ruta del archivo con los equipos");

            string path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
            {
                path = @"C:\Users\PcCom\Desktop\Prueba.txt";
            }

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
                        Rebote = int.Parse(line.Split(' ')[3]),
                        Tiro = int.Parse(line.Split(' ')[4])
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

                int ha = home.Atacar(rival);
                int hd = home.Defender(rival);
                int ra = rival.Atacar(home);
                int rd = rival.Defender(home);

                Console.WriteLine(string.Format("Home: {0} ({1} - {2})", home.Resultado(hd, ha), hd, ha));
                Console.WriteLine(string.Format("Rival: {0} ({1} - {2})", rival.Resultado(rd, ra), rd, ra));
            }
            finally
            {
                Console.WriteLine("Programa finalizado");
                Console.ReadLine();
            }
        }
    }
}
