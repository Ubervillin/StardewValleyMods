﻿using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomOreNodes
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {


        private void ReloadOreData(bool first = false)
        {

            customOreNodesList.Clear();
            CustomOreData data;
            int id = 42424000;
            Dictionary<int, int> existingPSIs = new Dictionary<int, int>();
            CustomOreConfig conf = Helper.Data.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
            foreach (KeyValuePair<int, int> psi in conf.parentSheetIndexes)
            {
                existingPSIs[psi.Value] = psi.Key;
            }
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                conf = contentPack.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
                foreach (KeyValuePair<int, int> psi in conf.parentSheetIndexes)
                {
                    existingPSIs[psi.Value] = psi.Key;
                }

            }
            try
            {
                if (File.Exists(Path.Combine(Helper.DirectoryPath, "custom_ore_nodes.json")))
                {
                    int add = 0;
                    try
                    {
                        data = Helper.Content.Load<CustomOreData>("custom_ore_nodes.json", ContentSource.ModFolder);

                    }
                    catch
                    {
                        var tempData = Helper.Content.Load<CustomOreDataOld>("custom_ore_nodes.json", ContentSource.ModFolder);
                        data = new CustomOreData();
                        for (int i = 0; i < tempData.nodes.Count; i++)
                        {
                            data.nodes.Add(new CustomOreNode(tempData.nodes[i]));
                        }
                        if (first)
                        {
                            Monitor.Log($"Rewriting custom_ore_nodes.json", LogLevel.Debug);
                            Helper.Data.WriteJsonFile("custom_ore_nodes.json", data);
                        }
                    }
                    conf = Helper.Data.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
                    foreach (object nodeObj in data.nodes)
                    {
                        CustomOreNode node = (CustomOreNode)nodeObj;

                        if (node.spriteType == "mod")
                        {
                            node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.ModFolder);
                        }
                        else
                        {
                            node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent);
                        }
                        if (conf.parentSheetIndexes.ContainsKey(add))
                        {
                            node.parentSheetIndex = conf.parentSheetIndexes[add];
                        }
                        else
                        {
                            while (existingPSIs.ContainsKey(id))
                                id++;
                            node.parentSheetIndex = id++;
                        }
                        conf.parentSheetIndexes[add] = node.parentSheetIndex;

                        customOreNodesList.Add(node);
                        add++;
                    }
                    if (first)
                    {
                        Monitor.Log($"Got {customOreNodesList.Count} ores from mod", LogLevel.Debug);
                        Helper.Data.WriteJsonFile("ore_config.json", conf);
                    }

                }
                else
                {
                    SMonitor.Log("No custom_ore_nodes.json in mod directory.");
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log("Error processing custom_ore_nodes.json: " + ex, LogLevel.Error);
            }

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    int add = 0;
                    conf = contentPack.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
                    if (first)
                        Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");

                    try
                    {
                        data = contentPack.ReadJsonFile<CustomOreData>("custom_ore_nodes.json");
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"exception {ex}", LogLevel.Error);
                        var tempData = contentPack.ReadJsonFile<CustomOreDataOld>("custom_ore_nodes.json");
                        data = new CustomOreData();
                        for (int i = 0; i < tempData.nodes.Count; i++)
                        {
                            data.nodes.Add(new CustomOreNode(tempData.nodes[i]));
                        }
                        if (first)
                        {
                            Monitor.Log($"Rewriting custom_ore_nodes.json", LogLevel.Debug);
                            contentPack.WriteJsonFile("custom_ore_nodes.json", data);
                        }
                    }

                    foreach (CustomOreNode node in data.nodes)
                    {
                        if (node.spriteType == "mod")
                        {
                            node.texture = contentPack.LoadAsset<Texture2D>(node.spritePath);

                        }
                        else
                        {
                            node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent);

                        }
                        if (conf.parentSheetIndexes.ContainsKey(add))
                        {
                            node.parentSheetIndex = conf.parentSheetIndexes[add];
                        }
                        else
                        {
                            while (existingPSIs.ContainsKey(id))
                                id++;
                            node.parentSheetIndex = id++;
                        }
                        conf.parentSheetIndexes[add] = node.parentSheetIndex;
                        customOreNodesList.Add(node);
                        add++;
                    }
                    if (first)
                    {
                        Monitor.Log($"Got {data.nodes.Count} ores from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                        contentPack.WriteJsonFile("ore_config.json", conf);
                    }
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Error processing custom_ore_nodes.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
                }
            }
            if (first)
                Monitor.Log($"Got {customOreNodesList.Count} ores total", LogLevel.Debug);
        }


        private static bool IsInRange(OreLevelRange range, GameLocation location, bool mineOnly)
        {

            int difficulty = (location is MineShaft) ? ((location as MineShaft).mineLevel > 120 ? Game1.netWorldState.Value.SkullCavesDifficulty : Game1.netWorldState.Value.MinesDifficulty) : 0;

            return (range.minLevel < 1 && !(location is MineShaft) && !mineOnly) || (location is MineShaft && (range.minLevel <= (location as MineShaft).mineLevel && (range.maxLevel < 0 || (location as MineShaft).mineLevel <= range.maxLevel))) && (range.minDifficulty <= difficulty) && (range.maxDifficulty < 0 || range.maxDifficulty >= difficulty);
        }
    }
}
 