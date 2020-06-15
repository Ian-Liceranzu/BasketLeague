using BasketLeague.Models;
using NAudio.Wave;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;

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

                    Console.WriteLine(string.Format("{0}: {1}", home.NombreCompleto, hr));
                    Console.WriteLine(string.Format("{0}: {1}", rival.NombreCompleto, rr));

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
            // string path = @"Data\TeamsSummerCamp.txt";

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
                    Codigo = int.Parse(line.Split(' ')[6]),
                    Dueño = line.Split(' ')[7],
                    Song = line.Split(' ')[8]
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

            FileInfo file = new FileInfo(@"Data\FrikiLeague.xlsx");
            ExcelPackage excelPackage = new ExcelPackage(file);
            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets[0];

            int row = excelWorksheet.Cells.First(c => c.Value == null && c.Start.Column == 14).Start.Row;

            excelWorksheet.Cells["N" + row].Value = home.Nombre;
            excelWorksheet.Cells["P" + row].Value = resultadoHome;

            excelWorksheet.Cells["O" + row].Value = rival.Nombre;
            excelWorksheet.Cells["Q" + row].Value = resultadoRival;

            int diferencia = resultadoHome - resultadoRival;

            if (diferencia == 0)
            {
                throw new Exception("Han empatado");
            }
            else if (diferencia > 0)
            {
                Console.WriteLine("Felicidades " + home.Dueño);
                PlaySound(home.Song);
            }
            else
            {
                Console.WriteLine("Felicidades " + rival.Dueño);
                PlaySound(home.Song);
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

            var rowNum = excelWorksheet.Cells[cellRange].First(c => c.Value?.ToString() == team.NombreCompleto).Start.Row;

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

        /// <summary>
        /// Reproduce un clip dentro de la carpeta data
        /// </summary>
        /// <param name="song">Archivo concreto para reproducir</param>
        static void PlaySound(string song)
        {
            if (song.Split('.')[song.Split().Length - 1] == "mp3")
            {
                SoundPlayer sp = new SoundPlayer();
                sp.SoundLocation = Environment.CurrentDirectory + @"\Data\Sound\" + song;
                sp.Play();
            }

            else
            {
                var wave = new WaveOut();
                var x = new AudioFileReader(Environment.CurrentDirectory + @"\Data\Sound\" + song);
                wave.Init(x);
                wave.Play();
            }

            int tempo = 0;
            do
            {
                System.Threading.Thread.Sleep(1000);
                tempo++;
            } while (tempo < 10);
        }
    }
}
