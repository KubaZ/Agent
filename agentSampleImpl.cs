﻿using System;
using Data.Realm;
using Data;

namespace CsClient
{
    public class Program
    {
        static AgentAPI agentTomek; //nasz agent, instancja klasy AgentAPI
        static int energy; //tu zapisujemy aktualną energię naszego agenta
        static WorldParameters cennikSwiata; //tu zapisujemy informacje o świecie

 
        // Nasza metoda nasłuchująca
        static void Listen(String krzyczacyAgent, String komunikat) {
            Console.WriteLine(krzyczacyAgent + " krzyczy " + komunikat);
        }
        
        static void Main(string[] args)
        {
            //powtarzamy czynnosci az nam się uda
            while (true)
            {
                agentTomek = new AgentAPI(Listen); //tworzymy nowe AgentAPI, podając w parametrze naszą metodę nasłuchującą

                // pobieramy parametry połączenia i agenta z klawiatury
                Console.Write("Podaj IP serwera: ");
                String ip = Console.ReadLine();

                Console.Write("Podaj nazwe druzyny: ");
                String groupname = Console.ReadLine();

                Console.Write("Podaj haslo: ");
                String grouppass = Console.ReadLine();

                Console.Write("Podaj nazwe swiata: ");
                String worldname = Console.ReadLine();
                    
                Console.Write("Podaj imie: ");      
                String imie = Console.ReadLine();

                try
                {
                    //łączymy się z serwerem. Odbieramy parametry świata i wyświetlamy je      
                    cennikSwiata = agentTomek.Connect(ip, 6008, groupname, grouppass, worldname, imie);
                    Console.WriteLine(cennikSwiata.initialEnergy + " - Maksymalna energia");
                    Console.WriteLine(cennikSwiata.maxRecharge + " - Maksymalne doładowanie");
                    Console.WriteLine(cennikSwiata.sightScope + " - Zasięg widzenia");
                    Console.WriteLine(cennikSwiata.hearScope + " - Zasięg słyszenia");
                    Console.WriteLine(cennikSwiata.moveCost + " - Koszt chodzenia");
                    Console.WriteLine(cennikSwiata.rotateCost + " - Koszt obrotu");
                    Console.WriteLine(cennikSwiata.speakCost + " - Koszt mówienia");

                    //ustawiamy nasza energie na poczatkowa energie kazdego agenta w danym swiecie
                    energy = cennikSwiata.initialEnergy;
                    //przechodzimy do obslugi zdarzen z klawiatury. Zamiast tej funkcji wstaw logikę poruszania się twojego agenta.
                    KeyReader();
                    //na koncu rozlaczamy naszego agenta
                    agentTomek.Disconnect();
                    Console.ReadKey();
                    break;
                }
                //w przypadku mało poważnego błędu, jak podanie złego hasła, rzucany jest wyjątek NonCriticalException; zaczynamy od nowa
                catch (NonCriticalException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                // w przypadku każdego innego wyjątku niż NonCriticalException powinniśmy zakończyć program; taki wyjątek nie powinien się zdarzyć
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ReadKey();
                }
            }
        }
        
        //funkcja wykonywująca określone akcję w zależności od naciśniętego przycisku
        static void KeyReader() {
            bool loop = true;
            while(loop) {
                Console.WriteLine("Moja energia: " + energy);
                switch(Console.ReadKey().Key) {
                    case ConsoleKey.Spacebar: Look(); 
                        break;
                    case ConsoleKey.R: Recharge();
                        break;
                    case ConsoleKey.UpArrow: StepForward();
                        break;
                    case ConsoleKey.LeftArrow: RotateLeft();
                        break;
                    case ConsoleKey.RightArrow: RotateRight();
                        break;
                    case ConsoleKey.Enter: Speak();
                        break;
                    case ConsoleKey.Q: loop = false;
                        break;
                    case ConsoleKey.D: agentTomek.Disconnect();
                        break;
                    default: Console.Beep();
                        break;
                }
            }
        }

        // ładujemy się
        private static void Recharge()
        {
            int added = agentTomek.Recharge();
            energy += added;
            Console.WriteLine("Otrzymano " + added + " energii");
        }

        //wysyłamy komunikat
        private static void Speak()
        {
            if (!agentTomek.Speak(Console.ReadLine(), 1))
                Console.WriteLine("Mowienie nie powiodlo sie - brak energii");
            else
                energy -= cennikSwiata.speakCost;
        }

        //obracamy się w lewo
        private static void RotateLeft()
        {
            if (!agentTomek.RotateLeft())
                Console.WriteLine("Obrot nie powiodl sie - brak energii");
            else
                energy -= cennikSwiata.rotateCost;
        }

        //obracamy się w prawo
        private static void RotateRight()
        {
            if (!agentTomek.RotateRight())
                Console.WriteLine("Obrot nie powiodl sie - brak energii");
            else
                energy -= cennikSwiata.rotateCost; //musimy sami zadbać o aktualizację naszej bieżącej energii - serwer nie dostarczy nam tej informacji
        }

        //idziemy do przodu
        private static void StepForward()
        {
            if (!agentTomek.StepForward())
                Console.WriteLine("Wykonanie kroku nie powiodlo sie");
            if (energy >= cennikSwiata.moveCost)
                energy -= cennikSwiata.moveCost;//musimy sami zadbać o aktualizację naszej bieżącej energii - serwer nie dostarczy nam tej informacji
                                                // w tym wypadku zużytą energię policzyliśmy błędnie - nie uwzględniliśmy różnicy wysokości terenu (patrz Dokumentacja)
        }

        private static void Look()
        {
            OrientedField[] pola = agentTomek.Look(); //dostajemy listę pól które widzi nasz agent

            //wyświetlamy informacje o wszystkich widzianych polach
            foreach (OrientedField pole in pola)
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine("POLE " + pole.x + "," + pole.y);
                Console.WriteLine("Wysokosc: " + pole.height);
                if (pole.energy != 0)
                    Console.WriteLine("Energia: " + pole.energy);
                if (pole.obstacle)
                    Console.WriteLine("Przeszkoda");
                if (pole.agent != null)
                    Console.WriteLine("Agent" + pole.agent.agentname + " i jest obrocony na " + pole.agent.direction.ToString());
                Console.WriteLine("-----------------------------");
            }
        }
        
    }
}
