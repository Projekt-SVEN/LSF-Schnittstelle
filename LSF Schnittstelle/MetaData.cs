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

            public Room()
            {
                Pausen = new Dictionary<string, List<Pause>>();
            }

            public void StartPause(DayOfWeek  dayOfWeek, TimeSpan time)
            {
                //Liste für Wochentag
                string nameOfDay = Enum.GetName(typeof(DayOfWeek), dayOfWeek);
                if (!Pausen.ContainsKey(nameOfDay))
                    Pausen.Add(nameOfDay, new List<Pause>());
                List<Pause> pausen = Pausen[nameOfDay];

                if (pausen.Count != 0)
                {//Elemente Vorhanden

                    //Check ob letzte Pause abgeschlossen
                    if (pausen[pausen.Count - 1].End == default(TimeSpan))
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
                    Begin = time
                };
                pausen.Add(pause);
            }

            public void EndPause(DayOfWeek dayOfWeek, TimeSpan time)
            {
                //Liste für Wochentag
                string nameOfDay = Enum.GetName(typeof(DayOfWeek), dayOfWeek);
                if (!Pausen.ContainsKey(nameOfDay))
                    Pausen.Add(nameOfDay, new List<Pause>());
                List<Pause> pausen = Pausen[nameOfDay];

                if (pausen.Count != 0)
                {//Elemente Vorhanden

                    //Check ob letzte Pause bereits abgeschlossen
                    if (pausen[pausen.Count - 1].End != default(TimeSpan))
                    {
                        //Log
                        Log.Error("Pause hat keinen Anfang");
                        return;
                    }

                    //Einfügen
                    pausen[pausen.Count - 1].End = time;
                }
                else
                    Log.Error("Pause hat keinen Anfang");
            }
        }

        public class Pause
        {
            public TimeSpan Begin { get; set; }
            public TimeSpan End { get; set; }
        }
    }
}
