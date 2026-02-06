using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_mreze
{
    internal class Igra_KoZnaZna
    {
        public string TekucePitanje {  get; set; }
        public int TacanOdgovor {  get; set; }
        public Dictionary<string, int> SvaPitanja {  get; set; }
        private List<string> iskoriscenaPitanja = new List<string>();
        private Random random = new Random();


        public Igra_KoZnaZna()
        {

        }

        public void UcitajPitanja()
        {
            SvaPitanja = new Dictionary<string, int>
            {
                { "Koje godine je pao Berlinski zid?\n1-1961\n2-1989\n3-Kad god da je objavljen film 'Atomic Blonde'", 2 },
                { "Koji je glavni grad Australije?\n1-Sidnej\n2-Melburn\n3-Kanbera", 3 },
                { "Koliko kontinenata ima na Zemlji?\n1-5\n2-6\n3-7", 3 },
                { "U kom gradu se nalazi Ajfelov toranj?\n1-London\n2-Berlin\n3-Pariz", 3 },
                { "U kom sportu se koristi termin 'as'?\n1-Košarka\n2-Tenis\n3-Fudbal", 2 },
                { "Koja država je domaćin Olimpijskih igara 2016?\n1-Kina\n2-Velika Britanija\n3-Brazil", 3 },
                { "Ko je autor romana 'Na Drini ćuprija'?\n1-Ivo Andrić\n2-Fjodor Dostojevski\n3-Lav Tolstoj", 1 },
                { "Koji je glavni grad Australije?\n1-Sidnej\n2-Melburn\n3-Kanbera", 3 },
                { "Ko je naslikao 'Mona Lisu'?\n1-Mikelanđelo\n2-Leonardo da Vinči\n3-Rafael", 2 },
                { "Ko je režirao film 'Titanik'?\n1-Stiven Spilberg\n2-Džejms Kameron\n3-Ridli Skot", 2 }
            };
        }

        public string NasumicnoPitanje()
        {
            if (iskoriscenaPitanja.Count >= SvaPitanja.Count)
                iskoriscenaPitanja.Clear();

            var dostupna = SvaPitanja.Keys.Where(p => !iskoriscenaPitanja.Contains(p)).ToList();

            if (dostupna.Count == 0)
                return null;

            TekucePitanje = dostupna[random.Next(dostupna.Count)];
            TacanOdgovor = SvaPitanja[TekucePitanje];
            iskoriscenaPitanja.Add(TekucePitanje);

            return TekucePitanje;
        }

        public bool ProveriTacanOdgovor(int odgovor)
        {
            return odgovor == TacanOdgovor;
        }

        public int DodeliPoene(bool tacan)
        {
            int poeni;

            if (tacan == true)
                poeni = 10;
            else
                poeni = -5;

            return poeni;
        }

    }
}
