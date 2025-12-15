using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // Für Dateizugriff damir wir die Karte speichern können als textdatei

public class RDG
{
    static char WAND = '#';   // Wand-Symbol
    static char GANG = '.';   // Gang-Symbol
    static char START = 'S';  // Start
    static char ENDE = 'E';   // Ende
    static char Schatz = 'T';   // Ende
    static char Fallen = 'F';   // Ende
    static Random zufaelig = new Random();

    static void Regeln_Einführung();

    static void Main(string[] args)
    {
        // TODO: Was noch gemacht werden muss
        // Spieler einbauen

        // Optional Sachen die wir noch machen können
        // Auf alten Karten zugreifen (müssen Herrn Gül fragen, ob wir List benutzen dürfen)
        // Wenn das Dungeon geschafft wurde, Abfrage, ob man noch eines spielen will oder das Programm verlassen will
        // Weitere Ideen hier eintragen

        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe = Eingabe(10, 25);

        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite = Eingabe(10, 50);

        Console.WriteLine($"Dungeon erstellt mit Höhe {hoehe} und Breite {breite}.");

        char[,] karte = InitialisiereKarte(breite, hoehe);
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

        Console.WriteLine("---ZUFALLSDUNGEON---");
        // Karte ausgeben
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                char symbol = karte[y, x]; // aktuelles Zeichen holen

                if (symbol == 'S')
                {
                    Console.ForegroundColor = ConsoleColor.Green;   // Start grün
                }
                else if (symbol == 'E')
                {
                    Console.ForegroundColor = ConsoleColor.Red;     // Ende rot
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
                Console.ResetColor(); // Farbe zurücksetzen
            }
            Console.WriteLine();
        }
        Console.ReadKey();

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
                festY++; karte[festY, festX] = '.';
            }
            else if (endY < festY)
            {
                festY--; karte[festY, festX] = '.';
            }
            if (endX > festX)
            {
                festX++; karte[festY, festX] = '.';
            }
            else if (endX < festX)
            {
                festX--; karte[festY, festX] = '.';
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
        Consoole.WriteLine("Spieler ist mit '@' gekenzeichnet")
        Console.WriteLine("Viel Glück und viel Spaß beim Erkunden des Dungeons!");
    }
