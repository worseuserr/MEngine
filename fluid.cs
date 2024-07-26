using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Numerics;
using System.Drawing;

namespace MEngine;

/* TODO
 * ------------------------------------------ GAME
 * Create bosses
 * Create healthbar
 * Create shoot modes
 * Create drones
 * 
 * -- later
 * fix entity.copy()
 */
public class Fluid : Window
{
    public static new float Framerate = 1000;
    public static new bool bShowDebugConsole = true;
    public static new RenderHandler Renderer = new(typeof(Fluid));
    public static new Vector2 Size = new(80, 40);
    public static new Vector2 Center = new(0, 0);


    //									INIT
    public static void Init()
    {
        _debug.WindowTick();
        Particle e = new();
        e.Position = new Vector2(0, 0);
        //e.Shape = new Shape2(Shapes2D.Triangle);

        Time.EachInterval(10, OnMouseHold);
    }

    public static bool OnMouseHold()
    {
        if (Controls.IsKeyDown("C"))
        {
            var particles = new List<Particle>(ParticleSystem.ActiveParticles);
            foreach (Particle p in particles)
            {
                p.Dispose();
            }
        }

        if (!Controls.IsKeyDown("Spacebar") || !IsPosInsideWindow(Controls.GetCursorPosition(true).FromPixels()))
            return false;

        SpawnParticle(Controls.GetCursorPosition(true).FromPixels());

        return false;
    }

    public static string[] symbols = {"#"};
    public static void SpawnParticle(Vector2 pos)
    {
        Particle p = new();
        p.Position = Controls.GetCursorPosition(true).FromPixels() + new Vector2(0.5,-1);
        p.Velocity = new((new Random().NextDouble()-0.5) * 0.1,0);
        p.Drag = 0.998;
        p.MaxLifetimeMilliseconds = -1;
        p.Acceleration = new(0, -1);
        p.Symbol = symbols[new Random().Next(0,symbols.Length)];
    }


    // desmos y\ =\left(-\left(\left(\frac{x}{m}+m\right)-m\right)^{1+c}\right)\cdot m+m

    public static double curve = 2.2;
    public static double GetFluidVectorProduct(double maxDistance, double distance)
    {
        double a = distance / maxDistance + maxDistance;
        double b = Math.Pow(a - maxDistance, 1 + curve);

        return 
            ( -b * maxDistance + maxDistance );

        //return maxDistance - distance;
    }

