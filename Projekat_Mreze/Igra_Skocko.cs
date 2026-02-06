using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_mreze
{
    internal class Igra_Skocko
    {
        private char[] simboli = { 'H', 'T', 'P', 'K', 'S', 'Z' };
        public string TrazenaKombinacija { get; set; }
        public int BrojPokusaja { get; set; }

        private Random random = new Random();

        public Igra_Skocko()
        {
            GenerisiKombinaciju();
            BrojPokusaja = 0;
        }

        public void GenerisiKombinaciju()
        {
            TrazenaKombinacija = "";

            for (int i = 0; i < 4; i++)
            {
                TrazenaKombinacija += simboli[random.Next(simboli.Length)];
            }
        }

        public string ProveriKombinaciju(string pokusaj)
        {
            BrojPokusaja++;

            if (pokusaj.Length != 4)
                return "Kombinacija mora imati 4 znaka!";

            int naMestu = 0;
            int pogodjeno = 0;

            var trazenaList = TrazenaKombinacija.ToList();
            var pokusajList = pokusaj.ToList();

            // Provera tačnih pozicija
            for (int i = 0; i < 4; i++)
            {
                if (pokusaj[i] == TrazenaKombinacija[i])
                {
                    naMestu++;
                    trazenaList.Remove(pokusaj[i]);
                    pokusajList.Remove(pokusaj[i]);
                }
            }

            // Provera pogodjenih ali ne na mestu
            foreach (var c in pokusajList)
            {
                if (trazenaList.Contains(c))
                {
                    pogodjeno++;
                    trazenaList.Remove(c);
                }
            }

            int nijeUPokusaju = 4 - (naMestu + pogodjeno);

            return $"{naMestu} znakova je na pravom mestu, {pogodjeno} znakova je deo kombinacije ali nije na pravom mestu";
        }


        public int DodeliPoene()
        {
            switch (BrojPokusaja)
            {
                case 1: return 30;
                case 2: return 25;
                case 3: return 20;
                case 4: return 15;
                case 5: return 10;
                case 6: return 10;
                default: return 0;
            }
        }

        public bool DaLiJePogodio(string pokusaj)
        {
            return pokusaj == TrazenaKombinacija;
        }

    }
}

