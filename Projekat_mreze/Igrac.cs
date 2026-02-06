using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_mreze
{
    public class Igrac
    {
        public int ID_igraca { get; set; }
        public string IME_igraca { get; set; }
        public int[] POENI_igraca { get; set; }

        public Igrac() { }

        public Igrac(int iD_igraca, string iME_igraca, int[] pOENI_igraca)
        {
            ID_igraca = iD_igraca;
            IME_igraca = iME_igraca;
            POENI_igraca = pOENI_igraca;
        }
    }
}
