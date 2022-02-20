﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace StardewRPG
{
    public partial class ModEntry
	{
		private static int statsX = 79;
		private static int statsY = 237;
		private static int statY = 85;
		private static void CharacterCustomization_setUpPositions_Postfix(CharacterCustomization __instance)
		{
			if (!Config.EnableMod || __instance.source != CharacterCustomization.Source.NewGame)
				return;

			int x = __instance.xPositionOnScreen + __instance.width + 4 + 8 + 88 + 4 + 8;
			int y = __instance.yPositionOnScreen;

			for (int i = 0; i < skillNames.Length; i++)
            {
				string name = skillNames[i];
				__instance.leftSelectionButtons.Add(new ClickableTextureComponent("SDRPG_" + name, new Rectangle(new Point(x + statsX + 80, y + statsY + i * statY), new Point(64, 64)), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 106 - i,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				__instance.rightSelectionButtons.Add(new ClickableTextureComponent("SDRPG_" + name, new Rectangle(new Point(x + statsX + 180, y + statsY + i * statY), new Point(64, 64)), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 106 - i,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}
		}
		private static void CharacterCustomization_selectionClick_Prefix(CharacterCustomization __instance, string name, int change)
		{
			if (!Config.EnableMod || __instance.source != CharacterCustomization.Source.NewGame || !name.StartsWith("SDRPG_") || (change == 1 && GetStatValue(Game1.player, "points") <= 0))
				return;
			string stat = name.Substring("SDRPG_".Length);
			int oldStat = GetStatValue(Game1.player, stat);
			if ((change == 1 && oldStat >= Config.MaxStatValue) || (change == -1 && oldStat <= Config.MinStatValue))
				return;
			SetModData(Game1.player, stat, oldStat + change);
			SetModData(Game1.player, "points", GetStatValue(Game1.player, "points") - change);
		}
		private static void CharacterCustomization_draw_Prefix(CharacterCustomization __instance, SpriteBatch b)
		{
			if (!Config.EnableMod || __instance.source != CharacterCustomization.Source.NewGame)
				return;
			int x = __instance.xPositionOnScreen + __instance.width + 4 + 8 + 88 + 4 + 8;
			int y = __instance.yPositionOnScreen;
			Game1.drawDialogueBox(x, __instance.yPositionOnScreen, 400, __instance.height, false, true, null, false, true, -1, -1, -1);
			Utility.drawTextWithShadow(b, SHelper.Translation.Get("stardew-rpg"), Game1.dialogueFont, new Vector2(x + 78, y + 112), Color.SaddleBrown, 1f, -1f, -1, -1, 1f, 3);

			int points = GetStatValue(Game1.player, "points");
			if (points < 0)
            {
				points = Config.StartStatExtraPoints;
				SetModData(Game1.player, "points", points);
            }

			Utility.drawTextWithShadow(b, string.Format(SHelper.Translation.Get("x-points-left"), points), Game1.smallFont, new Vector2(x + statsX, y + 128 + 50), Color.Black, 1f, -1f, -1, -1, 1f, 3);
			for (int i = 0; i < skillNames.Length; i++)
			{
				Utility.drawTextWithShadow(b, SHelper.Translation.Get(skillNames[i]), Game1.smallFont, new Vector2(x + statsX, y + statsY + 17 + i * statY), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				int val = GetStatValue(Game1.player, skillNames[i]);

				Utility.drawTextWithShadow(b, val.ToString(), Game1.smallFont, new Vector2(x + statsX + 130 + 17, y + statsY + 17 + i * statY), Color.Black, 1f, -1f, -1, -1, 1f, 3);
			}
		}
		private static void SkillsPage_draw_Postfix(SkillsPage __instance, SpriteBatch b)
        {
            if (!Config.EnableMod || !Config.ManualSkillUpgrades)
                return;

			int totalLevels = GetTotalSkillLevels(Game1.player);
			int newLevels = Math.Max(0, GetExperienceLevel(Game1.player) - totalLevels - 1);

			string pointString = string.Format(SHelper.Translation.Get("x-points"), newLevels);
			b.DrawString(Game1.smallFont, pointString, new Vector2((float)(__instance.xPositionOnScreen + 64 - 12 + 64) - Game1.smallFont.MeasureString(pointString).X / 2f, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 28), Game1.textColor);

			string levelString = string.Format(SHelper.Translation.Get("level-x"), GetExperienceLevel(Game1.player));
			b.DrawString(Game1.smallFont, levelString, new Vector2((float)(__instance.xPositionOnScreen + 64 - 12 + 64) - Game1.smallFont.MeasureString(levelString).X / 2f, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 252), Game1.textColor);

			if (newLevels <= 0)
                return;
			int x = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? (__instance.xPositionOnScreen + __instance.width - 448 - 48) : (__instance.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8));
			int y = __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 8;

			int addedX = 0;
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					Rectangle boundary = (i + 1) % 5 == 0 ? new Rectangle(new Point(addedX + x - 4 + i * 36, y + j * 56), new Point(14 * 4, 9 * 4)) : new Rectangle(new Point(addedX + x - 4 + i * 36, y + j * 56), new Point(8 * 4, 9 * 4));
					if(!boundary.Contains(Game1.getMousePosition(true)))
						continue;
					bool clickable = Game1.player.GetSkillLevel(j) == i;
					if (!clickable)
						return;
					if (SHelper.Input.IsDown(StardewModdingAPI.SButton.MouseLeft))
					{
						int newLevel = 1;
						foreach (int level in skillLevels)
						{
							if (Game1.player.experiencePoints[j] < level)
							{
								Game1.player.experiencePoints[j] = level;
								Game1.player.newLevels.Add(new Point(j, newLevel));

								switch (j)
								{
									case 0:
										Game1.player.FarmingLevel++;
										break;
									case 1:
										Game1.player.MiningLevel++;
										break;
									case 2:
										Game1.player.ForagingLevel++;
										break;
									case 3:
										Game1.player.FishingLevel++;
										break;
									case 4:
										Game1.player.CombatLevel++;
										break;
									case 5:
										Game1.player.LuckLevel++;
										break;
								}
								return;
							}
							newLevel++;
						}
					}
					if ((i + 1) % 5 == 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2(addedX + x + i * 36, y - 4 + j * 56), new Rectangle?(new Rectangle(145 + 14, 338, 14, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.871f);
					}
					else if ((i + 1) % 5 != 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2(addedX + x + i * 36, y - 4 + j * 56), new Rectangle?(new Rectangle(129 + 8 , 338, 8, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.871f);
					}
				}
				if ((i + 1) % 5 == 0)
				{
					addedX += 24;
				}
			}
		}
		public static bool ChatBox_runCommand_Prefix(string command)
		{
			if (!Config.EnableMod)
				return true;

			if (command.Equals("levelup"))
			{
				LevelUp();
				return false;
			}
			return true;
		}
    }
}