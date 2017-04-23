using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    static class Program
    {


        static string[] extensions = { ".docx", ".doc", ".txt", ".jpeg", ".pptx", ".pdf", ".rar", ".jpg" };
        private static IntPtr _hookID = IntPtr.Zero;

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

                // Create the source, if it does not already exist.
                if (!EventLog.SourceExists("Diplom")) {
                    //An event log source should not be created and immediately used.
                    //There is a latency time to enable the source, it should be created
                    //prior to executing the application that uses the source.
                    //Execute this sample a second time to use the new source.
                    EventLog.CreateEventSource("Diplom", "Log diplom");
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
            string value2 = (string)hkrazdel2.GetValue(""); // команда, открывающая стандартное приложение


            var match = Regex.Match(value2, "(\"[^\"]+\"|[^\\s]+)").Value; // путь к запускаемой программе
            var isRunDLL = match.Contains("rundll"); // проверяем, исполняемый файл или dll
            var filePath = isRunDLL ? fileName : '"' + fileName + '"';
            var arguments = value2.Replace(match, "").Replace("\"%1\"", filePath).Replace("%1", filePath); // в команду посдставляется имя файл, который нужно открыть
            process = Process.Start(match, arguments); // выполнение команды
            // File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.System | FileAttributes.ReadOnly);
            process.WaitForInputIdle(); // ожидание открытия приложения
            if (LockFile(fileName))
            { // блокируем файл
                _hookID = SetHook(process); // перехват системных событий (нажатие клавиши)
                var error = Marshal.GetLastWin32Error(); // получаем последнюю ошибку
                if (error == 0)
                {
                    SetWindowText(process.MainWindowHandle, "(Нажмите win для разблокировки файла) " + process.MainWindowTitle); // новый текст + старый заголовок
                }
            }
            process.WaitForExit();
            UnlockFile(process);

        }

        private static bool LockFile(string fileName)
        {
            try
            {
                file = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        #region Win32 API Stuff

        // Define the Win32 API methods we are going to use
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);

        private const int WM_KEYDOWN = 0x0100;
        private static FileStream file;
        private static Process process;
        private static System.Threading.Timer stateTimer;

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool AppendMenu(IntPtr hMenu, MenuFlags uFlags, uint uIDNewItem, string lpNewItem);

        [Flags]
        public enum MenuFlags : uint
        {
            MF_STRING = 0,
            MF_BYPOSITION = 0x400,
            MF_SEPARATOR = 0x800,
            MF_REMOVE = 0x1000,
        }

        #endregion

        private static IntPtr SetHook(Process process)
        {
            using (Process curProcess = Process.GetCurrentProcess()) // получаем текущий процесс и основной модуль процесса
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(HookType.WH_KEYBOARD_LL, new HookProc(HookCallback), GetModuleHandle(curModule.ModuleName), 0); //установка перехвата событий
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) //функция реагирования на системные события
        {
            if ((nCode >= 0) && (wParam == (IntPtr)WM_KEYDOWN)) //определение события, которое произошло (нажатие клавиши)
            {
                var vkCode = (Keys)Marshal.ReadInt32(lParam); // определяем, какую кнопку нажали
                if ((vkCode == Keys.LWin) || (vkCode == Keys.RWin)) // проверяем, что нажаты левая/правая клавиши win
                {

                    var fileName = file.Name;

                    UnlockFile(process);

                    // Create an AutoResetEvent to signal the timeout threshold in the
                    // timer callback has been reached.
                    var autoEvent = new AutoResetEvent(false);

                    var statusChecker = new StatusChecker(5, fileName);
                    stateTimer = new System.Threading.Timer(statusChecker.CheckStatus, autoEvent, 0, 1000);
                   
                    /*
                    if (file != null) //проверяем, что файл заблокирован
                    {
                        file.Close(); // снимаем блокировку 
                        file = null;
                    }
                    */
                    return (IntPtr)1; // сообщаем системе, что больше ничего от нажатия кнопки не требуется
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam); // вызов следующего перехватчика событий
        }

        private static void UnlockFile(Process process)
        {
            if (process.HasExited == false)
            {
                SetWindowText(process.MainWindowHandle, process.MainWindowTitle.Replace("(Нажмите win для разблокировки файла) ", ""));
            }
            if (file != null) //проверяем, что файл заблокирован
            {
                file.Close(); // снимаем блокировку 
                file = null;
            }
        }

        public static void UnHook()
        {
            //UnhookWindowsHookEx(hhook);
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
                var hashfile = CalculateHash(path);
                var hashfile2 = CalculateHash(Newpath);
                if (hashfile.Equals(hashfile2))
                {
                    File.Delete(Newpath);
                    File.Move(path, Newpath);
                }
                else
                {
                    var windows = MessageBox.Show(mess, "Срочное сообщение!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    //  (System.Windows.Forms.MessageBoxOptions)8192 /*MB_TASKMODAL*/
                    if (windows == DialogResult.Yes)
                    {
                        File.Delete(Newpath);
                        File.Move(path, Newpath);
                    }
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
                var hashfile = CalculateHash(path);
                var hashfile2 = CalculateHash(Newpath);
                if (hashfile.Equals(hashfile2))
                {
                    File.Delete(Newpath);
                    File.Copy(path, Newpath);
                }
                else
                {
                    var windows = MessageBox.Show(mess, "Срочное сообщение!", MessageBoxButtons.YesNo);
                    if (windows == DialogResult.Yes)
                    {
                        File.Delete(Newpath);
                        File.Copy(path, Newpath);
                    }
                }
                
            }
            else
            {
                File.Copy(path, Newpath);
            }
        }

        private static object CalculateHash(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash);
            }
        }
        class StatusChecker
        {
            private int invokeCount;
            private int maxCount;
            private string mainWindowTitle;
            private string fileName;

            public StatusChecker(int count, string path)
            {
                invokeCount = 0;
                maxCount = count;
                mainWindowTitle = process.MainWindowTitle;
                fileName = path;
            }

            // This method is called by the timer delegate.
            public void CheckStatus(Object stateInfo)
            {
                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                invokeCount++;
                SetWindowText(process.MainWindowHandle, "(Файл будет заблокирован через " + (maxCount - invokeCount) + " секунд) " + mainWindowTitle); // новый текст + старый заголовок


                if (invokeCount == maxCount)
                {
                    // Reset the counter and signal the waiting thread.
                    invokeCount = 0;
                    autoEvent.Set();
                    SetWindowText(process.MainWindowHandle, "(Нажмите win для разблокировки файла) " + mainWindowTitle); // новый текст + старый заголовок
                    
                    LockFile(fileName);

                    stateTimer.Dispose();

                }
            }
        }
    }
}
