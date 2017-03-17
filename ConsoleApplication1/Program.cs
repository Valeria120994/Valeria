using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;


class Program
    {

    static string[] extensions = { ".docx", ".doc",".txt",".jpeg"};

    static void Main(string[] args)
    {
        callProgram();
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
                var fileInf = new FileInfo(path);
                if (fileInf.Extension.Equals(".diplom"))
                {
                    string Newpath = path.Remove(path.Length - 7);
                    File.Copy(path, Newpath);
                    Process.Start("explorer.exe", Newpath); // открыть файл через проводник (проводник вызовет программу по умолчанию)
                    // отрыть файл
                }
                else
                {
                    ProcessFile(path); // string Newpath = path + @".diplom"; File.Move(path, Newpath);
                }
            }

        }
        /*
                WindowsIdentity identity = new WindowsIdentity("Valeria");
                    WindowsImpersonationContext context = identity.Impersonate();

                    RegistryKey hklm = Registry.ClassesRoot;
                    RegistryKey hkSoftware = hklm.OpenSubKey("Word.Document.12");
                    RegistryKey hkMicrosoft = hkSoftware.OpenSubKey("shell");
                    var a = hkMicrosoft.CreateSubKey("diplom");
                    a.SetValue("icon", @"%SystemRoot%\system32\SHELL32.dll,47"); 
                    */
    }

    private static void callProgram()
    {
        RegistryKey hkcr = Registry.ClassesRoot; // открыли большую ветку HKEY_CLASSES_ROOT
        RegistryKey hkrazdel = hkcr.OpenSubKey(".docx"); // открываем раздел с расширением
        string value = (string)hkrazdel.GetValue("");
        RegistryKey hkrazdel2 = hkcr.OpenSubKey(value).OpenSubKey("shell").OpenSubKey("Open").OpenSubKey("command");
        string value2 = (string)hkrazdel2.GetValue("");
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

