using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    static class Program
    {


        static string[] extensions = { ".docx", ".doc", ".txt", ".jpeg" };

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
          //  var n = Registry.ClassesRoot.OpenSubKey("DIPLOM");
            if (args.Length == 1 && args[0].Equals("--install"))
            {
                var appPath = Environment.GetEnvironmentVariables()["SystemRoot"] + "\\diplom.exe";
                File.Copy(Application.ExecutablePath, appPath, true);
                RegistryKey hklm = Registry.ClassesRoot;
                var papka = hklm.CreateSubKey("DIPLOM");
                papka.SetValue("", "Защищенный файл");
                var shell = papka.CreateSubKey("shell");
                var open = shell.CreateSubKey("open");
                var command = open.CreateSubKey("command");
                command.SetValue("", appPath + " \"%1\"");

                RegistryKey hkcr = Registry.ClassesRoot; // открыли большую ветку HKEY_CLASSES_ROOT
                var extension = hkcr.CreateSubKey(".diplom");
                extension.SetValue("", "DIPLOM");

                foreach (var item in extensions)
                {
                    RegistryKey hkrazdel = hkcr.OpenSubKey(item); // открываем раздел с расширением
                    string value = (string)hkrazdel.GetValue("");

                    RegistryKey hkSoftware = hkcr.OpenSubKey(value);
                    RegistryKey hkMicrosoft = hkSoftware.OpenSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    var a = hkMicrosoft.CreateSubKey("diplom");
                    a.SetValue("", "Защитить");
                    a.SetValue("icon", @"%SystemRoot%\system32\SHELL32.dll,47");
                    var comm = a.CreateSubKey("command");
                    comm.SetValue("", appPath + " \"%1\"");
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            if (args.Length > 0) // провепить, есть ли в массиве хоть один элемент  (args.Length)
            {
                // чтение входных параметров
                string path = args[0]; 

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
                        FileCopy(path, Newpath, "Незащищенный файл уже существет. Заменить?");

                        callProgram(Newpath);  // открыть файл через программу по умолчанию и дождаться завершения

                        FileMove(Newpath, path, "Защищенный файл уже существует. Заменить?");
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
            var isRunDLL = match.Contains("rundll");
            var arguments = value2.Replace(match, "").Replace("%1", isRunDLL ? fileName : '"' + fileName + '"');
            var process = Process.Start(match, arguments);
            process.WaitForExit();
        }

        private static void ProcessFile(string path)
        {
            var fileInf = new FileInfo(path);
            var index = Array.IndexOf(extensions, fileInf.Extension);
            if (index != -1)
            {
                string Newpath = path + @".diplom";
                FileMove(path, Newpath, "Защищенный файл уже существует. Заменить?");
            }

        }

        private static void FileMove(string path, string Newpath, string mess)
        {
            if (File.Exists(Newpath)) // существует ли файл
            {
                var windows = MessageBox.Show(mess, "Срочное сообщение!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                //  (System.Windows.Forms.MessageBoxOptions)8192 /*MB_TASKMODAL*/
                if (windows == DialogResult.Yes)
                {
                    File.Delete(Newpath);
                    File.Move(path, Newpath);
                }
            }
            else
            {
                File.Move(path, Newpath);
            }
        }

        private static void FileCopy(string path, string Newpath, string mess)
        {
            if (File.Exists(Newpath)) // существует ли файл
            {
                var windows = MessageBox.Show(mess, "Срочное сообщение!", MessageBoxButtons.YesNo);
                if (windows == DialogResult.Yes)
                {
                    File.Delete(Newpath);
                    File.Copy(path, Newpath);
                }
            }
            else
            {
                File.Copy(path, Newpath);
            }
        }
    }
}
