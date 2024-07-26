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

public class Fninja : Window
{
    public static new float Framerate = 1000;
    public static new bool bShowDebugConsole = false;
    public static new RenderHandler Renderer = new(typeof(Fninja));
    public static new Vector2 Size = new(80, 50);
    public static new Vector2 Center = new(0, 0);


    //									INIT
    public static int msSpawnInterval = 700;
    public static int BoxCount = 0;
    public static int ExplosionParticleCount = 60;
    public static int MinimumSpeed = 5;

    public static double SafezoneMult = 0.6;

    public static void Init()
    {
        _debug.WindowTick();

        Time.EachInterval(msSpawnInterval, SpawnBox);
    }

    public static Entity Trail = new()
    {
        Shape = new Shape2(
            [
            new VertexGroup(
                new Vector2(-1,0.5),
                new Vector2(1,0.5),
                new Vector2(0.5,-1)
                ),
            new VertexGroup(
                new Vector2(-1,0.5),
                new Vector2(1,0.5),
                new Vector2(0.5,1)
                )
            ]
            ),
        HasPhysics = false,
    };

    public static bool SpawnBox()
    {
        if (BoxCount > 9)
            return false;

        BoxCount++;
        Entity box = new();

        box.HasPhysics = true;
        box.HasGravity = false;
        box.Shape = new Shape2(Shapes2D.Square);
        box.Size = new Vector2(2);

        box.Position = new Vector2(
            new Random().Next((int)(ConsoleUtil.Width * SafezoneMult)) - ConsoleUtil.Width * SafezoneMult / 2
            ,-(ConsoleUtil.Height / 2)
            //new Random().Next((int)(ConsoleUtil.Height * SafezoneMult)) - ConsoleUtil.Height * SafezoneMult / 2
            //)
            ).FromPixels();

        box.ClickListenerEnabled = true;
        box.ClickedEvent += OnClick;

        box.LookDir = Vector2.FromDegrees(new Random().Next(361));

        box.Velocity = new Vector2(
            (new Random().NextDouble()-0.5) * 0.5
            ,
            Math.Clamp((new Random().NextDouble()+0.8), 0, 1.2)
            ) * 60;
        box.Acceleration = new Vector2(0,-70);

        box.RotationalVelocity = (new Random().NextDouble()-0.5) * 3;

        Boxes.Add(box);

        return false;
    }

    public static List<Entity> Boxes = new();

    public static void OnClick(object sender, ClickedEventArgs args)
    {
        KillBox(args.Entity);
    }

    public static ParticleEmitter Explosion = new()
    {
        Spread = 360,
        Velocity = new Vector2(0.7),
        Drag = 0.96,
        EmitRate = 0,
        VelocityRange = 0.5,
        MaxParticleLifetimeMilliseconds = 150,
        MaxParticleLifetimeRange = 200,
        MaxLifetimeMilliseconds = -1,
    };

    public static void Tick()
    {
        var size = new Vector2(ConsoleUtil.Width, -(ConsoleUtil.Height/2)).FromPixels();

        var boxes = new List<Entity>(Boxes);
        foreach (Entity box in boxes)
        {
            if (box.Position.Y >= size.Y - 2)
                continue;

            KillBox(box);
        }

        var mousePos = Controls.GetCursorPosition().FromPixels() + new Vector2(0.5, 0.5);
        Trail.Position = Util.Lerp(Trail.Position, mousePos, 0.15);
        Trail.LookDir = Trail.Position.LookAt(mousePos);

        double dist = Trail.GetDistance(mousePos);
        Trail.Size = new Vector2(1, dist);

        if (dist <= MinimumSpeed)
            return;

        var touching = Trail.GetTouchingEntities([Trail]);
        if (touching == null || touching.Length == 0)
            return;

        foreach (Entity entity in touching)
        {
            if (entity == null)
                continue;

            KillBox(entity);
        }
    }

    public static void KillBox(Entity box)
    {
        Explosion.Position = box.Position;
        Explosion.Emit(ExplosionParticleCount);

        box.Dispose();
        Boxes.Remove(box);
        BoxCount--;
    }

    public static void PostRenderTick()
    {
    }

    public static void Main()
    {
        Runtime.Initialize(typeof(Fninja));
    }
}

