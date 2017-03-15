using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Principal;


class Program
    {

    static string[] extensions = { ".docx", ".doc",".txt",".jpeg"};

    static void Main(string[] args)
        {
        if (args.Length > 0)
        {
            // чтение входных параметров
            string path = args[0];  // провепить, есть ли в массиве хоть один элемент  (args.Length)

            if (Directory.Exists(path))  // проверяем, что папка существует (папка это или файл)
            {
                foreach (var fileName in Directory.EnumerateFiles(path))  // цикл для каждого файла в папке
                {
                    ProcessFile(fileName);
                }
            }
            else
            {
                ProcessFile(path);
            }

        }
        /*
                WindowsIdentity identity = new WindowsIdentity("Valeria");
                    WindowsImpersonationContext context = identity.Impersonate();

                    RegistryKey hklm = Registry.ClassesRoot;
                    RegistryKey hkSoftware = hklm.OpenSubKey("Word.Document.12");
                    RegistryKey hkMicrosoft = hkSoftware.OpenSubKey("shell");
                    var a = hkMicrosoft.CreateSubKey("diplom2");
                    a.SetValue("icon", @"%SystemRoot%\system32\SHELL32.dll,47"); 
                    */
    }

    private static void ProcessFile(string path)
    {
        var fileInf = new FileInfo(path);
        var index = Array.IndexOf(extensions, fileInf.Extension);
        if (index != -1)
        {
            string Newpath = path + @".diplom";
            File.Move(path, Newpath);
        }
            
    }
}

