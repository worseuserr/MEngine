using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEngine;

public static class ParticleSystem
{
    public static List<Particle> ActiveParticles = new();
    public static List<ParticleEmitter> ActiveEmitters = new();

    public static int MaxParticles = 10000;

    public static void Tick()
    {
        var ActiveEmittersC = new List<ParticleEmitter>(ActiveEmitters);
        var ActiveParticlesC = new List<Particle>(ActiveParticles);

        foreach (ParticleEmitter e in ActiveEmittersC)
        {
            e.Tick();
        }
        foreach (Particle p in ActiveParticlesC) 
        { 
            p.Tick();
        }
    }
}

public class Particle
{
    /// <summary>
    /// Linearly interpolates the velocity of the particle from the first to last frame. <b>Acceleration AND Velocity must be NULL for this to work. Only one Vector2 field, other than Size, may be tweened at once.</b>
    /// </summary>
    public Vector2[] TPosition = null;
    public Vector2 Position = new();

    /// <summary>
    /// Linearly interpolates the Acceleration of the particle from the first to last frame. <b>Only one Vector2 field, other than Size, may be tweened at once.</b>
    /// </summary>
    public Vector2[] TAcceleration = null;
    public Vector2 Acceleration = new();

    /// <summary>
    /// Linearly interpolates the velocity of the particle from the first to last frame. <b>Acceleration must be NULL for this to work. Only one Vector2 field, other than Size, may be tweened at once.</b>
    /// </summary>
    public Vector2[] TVelocity = null;
    public Vector2 Velocity = new();

    /// <summary>
    /// Linearly interpolates the size of the particle from the first to the last frame.
    /// </summary>
    public double[] TSize = null;
    public double Size = new();
    public double Drag = 1;

    public string Symbol = "#";

    /// <summary>
    /// Lifetime of particles in milliseconds. Set to <b>-1</b> to make it infinite.
    /// </summary>
    public int MaxLifetimeMilliseconds = 1000;
    public int LifetimeMilliseconds = 0;
    public ParticleEmitter Owner = null;

    public Particle()
    {
        if (ParticleSystem.ActiveParticles.Count > ParticleSystem.MaxParticles)
        {
            Dispose();
            return;
        }
        ParticleSystem.ActiveParticles.Add(this);
    }

    public void Tick()
    {
        LifetimeMilliseconds += (int)(Runtime.dTime * 1000);
        if (LifetimeMilliseconds > MaxLifetimeMilliseconds && MaxLifetimeMilliseconds != -1 || MaxLifetimeMilliseconds == 0)
        {
            Dispose();
            return;
        }

        double LifetimeMultiplier = LifetimeMilliseconds / MaxLifetimeMilliseconds;

        if (TSize != null)
        {
            Size = Util.Lerp(TSize[0], TSize[1], LifetimeMultiplier);
        }

        if (TPosition != null)
        {
            Position = Util.Lerp(TPosition[0], TPosition[1], LifetimeMultiplier);
            return;
        } else if (TAcceleration != null)
        {
            Acceleration = Util.Lerp(TAcceleration[0], TAcceleration[1], LifetimeMultiplier);
        } else if (TVelocity != null)
        {
            Velocity = Util.Lerp(TVelocity[0], TVelocity[1], LifetimeMultiplier);
            Position += Velocity;
            return;
        }

        Position += Velocity;
        Velocity *= Drag;
        Velocity += Acceleration * Runtime.dTime;
            //(Acceleration * Runtime.dTime); fix this shit nigga
    }

    public void Dispose()
    {
        ParticleSystem.ActiveParticles.Remove(this);

        if (Owner == null)
            return;

        lock (Owner)
        {
            if (Owner.Particles == null)
               return;
            Owner.Particles.Remove(this);
        }
    }
    public double GetDistance(Vector2 otherVector)
    {
        double deltaX = Position.X - otherVector.X;
        double deltaY = Position.Y - otherVector.Y;

        double distanceSquared = deltaX * deltaX + deltaY * deltaY;
        return Math.Sqrt(distanceSquared);
    }
    public double GetDistance(double x, double y)
    {
        double deltaX = Position.X - x;
        double deltaY = Position.Y - y;

        double distanceSquared = deltaX * deltaX + deltaY * deltaY;
        return Math.Sqrt(distanceSquared);
    }
}

