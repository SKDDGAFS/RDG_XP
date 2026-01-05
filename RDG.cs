using System;
using System.IO; // Für Dateizugriff damit wir die Karte speichern können als Textdatei
using System.Threading;

public class RDG
{
    static char WAND = '#';   // Wand-Symbol
    static char GANG = '.';   // Gang-Symbol
    static char START = 'S';  // Start
    static char ENDE = 'E';   // Ende
    static char Schatz = 'T'; // Schatz
    static char Fallen = 'F'; // Falle
    static Random zufaelig = new Random();

    static void Main(string[] args)
    {
        // TODO: Was noch gemacht werden muss
        // kommentare und struktur verbessern und lesbarer machen


        // Optional Sachen die wir noch machen können
        // schwierigkeitsgrad auswählen (mehr schätze/fallen/leben)
        // Weitere Ideen hier eintragen

        while (true)
        {
            Console.CursorVisible = false;
            Console.WriteLine("Willkommen zum Random Dungeon Generator!");
            menü();
            StarteSpiel();
        }
    }

    static void menü()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Neues Spiel starten");
            Console.WriteLine("2. Anleitung lesen");
            Console.WriteLine("3. Programm beenden");
            Console.Write("Auswahl: ");

            string eingabe = Console.ReadLine();

            switch (eingabe)
            {
                case "1":
                    Console.Clear();
                    return;
                case "2":
                    Console.Clear();
                    Einführung();
                    break;

                case "3":
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("===== Beenden – Auf Wiedersehen! =====");
                    Console.ResetColor();
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                    break;

                default:
                    Console.Clear();
                    Console.WriteLine("Ungültige Eingabe! Bitte erneut eingeben...");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    static void StarteSpiel()
    {
        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe = Eingabe(10, 25);

        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite = Eingabe(10, 50);

        Console.Clear();
        Console.WriteLine($"Dungeon erstellt mit Höhe {hoehe} und Breite {breite}.");

        char[,] karte = InitialisiereKarte(breite, hoehe);
        Start_EndeErstellen(karte, breite, hoehe, out int startX, out int startY, out int endX, out int endY);
        ErzeugeHauptWeg(karte, startX, startY, endX, endY);

        int minNebenwege = 7 + (hoehe - 10) * 13 / 15;
        int maxNebenwege = 14 + (breite - 10) * 20 / 40;

        int min = Math.Min(minNebenwege, maxNebenwege);
        int max = Math.Max(minNebenwege, maxNebenwege);

        int anzahlNebenwege = zufaelig.Next(min, max + 1);

        if (hoehe <= 15 && breite <= 25)
        {
            ErzeugeNebenWegeKlein(karte, anzahlNebenwege, startX, startY, endX, endY, hoehe, breite);
        }
        else
        {
            ErzeugeNebenWegeGroß(karte, anzahlNebenwege, startX, startY, endX, endY, hoehe, breite);
        }

        Console.WriteLine("Wollen Sie, dass Schätze und Fallen mit 5% Wahrscheinlichkeit generiert werden?");
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
                Console.WriteLine("Ungültige Eingabe! Versuchen Sie es nochmal mit 'ja' oder 'nein'.");
            }
        }

        for (int i = 0; i < zufaelig.Next(2, 5); i++)
        {
            ErzeugeRaum(karte, hoehe, breite);
        }
        karte[startY, startX] = START;
        karte[endY, endX] = ENDE;

        SpeichereKarteMitAbfrage(karte, hoehe, breite);

        Console.WriteLine();
        Console.WriteLine("--- ZUFALLSDUNGEON ---");
        Console.WriteLine("Steuere den Spieler mit W (hoch), A (links), S (runter), D (rechts).");
        Console.WriteLine("Bitte nur Enter drücken, um fortzufahren...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        Console.Clear();
        // Spieler starten
        int spielerX = startX;
        int spielerY = startY;

        int leben = 1;
        int schatzAnzahl = 0;


        bool zielErreicht = false;

        while (!zielErreicht)
        {
            ZeichneKarte(karte, spielerX, spielerY, hoehe, breite);

            // Cursor unter die Karte setzen
            Console.SetCursorPosition(0, hoehe + 1);

            // Spieler bewegen
            zielErreicht = SpielerBewegen(karte, ref spielerX, ref spielerY, endX, endY, ref leben, ref schatzAnzahl);
        }

        ZeichneKarte(karte, spielerX, spielerY, hoehe, breite);

        if (leben <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("Du hast kein Leben mehr! Spiel vorbei.");
            Console.WriteLine($"Du hast insgesamt {schatzAnzahl} Schätze gesammelt.      ");
            Console.WriteLine();
            Console.ResetColor();
            Thread.Sleep(2000);
        }
        else if (zielErreicht)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Herzlichen Glückwunsch! Du hast das Ende erreicht!");
            Console.WriteLine($"Du hast {schatzAnzahl} Schätze gesammelt.           ");
            Console.WriteLine($"und noch {leben} Leben übrig.");
            Console.WriteLine();
            Console.ResetColor();
            Thread.Sleep(2000);
        }
        else
        {
            Console.WriteLine("Das Spiel wurde beendet.");
        }

