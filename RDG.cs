using System;

public class RDG
{
    // Konstanten für die Symbole
    const char WAND = '#';
    const char GANG = '.';
    const char START = 'S';
    const char ENDE = 'E';

    // VERBESSERUNG 1: Zentralisierte Random-Instanz.
    // Verhindert, dass Pfade aufgrund von Seeds identisch generiert werden.
    static Random _rnd = new Random();

    static void Main(string[] args)
    {
        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        // Verwendung der neuen Hilfsmethode
        int hoehe = HoleGueltigeEingabe(10, 25);

        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        // Verwendung der neuen Hilfsmethode
        int breite = HoleGueltigeEingabe(10, 50);

        char[,] karte = InitialisiereKarte(breite, hoehe);

        GaengeErstellen(karte, breite, hoehe, out int startX, out int startY, out int endX, out int endY);

        // Verbesserte Hauptweg-Erstellung mit Math.Abs
        ErzeugeHauptWeg(karte, startX, startY, endX, endY, breite, hoehe);

        ErzeugeNebenWege(karte, 20, breite, hoehe);

        // Sicherstellen, dass Start und Ende nicht überschrieben werden
        karte[startY, startX] = START;
        karte[endY, endX] = ENDE;

        KarteZeichnen(karte, breite, hoehe);

        Console.WriteLine("\nDrücken Sie eine Taste zum Beenden...");
        Console.ReadKey();
    }

    // HILFSMETHODE: Kapselt die Input-Validierung und Fehlerbehandlung
    static int HoleGueltigeEingabe(int min, int max)
    {
        while (true)
        {
            try
            {
                int wert = Convert.ToInt32(Console.ReadLine());
                if (wert < min || wert > max)
                    Console.WriteLine($"Der Wert muss zwischen {min} und {max} liegen. Versuchen Sie es erneut:");
                else
                    return wert;
            }
            catch
            {
                Console.WriteLine("Fehler! Bitte geben Sie eine gültige Zahl ein:");
            }
        }
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
        startY = _rnd.Next(1, hoehe - 1);
        startX = _rnd.Next(1, breite - 1);
        karte[startY, startX] = START;

        do
        {
            endY = _rnd.Next(1, hoehe - 1);
            endX = _rnd.Next(1, breite - 1);
        } while (endY == startY && endX == startX);

        karte[endY, endX] = ENDE;
    }

    // VERBESSERUNG 2: Hauptweg mit mathematischer Priorität (Math.Abs)
    static void ErzeugeHauptWeg(char[,] karte, int startX, int startY, int endX, int endY, int breite, int hoehe)
    {
        int x = startX, y = startY;

        while (x != endX || y != endY)
        {
            if (karte[y, x] != START && karte[y, x] != ENDE)
                karte[y, x] = GANG;

            // 1. Mathematische Distanzen berechnen
            int dx = Math.Abs(endX - x); // Absolute X-Distanz
            int dy = Math.Abs(endY - y); // Absolute Y-Distanz

            bool bewegeX = false;

            // 2. Priorisierung: Wähle die Achse mit der größeren Restdistanz
            if (dx > dy)
            {
                bewegeX = true; // X hat längeren Weg, priorisieren X
            }
            else if (dy > dx)
            {
                bewegeX = false; // Priorisiere Y
            }
            else // dx == dy: Gleiche Distanz, zufällig wählen
            {
                bewegeX = (_rnd.Next(0, 2) == 0);
            }

            // 3. Bewegung ausführen (80% der Zeit nach Priorität, 20% random für Schlenker)
            if (_rnd.Next(0, 100) < 80)
            {
                // Gezielte, orthogonale Bewegung (X oder Y)
                if (bewegeX)
                {
                    if (x < endX) x++;
                    else if (x > endX) x--;
                }
                else // bewegeY
                {
                    if (y < endY) y++;
                    else if (y > endY) y--;
                }
            }
            else
            {
                // Zufällige Abweichung (die 20% "Trunkenheit")
                int r = _rnd.Next(0, 4);
                if (r == 0 && x < breite - 2) x++;
                else if (r == 1 && x > 1) x--;
                else if (r == 2 && y < hoehe - 2) y++;
                else if (r == 3 && y > 1) y--;
            }

            // Sicherstellen, dass die Bewegung innerhalb der äußeren Begrenzung bleibt
            x = Math.Clamp(x, 1, breite - 2);
            y = Math.Clamp(y, 1, hoehe - 2);
        }
    }

    // Verwendet jetzt die zentrale _rnd-Instanz
    static void ErzeugeNebenWege(char[,] karte, int menge, int breite, int hoehe)
    {
        for (int i = 0; i < menge; i++)
        {
            int x = _rnd.Next(1, breite - 1);
            int y = _rnd.Next(1, hoehe - 1);

            if (karte[y, x] != WAND) continue;

            int laenge = _rnd.Next(5, 20);
            for (int s = 0; s < laenge; s++)
            {
                // WICHTIG: Start/Ende nicht überschreiben
                if (karte[y, x] != START && karte[y, x] != ENDE)
                    karte[y, x] = GANG;

                int r = _rnd.Next(0, 4);
                if (r == 0 && x < breite - 2) x++;
                else if (r == 1 && x > 1) x--;
                else if (r == 2 && y < hoehe - 2) y++;
                else if (r == 3 && y > 1) y--;
            }
        }
    }

    static void KarteZeichnen(char[,] karte, int breite, int hoehe)
    {
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                char symbol = karte[y, x];
                if (symbol == START) Console.ForegroundColor = ConsoleColor.Green;
                else if (symbol == ENDE) Console.ForegroundColor = ConsoleColor.Red;
                else if (symbol == GANG) Console.ForegroundColor = ConsoleColor.Gray;
                else Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write(symbol);
            }
            Console.WriteLine();
        }
        Console.ResetColor();
        console.ReadKey();
    }
}