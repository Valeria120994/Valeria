using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;

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
                var fileInf = new FileInfo(path);
                if (fileInf.Extension.Equals(".diplom"))
                {
                    string Newpath = path.Remove(path.Length - 7);
                    File.Move(path, Newpath);

                    callProgram(Newpath);  // открыть файл через программу по умолчанию и дождаться завершения

                    File.Move(Newpath, path);
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

    private static void callProgram(string fileName)
    {
        var fileInf = new FileInfo(fileName);  // получаем информацию о файле
        var extension = fileInf.Extension; // получаем расширение файла

        RegistryKey hkcr = Registry.ClassesRoot; // открыли большую ветку HKEY_CLASSES_ROOT
        RegistryKey hkrazdel = hkcr.OpenSubKey(extension); // открываем раздел с расширением
        string value = (string)hkrazdel.GetValue("");
        RegistryKey hkrazdel2 = hkcr.OpenSubKey(value).OpenSubKey("shell").OpenSubKey("Open").OpenSubKey("command");
        string value2 = (string)hkrazdel2.GetValue("");

        var match = Regex.Match(value2, "(\"[^\"]+\"|[^\\s]+)").Value;
       var process =  Process.Start(match, '"' + fileName + '"');
        process.WaitForExit();
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

