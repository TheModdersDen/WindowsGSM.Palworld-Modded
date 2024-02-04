using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Linq;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace WindowsGSM.Plugins
{
    public class Palworld_Modded : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Palworld-Modded", // WindowsGSM.XXXX
            author = "TheModdersDen, ohmcodes, and the WindowsGSM Community",
            description = "WindowsGSM plugin for supporting mods on a Palworld Dedicated Server.",
            version = "1.0.0",
            url = "https://github.com/TheModdersDen/WindowsGSM.Palworld-Modded", // Github repository link (Best practice)
            color = "#1E8449" // Color Hex
        };

        // - Standard Constructor and properties
        public Palworld_Modded(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice, PublicIP, apiDownloadURL, modAPIFileName, modAPIVersion;

        public bool DEBUG_MODE = false; // Set to true to enable debug mode (AND GET SPAMMED WITH CONSOLE OUTPUTS!)

        private readonly string publicIPRetrievalURL = "https://api.ipify.org"; // The public IP retrieval URL

        // The latest release API URL
        private readonly string apiVersionURL = "https://api.github.com/repos/UE4SS-RE/RE-UE4SS/releases/latest";

        //? - Game server Fixed variables:

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "2394010"; /* taken via https://steamdb.info/app/2394010/info/ */

        // - Game server Fixed variables
        public override string StartPath => @"Pal\Binaries\Win64\PalServer-Win64-Test-Cmd.exe"; // Game server start path 
        public string FullName = "Palworld Modded Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 2; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()

        // - Game server default values
        public string ServerName = "Palworld";
        public string Defaultmap = "MainWorld5"; // Original (MapName)
        public string Maxplayers = "32"; // WGSM reads this as string but originally it is number or int (MaxPlayers)
        public string Port = "8211"; // WGSM reads this as string but originally it is number or int
        public string QueryPort = "8212"; // WGSM reads this as string but originally it is number or int (SteamQueryPort)
        public string Additional = "EpicApp=PalServer -useperfthreads -NoAsyncLoadingThread -UseMultithreadForDS"; // Additional server start parameter

        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            string configPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini");

            if (!Directory.Exists(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer")))
            {
                Directory.CreateDirectory(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer"));
            }
            // If the file is empty, add the default config
            if (new FileInfo(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini")).Length == 0)
            {
                if (!File.Exists(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini")))
                {
                    File.Create(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini"));
                }
                else
                {
                    await GetPublicIP(); // Get the public IP of the server
                    await Task.Delay(1000);
                    string configContent = $"[/Script/Pal.PalWorldSettings]\nServerName=\"{_serverData.ServerName}\"\nServerDescription=\"A very cool server!\"\nAdminPassword=\"CHANGEME54321\"\nServerPassword=\"54321\"\nPublicIP=\"{PublicIP}\"\nPublicPort={_serverData.ServerPort}\nPublicQueryPort={_serverData.ServerQueryPort}\nServerMaxPlayerCount={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountCoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvP={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvE={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvE={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}\nServerMaxPlayerCountPvPvECoop={_serverData.ServerMaxPlayer}";
                    File.WriteAllText(configPath, configContent);
                    Console.WriteLine("Default 'PalWorldSettings.ini' created.\nPlease edit the file to your liking (or at least change the admin/server password!).");
                }
            }
            else
            {
                Console.WriteLine("PalWorldSettings.ini already exists, skipping creation!");
            }
        }

        // - Get the Mod API download URL from GitHub using the GitHub API:
        private async Task<string> GetModAPIDownloadURL()
        {
            var downloadURL = apiDownloadURL;
            await Task.Run(() =>
            {
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers.Add(HttpRequestHeader.UserAgent, "WindowsGSM");
                        string json = webClient.DownloadString(apiVersionURL);
                        JObject releaseInfo = JObject.Parse(json);

                        // Loop through the assets in the API response until we find the download URL for the mod API
                        foreach (var asset in releaseInfo["assets"]?.Children())
                        {
                            if (asset["name"].ToString().ToLower().Contains("xinput"))
                            {
                                // Set the download URL
                                downloadURL = asset["browser_download_url"].ToString();

                                // Set the mod API file name
                                string[] urlSegments = downloadURL.Split('/');
                                string filename = urlSegments[urlSegments.Length - 1];
                                modAPIFileName = filename;

                                break;
                            }
                        }
                    }
                    return downloadURL;
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return downloadURL;
                }
            });
            return downloadURL;
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            await Task.Run(() =>
            {
                string shipExePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
                if (!File.Exists(shipExePath))
                {
                    Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                    return null;
                }

                if (!File.Exists(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini")) || new FileInfo(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini")).Length == 0)
                {
                    try
                    {
                        CreateServerCFG();
                    }
                    catch (Exception e)
                    {
                        Error = e.Message;
                        return null;
                    }
                }

                string param = $" {_serverData.ServerParam} ";
                param += $"-publicip=\"{_serverData.ServerIP}\" ";
                param += $"-port={_serverData.ServerPort} ";
                param += $"-publicport={_serverData.ServerPort} ";
                param += $"-queryport={_serverData.ServerQueryPort} ";
                param += $"-publicqueryport={_serverData.ServerQueryPort} ";
                param += $"-players={_serverData.ServerMaxPlayer} ";
                param += $"-servername=\"{_serverData.ServerName}\"";

                // Prepare Process
                var p = new Process
                {
                    StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = shipExePath,
                    Arguments = param.ToString(),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false

                },
                    EnableRaisingEvents = true
                };

                // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
                if (AllowsEmbedConsole)
                {
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    var serverConsole = new ServerConsole(_serverData.ServerID);
                    p.OutputDataReceived += serverConsole.AddOutput;
                    p.ErrorDataReceived += serverConsole.AddOutput;
                }

                // Start Process
                try
                {
                    p.Start();
                    if (AllowsEmbedConsole)
                    {
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                    }

                    return p;
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null; // return null if fail to start
                }
            });
            return null; // return null if fail to start
        }

        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                Functions.ServerConsole.SendWaitToMainWindow("^c");
                Task.Delay(5000).Wait();
                Functions.ServerConsole.SendWaitToMainWindow("^c");
                Task.Delay(5000);

                // Kill the process if it's still running (for whatever reason)
                if (!p.HasExited)
                {
                    p.Kill(); // My name is WindowsGSM, you killed my server, prepare to die!
                    Notice = "Forced the server to stop";
                }
            });
            await Task.Delay(2000);
        }

        // - Verify if a file exists (or a folder exists). Return true if it exists, false if it doesn't
        public bool IsPathValid(string path, string method = "path")
        {
            try
            {
                if (path == null)
                {
                    return false;
                }
                if (method == "path" || method == "file")
                {
                    return File.Exists(path);
                }
                else if (method == "contents" || method == "data")
                {
                    return new FileInfo(path).Length != 0;
                }
                else if (method == "directory" || method == "folder" || method == "dir")
                {
                    return Directory.Exists(path);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
                return false;

            }
        }

        // - Update server function
        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            var (p, error) = await Installer.SteamCMD.UpdateEx(serverData.ServerID, AppId, validate, custom: custom, loginAnonymous: loginAnonymous);
            Error = error;
            await Task.Run(() =>
            {
                p.WaitForExit();
            });
            await Task.Delay(1000);
            // Download the mod API file if it doesn't exist
            await Task.Run(async () =>
            {
                await GetModAPIDownloadURL();
                await Task.Delay(5000);

                if (IsPathValid(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\xinput1_3.dll"), "data") == false || IsPathValid(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\UE4SS-settings.ini"), "data") == false || IsPathValid(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\Mods\"), "dir") == false)
                {
                    DownloadModAPI(apiDownloadURL);
                    InstallModAPI();
                }
                else if (IsPathValid(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\xinput1_3.dll"), "data") && IsPathValid(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\UE4SS-settings.ini"), "data") && IsPathValid(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\Mods\"), "data"))
                {
                    string localVersion = GetLocalModAPIVersion("registry");
                    string remoteVersion = GetLatestModAPIVersion("registry").Result;
                    if (localVersion != remoteVersion)
                    {
                        DownloadModAPI(apiDownloadURL);
                        InstallModAPI();
                    }
                }
                else
                {
                    Error = "Failed to update the mod API";
                }

            });
            await Task.Delay(1000);
            return p;
        }

        public bool IsInstallValid()
        {
            return File.Exists(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath));
        }

        public bool IsImportValid(string path)
        {
            string exePath = Path.Combine(path, "PackageInfo.bin");
            Error = $"Invalid Path! Fail to find {Path.GetFileName(exePath)}";
            return File.Exists(exePath);
        }

        public string GetLocalBuild()
        {
            var steamCMD = new Installer.SteamCMD();
            return steamCMD.GetLocalBuild(_serverData.ServerID, AppId);
        }

        public async Task<string> GetRemoteBuild()
        {
            var steamCMD = new Installer.SteamCMD();
            return await steamCMD.GetRemoteBuild(AppId);
        }

        private async void DownloadModAPI(string modApiDownloadURL)
        {
            string modAPIPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\" + modAPIFileName);
            if (File.Exists(modAPIPath))
            {
                File.Delete(modAPIPath);
            }
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFileAsync(new Uri(modApiDownloadURL), modAPIPath);
                    await Task.Delay(5000);
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
                Console.WriteLine("Failed to download the mod API: " + e.Message);
            }
        }

        private async Task<string> GetLatestModAPIVersion(string method = "json")
        {
            string latestVersion = modAPIVersion;
            await Task.Run(() =>
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "WindowsGSM");
                    string json = webClient.DownloadString(apiVersionURL);
                    JObject data = JObject.Parse(json);
                            
                    // Loop through the assets in the API response until we find the download URL for the mod API
                    foreach (var asset in data["assets"]?.Children())
                    {
                        if (asset["name"].ToString().ToLower().Contains("xinput"))
                        {
                            // Set the latest version
                            latestVersion = asset["tag_name"].ToString().Replace("v", "");
                            break;
                        }
                    }
                    
                }

                // Save the latest version to the modAPIVersion.json file or to the registry
                try
                {
                    if (latestVersion == "")
                    {
                        Error = "Failed to get the latest version from the API";
                        return latestVersion;
                    }
                    else if (latestVersion == modAPIVersion)
                    {
                        Notice = "The mod API is already up to date";
                        return latestVersion;
                    }
                    else
                    {
                        Notice = $"New mod API version available: {latestVersion}! Downloading and installing...";

                        if (method == "json")
                        {
                            // Try to write the latest version to the modAPIVersion.json file
                            string modAPIVersionPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\modAPIVersion.json");
                            if (File.Exists(modAPIVersionPath))
                            {
                                try
                                {
                                    JObject data = JObject.Parse(File.ReadAllText(modAPIVersionPath));
                                    data["version"] = latestVersion;
                                    string json = JsonConvert.SerializeObject(data);
                                    File.WriteAllText(modAPIVersionPath, json);
                                    return latestVersion;
                                }
                                catch (Exception e)
                                {
                                    Error = e.Message;
                                    return latestVersion;
                                }
                            }
                            else
                            {
                                Error = "Failed to write the latest version to the modAPIVersion.json file";
                                return latestVersion;
                            }
                        }
                        else if (method == "registry")
                        {
                            try
                            {
                                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Palworld", true);
                                key.SetValue("ModAPIVersion", latestVersion);
                                return latestVersion;
                            }
                            catch (Exception e)
                            {
                                Error = e.Message;
                                return latestVersion;
                            }
                        }
                        else
                        {
                            Error = "Invalid method";
                            return latestVersion;
                        }
                    }
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return latestVersion;
                }
            });
            return latestVersion;
        }

        private string GetLocalModAPIVersion(string method = "json")
        {
            string localVersion = modAPIVersion;
            string modAPIVersionPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\modAPIVersion.json");
            if (File.Exists(modAPIVersionPath))
            {
                try
                {
                    if (method == "json")
                    {
                        using (StreamReader r = new StreamReader(modAPIVersionPath))
                        {
                            string json = r.ReadToEnd();
                            JObject data = JObject.Parse(json);
                            
                            // Get the local version from the modAPIVersion.json file and remove the 'v' from the version number
                            localVersion = data["version"].ToString().Replace("v", "");
                            return localVersion;
                        }
                    }
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return localVersion;
                }
            }
            else if (method == "registry")
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Palworld", false);
                    localVersion = key.GetValue("ModAPIVersion").ToString();
                    return localVersion;
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return localVersion;
                }
            }
            else
            {
                Error = "Invalid method";
                return localVersion;
            }

            return localVersion;
        }

        // - Delete old files from the Mod API, if they exist, in a simplistic manner:
        private void DeleteOldFile(string relativePath)
        {
            string oldFile = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, relativePath);
            if (File.Exists(oldFile))
            {
                File.Delete(oldFile);
            }
        }

        // - Get and return the public IP of the server
        public async Task<string> GetPublicIP()
        {
            var publicIPv4Address = PublicIP;
            await Task.Run(() =>
            {

                var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";

                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
                        webClient.Headers.Add(HttpRequestHeader.Accept, "text/html");
                        publicIPv4Address = webClient.DownloadString(publicIPRetrievalURL);
                        Notice = $"Your Public IP is: {publicIPv4Address}";
                        return publicIPv4Address;
                    }
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return "127.0.0.1";
                }
            });
            return publicIPv4Address ?? "127.0.0.1";
        }

        private async void InstallModAPI()
        {
            string modAPIPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\" + modAPIFileName);

            Console.WriteLine("Installing the mod API...");

            if (DEBUG_MODE)
                Console.WriteLine("Mod API file path: " + modAPIPath);

            if (File.Exists(modAPIPath))
            {
                try
                {
                    Console.WriteLine("Extracting the mod API...");
                    await FileManagement.ExtractZip(modAPIPath, Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, @"Pal\Binaries\Win64\"));
                    await Task.Delay(5000);
                    File.Delete(modAPIPath);

                    // Delete misc files from the mod API, if they exist:
                    DeleteOldFile(@"Pal\Binaries\Win64\Changelog.md");
                    DeleteOldFile(@"Pal\Binaries\Win64\Readme.md");
                }
                catch (Exception e)
                {
                    Error = e.Message;
                }
            }
        }
    }
}