public class ParticleEmitter
{
    /// <summary>
    /// Linearly interpolates the velocity of the particle from the first to last frame. <b>Acceleration AND Velocity must be NULL for this to work. Only one Vector2 field, other than Size, may be tweened at once.</b>
    /// </summary>
    public Vector2[] TPosition = null;
    /// <summary>
    /// Linearly interpolates the Acceleration of the particle from the first to last frame. <b>Only one Vector2 field, other than Size, may be tweened at once.</b>
    /// </summary>
    public Vector2[] TAcceleration = null;
    /// <summary>
    /// Linearly interpolates the velocity of the particle from the first to last frame. <b>Acceleration must be NULL for this to work. Only one Vector2 field, other than Size, may be tweened at once.</b>
    /// </summary>
    public Vector2[] TVelocity = null;
    /// <summary>
    /// Linearly interpolates the size of the particle from the first to the last frame.
    /// </summary>
    public double[] TSize = null;
    public double Size = new();
    public double Drag = 1;

    public string Symbol = "#";

    /// <summary>
    /// Lifetime of particles in milliseconds. Set to <b>-1</b> to make it infinite.
    /// </summary>
    public int MaxLifetimeMilliseconds = 1000;
    public int LifetimeMilliseconds = 0;

    private Vector2 LookDir = new();
    public Vector2 EmitDirection = new Vector2(0,1);
    public Entity Parent = null;

    /// <summary>Spread of particles, in degrees.</summary>
    public double Spread = 30;
    public int MaxParticleLifetimeMilliseconds = 1000;

    ///<summary>Amount of particles to be emitted per second.</summary>
    public double EmitRate = 10;
    public List<Particle> Particles = new();
    private int Buildup = 0;

    public double VelocityRange = 0;
    public double AccelerationRange = 0;
    public double MaxParticleLifetimeRange = 0;

    public bool IsEnabled = true;

    public Vector2 Velocity = new();
    public Vector2 Position = new();
    public Vector2 Acceleration = new();

    public void Particle()
    {

    }
    public ParticleEmitter()
    {
        ParticleSystem.ActiveEmitters.Add(this);
    }

    public void Emit(int Amount = 0)
    {
        if (Parent != null)
        {
            Position = Parent.Position;
            LookDir = Vector2.FromDegrees(
                    EmitDirection.ToDegrees() +
                    Parent.LookDir.ToDegrees()
                );
        }
        else
            LookDir = EmitDirection;

        if (Amount == 0)
        {
            Buildup += (int)(Runtime.dTime * 1000d);

            if (Buildup > 1000/EmitRate)
            {
                Amount = (int)Math.Round(Buildup/(1000/EmitRate));
                Buildup = 0;
            } 
        }

        if (Amount == 0) // double pass because its changed above
            return;

        for (int i = 0; i < Amount; i++)
        {
            Particle p = new()
            {
                Position = new(Position),
                TPosition = TPosition,
                TVelocity = TVelocity,
                TAcceleration = TAcceleration,
                Size = Size,
                TSize = TSize,
                Owner = this,
                Symbol = Symbol,
                Drag = Drag,
                Velocity = Velocity
            };

            Vector2 angle = (Vector2.FromDegrees(LookDir.ToDegrees() + 
                (new Random().NextInt64((long)Spread) - Spread / 2d))).Normalize();

            double accel = 0;
            if (AccelerationRange != 0)
                accel = AccelerationRange * (new Random().NextDouble() - 0.5);

            double vel = 0;
            if (VelocityRange != 0)
                vel = VelocityRange * (new Random().NextDouble() - 0.5);

            double lifetime = 0;
            if (MaxParticleLifetimeRange != 0)
                lifetime = MaxParticleLifetimeRange * (new Random().NextDouble() - 0.5);

            p.Velocity = angle * Velocity + vel;
            p.Acceleration = Acceleration + accel;
            p.MaxLifetimeMilliseconds = MaxParticleLifetimeMilliseconds + (int)lifetime;
        }
    }

    public void Tick()
    {
        if (EmitRate == 0 || !IsEnabled)
            return;
        Emit();

        LifetimeMilliseconds += (int)(Runtime.dTime * 1000);
        if (LifetimeMilliseconds > MaxLifetimeMilliseconds && MaxLifetimeMilliseconds != -1 || MaxLifetimeMilliseconds == 0) 
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        ParticleSystem.ActiveEmitters.Remove(this);

        if (Particles == null)
            return;
        foreach (Particle p in Particles)
        {
            if (p.Owner == this)
                p.Owner = null;
        }
        Particles = null;
    }
}