    public static double collisionModifier = -1; // X line 97
    public static double maxDistance = 3;
    public static double grav = -2;
    public static void Tick()
    {
        //curve = Controls.IsRightMouseButtonDown() ? -0.8 : 0.8;
        _debug.Variable = IsPosInsideWindow(Controls.GetCursorPosition(true).FromPixels());
        collisionModifier = Runtime.dTime * 1;

        var consolePos = ConsoleUtil.GetConsolePosition(true).FromPixels();

        /*curve = -0.6;
        if (Controls.IsKeyDown("M"))
        {
            curve = 0.6;
        }*/

        foreach (Particle p in ParticleSystem.ActiveParticles)
        {
            var size = new Vector2(ConsoleUtil.Width, ConsoleUtil.Height).FromPixels();
            var pos = p.Position;
            if (!(
                pos.X < consolePos.X + size.X / 2
                        &&
                pos.X > consolePos.X - size.X / 2
            ))
            {
                p.Velocity = new Vector2(-p.Velocity.X * 0.7, p.Velocity.Y);
                p.Position = new Vector2(Math.Clamp(p.Position.X,
                    consolePos.X - size.X / 2+2,
                    consolePos.X + size.X / 2-2
                    ), p.Position.Y
                );
            }
            else if (!(
                pos.Y < consolePos.Y + size.Y / 2
                        &&
                pos.Y > consolePos.Y - size.Y / 2
                + 5 // bottom line clip fix
                ))
            {
                p.Velocity = new Vector2(p.Velocity.X, -p.Velocity.Y * 0.7);
                p.Position = new Vector2(p.Position.X, Math.Clamp(p.Position.Y, 
                    consolePos.Y - size.Y / 2 + 5, 
                    consolePos.Y + size.Y / 2
                    )
                );
            }

            foreach (Particle p2 in ParticleSystem.ActiveParticles)
            {
                if (p2 == p) continue;

                var dist = p.GetDistance(p2.Position);

                if (p.LifetimeMilliseconds < 500 || !(dist < maxDistance))
                    continue;

                var inverseDir = 
                    new Vector2(p2.Position.X, p2.Position.Y)
                        .LookAt(p.Position)
                        * (
                        /*maxDistance +
                        Math.Pow(dist+0.694, -maxDistance)
                        -maxDistance*/
                        GetFluidVectorProduct(maxDistance, dist)
                        );

                p.Velocity += inverseDir * collisionModifier;
            }

            var distX = Math.Min(p.GetDistance(
                    new Vector2(consolePos.X + size.X / 2, p.Position.Y)
                ) 
                ,
                p.GetDistance(
                    new Vector2(consolePos.X - size.X / 2, p.Position.Y)
                ));

            var distY = Math.Min(p.GetDistance(
                new Vector2(p.Position.X, consolePos.Y - size.Y / 2 + 5)
                )
                ,
                p.GetDistance(
                    new Vector2(p.Position.X, consolePos.Y + size.Y / 2)
                ));

            if (p.LifetimeMilliseconds < 500)
                continue;

            if (distX < maxDistance)
            {
                var inverseDirWall =
                    p.Position
                        .LookAt(new Vector2(consolePos.X, p.Position.Y))
                        * (
                        GetFluidVectorProduct(maxDistance, distX)
                        );

                p.Velocity += inverseDirWall * collisionModifier;
            }

            if (distY < maxDistance)
            {
                var inverseDirWall =
                    p.Position
                        .LookAt(new Vector2(p.Position.X, consolePos.Y))
                        * (
                        GetFluidVectorProduct(maxDistance, distY)
                        );

                p.Velocity += inverseDirWall * collisionModifier;
            }

            p.Acceleration = new Vector2(0, grav);
            if (Controls.IsKeyDown("Q"))
            {
                p.Velocity = p.Position.LookAt(Controls.GetCursorPosition(true).FromPixels()) * 2 
                    + new Vector2(0,-0.1);
            }
            if (Controls.IsKeyDown("E"))
            {
                p.Acceleration = p.Position.LookAt(Controls.GetCursorPosition(true).FromPixels()) * 0.7
                    + new Vector2(0, grav);
            }
        }
        var fps = Convert.ToInt32(Runtime.averageFPS);
        var count = ParticleSystem.ActiveParticles.Count;

        fpsCount.Position = new Vector2(ConsoleUtil.X, ConsoleUtil.Y).FromPixels() + new Vector2(-20.625, 18.125);
        particleCount.Position = new Vector2(ConsoleUtil.X, ConsoleUtil.Y).FromPixels() + new Vector2(15, 18.125);
        fpsCount.Text = $"FPS: {fps}";
        particleCount.Text = $"COUNT: {count}";
        //Controls.GetCursorPosition().FromPixels();
        //if (Controls.IsKeyDown("M"))
        //{
        //    throw new Exception((fpsAndParticleCount.Position - new Vector2(ConsoleUtil.X, ConsoleUtil.Y).FromPixels()).X.ToString() + ", " + (fpsAndParticleCount.Position - new Vector2(ConsoleUtil.X, ConsoleUtil.Y).FromPixels()).Y.ToString());
        //}

    }

    public static bool IsPosInsideWindow(Vector2 pos)
    {
        var size = new Vector2(ConsoleUtil.Width, ConsoleUtil.Height).FromPixels();
        if (
            pos.X < ConsoleUtil.GetConsolePosition(true).FromPixels().X + size.X / 2
            &&
            pos.X > ConsoleUtil.GetConsolePosition(true).FromPixels().X - size.X / 2
            &&
            pos.Y < ConsoleUtil.GetConsolePosition(true).FromPixels().Y + size.Y / 2
            &&
            pos.Y > ConsoleUtil.GetConsolePosition(true).FromPixels().Y - size.Y / 2

            +5 // bottom line clip fix
            )
            return true;
        return false;
    }

    public static void PostRenderTick()
    {
    }

    public static UIText fpsCount = new()
    {
        Centered = false,
        Text = "loading",
    };
    public static UIText particleCount = new()
    {
        Centered = false,
        Text = "loading",
    };

    public static void Main()
    {
        Runtime.Initialize(typeof(Fluid));
    }
}

