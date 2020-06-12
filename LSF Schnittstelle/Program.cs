using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace LSF_Schnittstelle
{
    class Program
    {
        static Configuration config;
        static MetaData metaData = new MetaData(); //Existieren nur einmalig

        static void Main(string[] args)
        {
            config = Configuration.ReadParams(args);
            if (config == null)
                System.Environment.Exit(1);

            DownloadFile();
            Dictionary<string, Raumplan> räume = ReadFile();
            PrintRaumplan(räume);

            metaData.print(config.metaFile);
        }

        private static void DownloadFile()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(config.url, config.tempFile);
            }
        }

        private static Dictionary<string, Raumplan> ReadFile()
        {
            Dictionary<string, Raumplan> raumpläne = new Dictionary<string, Raumplan>();

            //Datei öffnen
            StreamReader file = new StreamReader(config.tempFile);

            //Zeilen Einlesen
            string line = null;
            int zeile = 0;
            while ((line = file.ReadLine()) != null)
            {
                zeile++;

                //Zeile Zerlegen
                string[] entries = line.Split('|');
                if (entries.Length != 10) //Falsche anzahl an Spalten
                {
                    Log.Error(String.Format("Zeile {0} hat nur {1} Spalten.", zeile, entries.Length));
                    continue;
                }

                //Parsen
                string veranstaltungsnummer = entries[0].Trim();
                string name                 = entries[1].Trim();
                string semester             = entries[2].Trim();

                DateTime beginDate;
                if (!DateTime.TryParse(entries[3].Trim(), out beginDate))
                {
                    if (entries[3].Trim() == "\\N")
                    {//Keine BeginDatum
                        beginDate = DateTime.MinValue;
                    }
                    else
                    {
                        Log.Error(String.Format("Zeile {0}: Der Eintrag für beginDate [{1}] konnte nicht zu einem Datum geparst werden.", zeile, entries[3].Trim()));
                        continue;
                    }
                }
                beginDate = beginDate.Date;

                DateTime endDate;
                if (!DateTime.TryParse(entries[4].Trim(), out endDate))
                {
                    if (entries[4].Trim() == "\\N")
                    {//Kein EndDatum
                        endDate = DateTime.MaxValue;
                    }
                    else
                    {//Fehler
                        Log.Error(String.Format("Zeile {0}: Der Eintrag für endDate [{1}] konnte nicht zu einem Datum (begin) geparst werden.", zeile, entries[4].Trim()));
                        continue;
                    }
                }
                endDate = endDate.Date;

                DateTime beginTime_;
                if (!DateTime.TryParse(entries[5].Trim(), out beginTime_))
                {
                    if (entries[5].Trim() == "\\N")
                    {//Keine beginn Zeit
                        Log.ignor(zeile, "ganze Veranstalltung [" + name + "]", "beginTime = \\N");
                        continue;
                    }
                    else
                    {
                        Log.Error(String.Format("Zeile {0}: Der Eintrag für beginTime [{1}] konnte nicht zu einem Datum (begin) geparst werden.", zeile, entries[5].Trim()));
                        continue;
                    }
                }
                TimeSpan beginTime = beginTime_.TimeOfDay;

                DateTime endTime_;
                TimeSpan endTime;
                if (!DateTime.TryParse(entries[6].Trim(), out endTime_))
                {
                    if (entries[6].Trim() == "24:00")
                    {//Ende 24:00
                        endTime = new TimeSpan(24, 0, 0);
                    }
                    else
                    {
                        Log.Error(String.Format("Zeile {0}: Der Eintrag für endTime [{1}] konnte nicht zu einem Datum (begin) geparst werden.", zeile, entries[6].Trim()));
                        continue;
                    }
                }
                else
                    endTime = endTime_.TimeOfDay;

                List<DayOfWeek> wochentage = new List<DayOfWeek>();
                switch (entries[7].Trim())
                {
                    case "Mo":
                        wochentage.Add(DayOfWeek.Monday);
                        break;
                    case "Di":
                        wochentage.Add(DayOfWeek.Tuesday);
                        break;
                    case "Mi":
                        wochentage.Add(DayOfWeek.Wednesday);
                        break;
                    case "Do":
                        wochentage.Add(DayOfWeek.Thursday);
                        break;
                    case "Fr":
                        wochentage.Add(DayOfWeek.Friday);
                        break;
                    case "Sa":
                        wochentage.Add(DayOfWeek.Saturday);
                        break;
                    case "So":
                        wochentage.Add(DayOfWeek.Sunday);
                        break;
                    case "-":
                        break;
                    default:
                        Log.Error(String.Format("Zeile {0}: Der Eintrag für Wochentag [{1}] ist unbekannt.", zeile, entries[7].Trim()));
                        continue;
                }

                VeranstalltungArt veranstalltungArt;
                switch (entries[8].Trim())
                {
                    case "Einzeltermin":
                        veranstalltungArt = VeranstalltungArt.Einzeltermin;
                        break;
                    case "Blockveranstaltung + Sa und So":
                        veranstalltungArt = VeranstalltungArt.Blockveranstaltung_Sa_So;
                        wochentage.Add(DayOfWeek.Saturday);
                        wochentage.Add(DayOfWeek.Sunday);
                        break;
                    case "wöchentlich":
                        veranstalltungArt = VeranstalltungArt.Wöchentlich;
                        break;
                    case "14-täglich":
                        Log.ignor(zeile, "VeranstalltungArt", "14-täglich");
                        break;
                    default:
                        Log.Error(String.Format("Zeile {0}: Der Eintrag für VeranstalltungArt [{1}] ist unbekannt.", zeile, entries[8].Trim()));
                        continue;
                }

                string raumNummer = entries[9].Trim();

                if (wochentage.Count == 0)
                {//Weil Wochentag '-' möglich ist, Plausibilitätskontrolle
                    Log.Error(String.Format("Zeile {0}: Wochentage ist Leer!     Eingabewerte:     Wochentage=[{1}]     VeranstalltungArt={2}", zeile, entries[7].Trim(), entries[8].Trim()));
                    continue;
                }

                //Auswertung
                if (config.date >= beginDate && config.date <= endDate)
                {//Das konfigurrte Datum befindet sich innerhalb des Veranstalltungszeitraumes.

                    if (!raumpläne.ContainsKey(raumNummer))
                        raumpläne.Add(raumNummer, new Raumplan(raumNummer));

                    //Eintragen in den Raumplan
                    Raumplan raumplan = raumpläne[raumNummer];
                    raumplan.BelegeRaum(beginTime, endTime, config.vorheitzen, config.abkühlen, wochentage);
                }
            }

            //Datei schließen
            file.Close();

            return raumpläne;
        }

        private static void PrintRaumplan(Dictionary<string, Raumplan> raumpläne)
        {
            //Template: https://wiki.fhem.de/wiki/HomeMatic_HMInfo_TempList/Weekplan

            //Datei erstellen/überschreiben
            StreamWriter writer = new StreamWriter(File.Open(config.ausgabeFile, FileMode.Create));

            //Raumpläne erstellen
            foreach(Raumplan raumplan in raumpläne.Values)
            {
                writer.WriteLine("entities:{0}", Regex.Replace(raumplan.RaumNummer, @"\s+", "_"));
                foreach (DayOfWeek day in (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek))) //Für jeden Tag
                    PrintWeekDay(writer, raumplan, day);
            }

            writer.Close();
        }

        private static void PrintWeekDay(StreamWriter writer, Raumplan raumplan, DayOfWeek dayOfWeek)
        {
            //Tageskürzel laut vorgabe erstellen
            string day = "";
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    day = "Mon";
                    break;
                case DayOfWeek.Tuesday:
                    day = "Tue";
                    break;
                case DayOfWeek.Wednesday:
                    day = "Wed";
                    break;
                case DayOfWeek.Thursday:
                    day = "Thu";
                    break;
                case DayOfWeek.Friday:
                    day = "Fri";
                    break;
                case DayOfWeek.Saturday:
                    day = "Sat";
                    break;
                case DayOfWeek.Sunday:
                    day = "Sun";
                    break;
            }

            //Schriebe Tag
            writer.Write("R_0_tempList{0}>", day);

            //Tagesplan schriben
            for(int i = 1; i <= Raumplan.AnzahlSegmente; i++) //von 1 an zählen, da sonst die Zeitberechnungen verschoben sind
            {
                bool current = raumplan.getBelegungsplan(dayOfWeek)[i - 1];
                bool next = i < Raumplan.AnzahlSegmente ? raumplan.getBelegungsplan(dayOfWeek)[i] : !current;

                if (current != next)
                {//Grenze erreicht
                    //Zurückrechnen in Zeit
                    int stunden = i / Raumplan.SegmenteProStunde;
                    int minuten = i % Raumplan.SegmenteProStunde * Raumplan.SegmentGröße;
                    double temperatur = current ? config.temperaturGenutzt : config.temperaturUngenutzt;

                    //Überbrücken von Pausen
                    int segmentNachPause = i + config.pausenlänge / Raumplan.SegmentGröße;
                    if (current && segmentNachPause < Raumplan.AnzahlSegmente && raumplan.getBelegungsplan(dayOfWeek)[segmentNachPause])
                    {//vorher Belegt && Gültiges Segment && nach Pause Belegt
                        metaData.StartPause(raumplan.RaumNummer, dayOfWeek, new TimeSpan(stunden, minuten, 0));
                        continue;
                    }

                    int segmentVorPause = i - config.pausenlänge / Raumplan.SegmentGröße - 1;   //-1 Weil VOR der Pause
                    if (!current && segmentVorPause > 0 && raumplan.getBelegungsplan(dayOfWeek)[segmentVorPause])
                    {//gerade Pause && Gültiges Segment && vor Pause Belegt
                        metaData.EndPause(raumplan.RaumNummer, dayOfWeek, new TimeSpan(stunden, minuten, 0));
                        continue;
                    }

                    //Schreiben des Befehls
                    writer.Write("{0:00}:{1:00} {2:#.0} ", stunden, minuten, temperatur);
                }
            }

            //Zeile Abschließen
            writer.Write("\n");
        }

        private enum VeranstalltungArt
        {
            Einzeltermin = 0,
            Blockveranstaltung_Sa_So = 1,
            Wöchentlich = 2
        }
    }
}
