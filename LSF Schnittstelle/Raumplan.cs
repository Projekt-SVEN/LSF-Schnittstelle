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

        private bool[][] Belegungsplan { get; set; }    //true = belegt

        public Raumplan (string raumNummer)
        {
            RaumNummer = raumNummer;
            Belegungsplan = new bool[7][];

            for (int i = 0; i < 7; i++)
                Belegungsplan[i] = new bool[AnzahlSegmente];
        }

        public void BelegeRaum(TimeSpan begin, TimeSpan end, int beginOffset, int endOffset, List<DayOfWeek> weekDays)
        {
            //Umrechnung in Segmente
            int beginIndex = (int) begin.TotalMinutes / SegmentGröße - beginOffset / SegmentGröße;
            int endIndex = (int) end.TotalMinutes / SegmentGröße - endOffset / SegmentGröße;

            //Nachkorektur wegen Offset
            if (beginIndex < 0)
                beginIndex = 0;
            else if (beginIndex >= AnzahlSegmente)
                beginIndex = AnzahlSegmente - 1;

            if (endIndex < 0)
                endIndex = 0;
            else if (endIndex >= AnzahlSegmente)
                endIndex = AnzahlSegmente - 1;

            //Eintragen der Werte
            for (int i = beginIndex; i < endIndex; i++)
                foreach (DayOfWeek weekDay in weekDays)
                    Belegungsplan[(int)weekDay][i] = true;
        }

        public bool[] getBelegungsplan(DayOfWeek dayOfWeek)
        {
            return Belegungsplan[(int)dayOfWeek];
        }
    }
}