        Console.WriteLine("Drücke Enter, um wieder zum Menü zu gelangen...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        Console.Clear();

    }


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
                karte[y, x] = WAND;
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
                festY++;
            }
            else if (endY < festY)
            {
                festY--;
            }
            karte[festY, festX] = '.';

            if (endX > festX)
            {
                festX++;
            }
            else if (endX < festX)
            {
                festX--;
            }
            karte[festY, festX] = '.';

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
                if (richtung == 0)
                {
                    festY--;
                }
                if (richtung == 1)
                {
                    festY++;
                }
                if (richtung == 2)
                {
                    festX--;
                }
                if (richtung == 3)
                {
                    festX++;
                }

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
            }
        }
    }

    static void ErzeugeSchaetzeUndFallen(char[,] karte, int hoehe, int breite)
    {
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                if (karte[y, x] == '.')
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
                if (karte[raumY + y, raumX + x] != START &&
                    karte[raumY + y, raumX + x] != ENDE &&
                    karte[raumY + y, raumX + x] != Schatz &&
                    karte[raumY + y, raumX + x] != Fallen)
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

    static void Einführung()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("===== RANDOM DUNGEON GENERATOR =====");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("Du befindest dich in einem zufällig erzeugten Dungeon voller Geheimnisse.");
        Console.WriteLine("Dein Ziel ist einfach: Finde den Ausgang und überlebe dabei die Gefahren.");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("S  = Startpunkt");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("E  = Ausgang");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("T  = Schatz (sammelbar)");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("F  = Falle (zieht dir ein Leben ab)");
        Console.ResetColor();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("Du startest mit 3 Leben.");
        Console.WriteLine("Verlierst du alle Leben, ist das Spiel vorbei.");
        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine("Du steuerst deinen Charakter mit:");
        Console.WriteLine("W = hoch,  A = links,  S = runter,  D = rechts");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Viel Glück... du wirst es brauchen.");
        Console.ResetColor();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Drücke Enter, um zum Menü zurückzukehren...");
        Console.ResetColor();

        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

        Console.Clear();
        menü();
    }


    static bool SpielerBewegen(char[,] karte, ref int spielerX, ref int spielerY, int endX, int endY, ref int leben, ref int schatzAnzahl)
    {
        ConsoleKeyInfo taste = Console.ReadKey(true);

        int neuX = spielerX;
        int neuY = spielerY;

        if (taste.Key == ConsoleKey.W)
        {
            neuY--;
        }
        else if (taste.Key == ConsoleKey.S)
        {
            neuY++;
        }
        else if (taste.Key == ConsoleKey.A)
        {
            neuX--;
        }
        else if (taste.Key == ConsoleKey.D)
        {
            neuX++;
        }

        // Grenzen prüfen
        if (neuX < 0 || neuX >= karte.GetLength(1) || neuY < 0 || neuY >= karte.GetLength(0))
        {
            return false;
        }
        // Nur begehbare Felder erlauben
        char zielFeld = karte[neuY, neuX];
        if (zielFeld != GANG && zielFeld != START && zielFeld != ENDE && zielFeld != Schatz && zielFeld != Fallen)
        {
            return false;
        }

        if (zielFeld == Schatz)
        {
            schatzAnzahl++;
            Console.SetCursorPosition(0, karte.GetLength(0) + 3);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Du hast einen Schatz gefunden! Gesamt: {schatzAnzahl}");
            Console.ResetColor();
        }
        if (zielFeld == Fallen)
        {
            leben--;
            Console.SetCursorPosition(0, karte.GetLength(0));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Du bist in eine Falle getappt! Leben übrig: {leben}");
            Console.ResetColor();
            if (leben <= 0)
            {
                return true;
            }
        }

        if (karte[spielerY, spielerX] != START && karte[spielerY, spielerX] != ENDE)
        {
            karte[spielerY, spielerX] = GANG;
        }

        // Spielerposition aktualisieren
        spielerX = neuX;
        spielerY = neuY;

        // Ziel erreicht?
        return (spielerX == endX && spielerY == endY);
    }

    static void ZeichneKarte(char[,] karte, int spielerX, int spielerY, int hoehe, int breite)
    {
        Console.SetCursorPosition(0, 0);
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                if (x == spielerX && y == spielerY)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write('@');
                }
                else
                {
                    char symbol = karte[y, x];
                    if (symbol == 'S')
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if (symbol == 'E')
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (symbol == 'T')
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    }
                    else if (symbol == 'F')
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }
                    else
                    {
                        Console.ResetColor();
                    }
                    Console.Write(symbol);
                }
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }
}
