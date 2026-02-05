using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
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
            // Prima UDP prijavu
            byte[] udpBuffer = new byte[1024];
            EndPoint udpKlijentEP = new IPEndPoint(IPAddress.Any, 0);
            int udpBytes = udpSocket.ReceiveFrom(udpBuffer, ref udpKlijentEP);
            string prijava = Encoding.UTF8.GetString(udpBuffer, 0, udpBytes);

            Console.WriteLine($"Primio UDP od {udpKlijentEP}: {prijava}");

            if (prijava.StartsWith("PRIJAVA:"))
            {
                // Salje TCP info
                string odgovor = $"TCP:127.0.0.1:50005";
                byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                udpSocket.SendTo(odgovorBytes, udpKlijentEP);

                // Prihvata TCP konekciju
                Socket tcpKlijent = tcpSocket.Accept();
                Console.WriteLine($"TCP konekcija od {tcpKlijent.RemoteEndPoint}");

                //Pozdrav
                string[] delovi = prijava.Substring(8).Split(',');
                string ime = delovi[0].Trim();
                string pozdrav = $"Dobrodošli u trening igru kviza TV Slagalica, današnji takmičar je {ime}";
                byte[] pozdravBytes = Encoding.UTF8.GetBytes(pozdrav);
                tcpKlijent.Send(pozdravBytes);
                Console.WriteLine($"Poslat pozdrav: {pozdrav}");

                // Ceka se SPREMAN
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

                        // Salje potvrdu
                        string potvrda = "Čekamo ostale igrače...";
                        tcpKlijent.Send(Encoding.UTF8.GetBytes(potvrda));
                        Console.WriteLine("Poslata potvrda klijentu");
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Greška: {ex.Message}");
                }

                // SAMO SADA zatvara konekciju
                Console.WriteLine($"Zatvaram konekciju sa {tcpKlijent.RemoteEndPoint}\n");
                tcpKlijent.Close();
            }
        }
    }
}