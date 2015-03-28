﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeLeeSinReborn
{


    internal class JumpHandler
    {

        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static readonly List<Vector3> JunglePos = new List<Vector3>()
        {
            new Vector3(6271.479f, 12181.25f, 56.47668f),
            new Vector3(6971.269f, 10839.12f, 55.2f),
            new Vector3(8006.336f, 9517.511f, 52.31763f),
            new Vector3(10995.34f, 8408.401f, 61.61731f),
            new Vector3(10895.08f, 7045.215f, 51.72278f),
            new Vector3(12665.45f, 6466.962f, 51.70544f),
            new Vector3(4966.042f, 10475.51f, 71.24048f), // baron
            new Vector3(39000.529f, 7901.832f, 51.84973f),
            new Vector3(2106.111f, 8388.643f, 51.77686f),
            new Vector3(3753.737f, 6454.71f, 52.46301f),
            new Vector3(6776.247f, 5542.872f, 55.27625f),
            new Vector3(7811.688f, 4152.602f, 53.79456f),
            new Vector3(8528.921f, 2822.875f, 50.92188f),
            new Vector3(9850.102f, 4432.272f, 71.24072f), // Dragon
        };
        private static Geometry.Polygon _rect;
        private static bool _active;
        public static bool InitQ;

        public static void Load()
        {
            Drawing.OnDraw += args => Draw();
            Game.OnUpdate += args => Tick();
        }

        public static bool IsJumpable()
        {
            if (_rect == null)
                return false;
            var a = (from c in JunglePos where !_rect.IsOutside(c.To2D()) || c.Distance(Game.CursorPos) < 200 select c).FirstOrDefault();
            return (a.IsValid()) && CheckHandler.spells[SpellSlot.Q].IsReady();
        }

        private static void Tick()
        {
            if (Program.Config.Item("Wardjump").GetValue<KeyBind>().Active &&
                Program.Config.Item("escapeMode").GetValue<bool>())
            {
                Escape();
                _active = true;
            }
            else
            {
                _active = false;
            }
        }

        private static void Draw()
        {
            if (!Program.Config.Item("escapeMode").GetValue<bool>() || !Program.Config.Item("DES").GetValue<bool>())
                return;
            var lowFps = Program.Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Program.Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            if (_active && CheckHandler.spells[SpellSlot.Q].IsReady() && !lowFps)
            {
                _rect.Draw(Color.White);
            }
            foreach (var pos in JunglePos)
            {
                if (_rect != null)
                {
                    if (pos.Distance(Player.Position) < 2000)
                        Render.Circle.DrawCircle(
                            pos, 100, (_rect.IsOutside(pos.To2D()) ? Color.White : Color.DeepSkyBlue), lowFps ? lowFpsMode : 5);
                }
                else
                {
                    if (pos.Distance(Player.Position) < 2000)
                        Render.Circle.DrawCircle(
                            pos, 100, Color.White, lowFps ? lowFpsMode : 5);
                }
            }
        }

        private static void Escape()
        {
            if (CheckHandler.BuffedEnemy.IsValidTarget() && CheckHandler.BuffedEnemy.IsValid<Obj_AI_Hero>())
            {
                InitQ = false;
                return;
            }
            if (InitQ)
            {
                foreach (var point in JunglePos)
                {
                    if (Player.Distance(point) < 100 || CheckHandler.LastQ2 + 2000 < Environment.TickCount)
                        InitQ = false;
                }
            }
            _rect = new Geometry.Rectangle(Player.Position.To2D(), Player.Position.To2D().Extend(Game.CursorPos.To2D(), 1050), 100).ToPolygon();
            if (CheckHandler.QState && CheckHandler.spells[SpellSlot.Q].IsReady())
            {
                foreach (var pos in JunglePos)
                {
                    if (_rect.IsOutside(pos.To2D()))
                        continue;
                    InitQ = true;
                    CheckHandler.spells[SpellSlot.Q].Cast(pos);
                    return;
                }
            }
            else if (CheckHandler.spells[SpellSlot.Q].IsReady() && !CheckHandler.QState)
            {
                CheckHandler.spells[SpellSlot.Q].Cast();
                InitQ = true;
            }
        }
    }
}