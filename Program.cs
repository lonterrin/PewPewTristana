using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;


using Color = System.Drawing.Color;

namespace PewPewTristana
{
    internal class Program
    {
        public const string ChampName = "Tristana";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static int SpellRangeTick;
        private static SpellSlot Ignite;
        private static int LastCast;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            //Welcome Message upon loading assembly.
            Game.PrintChat(
                "<font color=\"#00BFFF\">PewPewTristana -<font color=\"#FFFFFF\"> #TEST Version Successfully Loaded.</font>");
            CustomEvents.Game.OnGameLoad += OnLoad;

        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q);

            //RocketJump Settings
            W = new Spell(SpellSlot.W, 900);
            W.SetSkillshot(0.25f, 150, 1200, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 630);
            R = new Spell(SpellSlot.R, 630);


            Config = new Menu("PewPewTristana", "Tristana", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("[PPT]: Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("[PPT]: Target Selector", "Target Selector")));

            //COMBOMENU

            var combo = Config.AddSubMenu(new Menu("[PPT]: Combo Settings", "Combo Settings"));
            var harass = Config.AddSubMenu(new Menu("[PPT]: Harass Settings", "Harass Settings"));
            var drawing = Config.AddSubMenu(new Menu("[PPT]: Draw Settings", "Draw"));

            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("wmana", "[W] Mana %").SetValue(new Slider(10, 100, 0)));
            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("emana", "[E] Mana %").SetValue(new Slider(10, 100, 0)));
            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("rmana", "[R] Mana %").SetValue(new Slider(15, 100, 0)));

            combo.SubMenu("[Q] Settings").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            combo.SubMenu("[W] Settings").AddItem(new MenuItem("UseW", "Use Rocket Jump").SetValue(true));
            combo.SubMenu("[W] Settings")
                .AddItem(new MenuItem("wnear", "Enemy Count").SetValue(new Slider(2, 5, 1)));
            combo.SubMenu("[W] Settings").AddItem(new MenuItem("whp", "Own HP %").SetValue(new Slider(75, 100, 0)));
            combo.SubMenu("[W] Settings")
                .AddItem(new MenuItem("wturret", "Don't jump into turret range").SetValue(true));

            combo.SubMenu("[E] Settings").AddItem(new MenuItem("UseE", "Use Explosive Charge").SetValue(true));
            combo.SubMenu("[E] Settings")
                .AddItem(new MenuItem("UseEW", "Use W on E stack count").SetValue(false));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("estack", "E stack count").SetValue(new Slider(3, 4, 1)));
            combo.SubMenu("[E] Settings")
            .AddItem(new MenuItem("enear", "Enemy Count").SetValue(new Slider(2, 5, 1)));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("ehp", "Enemy HP %").SetValue(new Slider(45, 100, 0)));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("ohp", "Own HP %").SetValue(new Slider(65, 100, 0)));

            combo.SubMenu("[R] Settings")
            .AddItem(
        new MenuItem("UseR", "Use R [FINISHER] (TOGGLE) ").SetValue(new KeyBind('K', KeyBindType.Toggle)));
            combo.SubMenu("[R] Settings")
                .AddItem(new MenuItem("UseRE", "Use ER [FINISHER]").SetValue(true));
            combo.SubMenu("[R] Settings")
                .AddItem(new MenuItem("manualr", "Cast R on your target").SetValue(new KeyBind('R', KeyBindType.Press)));



            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("useGhostblade", "Use Youmuu's Ghostblade").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("UseBOTRK", "Use Blade of the Ruined King").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("eL", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("oL", "  Own HP Percentage").SetValue(new Slider(65, 100, 0)));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseBilge", "Use Bilgewater Cutlass").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("HLe", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Summoner Settings").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

            //LANECLEARMENU
            Config.SubMenu("[PPT]: Laneclear Settings")
                .AddItem(new MenuItem("laneQ", "Use Q").SetValue(true));
            Config.SubMenu("[PPT]: Laneclear Settings")
                .AddItem(new MenuItem("laneE", "Use E").SetValue(true));
            Config.SubMenu("[PPT]: Laneclear Settings")
                .AddItem(new MenuItem("laneclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            //JUNGLEFARMMENU
            Config.SubMenu("[PPT]: Jungle Settings")
                .AddItem(new MenuItem("jungleQ", "Use Q").SetValue(true));
            Config.SubMenu("[PPT]: Jungle Settings")
                .AddItem(new MenuItem("jungleE", "Use E").SetValue(true));
            Config.SubMenu("[PPT]: Jungle Settings")
                .AddItem(new MenuItem("jungleclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(true));
            drawing.AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(true));
            drawing.AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(true));
            drawing.AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(true));

            harass.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassE", "Use E").SetValue(true));
            harass.AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            Config.SubMenu("[PPT]: Misc Settings").AddItem(new MenuItem("DrawD", "Damage Indicator").SetValue(true));
            Config.SubMenu("[PPT]: Misc Settings").AddItem(new MenuItem("interrupt", "Interrupt Spells").SetValue(true));
            Config.SubMenu("[PPT]: Misc Settings").AddItem(new MenuItem("antigap", "AntiGapCloser").SetValue(true));

            Config.AddToMainMenu();

            Drawing.OnDraw += OnDraw;
            TristSpellRanges();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnEndScene += OnEndScene;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapCloser_OnEnemyGapcloser;


        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (R.IsReady() && unit.IsValidTarget(E.Range) && Config.Item("interrupt").GetValue<bool>())
                R.CastOnUnit(unit);
        }

        private static void AntiGapCloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (R.IsReady() && gapcloser.Sender.IsValidTarget(E.Range) && Config.Item("antigap").GetValue<bool>())
                R.CastOnUnit(gapcloser.Sender);
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.SubMenu("[PPT]: Misc Settings").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.Green);
                }
            }
        }

        private static void combo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

            if (target.IsDead)
                return;

            if (Q.IsReady())
                qlogic();
            if (E.IsReady())
                elogic();
            if (W.IsReady())
                wlogic();
            if (R.IsReady())
                rlogic();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                items();
        }
        private static int CalcDamage(Obj_AI_Base target)
        {
            //Calculate Combo Damage
            var aa = player.GetAutoAttackDamage(target, true) * (1 + player.Crit);
            var damage = aa;
            Ignite = player.GetSpellSlot("summonerdot");

            if (Ignite.IsReady())
                damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //ITEM BOTRK

            if (Config.Item("UseE").GetValue<bool>()) // edamage
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target);
                }
            }

            if (R.IsReady() && Config.Item("UseR").GetValue<KeyBind>().Active) // rdamage
            {

                damage += R.GetDamage(target);
            }

            if (W.IsReady() && Config.Item("UseW").GetValue<bool>())
            {
                damage += W.GetDamage(target);
            }
            return (int)damage;


        }
        private static void wlogic()
        {
            var wmana = Config.Item("wmana").GetValue<Slider>().Value;

            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

            if (Config.Item("wturret").GetValue<bool>() && target.Position.UnderTurret(true))
                return;
            if (target.HasBuff("deathdefiedbuff"))
                        return;
            if (target.HasBuff("KogMawIcathianSurprise", true))
                        return;

            if (target.Buffs.Find(buff => buff.Name == "tristanaecharge").Count >= Config.Item("estack").GetValue<Slider>().Value
                && Config.Item("UseEW").GetValue<bool>()
                && target.Position.CountEnemiesInRange(700) <= Config.Item("enear").GetValue<Slider>().Value
                && target.HealthPercentage() <= Config.Item("ehp").GetValue<Slider>().Value
                && player.HealthPercentage() >= Config.Item("ohp").GetValue<Slider>().Value
                && player.ManaPercentage() >= wmana)

                W.Cast(target);

            if (W.IsReady() && target.IsValidTarget(W.Range)
                && target.Position.CountEnemiesInRange(700) <= Config.Item("wnear").GetValue<Slider>().Value
                && player.HealthPercentage() >= Config.Item("whp").GetValue<Slider>().Value
                && CalcDamage(target) >= target.Health - 15 * player.Level
                && player.ManaPercentage() >= wmana)

                W.Cast(target);
        }
        private static void qlogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range))
                Q.Cast(player);
        }

        public static int Estackcount(Obj_AI_Base target)
        {
            var buff = target.Buffs.Find(Buffer => Buffer.Name == "tristanaecharge");
            return buff != null ? buff.Count : 0;
        }

        private static void elogic()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            var wmana = Config.Item("wmana").GetValue<Slider>().Value;
            var emana = Config.Item("emana").GetValue<Slider>().Value;

            if (Config.Item("wturret").GetValue<bool>() && target.Position.UnderTurret(true))
                return;

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>()
            && player.ManaPercentage() >= emana)

                E.CastOnUnit(target);

        }  
          
        private static void rlogic()
        {
            var rmana = Config.Item("rmana").GetValue<Slider>().Value;

            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            int ecount = 0;

            var erdamage = E.GetDamage(target) + R.GetDamage(target);

            if (Config.Item("manualr").GetValue<KeyBind>().Active && R.IsReady())
                R.CastOnUnit(target);

            if (Config.Item("UseRE").GetValue<bool>()
                && R.IsReady()
                && Config.Item("UseR").GetValue<KeyBind>().Active
                && target.HasBuff("tristanaecharge") && erdamage > target.Health &&
                player.ManaPercentage() >= rmana)

                R.CastOnUnit(target);

            else if (Config.Item("UseR").GetValue<KeyBind>().Active && R.IsReady() &&
                R.GetDamage(target) >= target.Health - 45 &&
                player.ManaPercentage() >= rmana)
                R.CastOnUnit(target);
        }
        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void items()
        {
            Ignite = player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
            && target.HealthPercentage() <= Config.Item("eL").GetValue<Slider>().Value
            && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && player.HealthPercentage() <= Config.Item("oL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(player) && cutlass.IsInRange(target) &&
                target.HealthPercentage() <= Config.Item("HLe").GetValue<Slider>().Value
                && Config.Item("UseBilge").GetValue<bool>())

                cutlass.Cast(target);

            if (Ghost.IsReady() && Ghost.IsOwned(player) && target.IsValidTarget(E.Range)
                && Config.Item("useGhostblade").GetValue<bool>())

                Ghost.Cast();

            if (player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                combo();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                Laneclear();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                Jungleclear();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                harass();
        }

        private static void harass()
        {
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (E.IsReady()
                && Config.Item("hE").GetValue<bool>()
                && target.IsValidTarget(E.Range)
                && player.ManaPercentage() >= harassmana)

                E.CastOnUnit(target);

            if (Q.IsReady()
                && Config.Item("hQ").GetValue<bool>()
                && target.IsValidTarget(Q.Range)
                && player.ManaPercentage() >= harassmana)

                Q.Cast(player);
        }

        private static void Laneclear()
        {
            var lanemana = Config.Item("laneclearmana").GetValue<Slider>().Value;
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + W.Width - 50);

            var Qfarmpos = W.GetLineFarmLocation(allMinionsQ, W.Width);
            var Efarmpos = W.GetCircularFarmLocation(allMinionsE, W.Width);


            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Qfarmpos.MinionsHit >= 3 && allMinionsE.Count >= 2
                && Config.Item("laneQ").GetValue<bool>()
                && player.ManaPercentage() >= lanemana)

                Q.Cast(player);

            foreach (var minion in allMinionsE)
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                    && minion.IsValidTarget(E.Range) && Efarmpos.MinionsHit >= 2
                    && allMinionsE.Count >= 2 && Config.Item("laneE").GetValue<bool>()
                    && player.ManaPercentage() >= lanemana)

                    E.CastOnUnit(minion);

        }
        private static void Jungleclear()
        {
            var jlanemana = Config.Item("jungleclearmana").GetValue<Slider>().Value;
            var MinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var MinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + W.Width - 50, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            var Qfarmpos = W.GetLineFarmLocation(MinionsQ, W.Width + 100);
            var Efarmpos = W.GetCircularFarmLocation(MinionsE, W.Width - +100);


            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Qfarmpos.MinionsHit >= 1
                && MinionsE.Count >= 1 && Config.Item("jungleQ").GetValue<bool>()
                && player.ManaPercentage() >= jlanemana)

                Q.Cast(player);

            foreach (var minion in MinionsE)
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && minion.IsValidTarget(E.Range)
                    && Efarmpos.MinionsHit >= 1
                    && MinionsE.Count >= 1
                    && Config.Item("jungleE").GetValue<bool>()
                    && player.ManaPercentage() >= jlanemana)

                    E.CastOnUnit(minion);

        }
        private static void TristSpellRanges()
        {
            //Tristana Passive Calc - Credits: Lexxes
            {
                if (Environment.TickCount - SpellRangeTick < 100)
                    return;
                SpellRangeTick = Environment.TickCount;

                Q.Range = 600 + (7 * (ObjectManager.Player.Level - 1));
                E.Range = 550 + (7 * (ObjectManager.Player.Level - 1));
                R.Range = 550 + (7 * (ObjectManager.Player.Level - 1));
            }
        }

        private static void OnDraw(EventArgs args)
        {
            {

            }

            //Draw Skill Cooldown on Champ
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (Config.Item("UseR").GetValue<KeyBind>().Active)
                Drawing.DrawText(pos.X - 50, pos.Y + 50, Color.Gold, "[R] Finisher is Enabled!");


            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Orange : Color.Red);


            if (Config.Item("Wdraw").GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Gold : Color.Red);

            if (Config.Item("Edraw").GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Color.AntiqueWhite : Color.Red);

            if (Config.Item("Rdraw").GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range - 2,
                        R.IsReady() ? Color.Gray : Color.Red);

            var orbtarget = Orbwalker.GetTarget();
            Render.Circle.DrawCircle(orbtarget.Position, 100, Color.DarkOrange, 10);
        }
    }
}
