using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // Für Dateizugriff

// ******************************************************
// I. SPIELERKLASSE
// ******************************************************

public class Player
{
    public int X { get; private set; }
    public int Y { get; private set; }
    // Der Spieler braucht ein Inventar, um Schätze zu zählen
    public int TreasuresCollected { get; private set; } = 0;
    public readonly char Symbol = '@'; // Das Spieler-Symbol
    private readonly char[,] _map; // Referenz auf die generierte Karte

    public Player(int startX, int startY, char[,] mapData)
    {
        X = startX;
        Y = startY;
        _map = mapData;
    }

    // Zählt den gefundenen Schatz hoch
    public void CollectTreasure()
    {
        TreasuresCollected++;
    }

    // Versucht, den Spieler basierend auf der gedrückten Taste zu bewegen.
    public bool TryMove(ConsoleKeyInfo keyInfo, char wandSymbol)
    {
        int newX = X;
        int newY = Y;

        // 1. Bestimme die potenzielle neue Position (W/A/S/D)
        switch (keyInfo.Key)
        {
            case ConsoleKey.W: // Hoch
                newY--;
                break;
            case ConsoleKey.S: // Runter
                newY++;
                break;
            case ConsoleKey.A: // Links
                newX--;
                break;
            case ConsoleKey.D: // Rechts
                newX++;
                break;
            default:
                return false;
        }

        // 2. Prüfe, ob die potenzielle Bewegung gültig ist
        if (IsValidMove(newX, newY, wandSymbol))
        {
            // 3. Aktualisiere die Spielerposition
            X = newX;
            Y = newY;
            return true;
        }

        return false;
    }

    // Prüft die Kartenränder und Kollisionen mit Wänden.
    private bool IsValidMove(int x, int y, char wandSymbol)
    {
        int mapHeight = _map.GetLength(0);
        int mapWidth = _map.GetLength(1);

        // Prüfe Kartenränder (wir halten den Spieler 1 Feld vom äußersten Rand entfernt)
        if (y <= 0 || y >= mapHeight - 1 || x <= 0 || x >= mapWidth - 1)
        {
            return false;
        }

        // Prüfe auf Wände
        if (_map[y, x] == wandSymbol)
        {
            return false;
        }

        return true;
    }
}


// ******************************************************
// II. DUNGEON GENERATOR UND SPIELLOGIK
// ******************************************************

public class RDG
{
    // Statische Symbole für die Karte
    static char WAND = '#';
    static char GANG = '.';
    static char START = 'S';
    static char ENDE = 'E';
    static char Schatz = 'T';
    static char Fallen = 'F';

    static Random zufaelig = new Random();

    // ----------------------------------------------------------------------
    // SPIELSCHLEIFE UND ANZEIGE
    // ----------------------------------------------------------------------

    // Rendert die Karte und den Spieler.
    static void DisplayMap(char[,] karte, int hoehe, int breite, Player player)
    {
        Console.Clear();
        Regeln_Einführung_Kurz(); // Kurze Regeln für die Anzeige in der Schleife
        Console.WriteLine("\n--- ZUFALLSDUNGEON ---");

        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                char symbol = karte[y, x];
                ConsoleColor farbe = ConsoleColor.Reset;

                // Prüfe, ob der Spieler an dieser Position ist
                if (y == player.Y && x == player.X)
                {
                    symbol = player.Symbol;
                    farbe = ConsoleColor.Cyan; // Spieler in Cyan
                }
                // Ansonsten verwende die Farben für die anderen Elemente
                else if (symbol == START)
                {
                    farbe = ConsoleColor.Green;
                }
                else if (symbol == ENDE)
                {
                    farbe = ConsoleColor.Red;
                }
                else if (symbol == Schatz)
                {
                    farbe = ConsoleColor.DarkYellow;
                }
                else if (symbol == Fallen)
                {
                    farbe = ConsoleColor.Magenta;
                }

                Console.ForegroundColor = farbe;
                Console.Write(symbol);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
        Console.WriteLine("----------------------");
        Console.WriteLine($"Position: ({player.X}, {player.Y}) | Gesammelte Schätze: {player.TreasuresCollected}");
        Console.WriteLine("Bewegen mit W, A, S, D. Beenden mit Q.");
    }

