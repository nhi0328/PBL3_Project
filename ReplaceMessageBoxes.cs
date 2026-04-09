using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string dir = @"D:\OneDrive - The University of Technology\PBL3\PBL3";
        string[] files = Directory.GetFiles(dir, "*.xaml.cs", SearchOption.AllDirectories);

        // Matches MessageBox.Show(arg1, arg2, MessageBoxButton.OK, MessageBoxImage.X)
        var regex4Args = new Regex(@"MessageBox\.Show\(([^,]+),\s*([^,]+),\s*MessageBoxButton\.OK,\s*MessageBoxImage\.[a-zA-Z]+\)");
        var regex2Args = new Regex(@"MessageBox\.Show\(([^,]+),\s*([^,)]+)\)");
        var regex1Arg = new Regex(@"MessageBox\.Show\(([^,]+)\)");

        foreach (var file in files)
        {
            if (file.EndsWith("CustomMessageBox.xaml.cs") || file.EndsWith("ConfirmDeleteBox.xaml.cs")) continue;

            string content = File.ReadAllText(file);
            bool changed = false;

            // We only replace lines that don't have "MessageBoxResult" or "var result =" 
            // since those are meant for Choices (Yes/No). So we'll replace only if it doesn't contain assignment.
            
            var lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("MessageBox.Show") && !line.Contains("MessageBoxResult") && !line.Contains("MessageBoxButton.YesNo") && !line.Contains("MessageBoxButton.OKCancel"))
                {
                    // Pattern might span multiple lines but practically they are mostly on one line.
                    // We handle replacing strictly.
                    // Try 4 args
                    if (regex4Args.IsMatch(line))
                    {
                        line = regex4Args.Replace(line, "new CustomMessageBox($1, $2).ShowDialog()");
                        changed = true;
                    }
                    else if (regex2Args.IsMatch(line))
                    {
                        line = regex2Args.Replace(line, "new CustomMessageBox($1, $2).ShowDialog()");
                        changed = true;
                    }
                    else if (regex1Arg.IsMatch(line))
                    {
                        line = regex1Arg.Replace(line, "new CustomMessageBox($1).ShowDialog()");
                        changed = true;
                    }
                }
                lines[i] = line;
            }

            if (changed)
            {
                File.WriteAllText(file, string.Join("\n", lines));
                Console.WriteLine("Updated: " + file);
            }
        }
    }
}
