using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace LSF_Schnittstelle
{
    class MetaData
    {
        public Dictionary<string, Room> Rooms { get; set; } //Muss Member sein, sonst nicht Serialisiert
        public int Segmentgroesse { get; set; }

        public MetaData()
        {
            Rooms = new Dictionary<string, Room>();
        }

        public void StartPause(string roomNr, DayOfWeek dayOfWeek, TimeSpan time)
        {
            Room room = getRoom(roomNr);
            room.StartPause(dayOfWeek, time);
        }

        public void EndPause(string roomNr, DayOfWeek dayOfWeek, TimeSpan time)
        {
            Room room = getRoom(roomNr);
            room.EndPause(dayOfWeek, time);
        }

        public void setBelegung(string roomNr, bool[][] belegungsplan)
        {
            Room room = getRoom(roomNr);
            room.setBelegung(belegungsplan);
        }

        public void print(string metaFile)
        {
            //Erzeuge JSON
            string jsonString;
            jsonString = JsonSerializer.Serialize(this);

            //Schreibe Ausgabe
            StreamWriter writer = new StreamWriter(File.Open(metaFile, FileMode.Create));
            writer.Write(jsonString);
            writer.Close();
        }

        private Room getRoom(string roomNr)
        {
            if (!Rooms.ContainsKey(roomNr))
                Rooms.Add(roomNr, new Room());

            return Rooms[roomNr];
        }

        public class Room
        {
            public Dictionary<string, List<Pause>> Pausen { get; set; }
            public Dictionary<string, List<bool>> Belegung { get; set; }

            public Room()
            {
                Pausen = new Dictionary<string, List<Pause>>();
                Belegung = new Dictionary<string, List<bool>>();
            }

            public void StartPause(DayOfWeek  dayOfWeek, TimeSpan time)
            {
                //Korrektur um Offset
                time = time.Add(new TimeSpan(0, Program.config.vorheitzen, 0));
                
                //Liste für Wochentag
                string nameOfDay = Enum.GetName(typeof(DayOfWeek), dayOfWeek);
                if (!Pausen.ContainsKey(nameOfDay))
                    Pausen.Add(nameOfDay, new List<Pause>());
                List<Pause> pausen = Pausen[nameOfDay];

                if (pausen.Count != 0)
                {//Elemente Vorhanden

                    //Check ob letzte Pause abgeschlossen
                    if (pausen[pausen.Count - 1].End == new MetaTime(default(TimeSpan)))
                    {
                        //Log
                        Log.Error("Pause hat kein Endzeitpunkt");
                        //Pause von länge 0
                        pausen[pausen.Count - 1].End = pausen[pausen.Count - 1].Begin;
                    }
                }

                //Einfügen
                Pause pause = new Pause()
                {
                    Begin = new MetaTime(time),
                    End = new MetaTime(default(TimeSpan))
                };
                pausen.Add(pause);
            }

            public void EndPause(DayOfWeek dayOfWeek, TimeSpan time)
            {
                //Korrektur um Offset
                time = time.Add(new TimeSpan(0, Program.config.abkühlen, 0));

                //Liste für Wochentag
                string nameOfDay = Enum.GetName(typeof(DayOfWeek), dayOfWeek);
                if (!Pausen.ContainsKey(nameOfDay))
                    Pausen.Add(nameOfDay, new List<Pause>());
                List<Pause> pausen = Pausen[nameOfDay];

                if (pausen.Count != 0)
                {//Elemente Vorhanden

                    //Check ob letzte Pause bereits abgeschlossen
                    if (pausen[pausen.Count - 1].End != new MetaTime(default(TimeSpan)))
                    {
                        //Log
                        Log.Error("Pause hat keinen Anfang");
                        return;
                    }

                    //Einfügen
                    pausen[pausen.Count - 1].End = new MetaTime(time);
                }
                else
                    Log.Error("Pause hat keinen Anfang");
            }

            public void setBelegung(bool[][] belegungsplan)
            {
                for (int index = 0; index < belegungsplan.Length; index++)
                {
                    string nameOfDay = Enum.GetName(typeof(DayOfWeek), (DayOfWeek)index);
                    Belegung.Add(nameOfDay, new List<bool>(belegungsplan[index]));
                }
            }
        }

        public class Pause
        {
            public MetaTime Begin { get; set; }
            public MetaTime End { get; set; }
        }

        public class MetaTime
        {
            public int Hours { get; set; }
            public int Minutes { get; set; }
            public int Seconds { get; set; }

            public MetaTime(TimeSpan time)
            {
                Hours = time.Hours;
                Minutes = time.Minutes;
                Seconds = time.Seconds;
            }

            public static bool operator ==(MetaTime a, MetaTime b)
            {
                if (a is null)
                    return b is null;
                if (b is null)
                    return false;

                return a.Hours == b.Hours && a.Minutes == b.Minutes && a.Seconds == b.Seconds;
            }

            public static bool operator !=(MetaTime a, MetaTime b)
            {
                return !(a == b);
            }
        }
    }
}
