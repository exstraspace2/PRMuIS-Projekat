using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Projekat_mreze;

class Server
{

    private static List<Igrac> sviIgraci = new List<Igrac>();
    private static Dictionary<string, List<string>> redosledOdgovoraPoIgri = new Dictionary<string, List<string>>();
    private static int sledeciId = 1;
    

    static void Main()
    {
        Console.WriteLine("=== SERVER TV SLAGALICA ===\n");

        // UDP Socket
        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpSocket.Bind(new IPEndPoint(IPAddress.Any, 15000));
        Console.WriteLine("UDP socket na portu 15000");

        // TCP Socket
        Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        tcpSocket.Bind(new IPEndPoint(IPAddress.Any, 50005));
        tcpSocket.Listen(2);
        Console.WriteLine("TCP socket na portu 50005\n");

        while (true)
        {

            byte[] udpBuffer = new byte[1024];
            EndPoint udpKlijentEP = new IPEndPoint(IPAddress.Any, 0);
            int udpBytes = udpSocket.ReceiveFrom(udpBuffer, ref udpKlijentEP);
            string prijava = Encoding.UTF8.GetString(udpBuffer, 0, udpBytes);

            Console.WriteLine($"Primio UDP od {udpKlijentEP}: {prijava}");

            if (prijava.StartsWith("PRIJAVA:"))
            {
                string odgovor = $"TCP:127.0.0.1:50005";
                byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                udpSocket.SendTo(odgovorBytes, udpKlijentEP);

                Socket tcpKlijent = tcpSocket.Accept();
                Console.WriteLine($"TCP konekcija od {tcpKlijent.RemoteEndPoint}");


                string[] delovi = prijava.Substring(8).Split(',');
                string ime = delovi[0].Trim();
                string pozdrav = $"Dobrodošli u trening igru kviza TV Slagalica, današnji takmičar je {ime}";
                byte[] pozdravBytes = Encoding.UTF8.GetBytes(pozdrav);
                tcpKlijent.Send(pozdravBytes);
                Console.WriteLine($"Poslat pozdrav: {pozdrav}");

                Console.WriteLine("Čekam SPREMAN od klijenta...");

                byte[] tcpBuffer = new byte[1024];

                try
                {

                    int tcpBytes = tcpKlijent.Receive(tcpBuffer);
                    string spreman = Encoding.UTF8.GetString(tcpBuffer, 0, tcpBytes);

                    Console.WriteLine($"Primio od klijenta: {spreman}");

                    if (spreman.ToUpper() == "SPREMAN")
                    {
                        Console.WriteLine($"{ime} je spreman!");

                        Igrac noviIgrac = new Igrac(sledeciId++, ime, new int[delovi.Length - 1]);
                        sviIgraci.Add(noviIgrac);


                        PokreniKvizZaJednogIgraca(tcpKlijent, noviIgrac, delovi);
                        PrikaziRezultate();

                        string potvrda = "Čekamo ostale igrače...";
                        tcpKlijent.Send(Encoding.UTF8.GetBytes(potvrda));
                        Console.WriteLine("Poslata potvrda klijentu");
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Greška: {ex.Message}");
                }

                Console.WriteLine($"Zatvaram konekciju sa {tcpKlijent.RemoteEndPoint}\n");
                tcpKlijent.Close();
            }
        }
    }

    static void PokreniKvizZaJednogIgraca(Socket socket, Igrac igrac, string[] delovi)
    {
        try
        {
            string[] igre = new string[delovi.Length - 1];
            for (int i = 1; i < delovi.Length; i++)
            {
                igre[i - 1] = delovi[i].Trim().ToLower();
            }

            Igra_Moj_Broj igraMojBroj = new Igra_Moj_Broj();
            Igra_Skocko igraSkocko = new Igra_Skocko();
            Igra_KoZnaZna igraKoZnaZna = new Igra_KoZnaZna();


            int skockoBrojac = 0;

            foreach (string igra in igre)
            {
                if (!redosledOdgovoraPoIgri.ContainsKey(igra))
                {
                    redosledOdgovoraPoIgri.Add(igra, new List<string>());
                }
            }

            for (int i = 0; i < igre.Length; i++)
            {
                string igra = igre[i];
                string pitanje = "";

                switch (igra)
                {
                    case "sl":
                        pitanje = $"MOJ BROJ - Traženi broj: {igraMojBroj.TRAZENI_broj}\n";
                        pitanje += $"Ponuđeni brojevi: {igraMojBroj.PONUDJENI_brojevi}\n";
                        pitanje += "Unesi izraz:";
                        break;
                    case "sk":
                        skockoBrojac++;
                        if (skockoBrojac > 1)
                        {
                            igraSkocko.GenerisiKombinaciju();
                        }
                        pitanje = $"SKOČKO {skockoBrojac} - Pogodite kombinaciju od 4 znaka (H,T,P,K,S,Z)\n";
                        pitanje += "Primer: HTPK\nUnesi kombinaciju:";
                        break;
                    case "kzz":
                        string pitanjeTekst = igraKoZnaZna.NasumicnoPitanje();
                        if (pitanjeTekst == null)
                        {
                            pitanje = "Nema dostupnih pitanja.";
                        }
                        else
                        {
                            pitanje = $"KO ZNA ZNA - {pitanjeTekst}\n";
                            pitanje += "Unesi broj tačnog odgovora:";
                        }
                        break;
                }

                if (string.IsNullOrEmpty(pitanje) || pitanje == "Nema dostupnih pitanja.")
                {
                    socket.Send(Encoding.UTF8.GetBytes("Greška: nema pitanja za ovu igru."));
                    continue;
                }

                socket.Send(Encoding.UTF8.GetBytes(pitanje));

                string odgovor = null; 

                if (igra != "sk")
                {
                byte[] buffer = new byte[1024];
                int bytes = socket.Receive(buffer);
                odgovor = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                }

                int poeni = 0;
                switch (igra)
                {
                    case "sl":
                        poeni = igraMojBroj.Proveri_Izraz(odgovor);
                        break;
                    case "sk":
                        {
                         bool pogodjeno = false;
                         while (!pogodjeno && igraSkocko.BrojPokusaja < 6)
                            {
                             byte[] bufferSk = new byte[1024];
                             int bytesSk = socket.Receive(bufferSk);
                             string pokusaj = Encoding.UTF8.GetString(bufferSk, 0, bytesSk).Trim().ToUpper();
                             string rezultat = igraSkocko.ProveriKombinaciju(pokusaj);
                             socket.Send(Encoding.UTF8.GetBytes(rezultat));
                                if (igraSkocko.DaLiJePogodio(pokusaj))
                                {
                                 poeni = igraSkocko.DodeliPoene();
                                 socket.Send(Encoding.UTF8.GetBytes("POGODAK!"));
                                 pogodjeno = true;
                                }
                                else if (igraSkocko.BrojPokusaja >= 6)
                                {
                                socket.Send(Encoding.UTF8.GetBytes(
                                $"Netacno! Pravilna kombinacija je: {igraSkocko.TrazenaKombinacija}"
                                 ));
                                 break;
                                }
                            }
                            break;
                        }
                    case "kzz":
                        if (int.TryParse(odgovor, out int broj))
                        {
                            bool tacan = igraKoZnaZna.ProveriTacanOdgovor(broj);
                            poeni = tacan ? 10 : -5;
                        }
                        break;
                }

                if (poeni > 0 && igra != "sk")
                {
                    if (!redosledOdgovoraPoIgri[igra].Contains(igrac.IME_igraca))
                        redosledOdgovoraPoIgri[igra].Add(igrac.IME_igraca);

                    int redniBroj = redosledOdgovoraPoIgri[igra].IndexOf(igrac.IME_igraca) + 1;

                    for (int j = 1; j < redniBroj; j++)
                    {
                        poeni = (int)(poeni * 0.95);
                    }
                }

                igrac.POENI_igraca[i] += poeni;

                socket.Send(Encoding.UTF8.GetBytes($"Dobili ste {poeni} poena za {igra}"));
            }

            socket.Send(Encoding.UTF8.GetBytes("KRAJ_KVIZA"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška u kvizu: {ex.Message}");
        }
    }
    static void PrikaziRezultate()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("REZULTATI KVIZA");
        Console.WriteLine("==================================================");

        if (sviIgraci.Count == 0)
        {
            Console.WriteLine("Nema igraca.");
            return;
        }

        foreach (var igrac in sviIgraci)
        {
            Console.WriteLine($"\n{igrac.IME_igraca}:");
            int ukupno = 0;
            for (int i = 0; i < igrac.POENI_igraca.Length; i++)
            {
                ukupno += igrac.POENI_igraca[i];
                Console.WriteLine($"  - {igrac.POENI_igraca[i]} poena");
            }
            Console.WriteLine($"  UKUPNO: {ukupno} poena");
        }
        Console.WriteLine("==================================================");

        if (sviIgraci.Count == 1)
        {
            Console.WriteLine($"POBEDNIK: {sviIgraci[0].IME_igraca} (jedini igrac)");
        }
        else
        {
            var sortirani = sviIgraci.OrderByDescending(i => i.POENI_igraca.Sum()).ToList();
            Igrac prvi = sortirani[0];
            Igrac drugi = sortirani[1];

            if (prvi.POENI_igraca.Sum() == drugi.POENI_igraca.Sum())
            {
                Console.WriteLine("NERESENO (isti broj poena)");
            }
            else
            {
                Console.WriteLine($"POBEDNIK: {prvi.IME_igraca} ({prvi.POENI_igraca.Sum()} poena)");
            }
        }

        Console.WriteLine("==================================================");
    }
}