using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LSF_Schnittstelle
{
    class Log
    {
        const string ERRORFILE = "error.txt";
        const string IGNORINFOFILE = "ingnorierte_werte.txt";
        const string LOGDIRECTIONARY = "log";

        public static void Error (string message, Exception exception = null)
        {
            StringBuilder error = new StringBuilder();
            if (exception != null)
            {
                error.Append("\n");
                error.Append("===============================================\n");
                error.Append(exception.Message);
                error.Append("-----------------------------------------------\n");
                error.Append(exception.StackTrace);
                error.Append("===============================================\n");
            }

            Write("ERROR", message + error.ToString(), ERRORFILE);
        }

        public static void ignor(int zeile, string element, string wert)
        {
            StringBuilder info = new StringBuilder();
            info.Append(" Eingabe wurde Ignoriert\n");
            info.Append("===============================================\n");
            info.Append("Zeile:    ");
            info.Append(zeile);
            info.Append("\n");
            info.Append("Element:  ");
            info.Append(element);
            info.Append("\n");
            info.Append("Wert:     ");
            info.Append(wert);
            info.Append("\n");
            info.Append("===============================================\n");

            Write("IGNORE", info.ToString(), IGNORINFOFILE);
        }

        private static void Write (string kategorie, string message, string file)
        {
            string filePath = LOGDIRECTIONARY;
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            filePath = filePath + "/" + DateTime.Now.Date.ToString("yyyy-MM-dd");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            filePath = filePath + "/" + file;

            using (StreamWriter w = File.AppendText(filePath))
            {
                w.WriteLine(String.Format("{0,-10} [" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "]:    {1}", kategorie, message));
            }
        }
    }
}
