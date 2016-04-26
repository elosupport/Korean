// Copyright 2014 - 2014 Esk0r
// Config.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

#endregion

namespace Evade
{
    internal static class Config
    {
        public const bool PrintSpellData = false;
        public const bool TestOnAllies = false;
        public static int SkillShotsExtraRadius
        {
            get { return 9 + humanizer["ExtraEvadeRange"].Cast<Slider>().CurrentValue; }
        }
        public static int SkillShotsExtraRange
        {
            get { return 20 + humanizer["ExtraEvadeRange"].Cast<Slider>().CurrentValue; }
        }
        public const int GridSize = 10;
        public static int ExtraEvadeDistance
        {
            get { return 15 + humanizer["ExtraEvadeRange"].Cast<Slider>().CurrentValue; }
        }

        public static int ActivationDelay
        {
            get { return 0; /*return humanizer["ActivationDelay"].Cast<Slider>().CurrentValue;*/ }
        }
        public const int PathFindingDistance = 60;
        public const int PathFindingDistance2 = 35;

        public const int DiagonalEvadePointsCount = 7;
        public const int DiagonalEvadePointsStep = 20;

        public const int CrossingTimeOffset = 250;

        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 80;

        public const int EvadingRouteChangeTimeOffset = 250;

        public const int EvadePointChangeInterval = 300;
        public static int LastEvadePointChangeT = 0;

        public static Menu Menu, evadeSpells, skillShots, shielding, collision, drawings, misc, humanizer;
        public static Color EnabledColor, DisabledColor, MissileColor;

        public static void CreateMenu()
        {
            Menu = MainMenu.AddMenu("Evade", "evade");

            if (Menu == null)
            {
                Chat.Print("LOAD FAILED", Color.Red);
                Console.WriteLine("Evade:: LOAD FAILED");
                throw new NullReferenceException("Menu NullReferenceException");
            }

            //Create the evade spells submenus.
            evadeSpells = Menu.AddSubMenu("Evade spells", "evadeSpells");
            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                evadeSpells.AddGroupLabel(spell.Name);

                try
                {
                    evadeSpells.Add("DangerLevel" + spell.Name, new Slider("Danger level", spell._dangerLevel, 1, 5));
                }
                catch (Exception e)
                {
                    throw e;
                }

                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                {
                    evadeSpells.Add("WardJump" + spell.Name, new CheckBox("WardJump"));
                }

                evadeSpells.Add("Enabled" + spell.Name, new CheckBox("Enabled"));
            }

