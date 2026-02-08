using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Klient
{
    static void Main()
    {
        Console.WriteLine("=== KLIJENT TV SLAGALICA ===\n");

        try
        {
            Console.Write("Unesi ime: ");
            string ime = Console.ReadLine();

            Console.Write("Unesi igre (sl,sk,kzz): ");
            string igre = Console.ReadLine();

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string prijava = $"PRIJAVA:{ime},{igre}";
            byte[] prijavaBytes = Encoding.UTF8.GetBytes(prijava);

            EndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            udpSocket.SendTo(prijavaBytes, serverEP);
            Console.WriteLine("Poslata prijava\n");

            byte[] odgovorBuffer = new byte[1024];
            EndPoint odgovorEP = new IPEndPoint(IPAddress.Any, 0);
            int odgovorBytes = udpSocket.ReceiveFrom(odgovorBuffer, ref odgovorEP);
            string odgovor = Encoding.UTF8.GetString(odgovorBuffer, 0, odgovorBytes);
            Console.WriteLine($"Odgovor servera: {odgovor}");

            string[] tcpInfo = odgovor.Split(':');
            string tcpIP = tcpInfo[1];
            int tcpPort = int.Parse(tcpInfo[2]);

            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(tcpIP), tcpPort));
            Console.WriteLine($"Povezan na TCP server {tcpIP}:{tcpPort}\n");

            byte[] pozdravBuffer = new byte[1024];
            int pozdravBytes = tcpSocket.Receive(pozdravBuffer);
            string pozdrav = Encoding.UTF8.GetString(pozdravBuffer, 0, pozdravBytes);
            Console.WriteLine($"Server: {pozdrav}\n");

            Console.Write("Unesi SPREMAN kada si spreman: ");
            string unos = Console.ReadLine();

            if (unos.ToUpper() == "SPREMAN")
            {
                try
                {
                    byte[] spremanBytes = Encoding.UTF8.GetBytes("SPREMAN");
                    tcpSocket.Send(spremanBytes);
                    Console.WriteLine("Poslao SPREMAN serveru");

                    PokreniKviz(tcpSocket);

                    byte[] potvrdaBuffer = new byte[1024];
                    int potvrdaBytes = tcpSocket.Receive(potvrdaBuffer);
                    string potvrda = Encoding.UTF8.GetString(potvrdaBuffer, 0, potvrdaBytes);
                    Console.WriteLine($"\nServer: {potvrda}");
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"\nGreška pri slanju: {ex.Message}");
                }
            }

            tcpSocket.Close();
            udpSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nGreška: {ex.Message}");
        }

        Console.WriteLine("\nPritisni Enter za izlaz...");
        Console.ReadLine();
    }

    static void PokreniKviz(Socket socket)
    {
        try
        {
            bool kvizTraje = true;
            byte[] buffer = new byte[4096];

            while (kvizTraje)
            {
                int bytes = socket.Receive(buffer);

                if (bytes == 0) break;

                string poruka = Encoding.UTF8.GetString(buffer, 0, bytes);

                string[] delovi = poruka.Split(new string[] { "SKOČKO", "MOJ BROJ", "KO ZNA ZNA", "Dobili ste", "KRAJ_KVIZA" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string deo in delovi)
                {
                    if (string.IsNullOrWhiteSpace(deo)) continue;

                    string cetvrtinaPoruka = deo.Trim();

                    if (poruka.Contains("SKOČKO") && deo.Contains("Pogodite"))
                        cetvrtinaPoruka = "SKOČKO" + cetvrtinaPoruka;
                    else if (poruka.Contains("MOJ BROJ") && deo.Contains("Traženi"))
                        cetvrtinaPoruka = "MOJ BROJ" + cetvrtinaPoruka;
                    else if (poruka.Contains("KO ZNA ZNA") && deo.Contains("Unesi"))
                        cetvrtinaPoruka = "KO ZNA ZNA" + cetvrtinaPoruka;

                    Console.WriteLine($"\n{cetvrtinaPoruka}");

                    if (cetvrtinaPoruka.Contains("KRAJ_KVIZA") || cetvrtinaPoruka.Contains("pobednik") ||
                        cetvrtinaPoruka.Contains("ČESTITAMO") || cetvrtinaPoruka.Contains("Hvala"))
                    {
                        Console.WriteLine("\n*** KVIZ ZAVRŠEN ***");
                        kvizTraje = false;
                        return;
                    }

                    if (cetvrtinaPoruka.Contains("Unesi") || cetvrtinaPoruka.Contains("unesi") ||
                        cetvrtinaPoruka.Contains("Pošaljite") || cetvrtinaPoruka.Contains("pošaljite"))
                    {
                        Console.Write("\nTvoj odgovor: ");
                        string odgovor = Console.ReadLine();

                        socket.Send(Encoding.UTF8.GetBytes(odgovor));

                        if (cetvrtinaPoruka.Contains("SKOČKO") || cetvrtinaPoruka.Contains("Skočko"))
                        {
                            while (true)
                            {
                             bytes = socket.Receive(buffer);
                             string porukaSk = Encoding.UTF8.GetString(buffer, 0, bytes);
                             Console.WriteLine($"\n{porukaSk}");
                                if (porukaSk.Contains("POGODAK") || porukaSk.Contains("NISTE POGODILI"))
                                {
                                    break; // kraj Skocka
                                }
                            Console.Write("\nTvoj odgovor: ");
                            string noviPokusaj = Console.ReadLine();
                            socket.Send(Encoding.UTF8.GetBytes(noviPokusaj));
                            }
                        }
                        else
                        {
                            bytes = socket.Receive(buffer);
                            string poeni = Encoding.UTF8.GetString(buffer, 0, bytes);
                            Console.WriteLine($"\n{poeni}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nGreška u kvizu: {ex.Message}");
        }
    }
}