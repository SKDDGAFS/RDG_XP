using System;

// Hauptprogramm, das den zufälligen Dungeon erstellt
public class RDG
{
    // Die Zeichen, die wir auf der Karte verwenden (Unsere Symbole)
    const char WAND = '#';
    const char GANG = '.';
    const char START = 'S';
    const char ENDE = 'E';

    // Unser Werkzeug für Zufall: Nur einmal erstellen, damit die Zufallszahlen gut sind.
    static Random _rnd = new Random();

    static void Main(string[] args)
    {
        // 1. Größe des Dungeon vom Spieler fragen und prüfen.
        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe = HoleGueltigeEingabe(10, 25);

        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite = HoleGueltigeEingabe(10, 50);

        // 2. Die Karte erstellen – zuerst ist alles eine feste Wand.
        char[,] karte = InitialisiereKarte(breite, hoehe);

        // 3. Start- und Endpunkt zufällig auf der Karte finden.
        GaengeErstellen(karte, breite, hoehe, out int startX, out int startY, out int endX, out int endY);

        // 4. Den Hauptweg vom Start zum Ende graben.
        ErzeugeHauptWeg(karte, startX, startY, endX, endY, breite, hoehe);

        // 5. Zusätzliche Abzweigungen und Höhlen hinzufügen.
        ErzeugeNebenWege(karte, 20, breite, hoehe);

        // 6. Sicherstellen, dass S und E nicht durch das Graben überschrieben wurden.
        karte[startY, startX] = START;
        karte[endY, endX] = ENDE;

        // 7. Die fertige Karte anzeigen.
        KarteZeichnen(karte, breite, hoehe);

        Console.WriteLine("\nDrücken Sie eine Taste zum Beenden...");
        Console.ReadKey();
    }

    /// <summary>
    /// Fragt eine Zahl ab und sorgt dafür, dass sie im erlaubten Bereich liegt.
    /// </summary>
    static int HoleGueltigeEingabe(int min, int max)
    {
        while (true) // So lange fragen, bis die Eingabe passt
        {
            try
            {
                int wert = Convert.ToInt32(Console.ReadLine());
                if (wert < min || wert > max)
                    Console.WriteLine($"Der Wert muss zwischen {min} und {max} liegen. Versuchen Sie es erneut:");
                else
                    return wert; // Zahl ist gut, Schleife beenden
            }
            catch
            {
                Console.WriteLine("Fehler! Bitte geben Sie eine gültige Zahl (keine Buchstaben) ein:");
            }
        }
    }

    /// <summary>
    /// Erstellt das leere Spielfeld und macht alles zur Wand (#).
    /// </summary>
    static char[,] InitialisiereKarte(int breite, int hoehe)
    {
        char[,] karte = new char[hoehe, breite];
        // Geht jede Zelle durch und setzt sie auf Wand
        for (int y = 0; y < hoehe; y++)
            for (int x = 0; x < breite; x++)
                karte[y, x] = WAND;
        return karte;
    }

    /// <summary>
    /// Wählt zufällige Orte für den Start (S) und das Ende (E).
    /// </summary>
    static void GaengeErstellen(char[,] karte, int breite, int hoehe, out int startX, out int startY, out int endX, out int endY)
    {
        // Startposition finden
        startY = _rnd.Next(1, hoehe - 1);
        startX = _rnd.Next(1, breite - 1);
        karte[startY, startX] = START;

        // Endposition finden: Muss eine andere Zelle sein als der Start
        do
        {
            endY = _rnd.Next(1, hoehe - 1);
            endX = _rnd.Next(1, breite - 1);
        } while (endY == startY && endX == startX);

        karte[endY, endX] = ENDE;
    }

