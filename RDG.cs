using System;

public class RDG
{
    // Konstanten für die Symbole im Dungeon (gemäß Pflichtenheft)
    const char WAND = '#';    // Wand-Symbol
    const char GANG = '.';    // Gang-Symbol
    const char START = 'S';   // Start-Symbol
    const char ENDE = 'E';    // Ende-Symbol

    static void Main(string[] args)
    {
        // ===========================================
        // 1. DIMENSIONEN EINLESEN UND VALIDIEREN
        // ===========================================

        // --- HÖHE abfragen (min 10 & max 25) ---
        Console.WriteLine("Wie groß soll die Höhe des Dungeons sein (min 10 & max 25)?");
        int hoehe;
        while (true) // Endlosschleife zur Wiederholung bei ungültiger Eingabe
        {
            try
            {
                // Liest Eingabe und versucht, diese in Zahl umzuwandeln
                hoehe = Convert.ToInt32(Console.ReadLine());

                // Prüft, ob die Zahl im zulässigen Bereich liegt
                if (hoehe < 10 || hoehe > 25)
                {
                    Console.WriteLine("Die Höhe darf nicht kleiner als 10 oder Höhe als 25 sein. \nVersuchen Sie es erneut:");
                }
                else
                {
                    break; // Gültige Eingabe: Schleife verlassen
                }
            }
            catch
            {
                // Fehler, wenn Eingabe keine Zahl war
                Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:");
            }
        }

        // --- BREITE abfragen (min 10 & max 50) ---
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
                    break; // Gültige Eingabe: Schleife verlassen
                }
            }
            catch
            {
                Console.WriteLine("Fehler! Bitte geben Sie eine Zahl ein:");
            }
        }

        // ===========================================
        // 2. INITIALISIERUNG
        // ===========================================

        // Zentrales Objekt zur Generierung von Zufallszahlen
        Random zufaellig = new Random();

        // Erstellt das 2D-Array (char[,]) und füllt es komplett mit Wänden ('#')
        char[,] karte = InitialisiereKarte(breite, hoehe);

        // ===========================================
        // 3. ZUFÄLLIGE GANG-PLATZIERUNG (30% Zufallslöcher)
        // ===========================================

        int total = hoehe * breite;
        int gangAnzahl = total * 30 / 100; // Berechnet 30% der Gesamtfläche

        if (gangAnzahl > 0)
        {
            // Erstellt ein 1D-Array mit allen möglichen Indizes (0 bis total-1)
            int[] indices = new int[total];
            for (int i = 0; i < total; i++)
            {
                indices[i] = i;
            }

            // Schleife wählt genau 'gangAnzahl' Zellen zufällig aus (Fisher-Yates Shuffle)
            for (int i = 0; i < gangAnzahl; i++)
            {
                // Wählt eine zufällige Position 'j' im Rest des 'indices'-Arrays
                int j = zufaellig.Next(i, total);

                // --- Tausch (Swap) der Werte an Position i und j ---
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;

                // --- Umrechnung von 1D-Index in 2D-Koordinate (y, x) ---
                int index = indices[i];
                int y = index / breite;   // Zeile (Y) durch Ganzzahl-Division
                int x = index % breite;   // Spalte (X) durch Modulo (Rest)

                // Setzt die Zelle im 2D-Gitter auf Gang ('.')
                karte[y, x] = GANG;
            }
        }

        
    }

    // Erstellt das 2D-Array (char[,]) und füllt es vollständig mit Wänden ('#').
    static char[,] InitialisiereKarte(int breite, int hoehe)
    {
        // Erstellung des 2D-Arrays: [Zeilen (y), Spalten (x)]
        char[,] karte = new char[hoehe, breite];

        // Durchlaufen jeder Zelle des Gitters
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