    // Die Haupt-Spielschleife
    static void GameLoop(char[,] karte, int hoehe, int breite, int startX, int startY)
    {
        Player player = new Player(startX, startY, karte);

        // Der Gang an der Startposition muss wieder auf 'S' gesetzt werden, 
        // damit der Spieler das Startsymbol nicht überschreibt, 
        // falls er zurückgeht.
        if (karte[startY, startX] != START)
        {
            karte[startY, startX] = START;
        }

        while (true)
        {
            // 1. Karte anzeigen (mit Spieler)
            DisplayMap(karte, hoehe, breite, player);

            // 2. Auf Tastendruck warten
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // 3. Beenden prüfen
            if (keyInfo.Key == ConsoleKey.Q)
            {
                Console.WriteLine("\nSpiel beendet. Bis zum nächsten Mal!");
                break;
            }

            // 4. Alte Position speichern
            int oldX = player.X;
            int oldY = player.Y;

            // 5. Bewegung versuchen
            bool moved = player.TryMove(keyInfo, WAND);

            if (moved)
            {
                // 6. Prüfe auf Ziel oder Interaktion an der neuen Position
                char currentTile = karte[player.Y, player.X];

                if (currentTile == ENDE)
                {
                    // Spiel gewonnen
                    DisplayMap(karte, hoehe, breite, player);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n🎉 Glückwunsch! Sie haben den Ausgang gefunden!");
                    Console.WriteLine($"Sie haben {player.TreasuresCollected} Schätze gesammelt.");
                    Console.ResetColor();
                    break;
                }
                else if (currentTile == Schatz)
                {
                    // Schatz gefunden
                    player.CollectTreasure();
                    karte[player.Y, player.X] = GANG; // Schatz entfernen, um ihn nicht doppelt zu zählen
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Glückwunsch! Sie haben einen Schatz gefunden!");
                    Console.ResetColor();
                }
                else if (currentTile == Fallen)
                {
                    // Falle ausgelöst
                    DisplayMap(karte, hoehe, breite, player);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n💥 BOOM! Sie sind in eine tödliche Falle getappt!");
                    Console.WriteLine($"Spiel verloren. Gesammelte Schätze: {player.TreasuresCollected}");
                    Console.ResetColor();
                    break;
                }

                // Setze die alte Position, wo der Spieler stand, wieder auf GANG (außer es ist 'S' oder 'E')
                if (karte[oldY, oldX] != START && karte[oldY, oldX] != ENDE)
                {
                    karte[oldY, oldX] = GANG;
                }
            }
        }
        // Warten auf Eingabe, bevor das Konsolenfenster geschlossen wird
        Console.ReadKey();
    }

    // Eine schlankere Version der Regeln für die Anzeige in der Schleife
    static void Regeln_Einführung_Kurz()
    {
        Console.WriteLine("Ziel: 'E' erreichen (Ende). Schatz: 'T', Falle: 'F'. Spieler: '@'");
    }

    // ----------------------------------------------------------------------
    // MAIN METHODE & GENERIERUNGSLOGIK
    // ----------------------------------------------------------------------

    static void Main(string[] args)
    {
        Regeln_Einführung();

        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe = Eingabe(10, 25);

        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite = Eingabe(10, 50);

        Console.WriteLine($"Dungeon erstellt mit Höhe {hoehe} und Breite {breite}.");

        char[,] karte = InitialisiereKarte(breite, hoehe);

        // Start und Ende erstellen, Positionen in startX, startY speichern
        Start_EndeErstellen(karte, breite, hoehe, out int startX, out int startY, out int endX, out int endY);
        ErzeugeHauptWeg(karte, startX, startY, endX, endY);

        int minNebenwege = 7 + (hoehe - 10) * 13 / 15;
        int maxNebenwege = 14 + (breite - 10) * 20 / 40;
        int anzahlNebenwege = zufaelig.Next(minNebenwege, maxNebenwege + 1);

        if (hoehe <= 15 && breite <= 25)
        {
            ErzeugeNebenWegeKlein(karte, anzahlNebenwege, startX, startY, endX, endY, hoehe, breite);
        }
        else
        {
            ErzeugeNebenWegeGroß(karte, anzahlNebenwege, startX, startY, endX, endY, hoehe, breite);
        }


        Console.WriteLine("Wollen sie das Schätze und Fallen mit 5% Wahrscheinlichkeit generiert");
        while (true)
        {
            string Eingabe = Console.ReadLine().ToLower();
            if (Eingabe == "ja")
            {
                ErzeugeSchaetzeUndFallen(karte, hoehe, breite);
                break;
            }

            else if (Eingabe == "nein")
            {
                break;
            }
            else
            {
                Console.WriteLine("Ungültige Eingabe! Versuchen sie noch mal mit 'ja' oder 'nein'");
            }
        }
        for (int i = 0; i < zufaelig.Next(2, 5); i++)
        {
            ErzeugeRaum(karte, hoehe, breite);
        }
        SpeichereKarteMitAbfrage(karte, hoehe, breite);

        // Starte das Spiel
        GameLoop(karte, hoehe, breite, startX, startY);
    }

