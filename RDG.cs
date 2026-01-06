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

        // Endlosschleife: Menü nach jedem Spiel erneut anzeigen
        while (true)
        {
            // Cursor ausblenden (verhindert Flackern)
            Console.CursorVisible = false;

            Console.WriteLine("Willkommen zum Random Dungeon Generator!");
            menü();
            StarteSpiel();
        }

    }

    // Zeigt das Hauptmenü an und verarbeitet die Nutzereingaben
    static void menü()
    {
        // Endlosschleife, damit das Menü nach Aktionen erneut erscheint
        while (true)
        {
            Console.Clear();

            // Menüoptionen anzeigen
            Console.WriteLine("1. Neues Spiel starten");
            Console.WriteLine("2. Anleitung lesen");
            Console.WriteLine("3. Programm beenden");
            Console.Write("Auswahl: ");

            string eingabe = Console.ReadLine();

            // Eingabe auswerten
            switch (eingabe)
            {
                case "1":
                    // Spiel starten → zurück zur Hauptschleife im Hauptprogramm
                    Console.Clear();
                    return;

                case "2":
                    // Anleitung anzeigen
                    Console.Clear();
                    Einführung();
                    break;

                case "3":
                    // Programm sauber beenden
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("===== Beenden – Auf Wiedersehen! =====");
                    Console.ResetColor();
                    Thread.Sleep(2000);   // 2 Sekunden warten vor dem Beenden
                    Environment.Exit(0);
                    break;

                default:
                    // Ungültige Eingabe → Fehlermeldung anzeigen
                    Console.Clear();
                    Console.WriteLine("Ungültige Eingabe! Bitte erneut eingeben...");
                    Thread.Sleep(1000); // 1 Sekunde warten, damit der Nutzer die Meldung sieht
                    break;
            }
        }
    }

    // Startet Spiel: Dungeon erzeugen, Spieler steuern, Ergebnis anzeigen
    static void StarteSpiel()
    {
        // Dungeon-Höhe abfragen
        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe = Eingabe(10, 25);

        // Dungeon-Breite abfragen
        Console.WriteLine("Wie groß soll die Breite des Dungeons sein (min 10 & max 50)?");
        int breite = Eingabe(10, 50);

        Console.Clear();
        Console.WriteLine($"Dungeon erstellt mit Höhe {hoehe} und Breite {breite}.");

        // Karte initialisieren
        char[,] karte = InitialisiereKarte(breite, hoehe);

        // Start- und Endpunkt setzen
        Start_EndeErstellen(karte, breite, hoehe, out int startX, out int startY, out int endX, out int endY);

        // Hauptweg zwischen Start und Ende erzeugen
        ErzeugeHauptWeg(karte, startX, startY, endX, endY);

        // Anzahl der Nebenwege abhängig von Dungeon-Größe berechnen
        int minNebenwege = 7 + (hoehe - 10) * 13 / 15;
        int maxNebenwege = 14 + (breite - 10) * 20 / 40;

        // Sicherstellen, dass min <= max
        int min = Math.Min(minNebenwege, maxNebenwege);
        int max = Math.Max(minNebenwege, maxNebenwege);

        // Zufällige Anzahl an Nebenwegen bestimmen
        int anzahlNebenwege = zufaelig.Next(min, max + 1);

        // Je nach Dungeon-Größe unterschiedliche Nebenweg-Algorithmen nutzen
        if (hoehe <= 15 && breite <= 25)
        {
            ErzeugeNebenWegeKlein(karte, anzahlNebenwege, startX, startY, endX, endY, hoehe, breite);
        }
        else
        {
            ErzeugeNebenWegeGroß(karte, anzahlNebenwege, startX, startY, endX, endY, hoehe, breite);
        }

        // Optional Schätze und Fallen generieren
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

        // Zufällige Räume erzeugen (2 bis 4)
        for (int i = 0; i < zufaelig.Next(2, 5); i++)
        {
            ErzeugeRaum(karte, hoehe, breite);
        }

        // Start- und Endsymbol nochmal auf die Karte setzen
        karte[startY, startX] = START;
        karte[endY, endX] = ENDE;

        // Karte speichern (mit Abfrage)
        SpeichereKarteMitAbfrage(karte, hoehe, breite);

        // Kurze Spielanleitung anzeigen
        Console.WriteLine();
        Console.WriteLine("--- ZUFALLSDUNGEON ---");
        Console.WriteLine("Steuere den Spieler mit W (hoch), A (links), S (runter), D (rechts).");
        Console.WriteLine("Drücke Enter, um das Spiel zu starten...");

        // Warten bis Enter gedrückt wird
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        Console.Clear();

        // Spielerposition initialisieren
        int spielerX = startX;
        int spielerY = startY;

        // Spielerstatus
        int leben = 3;
        int schatzAnzahl = 0;

        bool zielErreicht = false;

        // Haupt-Spielschleife
        while (!zielErreicht)
        {
            // Karte zeichnen
            ZeichneKarte(karte, spielerX, spielerY, hoehe, breite);

            // Cursor unter die Karte setzen (für Statusmeldungen)
            Console.SetCursorPosition(0, hoehe + 1);

            // Spieler bewegen und prüfen, ob Ziel erreicht wurde
            zielErreicht = SpielerBewegen(karte, ref spielerX, ref spielerY, endX, endY, ref leben, ref schatzAnzahl);
        }

        // Karte ein letztes Mal zeichnen
        ZeichneKarte(karte, spielerX, spielerY, hoehe, breite);

        // Spielende auswerten
        if (leben <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("Du hast kein Leben mehr! Spiel vorbei.");
            Console.WriteLine($"Du hast insgesamt {schatzAnzahl} Schätze gesammelt.");
            Console.WriteLine();
            Console.ResetColor();
            Thread.Sleep(2000);
        }
        else if (zielErreicht)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Herzlichen Glückwunsch! Du hast das Ende erreicht!");
            Console.WriteLine($"Du hast {schatzAnzahl} Schätze gesammelt.");
            Console.WriteLine($"und noch {leben} Leben übrig.");
            Console.WriteLine();
            Console.ResetColor();
            Thread.Sleep(2000);   // 2 Sekunden warten
        }
        // Das wird eigentlich nie erreicht.
        else
        {
            Console.WriteLine("Das Spiel wurde beendet.");
            Thread.Sleep(2000);   // 2 Sekunden warten
        }

        // Zurück zum Menü
        Console.WriteLine("Drücke Enter, um wieder zum Menü zu gelangen...");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        Console.Clear();
    }

    // Liest eine Ganzzahl vom Benutzer ein und stellt sicher,
    // dass sie im angegebenen Bereich liegt
    static int Eingabe(int min, int max)
    {
        int wert;

        // Endlosschleife, bis eine gültige Eingabe erfolgt
        while (true)
        {
            try
            {
                // Versuch, die Eingabe in eine ganze Zahl umzuwandeln
                wert = Convert.ToInt32(Console.ReadLine());

                // Prüfen, ob der Wert im erlaubten Bereich liegt
                if (wert >= min && wert <= max)
                {
                    return wert; // gültige Eingabe → zurückgeben
                }
                else
                {
                    // Zahl ist gültig, aber außerhalb des Bereichs
                    Console.WriteLine($"Der Wert muss zwischen {min} und {max} liegen. Bitte erneut eingeben:");
                }
            }
            catch
            {
                // Eingabe war keine Zahl (z. B. Buchstaben)
                Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:");
            }
        }
    }

    // Erstellt eine neue Dungeon-Karte und füllt sie komplett mit Wänden
    static char[,] InitialisiereKarte(int breite, int hoehe)
    {
        // 2D-Array für die Karte anlegen (hoehe = Zeilen, breite = Spalten)
        char[,] karte = new char[hoehe, breite];

        // Jede Position der Karte durchlaufen
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                // Standardwert setzen: alles beginnt als Wand
                karte[y, x] = WAND;
            }
        }

        // Fertige Karte zurückgeben
        return karte;
    }

    // Setzt zufällig Start- und Endpunkt auf der Karte
    static void Start_EndeErstellen(char[,] karte, int breite, int hoehe, out int startX, out int startY, out int endX, out int endY)
    {
        // Mindestabstand zwischen Start und Ende berechnen
        int minAbstandY = zufaelig.Next(1, Math.Max(2, hoehe / 5));
        int minAbstandX = zufaelig.Next(1, Math.Max(2, breite / 5));

        // Zufällige Startposition innerhalb der Karte (nicht am Rand)
        startY = zufaelig.Next(1, hoehe - 1);
        startX = zufaelig.Next(1, breite - 1);

        // Startpunkt auf der Karte setzen
        karte[startY, startX] = START;

        // Endpunkt so lange zufällig setzen,
        // bis der Mindestabstand zu Start erfüllt ist
        do
        {
            endY = zufaelig.Next(1, hoehe - 1);
            endX = zufaelig.Next(1, breite - 1);

        } while (Math.Abs(endY - startY) < minAbstandY || Math.Abs(endX - startX) < minAbstandX);

        // Endpunkt auf der Karte setzen
        karte[endY, endX] = ENDE;
    }

    // Erzeugt den Hauptweg zwischen Start- und Endpunkt
    static void ErzeugeHauptWeg(char[,] karte, int startX, int startY, int endX, int endY)
    {
        // Aktuelle Position beginnt beim Startpunkt
        int festX = startX;
        int festY = startY;

        // Solange wir das Ziel noch nicht erreicht haben
        while (true)
        {
            // Schritt in Y-Richtung auf das Ziel zu
            if (endY > festY)
            {
                festY++;   // nach unten gehen
            }
            else if (endY < festY)
            {
                festY--;   // nach oben gehen
            }

            // Feld als Weg markieren
            karte[festY, festX] = '.';

            // Schritt in X-Richtung auf das Ziel zu
            if (endX > festX)
            {
                festX++;   // nach rechts gehen
            }
            else if (endX < festX)
            {
                festX--;   // nach links gehen
            }

            // Feld als Weg markieren
            karte[festY, festX] = '.';

            // Wenn beide Koordinaten erreicht sind → fertig
            if (festX == endX && festY == endY)
            {
                break;
            }
        }

        // Endpunkt explizit setzen (falls überschrieben)
        karte[endY, endX] = ENDE;
    }

    // Erzeugt kurze Nebenwege für kleine Dungeons
    static void ErzeugeNebenWegeKlein(char[,] karte, int anzahlNebenwege, int startX, int startY, int endX, int endY, int hoehe, int breite)
    {
        // Schleife für alle Nebenwege
        for (int i = 0; i < anzahlNebenwege; i++)
        {
            // Startpunkt des Nebenwegs (immer vom Start ausgehend)
            int festX = startX;
            int festY = startY;

            // Zufällige Länge des Nebenwegs (3–14 Felder)
            int laenge = zufaelig.Next(3, 15);

            // Zufällige Richtung: 0=oben, 1=unten, 2=links, 3=rechts
            int richtung = zufaelig.Next(0, 4);

            // Nebenweg Schritt für Schritt erzeugen
            for (int l = 0; l < laenge; l++)
            {
                // Bewegung entsprechend der gewählten Richtung
                if (richtung == 0) festY--;     // nach oben
                if (richtung == 1) festY++;     // nach unten
                if (richtung == 2) festX--;     // nach links
                if (richtung == 3) festX++;     // nach rechts

                // Wenn der Weg die Karte verlassen würde → abbrechen
                if (festX <= 0 || festX >= breite - 1 || festY <= 0 || festY >= hoehe - 1)
                {
                    break;
                }

                // Wenn das Feld kein Wand ist → überspringen
                if (karte[festY, festX] != WAND)
                {
                    continue;
                }

                // Feld als Gang setzen
                karte[festY, festX] = GANG;
            }
        }
    }

    // Erzeugt längere Nebenwege für große Dungeons
    static void ErzeugeNebenWegeGroß(char[,] karte, int anzahlNebenwege, int startX, int startY, int endX, int endY, int hoehe, int breite)
    {
        // Schleife für alle Nebenwege
        for (int i = 0; i < anzahlNebenwege; i++)
        {
            int festX, festY;

            // Startpunkt des Nebenwegs: zufälliger Gang auf der Karte
            do
            {
                festX = zufaelig.Next(1, breite - 1);
                festY = zufaelig.Next(1, hoehe - 1);
            }
            while (karte[festY, festX] != GANG);

            // Länge des Nebenwegs 
            int laenge = zufaelig.Next(30, 70);

            // Nebenweg Schritt für Schritt erzeugen
            for (int l = 0; l < laenge; l++)
            {
                // Zufällige Richtung wählen
                int richtung = zufaelig.Next(0, 4);

                // Bewegung nur durchführen, wenn innerhalb der Karte
                if (richtung == 0 && festY > 1)          // oben
                    festY--;
                else if (richtung == 1 && festY < hoehe - 2) // unten
                    festY++;
                else if (richtung == 2 && festX > 1)     // links
                    festX--;
                else if (richtung == 3 && festX < breite - 2) // rechts
                    festX++;

                // Wenn der Weg die Karte verlassen würde → abbrechen
                if (festX <= 0 || festX >= breite - 1 || festY <= 0 || festY >= hoehe - 1)
                {
                    break;
                }

                // Keine Überschreibung von bestehenden Gängen oder dem Endpunkt
                if (karte[festY, festX] == GANG || (festX == endX && festY == endY))
                {
                    continue;
                }

                // Prüfen, wie viele angrenzende Felder bereits Gänge sind
                // → verhindert große offene Flächen oder Schleifen
                int angrenzendeGange = 0;

                if (karte[festY - 1, festX] == GANG) angrenzendeGange++;
                if (karte[festY + 1, festX] == GANG) angrenzendeGange++;
                if (karte[festY, festX - 1] == GANG) angrenzendeGange++;
                if (karte[festY, festX + 1] == GANG) angrenzendeGange++;

                // Wenn mehr als 1 Gang angrenzend ist → kein neuer Weg
                if (angrenzendeGange > 1)
                {
                    continue;
                }

                // Feld als Gang markieren
                karte[festY, festX] = GANG;
            }
        }
    }

    // Erzeugt zufällig Schätze und Fallen auf der Karte mit 5% Wahrscheinlichkeit pro Feld
    static void ErzeugeSchaetzeUndFallen(char[,] karte, int hoehe, int breite)
    {
        // Alle Felder der Karte durchlaufen
        for (int y = 0; y < hoehe; y++)
        {
            for (int x = 0; x < breite; x++)
            {
                // Nur auf Gängen platzieren
                if (karte[y, x] == '.')
                {
                    // 5% Wahrscheinlichkeit
                    int zahl = zufaelig.Next(1, 101);

                    if (zahl <= 5)
                    {
                        // Zufällig entscheiden, ob Schatz oder Falle
                        int ToderF = zufaelig.Next(0, 2);

                        if (ToderF == 0)
                        {
                            karte[y, x] = Schatz;   // Schatz platzieren
                        }
                        else
                        {
                            karte[y, x] = Fallen;   // Falle platzieren
                        }
                    }
                }
            }
        }
    }

    // Erzeugt einen rechteckigen Raum an einem zufälligen Gang ohne überschreiben wichtiger Symbole
    static void ErzeugeRaum(char[,] karte, int hoehe, int breite)
    {
        // Zufällige Raumgröße bestimmen
        int raumHoehe = zufaelig.Next(4, 8);   // Höhe: 4–7
        int raumBreite = zufaelig.Next(5, 10); // Breite: 5–9

        int eingangY, eingangX;

        // Einen zufälligen Gang als Eingangspunkt finden
        do
        {
            eingangX = zufaelig.Next(2, breite - 2);
            eingangY = zufaelig.Next(2, hoehe - 2);
        }
        while (karte[eingangY, eingangX] != GANG);

        // Prüfen, ob Raum in die Karte passt
        int raumX = eingangX + 1;                 // Raum rechts vom Eingang
        int raumY = eingangY - raumHoehe / 2;     // Raum mittig zum Eingang

        // Falls der Raum rechts aus der Karte ragen würde
        if (raumX + raumBreite >= breite - 1)
        {
            return; // Raum passt nicht → abbrechen
        }

        // Falls der Raum oben aus der Karte ragen würde → nach unten verschieben
        if (raumY < 1)
        {
            raumY = 1;
        }

        // Falls der Raum unten aus der Karte ragen würde 
        if (raumY + raumHoehe >= hoehe - 1)
        {
            return; // Raum passt nicht → abbrechen
        }

        // Raum zeichnen
        for (int y = 0; y < raumHoehe; y++)
        {
            for (int x = 0; x < raumBreite; x++)
            {
                // Nur Raumfläche setzen, wenn kein wichtiger Punkt überschrieben wird
                if (karte[raumY + y, raumX + x] != START &&
                    karte[raumY + y, raumX + x] != ENDE &&
                    karte[raumY + y, raumX + x] != Schatz &&
                    karte[raumY + y, raumX + x] != Fallen)
                {
                    karte[raumY + y, raumX + x] = GANG; // Raumfläche setzen
                }
            }
        }

        // Eingang wieder als Gang markieren
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

    // Einführung und Spielanleitung 
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