            //Create the skillshots submenus.
            skillShots = Menu.AddSubMenu("Skillshots", "Skillshots");

            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                if (hero.Team != ObjectManager.Player.Team || Config.TestOnAllies)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (String.Equals(spell.ChampionName, hero.ChampionName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            skillShots.AddGroupLabel(spell.SpellName);
                            skillShots.Add("DangerLevel" + spell.MenuItemName, new Slider("Danger level", spell.DangerValue, 1, 5));

                            skillShots.Add("IsDangerous" + spell.MenuItemName, new CheckBox("Is Dangerous", spell.IsDangerous));

                            skillShots.Add("Draw" + spell.MenuItemName, new CheckBox("Draw"));
                            skillShots.Add("Enabled" + spell.MenuItemName, new CheckBox("Enabled", !spell.DisabledByDefault));
                        }
                    }
                }
            }

            shielding = Menu.AddSubMenu("Ally shielding", "Shielding");

            foreach (var ally in ObjectManager.Get<AIHeroClient>())
            {
                if (ally.IsAlly && !ally.IsMe)
                {
                    shielding.Add("shield" + ally.ChampionName, new CheckBox("Shield " + ally.ChampionName));
                }
            }

            collision = Menu.AddSubMenu("Collision", "Collision");
            collision.Add("MinionCollision", new CheckBox("Minion collision"));
            collision.Add("HeroCollision", new CheckBox("Hero collision"));
            collision.Add("YasuoCollision", new CheckBox("Yasuo wall collision"));
            collision.Add("EnableCollision", new CheckBox("Enabled"));
            //TODO add mode.

            drawings = Menu.AddSubMenu("Drawings", "Drawings");
            //EnabledColor = drawings.AddColor(new[] { "ECA", "ECR", "ECG", "ECB"}, "Enabled Spell Color", Color.White);
            //DisabledColor = drawings.AddColor(new[] {"DCA", "DCR", "DCG", "DCB"}, "Disabled Spell Color", Color.Red);
            //MissileColor = drawings.AddColor(new[] { "MCA", "MCR", "MCG", "MCB" }, "Missile Color", Color.Red);

            drawings.AddLabel("Enabled Draw Color");
            drawings.Add("EnabledDrawA", new Slider("A", 255, 1, 255));
            drawings.Add("EnabledDrawR", new Slider("R", 255, 1, 255));
            drawings.Add("EnabledDrawG", new Slider("G", 255, 1, 255));
            drawings.Add("EnabledDrawB", new Slider("B", 255, 1, 255));

            EnabledColor = Color.FromArgb(drawings["EnabledDrawA"].Cast<Slider>().CurrentValue,
                drawings["EnabledDrawR"].Cast<Slider>().CurrentValue,
                drawings["EnabledDrawG"].Cast<Slider>().CurrentValue,
                drawings["EnabledDrawB"].Cast<Slider>().CurrentValue);

            drawings.AddSeparator(5);

            drawings.AddLabel("Disabled Draw Color");
            drawings.Add("DisabledDrawA", new Slider("A", 255, 1, 255));
            drawings.Add("DisabledDrawR", new Slider("R", 255, 1, 255));
            drawings.Add("DisabledDrawG", new Slider("G", 255, 1, 255));
            drawings.Add("DisabledDrawB", new Slider("B", 255, 1, 255));

            DisabledColor = Color.FromArgb(drawings["DisabledDrawA"].Cast<Slider>().CurrentValue,
                drawings["DisabledDrawR"].Cast<Slider>().CurrentValue,
                drawings["DisabledDrawG"].Cast<Slider>().CurrentValue,
                drawings["DisabledDrawB"].Cast<Slider>().CurrentValue);

            drawings.AddSeparator(5);

            drawings.AddLabel("Missile Draw Color");
            drawings.Add("MissileDrawA", new Slider("A", 255, 1, 255));
            drawings.Add("MissileDrawR", new Slider("R", 255, 1, 255));
            drawings.Add("MissileDrawG", new Slider("G", 255, 1, 255));
            drawings.Add("MissileDrawB", new Slider("B", 255, 1, 255));

            MissileColor = Color.FromArgb(drawings["MissileDrawA"].Cast<Slider>().CurrentValue,
                drawings["MissileDrawR"].Cast<Slider>().CurrentValue,
                drawings["MissileDrawG"].Cast<Slider>().CurrentValue,
                drawings["MissileDrawB"].Cast<Slider>().CurrentValue);

            drawings.AddSeparator(5);

            drawings.Add("EnabledDraw", new CheckBox("Draw Enabled"));
            drawings.Add("DisabledDraw", new CheckBox("Draw Disabled"));
            drawings.Add("MissileDraw", new CheckBox("Draw Missile"));

            //drawings.AddItem(new MenuItem("EnabledColor", "Enabled spell color").SetValue(Color.White));
            //drawings.AddItem(new MenuItem("DisabledColor", "Disabled spell color").SetValue(Color.Red));
            //drawings.AddItem(new MenuItem("MissileColor", "Missile color").SetValue(Color.LimeGreen));
            drawings.Add("Border", new Slider("Border Width", 1, 5, 1));

            drawings.Add("EnableDrawings", new CheckBox("Enabled"));
            drawings.Add("ShowEvadeStatus", new CheckBox("Draw Evade Status"));

            humanizer = Menu.AddSubMenu("Humanizer");

            //humanizer.Add("ActivationDelay", new Slider("Activation Delay"));
            humanizer.Add("ExtraEvadeRange", new Slider("Extra Evade Range"));

            misc = Menu.AddSubMenu("Misc", "Misc");
            misc.AddStringList("BlockSpells", "Block spells while evading", new[] { "No", "Only dangerous", "Always" }, 1);
            //misc.Add("BlockSpells", "Block spells while evading").SetValue(new StringList(new []{"No", "Only dangerous", "Always"}, 1)));
            misc.Add("DisableFow", new CheckBox("Disable fog of war dodging"));
            misc.Add("ShowEvadeStatus", new CheckBox("Show Evade Status"));
            if (ObjectManager.Player.BaseSkinName == "Olaf")
            {
                misc.Add("DisableEvadeForOlafR", new CheckBox("Automatic disable Evade when Olaf's ulti is active!"));
            }

            Menu.Add("Enabled", new KeyBind("Enabled", true, KeyBind.BindTypes.PressToggle, "K".ToCharArray()[0]));

            Menu.Add("OnlyDangerous", new KeyBind("Dodge only dangerous", false, KeyBind.BindTypes.HoldActive, 32)); //Space
        }
    }
}
