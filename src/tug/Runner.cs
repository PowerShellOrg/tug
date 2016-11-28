using System.Diagnostics;

namespace tug
{
    public static class Runner
    {
        public static string Run(string command)
        {

            /* this is sub-optimal; see NOTES.md */
            
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.FileName = "powershell";
            p.StartInfo.Arguments = System.String.Format("-command \"& {{ {0} }}\"",command);
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output;
        }
    }
} 

