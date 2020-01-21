using System;

namespace BasketLeague.Models
{
    class Team
    {
        public string Nombre { get; set; }
        public int Ataque { get; set; }
        public int Defensa { get; set; }
        public int Tiro { get; set; }
        public int Rebote { get; set; }

        public int Atacar(Team rival, Random rnd)
        {
            return 50 + (rnd.Next(5, 10) * Ataque + rnd.Next(5, 10) * Tiro) - (rnd.Next(1, 5) * rival.Defensa + rnd.Next(1, 5) * rival.Rebote);
        }

        public int Defender(Team rival, Random rnd)
        {
            return 20 + (rnd.Next(5, 10) * Defensa + rnd.Next(5, 10) * Rebote) - (rnd.Next(1, 5) * rival.Ataque + rnd.Next(1, 5) * rival.Tiro);
        }

        public int Resultado(int defender, int atacar)
        {
            Random rnd = new Random();
            int result;
            if (defender < atacar)
            {
                result = rnd.Next(defender, atacar);
            }
            else
            {
                result = rnd.Next(atacar, defender);
            }
            return result;
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
