using BasketLeague.Models;
using NAudio.Wave;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Security.Cryptography;
using System.Text;

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
                }
                else
                {
                    hr = home.Resultado(hd, ha);
                    rr = rival.Resultado(rd, ra);
                }

                Console.WriteLine(string.Format("{0}: {1}", home.NombreCompleto, hr));
                Console.WriteLine(string.Format("{0}: {1}", rival.NombreCompleto, rr));

                if (WriteDataToFile(home, hr, rival, rr))
                {
                    Tweet(string.Format("{0} - {1}  {2} - {3}", home.NombreCompleto, hr, rival.NombreCompleto, rr));
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
        static bool WriteDataToFile(Team home, int resultadoHome, Team rival, int resultadoRival)
        {
            bool correct = false;

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

                correct = true;
            }
            else
            {
                Console.WriteLine("Felicidades " + rival.Dueño);
                PlaySound(rival.Song);

                correct = true;
            }

            excelWorksheet.Cells["R" + row].Value = diferencia;

            ModificarTabla(excelWorksheet, home, diferencia);
            ModificarTabla(excelWorksheet, rival, diferencia * -1);

            excelPackage.Save();

            return correct;
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

        static void Tweet(string message)
        {
            // Application tokens
            const string CONSUMER_KEY = "GRpdf4ggSQVFaE9BuYRqR14je";
            const string CONSUMER_SECRET = "zZHrgADb1HaIuN3etk45tHJIKO4DHEeRxPIhJfAKVI3jsKJa1g";
            // Access tokens
            const string ACCESS_TOKEN = "1292783792856473601-xpmkThqVZW5gbDKofgHji75pNlHcoo";
            const string ACCESS_TOKEN_SECRET = "rZq3GUbFdGy7bN0ihy9oD0aCOzu6T2oRHBJ4ejTnz3h0A";

            string twitterURL = "https://api.twitter.com/1.1/statuses/update.json";

            // set the oauth version and signature method
            string oauth_version = "1.0";
            string oauth_signature_method = "HMAC-SHA1";

            // create unique request details
            string oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            System.TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            string oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            // create oauth signature
            string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" + "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

            string baseString = string.Format(
                baseFormat,
                CONSUMER_KEY,
                oauth_nonce,
                oauth_signature_method,
                oauth_timestamp, ACCESS_TOKEN,
                oauth_version,
                Uri.EscapeDataString(message)
            );

            string oauth_signature = null;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(CONSUMER_SECRET) + "&" + Uri.EscapeDataString(ACCESS_TOKEN_SECRET))))
            {
                oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(twitterURL) + "&" + Uri.EscapeDataString(baseString))));
            }

            // create the request header
            string authorizationFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", " + "oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " + "oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

            string authorizationHeader = string.Format(
                authorizationFormat,
                Uri.EscapeDataString(CONSUMER_KEY),
                Uri.EscapeDataString(oauth_nonce),
                Uri.EscapeDataString(oauth_signature),
                Uri.EscapeDataString(oauth_signature_method),
                Uri.EscapeDataString(oauth_timestamp),
                Uri.EscapeDataString(ACCESS_TOKEN),
                Uri.EscapeDataString(oauth_version)
            );

            HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(twitterURL);
            objHttpWebRequest.Headers.Add("Authorization", authorizationHeader);
            objHttpWebRequest.Method = "POST";
            objHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            using (Stream objStream = objHttpWebRequest.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes("status=" + Uri.EscapeDataString(message));
                objStream.Write(content, 0, content.Length);
            }

            var responseResult = "";

            try
            {
                //success posting
                WebResponse objWebResponse = objHttpWebRequest.GetResponse();
                StreamReader objStreamReader = new StreamReader(objWebResponse.GetResponseStream());
                responseResult = objStreamReader.ReadToEnd().ToString();
            }
            catch (Exception ex)
            {
                responseResult = "Twitter Post Error: " + ex.Message.ToString() + ", authHeader: " + authorizationHeader;
            }
        }
    }
}
