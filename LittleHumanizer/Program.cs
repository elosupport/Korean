using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace LittleHumanizer
{
    static class Program
    {
        static Menu _menu, _setting;
        static Random _random;
        static Dictionary<string, int> _lastCommandT;
        static int _blockedCount = 0;
        static bool _thisMovementCommandHasBeenTamperedWith = false;
        public static LastSpellCast LastSpell = new LastSpellCast();
        public static List<LastSpellCast> LastSpellsCast = new List<LastSpellCast>();

        static double Randomize(int min, int max)
        {
            var x = _random.Next(min, max) + 1 + 1 - 1 - 1;
            var y = _random.Next(min, max);
            if (_random.Next(0, 1) > 0)
            {
                return x;
            }
            if (1 == 1)
            {
                return (x + y) / 2d;
            }
            return y;
        }

        static void Main(string[] args)
        {
            _lastCommandT = new Dictionary<string, int>();
            foreach (var order in Enum.GetValues(typeof(GameObjectOrder)))
            {
                _lastCommandT.Add(order.ToString(), 0);
            }
            foreach (var spellslot in Enum.GetValues(typeof(SpellSlot)))
            {
                _lastCommandT.Add("spellcast" + spellslot, 0);
            }
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            _random = new Random(Environment.TickCount - Environment.TickCount);
            _menu = MainMenu.AddMenu("LittleHumanizer", "LittleHumanizer");
            _menu.AddGroupLabel("LittleHumanizer");
            _menu.AddLabel("Based on 'I'm a Sharp Human Pro'");
            _setting = _menu.AddSubMenu("Settings", "settings");
            _setting.AddGroupLabel("Settings");
            _setting.AddLabel("Delays");
            _setting.Add("Spells", new CheckBox("Humanize Spells"));
            _setting.Add("Attacks", new CheckBox("Humanize Attacks"));
            _setting.Add("Movements", new CheckBox("Humanize Movements"));
            _setting.Add("MinClicks", new Slider("Min clicks per second", _random.Next(6, 7), 1, 7));
            _setting.Add("MaxClicks", new Slider("Max clicks per second", _random.Next(0, 1) > 0 ? (int)Math.Floor(RandomizeSliderValues(7, 11)) : (int)Math.Ceiling(RandomizeSliderValues(7, 11)), 7, 15));
        }

        public static void Drawings_OnDraw(EventArgs args)
        {
            Drawing.DrawText(Drawing.Width - 190, 100, System.Drawing.Color.Lime, "Blocked : " + _blockedCount + " Clicks");
        }

        public static void Player_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs issueOrderEventArgs)
        {
            if (sender.IsMe && !issueOrderEventArgs.IsAttackMove)
            {
                if (issueOrderEventArgs.Order == GameObjectOrder.AttackUnit ||
                    issueOrderEventArgs.Order == GameObjectOrder.AttackTo &&
                    !_menu["Attacks"].Cast<CheckBox>().CurrentValue)
                    return;
                if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo &&
                    !_menu["Movement"].Cast<CheckBox>().CurrentValue)
                    return;
            }

            var orderName = issueOrderEventArgs.Order.ToString();
            var order = _lastCommandT.FirstOrDefault(e => e.Key == orderName);
            if (Environment.TickCount - order.Value <
                Randomize(
                    1000 / _menu["MaxClicks"].Cast<CheckBox>().CurrentValue,
                    1000 / _menu["MinClicks"].Cast<CheckBox>().CurrentValue) + _random.Next(-10, 10))
            {
                _blockedCount += 1;
                issueOrderEventArgs.Process = false;
                return;
            }
            if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo &&
                        issueOrderEventArgs.TargetPosition.IsValid() && !_thisMovementCommandHasBeenTamperedWith)
            {
                _thisMovementCommandHasBeenTamperedWith = true;
                issueOrderEventArgs.Process = false;
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                    issueOrderEventArgs.TargetPosition.Randomize(-10, 10));
            }
            _thisMovementCommandHasBeenTamperedWith = false;
            _lastCommandT.Remove(orderName);
            _lastCommandT.Add(orderName, Environment.TickCount);
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!_menu["Spells"].Cast<CheckBox>().CurrentValue)
                return;
            if (!sender.Owner.IsMe)
                return;
            if (!(new[]
            {
                SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R, SpellSlot.Summoner1, SpellSlot.Summoner2
                , SpellSlot.Item1, SpellSlot.Item2, SpellSlot.Item3, SpellSlot.Item4, SpellSlot.Item5, SpellSlot.Item6,
                SpellSlot.Trinket
            })
                .Contains(args.Slot))
                return;
            if (Environment.TickCount - LastSpell.CastTick < 50)
            {
                args.Process = false;
                BlockedSpellsCount += 1;
            }
            else
            {
                LastSpell = new LastSpellCast { Slot = args.Slot, CastTick = Environment.TickCount };
            }
            if (LastSpellsCast.Any(x => x.Slot == args.Slot))
            {
                var spell = LastSpellsCast.FirstOrDefault(x => x.Slot == args.Slot);
                if (spell != null)
                {
                    if (Environment.TickCount - spell.CastTick <= 250 + Game.Ping)
                    {
                        args.Process = false;
                        BlockedClicksCount += 1;
                    }
                    else
                    {
                        LastSpellsCast.RemoveAll(x => x.Slot == args.Slot);
                        LastSpellsCast.Add(new LastSpellCast { Slot = args.Slot, CastTick = Environment.TickCount });
                    }
                }
                else
                {
                    LastSpellsCast.Add(new LastSpellCast { Slot = args.Slot, CastTick = Environment.TickCount });
                }
            }
            else
            {
                LastSpellsCast.Add(new LastSpellCast { Slot = args.Slot, CastTick = Environment.TickCount });
            }
        }

        public static Vector3 Randomize(Vector3 position, int min, int max)
        {
            var ran = new Random(Environment.TickCount);
            return position + new Vector2(ran.Next(min, max), ran.Next(min, max)).To3D();
        }

        public static bool IsWall(Vector3 vector)
        {
            return NavMesh.GetCollisionFlags(vector.X, vector.Y).HasFlag(CollisionFlags.Wall);
        }

        public class LastSpellCast
        {
            public int CastTick;
            public SpellSlot Slot = SpellSlot.Unknown;
        }

    }
}
