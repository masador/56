﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeLeeSinReborn
{
    class InsecHandler
    {
        private static Obj_AI_Base _selectedUnit;
        private static Obj_AI_Base _selectedEnemy;
        public static bool FlashR;
        public static Vector3 FlashPos;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static void OnClick(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
                return;
            var unit2 = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(a => (a.IsValid<Obj_AI_Hero>()) && a.IsEnemy && a.Distance(Game.CursorPos) < a.BoundingRadius + 80 && a.IsValidTarget());
            if (unit2 != null)
            {
                _selectedEnemy = unit2;
                return;
            }
            var unit = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(a => (a.IsValid<Obj_AI_Hero>() || a.IsValid<Obj_AI_Minion>() || a.IsValid<Obj_AI_Turret>()) && a.IsAlly && a.Distance(Game.CursorPos) < a.BoundingRadius + 80 && a.IsValid && !a.IsDead && !a.Name.ToLower().Contains("ward") && !a.IsMe);
            _selectedUnit = unit;
            if (_selectedUnit == null)
                _selectedEnemy = null;
        }

        public static void Draw()
        {
            var lowFps = Program.Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Program.Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            if (_selectedUnit != null)
            {
                if (lowFpsMode != 1 && lowFps) Render.Circle.DrawCircle(_selectedUnit.Position, _selectedUnit.BoundingRadius + 50, Color.White, lowFps ? lowFpsMode : 5);
                if (lowFpsMode == 1 && lowFps) Drawing.DrawText(Drawing.WorldToScreen(_selectedUnit.Position).X - 40, Drawing.WorldToScreen(_selectedUnit.Position).Y + 10, Color.White, "Selected Ally");
            }
            if (_selectedEnemy.IsValidTarget() && _selectedEnemy.IsVisible && !_selectedEnemy.IsDead)
            {
                Drawing.DrawText(Drawing.WorldToScreen(_selectedEnemy.Position).X - 40, Drawing.WorldToScreen(_selectedEnemy.Position).Y + 10, Color.White, "Insec Target");
                if (lowFpsMode != 1 || !lowFps) Render.Circle.DrawCircle(_selectedEnemy.Position, _selectedEnemy.BoundingRadius + 50, Color.SteelBlue, lowFps ? lowFpsMode : 5);
                if (InsecPos().IsValid() && !(lowFpsMode == 1 && lowFps))
                {
                    Render.Circle.DrawCircle(InsecPos(), 110, Color.SteelBlue, lowFps ? lowFpsMode : 5);
                }
            }
        }

        public static Vector3 InsecPos()
        {
            if (_selectedUnit != null && _selectedEnemy.IsValidTarget() && Program.Config.Item("clickInsec").GetValue<bool>())
            {
                return _selectedUnit.Position.Extend(_selectedEnemy.Position, _selectedUnit.Distance(_selectedEnemy) + 250);
            }
            if (_selectedEnemy.IsValidTarget() && Program.Config.Item("easyInsec").GetValue<bool>())
            {
                foreach (var tower in ObjectManager.Get<Obj_AI_Turret>().Where(tower => tower.IsAlly && tower.Health > 0 && tower.Distance(_selectedEnemy) < 2000))
                {
                    return tower.Position.Extend(_selectedEnemy.Position, tower.Distance(_selectedEnemy) + 250);
                }
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsMe && ally.HealthPercent > 10 && ally.Distance(_selectedEnemy) < 2000))
                {
                    return ally.Position.Extend(_selectedEnemy.Position, ally.Distance(_selectedEnemy) + 250);
                }
            }
            if (_selectedUnit == null && _selectedEnemy.IsValidTarget() && Program.Config.Item("mouseInsec").GetValue<bool>())
            {
                return Game.CursorPos.Extend(_selectedEnemy.Position, Game.CursorPos.Distance(_selectedEnemy.Position) + 250);
            }
            return new Vector3();
        }

        public static void DoInsec()
        {
            if (_selectedEnemy == null && Program.Config.Item("insecOrbwalk").GetValue<bool>())
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                return;
            }
            if (_selectedEnemy != null)
            {
                Orbwalking.Orbwalk(
                    Orbwalking.InAutoAttackRange(_selectedEnemy) ? _selectedEnemy : null,
                    _selectedUnit == null ? _selectedEnemy.Position : Game.CursorPos);
            }
            if (!InsecPos().IsValid() || !_selectedEnemy.IsValidTarget() || !CheckHandler.spells[SpellSlot.R].IsReady())
                    return;
                if (Player.Distance(InsecPos()) <= 120)
                {
                    CheckHandler.spells[SpellSlot.R].CastOnUnit(_selectedEnemy);
                    return;
                }
                if (Player.Distance(InsecPos()) < 600)
                {
                    if (CheckHandler.WState && CheckHandler.spells[SpellSlot.W].IsReady() && CheckHandler.CheckQ)
                    {
                        Obj_AI_Base unit = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(a => a.IsAlly && a.Distance(InsecPos()) < 120);
                        if (unit != null)
                        {
                            CheckHandler.spells[SpellSlot.W].CastOnUnit(unit);
                        }
                        else if (CheckHandler.LastWard + 500 < Environment.TickCount && Items.GetWardSlot() != null && Player.Distance(InsecPos()) < 600)
                        {
                            Player.Spellbook.CastSpell(Items.GetWardSlot().SpellSlot, InsecPos());
                        }
                        return;
                    }
                    if (!Program.Config.Item("flashInsec").GetValue<bool>() || CheckHandler.WState && CheckHandler.spells[SpellSlot.W].IsReady() && Items.GetWardSlot() != null || CheckHandler.LastW + 2000 > Environment.TickCount) return;
                    if (_selectedEnemy.Distance(Player) < CheckHandler.spells[SpellSlot.R].Range)
                    {
                        CheckHandler.spells[SpellSlot.R].CastOnUnit(_selectedEnemy);
                        FlashPos = InsecPos();
                        FlashR = true;
                    }
                    else
                    {
                        if (InsecPos().Distance(Player.Position) < 400)
                        {
                            Player.Spellbook.CastSpell(Player.GetSpellSlot("summonerflash"), InsecPos());
                        }
                    }
                }
                if (Player.Distance(_selectedEnemy) < CheckHandler.spells[SpellSlot.Q].Range && CheckHandler.QState && CheckHandler.spells[SpellSlot.Q].IsReady())
                {
                    CheckHandler.spells[SpellSlot.Q].Cast(_selectedEnemy);
                }
                if (!CheckHandler.QState && _selectedEnemy.HasQBuff() || (Program.Config.Item("q2InsecRange").GetValue<bool>() && CheckHandler.BuffedEnemy.IsValidTarget() && CheckHandler.BuffedEnemy.Distance(InsecPos()) < 500))
                {
                    CheckHandler.spells[SpellSlot.Q].Cast();
                }
            
            if (Program.Config.Item("q1InsecRange").GetValue<bool>() || !CheckHandler.QState || !CheckHandler.spells[SpellSlot.Q].IsReady() || !InsecPos().IsValid())
            {
                return;
            }
            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            a =>
                                a.IsEnemy && (a.IsValid<Obj_AI_Hero>() || a.IsValid<Obj_AI_Minion>()) &&
                                a.Distance(InsecPos()) < 400))
            {
                if (!unit.IsValidTarget())
                    return;
                CheckHandler.spells[SpellSlot.Q].Cast(unit);
            }
        }
    }
}
