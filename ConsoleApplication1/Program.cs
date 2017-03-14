using Microsoft.Win32;
using System.IO;
using System.Security.Principal;


class Program
    {
        static void Main(string[] args)
        {
        string path = args[0];
        string Newpath = path+@".diplom";
        File.Copy(path, Newpath, true);
    
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

