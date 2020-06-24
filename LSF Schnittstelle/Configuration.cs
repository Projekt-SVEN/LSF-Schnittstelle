using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LSF_Schnittstelle
{
    class Configuration
    {
        public string url = "https://auskunft.hs-harz.de/Studenten/veranstaltungen.txt";
        public string tempFile = "veranstaltungen.txt";
        public string ausgabeFile = "plan.cfg";
        public string metaFile = "metadata.json";

        //public DateTime date = DateTime.Parse("15.10.2019").Date;
        public DateTime date = DateTime.Now;

        public double temperaturUngenutzt = 17.0;
        public double temperaturGenutzt = 22.0;

        public int vorheitzen = 45;         //Wie viel Minuten vor beginn soll vorgeheitzt werden (Minuten)
        public int abkühlen = 45;           //Wie viel früher soll die Heizung abgeschaltet werden (Minuten)
        public int pausenlänge = 15;       //Länge der Pausen die beim Heizen Überbrückt werden sollen

        private Configuration() { }

        public static Configuration ReadParams(string[] args)
        {
            Configuration config = new Configuration();

            int i = 0;
            try
            {
                for (i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-url":
                        case "-u":
                            config.url = args[++i];

                            //Check is URI      Quelle:https://stackoverflow.com/questions/7578857/how-to-check-whether-a-string-is-a-valid-http-url
                            Uri uriResult;
                            bool result = Uri.TryCreate(config.url, UriKind.Absolute, out uriResult)
                                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                            if (!result)
                            {
                                Console.WriteLine("\"{0}\" ist keine gültige url", config.url);
                                return null;
                            }

                            break;
                        case "-tempFile":
                        case "-t":
                            config.tempFile = args[++i];

                            //gültiger Name?
                            if (config.tempFile.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                            {
                                Console.WriteLine("\"{0}\" kein valider Dateiname", config.tempFile);
                                return null;
                            }

                            break;
                        case "-out":
                        case "-o":
                            config.ausgabeFile = args[++i];

                            //gültiger Name?
                            if (config.ausgabeFile.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                            {
                                Console.WriteLine("\"{0}\" kein valider Dateiname", config.ausgabeFile);
                                return null;
                            }
                            break;
                        case "-outMeta":
                        case "-m":
                            config.metaFile = args[++i];

                            //gültiger Name?
                            if (config.ausgabeFile.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                            {
                                Console.WriteLine("\"{0}\" kein valider Dateiname", config.metaFile);
                                return null;
                            }
                            break;
                        case "-date":
                        case "-d":
                            if (!DateTime.TryParse(args[++i], out config.date))
                            {
                                Console.WriteLine("\"{0}\" konnte nicht zu einem Datum geparst werden", config.date);
                                return null;
                            }
                            break;
                        case "-tempUsed":
                        case "-tg":
                            if (!Double.TryParse(args[++i], out config.temperaturGenutzt))
                            {
                                Console.WriteLine("\"{0}\" ist keine gültige Gleitkommazahl", args[i]);
                                return null;
                            }
                            break;
                        case "-tempUnused":
                        case "-tu":
                            if (!Double.TryParse(args[++i], out config.temperaturUngenutzt))
                            {
                                Console.WriteLine("\"{0}\" ist keine gültige Gleitkommazahl", args[i]);
                                return null;
                            }
                            break;
                        case "-preUse":
                        case "-p":
                            if (!Int32.TryParse(args[++i], out config.vorheitzen))
                            {
                                Console.WriteLine("\"{0}\" ist nicht ganzzahlig", args[i]);
                                return null;
                            }
                            break;
                        case "-afterUse":
                        case "-a":
                            if (!Int32.TryParse(args[++i], out config.abkühlen))
                            {
                                Console.WriteLine("\"{0}\" ist nicht ganzzahlig", args[i]);
                                return null;
                            }
                            break;
                        case "-break":
                        case "-b":
                            if (!Int32.TryParse(args[++i], out config.pausenlänge))
                            {
                                Console.WriteLine("\"{0}\" ist nicht ganzzahlig", args[i]);
                                return null;
                            }
                            break;
                        case "-h":
                        case "?":
                            Console.WriteLine("-u  | -url           URL von der der Stundenplan abgerufen werden soll");
                            Console.WriteLine("-t  | -tempFile      Dateiname zum zwischenspeichern des Stundenplanes");
                            Console.WriteLine("-o  | -out           Ausgabedatei Heizplan");
                            Console.WriteLine("-m  | -outMeta       Ausgabedatei Metadaten");
                            Console.WriteLine("-d  | -date          Datum für das ein Heizplan erzeugt wird");
                            Console.WriteLine("-tg | -tempUsed      Solltemperatur bei Nutzung");
                            Console.WriteLine("-tu | -tempUnused    Solltemperatur bei Nichtnutzung");
                            Console.WriteLine("-p  | -preUse        Minuten die der Raum Vorgeheitzt werden soll");
                            Console.WriteLine("-a  | -afterUse      Heizdauer durch Restwärme (Minuten)");
                            Console.WriteLine("-b  | -break         Pausenlänge überbrücken (Minuten)");
                            return null;
                        default:
                            Console.WriteLine("unbekannter Parameter: {0}", args[i]);
                            return null;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("auf den Parameter {0} folgt kein Wert", args[i-1]);
                return null;
            }

            return config;
        }
    }
}
