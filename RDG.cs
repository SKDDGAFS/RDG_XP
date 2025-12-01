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
        int hoehe;
        while (true)
        {
            try
            {
                hoehe = Convert.ToInt32(Console.ReadLine());
                if (hoehe < 10 || hoehe > 25)
                    Console.WriteLine("Die Höhe darf nicht kleiner als 10 oder größer als 25 sein. \nVersuchen Sie es erneut:");
                else break;
            }
            catch { Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:"); }
        }

        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite;
        while (true)
        {
            try
            {
                breite = Convert.ToInt32(Console.ReadLine());
                if (breite < 10 || breite > 50)
                    Console.WriteLine("Die Breite darf nicht kleiner als 10 oder größer als 50 sein. \nVersuchen Sie es erneut:");
                else break;
            }
            catch { Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:"); }
        }

        char[,] karte = InitialisiereKarte(breite, hoehe);
        GaengeErstellen(karte, breite, hoehe, out int startX, out int startY, out int endX, out int endY);

        ErzeugeHauptWeg(karte, startX, startY, endX, endY, breite, hoehe);
        ErzeugeNebenWege(karte, 20, breite, hoehe);

        // Karte ausgeben
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                char symbol = karte[y, x];
                if (symbol == START) Console.ForegroundColor = ConsoleColor.Green;
                else if (symbol == ENDE) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ResetColor();

                Console.Write(symbol);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
        Console.ReadKey();
    }

    static char[,] InitialisiereKarte(int breite, int hoehe)
    {
        char[,] karte = new char[hoehe, breite];
        for (int y = 0; y < hoehe; y++)
            for (int x = 0; x < breite; x++)
                karte[y, x] = WAND;
        return karte;
    }

    static void GaengeErstellen(char[,] karte, int breite, int hoehe, out int startX, out int startY, out int endX, out int endY)
    {
        Random zufaelig = new Random();
        startY = zufaelig.Next(1, hoehe - 1);
        startX = zufaelig.Next(1, breite - 1);
        karte[startY, startX] = START;

        do
        {
            endY = zufaelig.Next(1, hoehe - 1);
            endX = zufaelig.Next(1, breite - 1);
        } while (endY == startY && endX == startX);
        karte[endY, endX] = ENDE;
    }

    static void ErzeugeHauptWeg(char[,] karte, int startX, int startY, int endX, int endY, int breite, int hoehe)
    {
        int x = startX, y = startY;
        Random rnd = new Random();

        while (x != endX || y != endY)
        {
            if (karte[y, x] != START && karte[y, x] != ENDE)
                karte[y, x] = GANG;

            if (rnd.Next(0, 100) < 70)
            {
                if (x < endX) x++;
                else if (x > endX) x--;
                if (y < endY) y++;
                else if (y > endY) y--;
            }
            else
            {
                int r = rnd.Next(0, 4);
                if (r == 0 && x < breite - 2) x++;
                else if (r == 1 && x > 1) x--;
                else if (r == 2 && y < hoehe - 2) y++;
                else if (r == 3 && y > 1) y--;
            }
        }
        karte[endY, endX] = ENDE;
    }

    static void ErzeugeNebenWege(char[,] karte, int menge, int breite, int hoehe)
    {
        Random rnd = new Random();
        for (int i = 0; i < menge; i++)
        {
            int x = rnd.Next(1, breite - 1);
            int y = rnd.Next(1, hoehe - 1);
            if (karte[y, x] != WAND) continue;

            int laenge = rnd.Next(10, 40);
            for (int s = 0; s < laenge; s++)
            {
                karte[y, x] = GANG;
                int r = rnd.Next(0, 4);
                if (r == 0 && x < breite - 2) x++;
                else if (r == 1 && x > 1) x--;
                else if (r == 2 && y < hoehe - 2) y++;
                else if (r == 3 && y > 1) y--;
            }
        }
    }
}
