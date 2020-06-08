﻿using BasketLeague.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                int hr;
                int rr;
                if (ha < hd && ra < rd) // En caso de dos equipos muy defensivos, el resultado se divide entre 2
                {
                    hr = home.Resultado(hd, ha) / 2;
                    rr = rival.Resultado(hd, ha) / 2;

                    Console.WriteLine(string.Format("{0}: {1}", home.NombreCompleto, hr));
                    Console.WriteLine(string.Format("{0}: {1}", rival.NombreCompleto, rr));

                    WriteDataToFile(home, hr, rival, rr);
                }
                else
                {
                    hr = home.Resultado(hd, ha);
                    rr = rival.Resultado(rd, ra);

                    Console.WriteLine(string.Format("{0}: {1}", home.NombreCompleto, home.Resultado(hd, ha)));
                    Console.WriteLine(string.Format("{0}: {1}", rival.NombreCompleto, rival.Resultado(rd, ra)));

                    WriteDataToFile(home, hr, rival, rr);
                }
                Console.WriteLine("----------");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(string.Format("ERROR: {0}", e.Message));
                Console.ResetColor();
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
                    NombreCompleto = line.Split(' ')[5].Replace('_', ' '),
                    Codigo = int.Parse(line.Split(' ')[6])
                };

                line = sr.ReadLine();

                teams.Add(team);
            }
            sr.Close();

            return teams;
        }

        /// <summary>
        /// Escribe el resultado del partido en la zona de resultados
        /// </summary>
        /// <param name="home">Equipo que ha jugado en casa</param>
        /// <param name="resultadoHome">Puntos anotados por el equipo de casa</param>
        /// <param name="rival">Equipo que ha jugado fuera de casa</param>
        /// <param name="resultadoRival">Puntos anotados por el equipo de fuera</param>
        static void WriteDataToFile(Team home, int resultadoHome, Team rival, int resultadoRival)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            FileInfo file = new FileInfo(@"C:\Users\IanLiceranzu\Desktop\FrikiLeague\FrikiLeague_v2.xlsx");
            ExcelPackage excelPackage = new ExcelPackage(file);
            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets[0];

            int row = excelWorksheet.Cells.First(c => c.Value == null && c.Start.Column == 14).End.Row;

            excelWorksheet.Cells["N" + row].Value = home.Nombre;
            excelWorksheet.Cells["P" + row].Value = resultadoHome;

            excelWorksheet.Cells["O" + row].Value = rival.Nombre;
            excelWorksheet.Cells["Q" + row].Value = resultadoRival;

            int diferencia = resultadoHome - resultadoRival;

            if (diferencia == 0)
            {
                throw new Exception("Han empatado");
            }

            excelWorksheet.Cells["R" + row].Value = diferencia;

            ModificarTabla(excelWorksheet, home, diferencia);
            ModificarTabla(excelWorksheet, rival, diferencia * -1);

            excelPackage.Save();
        }

        /// <summary>
        /// Modifica la tabla de las clasificaciones con los resultados
        /// </summary>
        /// <param name="excelWorksheet">Excel</param>
        /// <param name="team">Equipo a modificar</param>
        /// <param name="diferencia">Diferencia en el resultado del partido</param>
        static void ModificarTabla(ExcelWorksheet excelWorksheet, Team team, int diferencia)
        {
            string cellRange = "B2:B12"; // Tabla de los equipos

            var searchCell = from cell in excelWorksheet.Cells[cellRange]
                             where cell.Value?.ToString() == team.NombreCompleto
                             select cell.Start.Row;

            int rowNum = searchCell.First();

            // Sumar partidos jugados
            excelWorksheet.Cells["C" + rowNum].Value = int.Parse(excelWorksheet.Cells["C" + rowNum].Value.ToString()) + 1;
            if (diferencia > 0)
            {
                // Sumar partidos ganados
                excelWorksheet.Cells["D" + rowNum].Value = int.Parse(excelWorksheet.Cells["D" + rowNum].Value.ToString()) + 1;
            }
            else
            {
                // Sumar partidos perdidos
                excelWorksheet.Cells["E" + rowNum].Value = int.Parse(excelWorksheet.Cells["E" + rowNum].Value.ToString()) + 1;
            }

            // Calcular % victorias sobre 1
            excelWorksheet.Cells["F" + rowNum].Value = float.Parse(excelWorksheet.Cells["D" + rowNum].Value.ToString()) / float.Parse(excelWorksheet.Cells["C" + rowNum].Value.ToString());
            // Calcular nueva diferencia
            excelWorksheet.Cells["G" + rowNum].Value = int.Parse(excelWorksheet.Cells["G" + rowNum].Value.ToString()) + diferencia;
        }
    }
}
