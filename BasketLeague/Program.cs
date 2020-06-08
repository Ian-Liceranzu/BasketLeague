﻿using BasketLeague.Models;
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

            try
            {
                List<Team> teams = CargarEquipos();

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

                int ha = home.Atacar(rival, rnd);
                int hd = home.Defender(rival, rnd);

                int ra = rival.Atacar(home, rnd);
                int rd = rival.Defender(home, rnd);

                Console.WriteLine("----------");
                if (ha < hd && ra < rd) // En caso de dos equipos muy defensivos, el resultado se divide entre 2
                {
                    Console.WriteLine(string.Format("{0}: {1}", home.NombreCompleto, home.Resultado(hd, ha) / 2));
                    Console.WriteLine(string.Format("{0}: {1}", rival.NombreCompleto, rival.Resultado(rd, ra) / 2));
                }
                else
                {
                    Console.WriteLine(string.Format("{0}: {1}", home.NombreCompleto, home.Resultado(hd, ha)));
                    Console.WriteLine(string.Format("{0}: {1}", rival.NombreCompleto, rival.Resultado(rd, ra)));
                }
                Console.WriteLine("----------");
            }
            finally
            {
                Console.WriteLine("Programa finalizado");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Carga los equipos del archivo Teams.txt
        /// </summary>
        /// <returns>Lista de los equipos cargados</returns>
        static List<Team> CargarEquipos()
        {
            string path = @"Data\Teams.txt";

            List<Team> teams = new List<Team>();

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
                    NombreCompleto = line.Split(' ')[5].Replace('_', ' ')
                };

                line = sr.ReadLine();

                teams.Add(team);
            }
            sr.Close();

            return teams;
        }
    }
}
