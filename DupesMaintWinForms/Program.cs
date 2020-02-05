using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DupesMaintWinForms
{
    static class Program
    {
        public static PopsModel popsModel { get; set; }
        public const string targetRootFolder = @"C:\Users\User\Google Drive\OneDriveDupes\";

        public static bool targetRootFolderExists = false;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // instantiate the data model
            popsModel = new PopsModel();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SelectBySHA());
        }

        public static void DisplayException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("An exception of type \"");
            sb.Append(ex.GetType().FullName);
            sb.Append("\" has occurred.\r\n");
            sb.Append(ex.Message);
            sb.Append("\r\nStack trace information:\r\n");
            MatchCollection matchCol = Regex.Matches(ex.StackTrace, @"(at\s)(.+)(\.)([^\.]*)(\()([^\)]*)(\))((\sin\s)(.+)(:line )([\d]*))?");
            int L = matchCol.Count;
            string[] argList;
            Match matchObj;
            int y, K;
            for (int x = 0; x < L; x++)
            {
                matchObj = matchCol[x];
                sb.Append(matchObj.Result("\r\n\r\n$1 $2$3$4$5"));
                argList = matchObj.Groups[6].Value.Split(new char[] { ',' });
                K = argList.Length;
                for (y = 0; y < K; y++)
                {
                    sb.Append("\r\n    ");
                    sb.Append(argList[y].Trim().Replace(" ", "        "));
                    sb.Append(',');
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\r\n)");
                if (0 < matchObj.Groups[8].Length)
                {
                    sb.Append(matchObj.Result("\r\n$10\r\nline $12"));
                }
            }
            Console.WriteLine(sb.ToString());
        }

    }
}
