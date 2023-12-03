﻿using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ModTool
{
    public class Build
    {
        private int ModID;

        public Build(int ModID)
        {
            this.ModID = ModID;
        }

        public bool Compile(bool showMessage = true)
        {
            try
            {
                switch (Program.Projects[ModID].Type)
                {
                    case ModType.SnakeSkin:
                        CompileSkin(0, showMessage);
                        break;
                    case ModType.FlappyBirdSkin:
                        CompileSkin(1, showMessage);
                        break;
                    case ModType.SnakeMode:
                        CompileMode(0, showMessage);
                        break;
                    case ModType.FlappyBirdMode:
                        CompileMode(1, showMessage);
                        break;
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        private void CompileSkin(int Type = 0, bool showMessage = true)
        {
            if (File.Exists($@"{FManager.GetProjectFolder(ModID)}\image\skin_info.json"))
            {
                var jsonFile = File.ReadAllText($@"{FManager.GetProjectFolder(ModID)}\image\skin_info.json");
                var allSkins = FManager.GetAllSkinsFiles();
                var rpaFile = "Empty";

                switch (Type)
                {
                    case 0:
                        rpaFile = File.ReadAllText($@"{FManager.GetExampleFolder()}\snake_skin.rpy");
                        break;
                    case 1:
                        rpaFile = File.ReadAllText($@"{FManager.GetExampleFolder()}\flappy_bird_skin.rpy");
                        break;
                }
                

                List<Classes.Skin> Skins = new List<Classes.Skin>();
                Dictionary<int, string> UserTextures = JsonConvert.DeserializeObject<Dictionary<int, string>>(jsonFile);

                foreach (string file in allSkins)
                {
                    var jsonSkinFile = File.ReadAllText(file);
                    Classes.Skin skinTP = JsonConvert.DeserializeObject<Classes.Skin>(jsonSkinFile);
                    Skins.Add(skinTP);
                }

                rpaFile = rpaFile.Replace("{label}", Program.Projects[ModID].Name.ToLower().Replace(" ", "_").Replace("  ", "__").Replace("-", "_"));
                rpaFile = rpaFile.Replace("{name}", Program.Projects[ModID].Name);

                var currentSkin = Skins.Find(item => item.Mode == Type);

                List<string> texturesTemp = new List<string>();

                foreach (var pair in UserTextures.OrderBy(kvp => kvp.Key))
                {
                    string textureLine = $"\"{currentSkin.List[pair.Key].Game}\": \"{pair.Value}\"";

                    if (pair.Key != UserTextures.Keys.Max())
                        texturesTemp.Add($"\"{currentSkin.List[pair.Key].Game}\": \"{pair.Value}\",");
                    else
                        texturesTemp.Add($"\"{currentSkin.List[pair.Key].Game}\": \"{pair.Value}\"");
                }

                string textureTemp = null;
                foreach (string tTemp in texturesTemp)
                {
                    string tTempRep = tTemp.Replace(Path.GetFullPath($@"{AppDomain.CurrentDomain.BaseDirectory}..\game\"), string.Empty);
                    tTempRep = tTemp.Replace(Path.GetFullPath($@"{AppDomain.CurrentDomain.BaseDirectory}../game/"), string.Empty);

                    if (SteamAPI.IsSteamRunning())
                    {
                        string pathFolder; uint folderSize = 100000000;
                        SteamApps.GetAppInstallDir(Steam.m_AppID, out pathFolder, folderSize);
                        
                        tTempRep = tTempRep.Replace(Path.GetFullPath($@"{pathFolder}/game/"), string.Empty);
                        tTempRep = tTempRep.Replace(Path.GetFullPath($@"{pathFolder}\game\"), string.Empty);
                    }

                    tTempRep = tTempRep.Replace(@"\", "/");

                    textureTemp += tTempRep + Environment.NewLine + "        ";
                }

                textureTemp = textureTemp.TrimEnd('\r', '\n', ' ', '\t');
                rpaFile = rpaFile.Replace("{replace}", textureTemp);

                File.Create($@"{FManager.GetProjectFolder(ModID)}\{Program.Projects[ModID].Name.ToLower().Replace(" ", "_")}.rpy").Close();
                File.WriteAllText($@"{FManager.GetProjectFolder(ModID)}\{Program.Projects[ModID].Name.ToLower().Replace(" ", "_")}.rpy", rpaFile);

                if(showMessage)
                    MessageBox.Show(Config.GetText("info_build_success_message"), $"{Config.GameName} - Mod Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (showMessage)
                {
                    MessageBox.Show(Config.GetText("error_texture_none_message"), $"{Config.GameName} - Mod Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception();
                }
            }
        }

        private void CompileMode(int Type = 0, bool showMessage = true)
        {
            if (File.Exists($@"{FManager.GetProjectFolder(ModID)}\mode_info.json"))
            {
                var jsonFile = File.ReadAllText($@"{FManager.GetProjectFolder(ModID)}\mode_info.json");
                var allModes = FManager.GetAllModesFiles();
                var rpaFile = "Empty";

                switch (Type)
                {
                    case 0:
                        rpaFile = File.ReadAllText($@"{FManager.GetExampleFolder()}\snake_mode.rpy");
                        break;
                    case 1:
                        rpaFile = File.ReadAllText($@"{FManager.GetExampleFolder()}\flappy_bird_mode.rpy");
                        break;
                }


                List<Classes.ModG> Modes = new List<Classes.ModG>();
                Dictionary<string, string> UserParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFile);

                foreach (string file in allModes)
                {
                    var jsonSkinFile = File.ReadAllText(file);
                    Classes.ModG skinTP = JsonConvert.DeserializeObject<Classes.ModG>(jsonSkinFile);
                    Modes.Add(skinTP);
                }

                rpaFile = rpaFile.Replace("{label}", Program.Projects[ModID].Name.ToLower().Replace(" ", "_").Replace("  ", "__").Replace("-", "_"));
                rpaFile = rpaFile.Replace("{name}", Program.Projects[ModID].Name);

                var currentSkin = Modes.Find(item => item.Mode == Type);

                List<string> texturesTemp = new List<string>();

                for (int i = 0; UserParams.Count > i; i++)
                {

                    if (decimal.TryParse(UserParams.ElementAt(i).Value.Replace(".", ","), out decimal value_dcm))
                    {
                        if ((i + 1) != UserParams.Count)
                            texturesTemp.Add($"\"{currentSkin.List[i].Game}\": {UserParams.ElementAt(i).Value.Replace(",", ".")},");
                        else
                            texturesTemp.Add($"\"{currentSkin.List[i].Game}\": {UserParams.ElementAt(i).Value.Replace(",", ".")}");
                    }

                    else if (bool.TryParse(UserParams.ElementAt(i).Value.Replace(".", ","), out bool value_bl))
                    {
                        if ((i + 1) != UserParams.Count)
                            texturesTemp.Add($"\"{currentSkin.List[i].Game}\": {value_bl},");
                        else
                            texturesTemp.Add($"\"{currentSkin.List[i].Game}\": {value_bl}");
                    }

                    else
                    {
                        if ((i + 1) != UserParams.Count)
                            texturesTemp.Add($"\"{currentSkin.List[i].Game}\": \"{UserParams.ElementAt(i).Value}\",");
                        else
                            texturesTemp.Add($"\"{currentSkin.List[i].Game}\": \"{UserParams.ElementAt(i).Value}\"");
                    }
                }

                string textureTemp = null;
                foreach (string tTemp in texturesTemp)
                {
                    string tTempRep = tTemp.Replace($@"{AppDomain.CurrentDomain.BaseDirectory}..\game\", string.Empty);

                    if (SteamAPI.IsSteamRunning())
                    {
                        string pathFolder; uint folderSize = 100000000;
                        SteamApps.GetAppInstallDir(Steam.m_AppID, out pathFolder, folderSize);

                        tTempRep = tTempRep.Replace($@"{pathFolder}/game/", string.Empty);
                        tTempRep = tTempRep.Replace($@"{pathFolder}\game\", string.Empty);
                    }

                    tTempRep = tTempRep.Replace(@"\", "/");

                    textureTemp += tTempRep + Environment.NewLine + "        ";
                }

                textureTemp = textureTemp.TrimEnd('\r', '\n', ' ', '\t');
                rpaFile = rpaFile.Replace("{replace}", textureTemp);

                File.Create($@"{FManager.GetProjectFolder(ModID)}\{Program.Projects[ModID].Name.ToLower().Replace(" ", "_")}.rpy").Close();
                File.WriteAllText($@"{FManager.GetProjectFolder(ModID)}\{Program.Projects[ModID].Name.ToLower().Replace(" ", "_")}.rpy", rpaFile);
                
                if (showMessage)
                    MessageBox.Show(Config.GetText("info_build_success_message"), $"{Config.GameName} - Mod Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (showMessage)
                {
                    MessageBox.Show(Config.GetText("error_param_none_message"), $"{Config.GameName} - Mod Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception();
                }
            }      
        }
    }
}
