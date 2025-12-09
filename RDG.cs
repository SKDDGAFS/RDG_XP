using System;
using System.Threading; // Neu: Benötigt für Thread.Sleep

public class RDG
{
    // Symbole für die Dungeon-Karte
    const char WAND = '#';
    const char GANG = '.';
    const char START = 'S';
    const char ENDE = 'E';
    const char SPIELER = '@';

    static Random _rnd = new Random();

    // Globale Variablen für den Spieler und das Spiel
    static int _spielerX;
    static int _spielerY;
    static bool _spielLaeuft = true; // Game loop Steurung

    // ==========================================================
    // MAIN: Jetzt mit Setup und Game Loop
    // ==========================================================
    static void Main(string[] args)
    {
        // 1. EINMALIGER SETUP: Dungeon generieren und Spieler platzieren
        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe = HoleGueltigeEingabe(10, 25);
        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite = HoleGueltigeEingabe(10, 50);

        char[,] karte = InitialisiereKarte(breite, hoehe);

        int endX, endY;

        // GaengeErstellen übergibt jetzt die Startkoordinaten direkt an _spielerX/_spielerY
        GaengeErstellen(karte, breite, hoehe, out _spielerX, out _spielerY, out endX, out endY);

        ErzeugeHauptWeg(karte, _spielerX, _spielerY, endX, endY, breite, hoehe);
        ErzeugeNebenWege(karte, 20, breite, hoehe);

        // Das ENDE-Symbol muss fixiert werden
        karte[endY, endX] = ENDE;

        // Wenn der Spieler direkt auf dem Startpunkt steht, muss dieser S-Marker nicht extra gesetzt werden.


        // 2. INTERAKTIVER GAME LOOP
        Console.CursorVisible = false; // Cursor verstecken, damit er nicht flackert
        Console.Clear();

        while (_spielLaeuft)
        {
            // Karte zeichnen (mit Spieler!)
            KarteZeichnen(karte, breite, hoehe);

            // Spieler bewegen, Wände prüfen und Ziel prüfen
            BewegeSpieler(karte, breite, hoehe, endX, endY);

            // Kleine Pause, um das Flackern zu reduzieren
            Thread.Sleep(50);
        }


        // 3. ABSCHLUSS
        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("---------------------------------------------");
        Console.WriteLine("🎉 Ziel erreicht! Du hast den Dungeon verlassen.");
        Console.WriteLine("---------------------------------------------");
        Console.ReadKey();
    }

    // ==========================================================
    // NEUE METHODE: BEWEGUNG DES SPIELERS
    // ==========================================================

    /// <summary>
    /// Wartet auf Tastendruck und bewegt den Spieler, falls der Zug gültig (keine Wand) ist.
    /// </summary>
    static void BewegeSpieler(char[,] karte, int breite, int hoehe, int endX, int endY)
    {
        // Prüft, ob eine Taste gedrückt wurde, blockiert aber nicht (KeyAvailable)
        if (!Console.KeyAvailable)
            return;

        // Wartet auf den nächsten Tastendruck (true = Key wird nicht im Konsolenpuffer angezeigt)
        ConsoleKeyInfo taste = Console.ReadKey(true);

        int neuX = _spielerX;
        int neuY = _spielerY;

        // Berechne die Zielposition
        if (taste.Key == ConsoleKey.UpArrow)
            neuY--;
        else if (taste.Key == ConsoleKey.DownArrow)
            neuY++;
        else if (taste.Key == ConsoleKey.LeftArrow)
            neuX--;
        else if (taste.Key == ConsoleKey.RightArrow)
            neuX++;
        else
            return; // Wenn keine Pfeiltaste gedrückt wurde, beende die Methode

        // SICHERUNG: Stellt sicher, dass die gewünschte Position innerhalb der äußeren Grenzen liegt
        neuX = ClampWert(neuX, 1, breite - 2);
        neuY = ClampWert(neuY, 1, hoehe - 2);

        // WAND-Check: Prüfen, was sich an der Zielposition befindet
        char zielSymbol = karte[neuY, neuX];

        if (zielSymbol != WAND)
        {
            // Bewegung ist erlaubt!
            _spielerX = neuX;
            _spielerY = neuY;

            // ZIEL-Check: Spiel beenden?
            if (zielSymbol == ENDE)
            {
                _spielLaeuft = false;
            }
        }
    }