    // ----------------------------------------------------------------------
    // HELFER-METHODEN
    // ----------------------------------------------------------------------

    static int Eingabe(int min, int max)
    {
        int wert;
        while (true)
        {
            try
            {
                wert = Convert.ToInt32(Console.ReadLine());
                if (wert >= min && wert <= max)
                {
                    return wert;
                }
                else
                {
                    Console.WriteLine($"Der Wert muss zwischen {min} und {max} liegen. Bitte erneut eingeben:");
                }
            }
            catch
            {
                Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:");
            }
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

    static void Start_EndeErstellen(char[,] karte, int breite, int hoehe, out int startX, out int startY, out int endX, out int endY)
    {
        int minAbstandY = zufaelig.Next(1, Math.Max(2, hoehe / 5));
        int minAbstandX = zufaelig.Next(1, Math.Max(2, breite / 5));

        startY = zufaelig.Next(1, hoehe - 1);
        startX = zufaelig.Next(1, breite - 1);
        karte[startY, startX] = START;

        do
        {
            endY = zufaelig.Next(1, hoehe - 1);
            endX = zufaelig.Next(1, breite - 1);
        } while (Math.Abs(endY - startY) < minAbstandY || Math.Abs(endX - startX) < minAbstandX);
        karte[endY, endX] = ENDE;

    }

    static void ErzeugeHauptWeg(char[,] karte, int startX, int startY, int endX, int endY)
    {
        int festX = startX;
        int festY = startY;
        while (true)
        {
            if (endY > festY)
            {
                festY++; karte[festY, festX] = GANG;
            }
            else if (endY < festY)
            {
                festY--; karte[festY, festX] = GANG;
            }
            if (endX > festX)
            {
                festX++; karte[festY, festX] = GANG;
            }
            else if (endX < festX)
            {
                festX--; karte[festY, festX] = GANG;
            }
            if (festX == endX && festY == endY)
            {
                break;
            }
        }
        karte[endY, endX] = ENDE;
    }

    static void ErzeugeNebenWegeKlein(char[,] karte, int anzahlNebenwege, int startX, int startY, int endX, int endY, int hoehe, int breite)
    {
        for (int i = 0; i < anzahlNebenwege; i++)
        {
            int festX = startX;
            int festY = startY;

            int laenge = zufaelig.Next(3, 15);

            int richtung = zufaelig.Next(0, 4);

            for (int l = 0; l < laenge; l++)
            {
                if (richtung == 0) festY--;
                if (richtung == 1) festY++;
                if (richtung == 2) festX--;
                if (richtung == 3) festX++;

                if (festX <= 0 || festX >= breite - 1 || festY <= 0 || festY >= hoehe - 1)
                {
                    break;
                }
                if (karte[festY, festX] != WAND)
                {
                    continue;
                }

                karte[festY, festX] = GANG;
            }

        }
    }

    static void ErzeugeNebenWegeGroß(char[,] karte, int anzahlNebenwege, int startX, int startY, int endX, int endY, int hoehe, int breite)
    {
        for (int i = 0; i < anzahlNebenwege; i++)
        {
            int festX, festY;


            do
            {
                festX = zufaelig.Next(1, breite - 1);
                festY = zufaelig.Next(1, hoehe - 1);
            } while (karte[festY, festX] != GANG);

            int laenge = zufaelig.Next(30, 70);

            for (int l = 0; l < laenge; l++)
            {
                int richtung = zufaelig.Next(0, 4);
                if (richtung == 0 && festY > 1)
                {
                    festY--;
                }
                else if (richtung == 1 && festY < hoehe - 2)
                {
                    festY++;
                }
                else if (richtung == 2 && festX > 1)
                {
                    festX--;
                }
                else if (richtung == 3 && festX < breite - 2)
                {
                    festX++;
                }
                if (festX <= 0 || festX >= breite - 1 || festY <= 0 || festY >= hoehe - 1)
                {
                    break;
                }
                if (karte[festY, festX] == GANG || (festX == endX && festY == endY))
                {
                    continue;
                }
                int angrenzendeGange = 0;
                if (karte[festY - 1, festX] == GANG)
                {
                    angrenzendeGange++;
                }
                if (karte[festY + 1, festX] == GANG)
                {
                    angrenzendeGange++;
                }
                if (karte[festY, festX - 1] == GANG)
                {
                    angrenzendeGange++;
                }
                if (karte[festY, festX + 1] == GANG)
                {
                    angrenzendeGange++;
                }

                if (angrenzendeGange > 1)
                {
                    continue;
                }
                karte[festY, festX] = GANG;

                if (festX + 1 < breite - 1 && karte[festY, festX + 1] == WAND)
                {
                    karte[festY, festX + 1] = GANG;
                }
                if (festY + 1 < hoehe - 1 && karte[festY + 1, festX] == WAND)
                {
                    karte[festY + 1, festX] = GANG;
                }
            }
        }
    }

    static void ErzeugeSchaetzeUndFallen(char[,] karte, int hoehe, int breite)
    {
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                if (karte[y, x] == GANG)
                {
                    int zahl = zufaelig.Next(1, 101);
                    if (zahl <= 5)
                    {
                        int ToderF = zufaelig.Next(0, 2);
                        if (ToderF == 0)
                        {
                            karte[y, x] = Schatz;
                        }
                        else
                        {
                            karte[y, x] = Fallen;
                        }
                        Console.ResetColor();
                    }
                }
            }
        }
    }

