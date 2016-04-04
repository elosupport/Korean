using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace BaseUltPlus
{
    public class Program
    {
        public static Menu BaseUltMenu { get; set; }

        public static void Main(string[] args)
        {
            // Wait till the name has fully loaded
            Loading.OnLoadingComplete += LoadingOnOnLoadingComplete;
        }

        private static void LoadingOnOnLoadingComplete(EventArgs args)
        {
            //Menu
            BaseUltMenu = MainMenu.AddMenu("BaseUlt+", "baseUltMenu");
            BaseUltMenu.AddGroupLabel("BaseUlt+ General");
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("baseult", new CheckBox("BaseUlt"));
            BaseUltMenu.Add("showrecalls", new CheckBox("Show Recalls"));
            BaseUltMenu.Add("showallies", new CheckBox("Show Allies"));
            BaseUltMenu.Add("showenemies", new CheckBox("Show Enemies"));
            BaseUltMenu.Add("checkcollision", new CheckBox("Check Collision"));
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("timeLimit", new Slider("FOW Time Limit (SEC)", 20, 0, 120));
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("nobaseult", new KeyBind("No BaseUlt while", false, KeyBind.BindTypes.HoldActive, 32));
            BaseUltMenu.AddSeparator();
            BaseUltMenu.Add("x", new Slider("Offset X", 0, -500, 500));
            BaseUltMenu.Add("y", new Slider("Offset Y", 0, -500, 500));
            BaseUltMenu.AddGroupLabel("BaseUlt+ Targets");
            foreach (var unit in HeroManager.Enemies)
            {
                BaseUltMenu.Add("target" + unit.ChampionName, new CheckBox(string.Format("{0} ({1})", unit.ChampionName, unit.Name)));
            }
            BaseUltMenu.AddGroupLabel("BaseUlt+ Credits");
            BaseUltMenu.AddLabel("By: LunarBlue");
            BaseUltMenu.AddLabel("Testing: FinnDev");

            // Initialize the Addon
            OfficialAddon.Initialize();

            // Listen to the two main events for the Addon
            Game.OnUpdate += args1 => OfficialAddon.OnUpdate();
            Drawing.OnEndScene += args1 => OfficialAddon.OnEndScene();
            Teleport.OnTeleport += OfficialAddon.OnTeleport;
        }
    }
}
