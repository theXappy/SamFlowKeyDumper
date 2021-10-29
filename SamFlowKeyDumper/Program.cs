using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using RemoteNET;
using RemoteNET.Internal.Extensions;

namespace SamFlowKeyDumper
{
    class Program
    {
        private const int RETRIES = 3;

        static void Main()
        {
            (bool success, Process process) = GetProcess("SamsungFlowDesktop");
            if (!success)
                return;

            // Check version and notify user if it might be unsupported
            Version currVersion = GetFlowVersion(process);
            Version latestTested = new Version("4.8.5.0");
            if (currVersion.CompareTo(latestTested) > 0)
            {
                Console.Error.WriteLine($"WARNING: Current SamsungFlowDesktop version is {currVersion}. " +
                                  $"This is a newer version than the latest confirmed one to work with this tool which is {latestTested}.");
            }

            Console.Error.WriteLine("Connecting to target process...");
            RemoteApp app = RemoteApp.Connect(process);

            Console.Error.WriteLine("Querying for SessionKeyManager objects...");
            RemoteObject sessionKeyManager;
            (success, sessionKeyManager) = GetRemoteObject(app, "*SessionKeyManager");
            if (!success)
                return;
            Console.Error.WriteLine("Got remote SessionKeyManager instance!");

            Console.Error.WriteLine("Invoking ToString for sanity:");
            dynamic dynSessionKeyManager = sessionKeyManager.Dynamify();
            Console.Error.WriteLine(dynSessionKeyManager.ToString());

            Console.Error.WriteLine("Dumping Key and IV:");
            byte[] key = (byte[])(dynSessionKeyManager._key);
            byte[] iv = (byte[])(dynSessionKeyManager._iv);
            Console.WriteLine(key.ToHex());
            Console.WriteLine(iv.ToHex());

            Console.Error.WriteLine("Cleaning up.");
            sessionKeyManager.Dispose();
        }

        public static (bool, Process) GetProcess(string procName)
        {
            Process target = null;
            for (int i = 1; i <= RETRIES; i++)
            {
                if (i != 1)
                    Console.Error.WriteLine("Retrying...");

                Process[] procs = new Process[0];
                try
                {
                    procs = Process.GetProcessesByName(procName);
                    if (procs.Length > 1)
                    {
                        // Too many matches. try to narrow it down to those with existing divers
                        // (in case this is a re-attach)
                        procs = procs.Where(proc =>
                            proc.Modules.AsEnumerable()
                                .Any(mod => mod.FileName.Contains("BootstrapDLL")))
                            .ToArray();
                        target = procs.First();
                        break;
                    }
                    target = procs.Single();
                    break;
                }
                catch
                {

                    Console.Error.WriteLine($"Try #{i} failed. Found {procs.Length} matching processes (Expecting only 1).");
                    Thread.Sleep(100);
                    continue;
                }
            }
            if (target == null)
            {
                Console.Error.WriteLine($"Failed to find a unique process named '{procName}'.");
                return (false, null);
            }
            return (true, target);
        }

        public static Version GetFlowVersion(Process samsungFlowDesktopProc)
        {
            var fileName = samsungFlowDesktopProc.MainModule.FileName;
            Regex r = new Regex(@"SamsungFlux_(\d+\.\d+\.\d+\.\d+)_");
            Match match = r.Match(fileName);
            return new Version(match.Groups[1].ToString());
        }

        public static (bool, RemoteObject) GetRemoteObject(RemoteApp app, string typeQuery)
        {
            RemoteObject sessionKeyManager = null;
            for (int i = 1; i < RETRIES; i++)
            {
                if (i != 1)
                    Console.Error.WriteLine("Retrying...");

                try
                {
                    IEnumerable<CandidateObject> candidates = app.QueryInstances(typeQuery);
                    sessionKeyManager = app.GetRemoteObject(candidates.Single());
                    break;
                }
                catch
                {
                    Console.Error.WriteLine($"Try #{i} failed.");
                    Thread.Sleep(100);
                    continue;
                }
            }

            if (sessionKeyManager == null)
            {
                Console.Error.WriteLine("Failed to get SessionKeyManager object. Aborting.");
                return (false, null);
            }

            return (true, sessionKeyManager);
        }
    }
}
