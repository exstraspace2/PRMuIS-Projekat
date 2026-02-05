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

            // 1. UDP prijava
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string prijava = $"PRIJAVA:{ime},{igre}";
            byte[] prijavaBytes = Encoding.UTF8.GetBytes(prijava);

            EndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15000);
            udpSocket.SendTo(prijavaBytes, serverEP);
            Console.WriteLine("Poslata prijava\n");

            // 2. Prima TCP info
            byte[] odgovorBuffer = new byte[1024];
            EndPoint odgovorEP = new IPEndPoint(IPAddress.Any, 0);
            int odgovorBytes = udpSocket.ReceiveFrom(odgovorBuffer, ref odgovorEP);
            string odgovor = Encoding.UTF8.GetString(odgovorBuffer, 0, odgovorBytes);
            Console.WriteLine($"Odgovor servera: {odgovor}");

            // 3. Povezuje se na TCP
            string[] tcpInfo = odgovor.Split(':');
            string tcpIP = tcpInfo[1];
            int tcpPort = int.Parse(tcpInfo[2]);

            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(tcpIP), tcpPort));
            Console.WriteLine($"Povezan na TCP server {tcpIP}:{tcpPort}\n");

            // 4. Prima pozdrav
            byte[] pozdravBuffer = new byte[1024];
            int pozdravBytes = tcpSocket.Receive(pozdravBuffer);
            string pozdrav = Encoding.UTF8.GetString(pozdravBuffer, 0, pozdravBytes);
            Console.WriteLine($"Server: {pozdrav}\n");

            // 5. Šalje SPREMAN
            Console.Write("Unesi SPREMAN kada si spreman: ");
            string unos = Console.ReadLine();

            if (unos.ToUpper() == "SPREMAN")
            {
                try
                {
                    byte[] spremanBytes = Encoding.UTF8.GetBytes("SPREMAN");
                    tcpSocket.Send(spremanBytes);
                    Console.WriteLine("Poslao SPREMAN serveru");

                    // Prima potvrdu
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

            // Cisti
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
}