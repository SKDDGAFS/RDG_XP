using System;

public class RDG
{
    const char WAND = '#';   // Wand-Symbol
    const char GANG = '.';   // Gang-Symbol
    const char START = 'S';  // Start
    const char ENDE = 'E';   // Ende

    static void Main(string[] args)
    {
        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe = Convert.ToInt32(Console.ReadLine());
        while (true)
        {
            try
            {
                breite = Convert.ToInt32(Console.ReadLine());
                if (hoehe < 10 || hoehe > 25)
                {
                    Console.WriteLine("Die Höhe darf nicht kleiner als 10 oder Höhe als 25 sein. \nVersuchen Sie es erneut:");
                }
                else
                {
                    break;
                }
            }
            catch
            {
                Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:");
            }
        }

        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite;
        while (true)
        {
            try
            {
                breite = Convert.ToInt32(Console.ReadLine());
                if (breite < 10 || breite > 50)
                {
                    Console.WriteLine("Die Breite darf nicht kleiner als 10 oder größer als 50 sein. \nVersuchen Sie es erneut:");
                }
                else
                {
                    break;
                }
            }
            catch
            {
                Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:");
            }
        }

        Random zufaelig = new Random();

        char[,] karte = InitialisiereKarte(breite, hoehe);

        // Karte ausgeben
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                Console.Write(karte[y, x]);
            }
            Console.WriteLine();
        }
    }

    static char[,] InitialisiereKarte(int breite, int hoehe)
    {
        char[,] karte = new char[hoehe, breite];

        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                karte[y, x] = WAND; // Jede Position mit Wand füllen
            }
        }

        return karte;
    }
}