    /// <summary>
    /// Gräbt den Weg vom Start zum Ende. Die Bewegung ist zu 80% zielgerichtet, damit der Weg nicht zu lange wird.
    /// </summary>
    static void ErzeugeHauptWeg(char[,] karte, int startX, int startY, int endX, int endY, int breite, int hoehe)
    {
        int x = startX, y = startY;

        while (x != endX || y != endY) // Solange laufen, bis wir das Ziel erreichen
        {
            if (karte[y, x] != START && karte[y, x] != ENDE)
                karte[y, x] = GANG; // Aktuelle Zelle wird zum Gang

            // 1. Prüfen, wie weit das Ziel in X- und Y-Richtung noch entfernt ist.
            int dx = Math.Abs(endX - x);
            int dy = Math.Abs(endY - y);

            bool bewegeX = false;

            // 2. Entscheiden, welche Richtung (X oder Y) zuerst dran ist. Die längere Distanz hat Vorrang.
            if (dx > dy)
                bewegeX = true;
            else if (dy > dx)
                bewegeX = false;
            else // Falls gleich weit entfernt, zufällig wählen
                bewegeX = (_rnd.Next(0, 2) == 0);

            // 3. Bewegung ausführen (80% zielgerichtet, 20% zufälliges Abweichen/Schlenker)
            if (_rnd.Next(0, 100) < 80)
            {
                // Gezielte Bewegung zum Ende hin
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
                // Zufälliges Abweichen (macht den Weg kurviger)
                int r = _rnd.Next(0, 4);
                if (r == 0) x++;
                else if (r == 1) x--;
                else if (r == 2) y++;
                else if (r == 3) y--;
            }

            // SICHERUNG: Stellt sicher, dass wir immer innerhalb der äußeren Wand bleiben.
            x = Math.Clamp(x, 1, breite - 2);
            y = Math.Clamp(y, 1, hoehe - 2);
        }
    }

    /// <summary>
    /// Fügt dem Hauptweg 20 zusätzliche, zufällige Seitenwege hinzu (Abzweigungen).
    /// </summary>
    static void ErzeugeNebenWege(char[,] karte, int menge, int breite, int hoehe)
    {
        int erstelleWege = 0;
        int versuche = 0;

        while (erstelleWege < menge && versuche < 1000) // Versucht, 20 Nebenwege zu erstellen
        {
            versuche++;
            int x = _rnd.Next(1, breite - 1);
            int y = _rnd.Next(1, hoehe - 1);

            // WICHTIG: Prüft, ob der Startpunkt für den Nebenweg bereits ein Gang ist.
            // Dies stellt sicher, dass der neue Weg mit dem Hauptweg verbunden wird.
            bool istBoden = karte[y, x] != WAND;

            if (!istBoden)
                continue; // Wenn es eine Wand ist, neuen Startpunkt suchen

            int laenge = _rnd.Next(5, 20); // Länge des Seitenwegs
            int cx = x;
            int cy = y;

            for (int s = 0; s < laenge; s++)
            {
                // Gräbt den Gang, falls wir auf eine Wand treffen
                if (karte[cy, cx] == WAND)
                {
                    karte[cy, cx] = GANG;
                }

                // Rein zufällige Schritte (kein Ziel)
                int r = _rnd.Next(0, 4);
                if (r == 0 && cx < breite - 2) cx++;
                else if (r == 1 && cx > 1) cx--;
                else if (r == 2 && cy < hoehe - 2) cy++;
                else if (r == 3 && cy > 1) cy--;
            }
            erstelleWege++;
        }
    }

    /// <summary>
    /// Zeigt die Karte in der Konsole an und benutzt Farben für bessere Sichtbarkeit.
    /// </summary>
    static void KarteZeichnen(char[,] karte, int breite, int hoehe)
    {
        // Geht jede Zelle der Karte durch
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                char symbol = karte[y, x];
                // Wählt die passende Farbe für das Symbol
                if (symbol == START) Console.ForegroundColor = ConsoleColor.Green;
                else if (symbol == ENDE) Console.ForegroundColor = ConsoleColor.Red;
                else if (symbol == GANG) Console.ForegroundColor = ConsoleColor.Gray;
                else Console.ForegroundColor = ConsoleColor.DarkGray; // Wandfarbe

                Console.Write(symbol); // Gibt das Zeichen aus
            }
            Console.WriteLine(); // Geht zur nächsten Zeile
        }
        Console.ResetColor(); // Stellt die Standardfarbe der Konsole wieder her
    }
}