using Microsoft.Win32;
using System.IO;
using System.Security.Principal;


class Program
    {
        static void Main(string[] args)
        {
        // чтение входных параметров
        string path = args[0];
        
        if (Directory.Exists(path))  // проверяем, что папка существует (папка это или файл)
        {
            foreach (var fileName in Directory.EnumerateFiles(path))  // цикл для каждого файла в папке
            {
                string Newpath = fileName + @".diplom";
                File.Move(fileName, Newpath);
            }
        }
        else
        {
            string Newpath = path + @".diplom";
            File.Move(path, Newpath);
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
    }

