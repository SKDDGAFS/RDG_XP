using System;

public class RDG
{
	public RDG()
	{
		const char WAND = '#';      // Wand symbol

        static char[,] InitialisireKarte(int breite, int hoehe)     // Methode zur Initialisierung der Karte
        {
			char[,] Karte = new char[hoehe, breite];        // 2D-Array für die Karte erstellen

            for (int y = 0; y < hoehe; y++)     // Äusserste Schleife für die Höhe der Karte(y)
            {
				for (int x = 0; x < breite; x++)        // Innere Schleife für die Breite der Karte(x)
                {
					Karte[x, y] = WAND;     // Jede Position der Karte mit dem Wand-Symbol füllen
                }
            }
			return Karte;       // Karte zurückgeben
        }
    }
}
