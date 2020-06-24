using System;
using System.Collections.Generic;
using System.Text;

namespace LSF_Schnittstelle
{
    class Raumplan
    {
        public string RaumNummer { get; private set; }
        public const int AnzahlSegmente = 24 * 60 / SegmentGröße;
        public const int SegmentGröße = 15;
        public const int SegmenteProStunde = 60 / SegmentGröße;

        private bool[][] Heizplan { get; set; }    //true = belegt
        public bool[][] Belegungsplan { get; private set; } //Heizplan ohne offset

        public Raumplan (string raumNummer)
        {
            RaumNummer = raumNummer;
            Heizplan = new bool[7][];
            Belegungsplan = new bool[7][];

            for (int i = 0; i < 7; i++)
            {
                Heizplan[i] = new bool[AnzahlSegmente];
                Belegungsplan[i] = new bool[AnzahlSegmente];
            }
        }

        public void BelegeRaum(TimeSpan begin, TimeSpan end, int beginOffset, int endOffset, List<DayOfWeek> weekDays)
        {
            //Umrechnung in Segmente
            int beginIndexOhneOffset = (int)begin.TotalMinutes / SegmentGröße;
            int endIndexOhneOffset = (int)end.TotalMinutes / SegmentGröße;
            int beginIndex = (int)beginIndexOhneOffset - beginOffset / SegmentGröße;
            int endIndex = (int)endIndexOhneOffset - endOffset / SegmentGröße;

            //Nachkorektur wegen Offset
            if (beginIndex < 0)
                beginIndex = 0;
            else if (beginIndex >= AnzahlSegmente)
                beginIndex = AnzahlSegmente - 1;

            if (endIndex < 0)
                endIndex = 0;
            else if (endIndex >= AnzahlSegmente)
                endIndex = AnzahlSegmente - 1;

            //Eintragen der Werte Heizplan
            for (int i = beginIndex; i < endIndex; i++)
                foreach (DayOfWeek weekDay in weekDays)
                    Heizplan[(int)weekDay][i] = true;

            //Eintragen der Werte Belegungslan
            for (int i = beginIndexOhneOffset; i < endIndexOhneOffset; i++)
                foreach (DayOfWeek weekDay in weekDays)
                    Belegungsplan[(int)weekDay][i] = true;
        }

        public bool[] getHeizplan(DayOfWeek dayOfWeek)
        {
            return Heizplan[(int)dayOfWeek];
        }
    }
}
