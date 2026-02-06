using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_mreze
{
    public class Igra_Moj_Broj
    {
        public int TRAZENI_broj { get; set; }
        public string PONUDJENI_brojevi { get; set; }

        public string REZULTATI { get; set; }

        private Random nasumicni = new Random();
        public Igra_Moj_Broj()
        {
            TRAZENI_broj = nasumicni.Next(100, 999);
            Generisi_Brojeve();
        }
        public Igra_Moj_Broj(int tRAZENI_broj, string pONUDJENI_brojevi, string rEZULTATI)
        {
            TRAZENI_broj = tRAZENI_broj;
            PONUDJENI_brojevi = pONUDJENI_brojevi;
            REZULTATI = rEZULTATI;
        }

        public void Generisi_Brojeve()
        {
            List<int> Dati_brojevi = new List<int>();

            Dati_brojevi.Add(1);

            for (int i = 0; i < 3; i++)
            {
                Dati_brojevi.Add(nasumicni.Next(0, 10));
            }

            int[] dvocifreni1 = { 10, 15, 15, 20 };
            int[] dvocifreni2 = { 20, 50, 75, 100 };

            Dati_brojevi.Add(dvocifreni1[nasumicni.Next(dvocifreni1.Length)]);
            Dati_brojevi.Add(dvocifreni2[nasumicni.Next(dvocifreni2.Length)]);

            PONUDJENI_brojevi = string.Join(" ", Dati_brojevi);

        }

        public int Proveri_Izraz(string Izraz_za_proveru){

            int rezultati = 0;
            REZULTATI = Izraz_za_proveru;

            try
            {
                int index = 0;
                rezultati = Parsiranje1(Izraz_za_proveru.Replace(" ", ""), ref index);
            }
            catch
            {
                return 0;
            }
            if(rezultati < 0)
            {
                return 0;
            }

            int razlika_abs = Math.Abs(TRAZENI_broj - rezultati);

            if (razlika_abs == 0)
            {
                return 10;
            }else if(razlika_abs <= 50)
            {
                return 7;
            }else if(razlika_abs <= 100)
            {
                return 5;
            }

            return 3;
        }

        public int Parsiranje1(string Za_proveru, ref int indexsi)
        {
            int value1 = Parsiranje2(Za_proveru, ref indexsi);

            while (indexsi < Za_proveru.Length)
            {
                if (Za_proveru[indexsi] == '+')
                {
                    indexsi++;
                    value1 += Parsiranje2(Za_proveru, ref indexsi);
                }
                else if (Za_proveru[indexsi] == '-')
                {
                    indexsi++;
                    value1 -= Parsiranje2(Za_proveru, ref indexsi);
                }
                else
                {
                    break;
                }
            }
            return value1;

        }

        public int Parsiranje2(string Za_proveru, ref int indexsi)
        {
            int value2 = Parsiranje3(Za_proveru, ref indexsi);

            while (indexsi < Za_proveru.Length)
            {
                if (Za_proveru[indexsi] == '*')
                {
                    indexsi++;
                    value2 *= Parsiranje3(Za_proveru, ref indexsi);
                }
                else if (Za_proveru[indexsi] == '/')
                {
                    indexsi++;
                    int delilac = Parsiranje3(Za_proveru, ref indexsi);
                    if (delilac == 0)
                    {
                        throw new Exception("Deljenje sa nulom nije dozvoljeno!!!");
                    }
                    value2 /= delilac;
                }
                else {
                    break;
                }
            }

            return value2;
        }

        public int Parsiranje3(string Za_proveru, ref int indexsi)
        {
            if (Za_proveru[indexsi] == '(')
            {
                indexsi++;
                int value3 = Parsiranje1(Za_proveru,ref indexsi);
                indexsi++;
                return value3;
            }
            int start = indexsi;

            while(indexsi < Za_proveru.Length && char.IsDigit(Za_proveru[indexsi]))
            {
                indexsi++;
            }

            if(start== indexsi)
            {
                throw new Exception("Broj nije dat!!!");
            }

            return int.Parse(Za_proveru.Substring(start, indexsi- start));
        }

    }
}