    // ==========================================================
    // ANGEPASSTE METHODEN
    // ==========================================================

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

    // ACHTUNG: Die out-Parameter sind jetzt _spielerX/_spielerY in Main!
    static void GaengeErstellen(char[,] karte, int breite, int hoehe, out int startX, out int startY, out int endX, out int endY)
    {
        startY = _rnd.Next(1, hoehe - 1);
        startX = _rnd.Next(1, breite - 1);

        // WICHTIG: Hier wird nur das S gesetzt, der Spieler (@) wird später darüber gezeichnet.
        karte[startY, startX] = START;

        do
        {
            endY = _rnd.Next(1, hoehe - 1);
            endX = _rnd.Next(1, breite - 1);
        } while (endY == startY && endX == startX);

        karte[endY, endX] = ENDE;

        // Rückgabe der Positionen (diese gehen in die globalen Variablen _spielerX/_spielerY)
        startX = startX;
        startY = startY;
    }

    static void ErzeugeHauptWeg(char[,] karte, int startX, int startY, int endX, int endY, int breite, int hoehe)
    {
        int x = startX, y = startY;

        while (x != endX || y != endY)
        {
            if (karte[y, x] != START && karte[y, x] != ENDE)
                karte[y, x] = GANG;

            int dx = Math.Abs(endX - x);
            int dy = Math.Abs(endY - y);

            bool bewegeX = false;

            if (dx > dy)
                bewegeX = true;
            else if (dy > dx)
                bewegeX = false;
            else
                bewegeX = (_rnd.Next(0, 2) == 0);

            if (_rnd.Next(0, 100) < 80)
            {
                if (bewegeX)
                {
                    if (x < endX) x++;
                    else if (x > endX) x--;
                }
                else
                {
                    if (y < endY) y++;
                    else if (y > endY) y--;
                }
            }
            else
            {
                int r = _rnd.Next(0, 4);
                if (r == 0) x++;
                else if (r == 1) x--;
                else if (r == 2) y++;
                else if (r == 3) y--;
            }

            x = ClampWert(x, 1, breite - 2);
            y = ClampWert(y, 1, hoehe - 2);
        }
    }

    static void ErzeugeNebenWege(char[,] karte, int menge, int breite, int hoehe)
    {
        int erstelleWege = 0;
        int versuche = 0;

        while (erstelleWege < menge && versuche < 1000)
        {
            versuche++;
            int x = _rnd.Next(1, breite - 1);
            int y = _rnd.Next(1, hoehe - 1);

            bool istBoden = karte[y, x] != WAND;

            if (!istBoden)
                continue;

            int laenge = _rnd.Next(5, 20);
            int cx = x;
            int cy = y;

            for (int s = 0; s < laenge; s++)
            {
                if (karte[cy, cx] == WAND)
                {
                    karte[cy, cx] = GANG;
                }

                int r = _rnd.Next(0, 4);
                if (r == 0 && cx < breite - 2) cx++;
                else if (r == 1 && cx > 1) cx--;
                else if (r == 2 && cy < hoehe - 2) cy++;
                else if (r == 3 && cy > 1) cy--;
            }
            erstelleWege++;
        }
    }

    // WICHTIG: Diese Methode wurde angepasst, um den Spieler zu zeichnen!
    static void KarteZeichnen(char[,] karte, int breite, int hoehe)
    {
        // Konsolen-Cursor an den Anfang setzen, damit die Karte überschrieben wird
        Console.SetCursorPosition(0, 0);

        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                char symbol = karte[y, x];

                // PRÜFUNG: Wenn wir uns an der Spielerposition befinden, zeichne das Spieler-Symbol (@)
                if (x == _spielerX && y == _spielerY)
                {
                    symbol = SPIELER;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                // Ansonsten nutze die normalen Symbole
                else if (symbol == START) Console.ForegroundColor = ConsoleColor.Green;
                else if (symbol == ENDE) Console.ForegroundColor = ConsoleColor.Red;
                else if (symbol == GANG) Console.ForegroundColor = ConsoleColor.Gray;
                else Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write(symbol);
            }
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    static int ClampWert(int v, int min, int max)
    {
        if (v < min)
            return min;
        if (v > max)
            return max;
        return v;
    }
}