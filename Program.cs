using System;
using System.Dynamic;
using System.Linq;
using System.Net;
using LeagueSharp.Common;
using LeagueSharp;
using Color = System.Drawing.Color;

namespace PewPewTristana
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Welcome Message upon loading assembly.
            Game.PrintChat(
                "<font color=\"#00BFFF\">PewPewTristana -<font color=\"#FFFFFF\"> Recommended Version Successfully Loaded.</font>");
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        //Champ Name, Dynamic Range settings, Orbwalker and stuff
        public const string ChampName = "Tristana";
        public static int SpellRangeTick;
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;


        //Specific Spells - Combos etc.
        //Can be used to create custom spells to use for example in seperate combos
        //Ez Life
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell OneShot;

        //ObjectManager,
        private static Obj_AI_Hero player = ObjectManager.Player;

        public static SpellSlot IgniteSlot;

        public static HpBarIndicator Hpi = new HpBarIndicator();

        private static void OnLoad(EventArgs args)
        {
            //Loads if player.pick = Tristana,
            if (player.ChampionName != ChampName)
                return;

            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q);

            //RocketJump Settings
            W = new Spell(SpellSlot.W, 900);
            W.SetSkillshot(0.25f, 150, 1200, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 630);
            R = new Spell(SpellSlot.R, 630);

            E.SetTargetted(0.25f, 2000f);


            //OneShot Settings
            OneShot = new Spell(SpellSlot.R, 630);

            //Items

            //Main Menu & Sub Menus
            Config = new Menu("PewPewTristana", "Tristark", true);

            //Orbwalker
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            //Combo Options Menu
            var animePussy = Config.AddSubMenu(new Menu("Combo Mode", "Combo Settings"));
            animePussy.AddItem(new MenuItem("UseQ", "Use Q - Rapid Fire").SetValue(true));
            animePussy.AddItem(new MenuItem("UseE", "Use E - Explosive Charge").SetValue(true));
            animePussy.AddItem(new MenuItem("UseR", "Use R Logic - Bustershot").SetValue(true));
            animePussy.AddItem(new MenuItem("Blank", "                                         "));
            animePussy.AddItem(new MenuItem("UseW", "Use W  Logic - Rocket Jump").SetValue(true));
            animePussy.AddItem(new MenuItem("wturret", "Check if target is in turret range").SetValue(true));
            animePussy.AddItem(new MenuItem("WzL", "  Own HP Percentage").SetValue(new Slider(65, 100, 0)));
            //Wlogic
            animePussy.AddItem(new MenuItem("WL", "  Amount of Enemies").SetValue(new Slider(1, 5, 1)));
            animePussy.AddItem(new MenuItem("eBlank", "                                         "));
            animePussy.AddItem(new MenuItem("UseBOTRK", "Use Blade of the Ruined King").SetValue(true));
            animePussy.AddItem(new MenuItem("eL", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            animePussy.AddItem(new MenuItem("oL", "  Own HP Percentage").SetValue(new Slider(65, 100, 0)));
            animePussy.AddItem(new MenuItem("UseBilge", "Use Bilgewater Cutlass").SetValue(true));
            animePussy.AddItem(new MenuItem("HLe", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));

            //Laneclear

            Config.SubMenu("Harass Settings").AddItem(new MenuItem("hQ", "Use Q - Rapid Fire").SetValue(true));
            Config.SubMenu("Harass Settings").AddItem(new MenuItem("hE", "Use E - Explosive Charge").SetValue(true));

            //Misc Options Menu
            Config.SubMenu("Misc").AddItem(new MenuItem("AntiGap", "Anti Gapcloser - R").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Interrupt Spells - R").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("UsePackets", "Packets are DISBALED FOR NOW").SetValue(true));


            //Spell Ranges
            Config.SubMenu("Drawing")
                .AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Wdraw", "Draw W - Rocket Jump").SetValue(true));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Edraw", "Draw E - Explosive Charge").SetValue(true));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Rdraw", "Draw R - Bustershot").SetValue(true));
            Config.SubMenu("Drawing").AddItem(new MenuItem("Rrdy", "Draw R - Status").SetValue(true));
            Config.SubMenu("Drawing")
                .AddItem(new MenuItem("turretdraw", "Draw if target is in Turret Range").SetValue(true));



            //Damage Indc
            Config.SubMenu("Damage Indicator").AddItem(new MenuItem("DrawD", "         <<Draw Damage>>").SetValue(true));
            Config.SubMenu("Damage Indicator")
                .AddItem(new MenuItem("DrawW", "Draw Rocket Jump Damage (W)").SetValue(false));
            Config.SubMenu("Damage Indicator")
                .AddItem(new MenuItem("DrawE", "Draw Explosive Shot Damage (E)").SetValue(true));
            Config.SubMenu("Damage Indicator")
                .AddItem(new MenuItem("DrawR", "Draw Bustershot Damage (R)").SetValue(true));


            Config.AddToMainMenu();

            //Idk what this is called but it's something <3
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapCloser_OnEnemyGapcloser;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
            TristSpellRanges();


        }


        private static void OnEndScene(EventArgs args)
        {
            //Damage Indicator
            if (Config.SubMenu("Damage Indicator").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.DarkGreen);
                }
            }
        }


        private static int CalcDamage(Obj_AI_Base target)
        {
            //Calculate Combo Damage

            var aa = player.GetAutoAttackDamage(target, true);
            var damage = aa;

            if (IgniteSlot != SpellSlot.Unknown &&
                player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //ITEM BOTRK

            if (Config.Item("UseE").GetValue<bool>()) // edamage
            {
                if (E.IsReady() && Config.Item("DrawE").GetValue<bool>())
                {
                    damage += E.GetDamage(target);
                }
            }

            if (R.IsReady() && Config.Item("DrawR").GetValue<bool>() && Config.Item("UseR").GetValue<bool>()) // rdamage
            {

                damage += R.GetDamage(target);
            }

            if (Config.Item("DrawW").GetValue<bool>() && W.IsReady() && Config.Item("UseW").GetValue<bool>())
            {
                damage += W.GetDamage(target);
            }
            return (int) damage;
        }

        private static void TristSpellRanges()
        {
            //Tristana Passive Calc - Credits: Lexxes
            {
                if (Environment.TickCount - SpellRangeTick < 100)
                    return;
                SpellRangeTick = Environment.TickCount;

                Q.Range = 600 + (4*(ObjectManager.Player.Level - 1));
                E.Range = 550 + (7*(ObjectManager.Player.Level - 1));
                R.Range = 550 + (7*(ObjectManager.Player.Level - 1));
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            //AbilityInterrupter
            if (R.IsReady() && unit.IsValidTarget(R.Range) && Config.Item("Interrupt").GetValue<bool>())
                R.CastOnUnit(unit);
        }

        private static void AntiGapCloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //AntiGapCloser
            if (R.IsReady() && gapcloser.Sender.IsValidTarget(R.Range) && Config.Item("AntiGap").GetValue<bool>())
                R.CastOnUnit(gapcloser.Sender);
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                //HARASS
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (E.IsReady() && Config.Item("hE").GetValue<bool>() && t.IsValidTarget(E.Range))
                    E.CastOnUnit(t);
                {
                    if (Q.IsReady() && Config.Item("hQ").GetValue<bool>() && t.IsValidTarget(Q.Range))
                        Q.CastOnUnit(ObjectManager.Player);
                }

            }
            //COMBO
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var nearest = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                var ort = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                var eort = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
                var pos = Drawing.WorldToScreen(ort.Position);
                var dmg1 = player.GetComboDamage(ort, new[] {SpellSlot.E, SpellSlot.R});
                var botrk = LeagueSharp.Common.Data.ItemData.Blade_of_the_Ruined_King.GetItem();
                var cutlass = LeagueSharp.Common.Data.ItemData.Bilgewater_Cutlass.GetItem();


                {
                    if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(ort)
                        && ort.HealthPercentage() <= Config.Item("eL").GetValue<Slider>().Value
                        && Config.Item("UseBOTRK").GetValue<bool>())

                        botrk.Cast(ort);
                }
                {

                    if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(ort)
                        && player.HealthPercentage() <= Config.Item("oL").GetValue<Slider>().Value
                        && Config.Item("UseBOTRK").GetValue<bool>())

                        botrk.Cast(ort);
                }
                {

                    //BILGEWATER SWORD THINGY
                    if (cutlass.IsReady() && cutlass.IsOwned(player) && cutlass.IsInRange(ort) &&
                        ort.HealthPercentage() <= Config.Item("HLe").GetValue<Slider>().Value
                        && Config.Item("UseBilge").GetValue<bool>())

                        cutlass.Cast(ort);
                }
                {



                    if (E.IsReady() && Config.Item("UseE").GetValue<bool>() &&
                        ort.IsValidTarget(E.Range))
                        E.CastOnUnit(ort);

                }
                {
                    if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() &&
                        ort.IsValidTarget(Q.Range))
                        Q.CastOnUnit(ObjectManager.Player);
                }

                {
                    if (R.IsReady() && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) &&
                        Config.Item("UseR").GetValue<bool>() &&
                        (R.GetDamage(ort) > ort.Health - 50))

                        R.CastOnUnit(ort);
                }
                {

                    if (R.IsReady() && Config.Item("UseR").GetValue<bool>() &&
                        (Orbwalker.ActiveMode <= Orbwalking.OrbwalkingMode.Combo &&
                         eort.HasBuff("detonatingshot", true) && (dmg1) > ort.Health))

                        R.CastOnUnit(ort);
                }
                {
                    //WLOGIC
                    if (ort.Position.UnderTurret(true) && Config.Item("wturret").GetValue<bool>())
                    {
                        return;
                    }


                    {
                        if (W.IsReady() && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) &&
                            ort.IsValidTarget(W.Range) &&
                            ort.Position.CountEnemiesInRange(700) <=
                            Config.Item("WL").GetValue<Slider>().Value &&
                            Config.Item("UseW").GetValue<bool>() &&
                            (CalcDamage(ort) + 150 > ort.Health)
                            &&
                            player.HealthPercentage() >= Config.Item("WzL").GetValue<Slider>().Value)

                            W.Cast(ort.Position);


                    }

                }
            }
        }
    








        private static
            void OnDraw
            (EventArgs
                args)
        {
            {

                }
                //Draw Skill Cooldown on Champ
                var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                if (R.IsReady() && Config.Item("Rrdy").GetValue<bool>())
                {
                    Drawing.DrawText(pos.X, pos.Y, Color.Gold, "R is Ready!");
                }

                if (Config.Item("Draw_Disabled").GetValue<bool>())
                    return;

                foreach (var tar in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(2000)))
                {
                }

                if (Config.Item("Wdraw").GetValue<bool>())
                    if (W.Level > 0)
                        Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Gold : Color.Red);

                if (Config.Item("Edraw").GetValue<bool>())
                    if (E.Level > 0)
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                            E.IsReady() ? Color.Blue : Color.Red);

                if (Config.Item("Rdraw").GetValue<bool>())
                    if (R.Level > 0)
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range - 2,
                            R.IsReady() ? Color.MediumPurple : Color.Red);
            }
        }
    }









