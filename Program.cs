/*
 C# Proxy scraper, checker and connecter!
 This is code written on 3-6-2018
 */

using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace GeoIP
{
    internal static class Program
    {
        public static void Main()
        {
            while (true)
            {
                Console.Title = "sProxy";
                Console.Write("sProxy\nScraper, Checker, Connections\n\nFunction:         ");

                var function = Console.ReadLine()?.ToLower();

                if (!string.IsNullOrEmpty(function) && function.Length != 1)
                {
                    if (function[0] == 's')
                        Proxys.Scraper();
                    else if (function[0] == 'c' && function[1] == 'h')
                        Proxys.Checker();
                    else if (function[0] == 'c' && function[1] == 'o') Proxys.Connect();
                }
                else
                {
                    Console.Clear();
                    continue;
                }

                break;
            }
        }
    }

    internal static class Proxys
    {
        [DllImport("wininet.dll")]
        private static extern bool
            InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private const int InternetOptionSettingsChanged = 39;
        private const int InternetOptionRefresh = 37;

        public static void Connect()
        {
            Console.Clear();
            if (!File.Exists("proxys.txt"))
            {
                Console.Write(
                    "It looks like you don't got a proxy list to connect to , use the scraper first!\nPress any key to go back to the main panel.");
                Console.ReadKey();
            }
            else
            {
                var regKey = Registry.CurrentUser.OpenSubKey(
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                if (regKey != null && regKey.GetValue("ProxyEnable").ToString() == "1")
                {
                    Console.Write("You are currently connected to the proxy server: " +
                                  regKey.GetValue("ProxyServer").ToString() + "\n");
                    Console.Write("Disconnect from the proxy server? (Yes/No):");
                    var responsetemp = Console.ReadLine()?.ToLower();
                    if (responsetemp == null || responsetemp[0] != 'y') return;
                    var currproxtemp = regKey.GetValue("ProxyServer").ToString();
                    regKey = Registry.CurrentUser.OpenSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                    if (regKey != null)
                    {
                        regKey.SetValue("ProxyEnable", false, RegistryValueKind.DWord);
                        regKey.Close();
                    }

                    InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
                    InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
                    Console.Write("You have been disconnected from proxy: " + currproxtemp +
                                  "\n\nPress a key to go back to the main panel");
                    Console.ReadKey();
                    Console.Clear();
                    Program.Main();
                }
                else
                {
                    Console.Write("Connect to random proxy server? (Yes/No):");
                    if (regKey != null)
                    {
                        regKey.Close();
                        var response = Console.ReadLine()?.ToLower();

                        if (response != null && response[0] == 'y')
                        {
                            var tmprand = new Random();
                            var temp = File.ReadAllText("proxys.txt");
                            var list = temp.Split('\n');
                            var proxytmp = list[tmprand.Next(0, list.Length - 1)];

                            regKey = Registry.CurrentUser.OpenSubKey(
                                "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                            if (regKey != null)
                            {
                                regKey.SetValue("ProxyEnable", true, RegistryValueKind.DWord);
                                regKey.SetValue("ProxyServer", proxytmp.Split(':')[0] + ":" + proxytmp.Split(':')[1]);
                                regKey.Close();
                            }

                            InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
                            InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
                            Console.Write("You have been connected to proxy: " + proxytmp +
                                          "\n\nPress a key to go back to the main panel");
                            Console.ReadKey();
                            Console.Clear();
                            Program.Main();
                        }
                        else
                        {
                        }
                    }
                }
            }
        }

        public static void Scraper()
        {
            Console.Clear();
            var client = new WebClient();
            var counter = 0;
            var sProxys = "";
            var urls = new string[] {"https://free-proxy-list.net/", "https://www.sslproxies.org/"};
            try
            {
                for (var i = 0; i < 2; i++)
                {
                    var result = client.DownloadString(urls[i]);
                    foreach (Match m in Regex.Matches(result,
                        "[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}</td><td>[0-9]{1,5}"))
                    {
                        counter++;
                        sProxys += m + "\n";

                        sProxys = sProxys.Replace("</td><td>", ":");
                    }



                    Console.Write(sProxys);
                }
            }
            catch (Exception)
            {
                throw;
            }

            Console.Write("\nProxy's scraped: " + counter + "\nWanna save the proxys? (Yes/No):");
            var response = Console.ReadLine()?.ToLower();

            if (response != null && response[0] == 'y')
            {
                if (!File.Exists("proxys.txt"))
                {
                    File.WriteAllText("proxys.txt", sProxys);
                    Console.Write("File proxys.txt has been created\n\n");
                }
                else
                {
                    File.WriteAllText("proxys.txt", sProxys);
                    Console.Write("File proxys.txt already existed and has been overwritten with the new proxy's\n\n");
                }
            }

            Console.Write("Press a key to go back to the main panel");
            Console.ReadKey();
            Console.Clear();
            Program.Main();
        }

        public static void Checker()
        {
            Console.Clear();
            var counter = 0;
            var sProxys = "";
            if (!File.Exists("proxys.txt"))
            {
                Console.Write(
                    "It looks like you don't got a proxy list to check yet, use the scraper first!\nPress any key to go back to the main panel.");
                Console.ReadKey();
            }
            else
            {
                var temp = File.ReadAllText("proxys.txt");
                var list = temp.Split('\n');
                for (var i = 0; i < list.Length - 1; i++)
                {
                    try
                    {
                        var check = new Ping();
                        var resulttemp = check.Send(list[i].Split(':')[0], 5);
                        if (resulttemp == null || resulttemp.Status == IPStatus.TimedOut) continue;
                        sProxys += list[i] + "\n";
                        Console.Write(list[i] + "[" + resulttemp.RoundtripTime.ToString() + "ms]\n");
                        counter++;
                    }
                    catch (Exception)
                    {
                        throw;

                    }
                }

                Console.Write("\nProxy's working:" + counter + "\nWanna save the proxys? (Yes/No):");
                var response = Console.ReadLine()?.ToLower();

                if (response != null && response[0] == 'y')
                {
                    if (!File.Exists("proxys.txt"))
                    {
                        File.WriteAllText("proxys.txt", sProxys);
                        Console.Write("File proxys.txt has been created\n\n");
                    }
                    else
                    {
                        File.WriteAllText("proxys.txt", sProxys);
                        Console.Write(
                            "File proxys.txt already existed and has been overwritten with the new proxy's\n\n");
                    }
                }

                Console.Write("Press a key to go back to the main panel");
                Console.ReadKey();
                Console.Clear();
                Program.Main();
            }
        }
    }
}