    static void ErzeugeRaum(char[,] karte, int hoehe, int breite)
    {
        int raumHoehe = zufaelig.Next(4, 8);
        int raumBreite = zufaelig.Next(5, 10);

        int eingangY, eingangX;
        do
        {
            eingangX = zufaelig.Next(2, breite - 2);
            eingangY = zufaelig.Next(2, hoehe - 2);
        } while (karte[eingangY, eingangX] != GANG);

        int raumX = eingangX + 1;
        int raumY = eingangY - raumHoehe / 2;

        if (raumX + raumBreite >= breite - 1)
        {
            return;
        }
        if (raumY < 1)
        {
            raumY = 1;
        }
        if (raumY + raumHoehe >= hoehe - 1)
        {
            return;
        }
        for (int y = 0; y < raumHoehe; y++)
        {
            for (int x = 0; x < raumBreite; x++)
            {
                if (karte[raumY + y, raumX + x] != START && karte[raumY + y, raumX + x] != ENDE && karte[raumY + y, raumX + x] != Schatz && karte[raumY + y, raumX + x] != Fallen)
                {
                    karte[raumY + y, raumX + x] = GANG;
                }
            }
        }
        karte[eingangY, eingangX] = GANG;
    }

    static void SpeichereKarteMitAbfrage(char[,] karte, int hoehe, int breite)
    {
        Console.WriteLine("Möchten Sie die Karte als Datei speichern? (ja/nein)");
        while (true)
        {
            string eingabe = Console.ReadLine().ToLower();

            if (eingabe == "ja")
            {
                Console.WriteLine("Bitte geben Sie den Dateinamen ein (ohne Endung, z. B. dungeon):");

                while (true)
                {
                    string dateiname = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(dateiname))
                    {
                        Console.WriteLine("Ungültiger Dateiname! Bitte erneut eingeben:");
                        continue;
                    }

                    if (!dateiname.EndsWith(".txt"))
                    {
                        dateiname += ".txt";
                    }

                    string pfad = Path.Combine(Directory.GetCurrentDirectory(), dateiname);

                    using (StreamWriter writer = new StreamWriter(pfad))
                    {
                        for (int y = 0; y < hoehe; y++)
                        {
                            for (int x = 0; x < breite; x++)
                            {
                                writer.Write(karte[y, x]);
                            }
                            writer.WriteLine();
                        }
                    }

                    Console.WriteLine($"Karte erfolgreich gespeichert unter: {pfad}");
                    break;
                }
                break;
            }
            else if (eingabe == "nein")
            {
                Console.WriteLine("Karte wurde nicht gespeichert.");
                break;
            }
            else
            {
                Console.WriteLine("Ungültige Eingabe! Bitte 'ja' oder 'nein' eingeben.");
            }
        }
    }

    static void Regeln_Einführung()
    {
        Console.WriteLine("Willkommen zum Random Dungeon Generator by XP !");
        Console.WriteLine("In diesem Spiel geht es darum, einen Dungeon zu entkommen, Schätze zu finden und Fallen zu vermeiden.");
        Console.WriteLine("Der Startpunkt ist mit 'S' markiert und das Ende mit 'E'.");
        Console.WriteLine("Schätze sind mit 'T' und Fallen mit 'F' gekennzeichnet.");
        Console.WriteLine("Viel Glück und viel Spaß beim Erkunden des Dungeons!");
        Console.WriteLine("Bitte nur Enter drücken, um fortzufahren...");
        Console.ReadKey(true);
        Console.Clear();
    }

}