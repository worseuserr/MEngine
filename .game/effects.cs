using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEngine;

public static class FX
{
    public static ParticleEmitter BigExplosion = new()
    {
        Size = 1,
        Velocity = new Vector2(3),
        MaxParticleLifetimeRange = 500,
        Spread = 360,

        EmitRate = 0,
        EmitDirection = Vector2.FromDegrees(0),
        MaxLifetimeMilliseconds = 0,
        MaxParticleLifetimeMilliseconds = 250,
        VelocityRange = 2,
        Drag = 0.9
    };

    public static ParticleEmitter Explosion = new()
    {
        Size = 1,
        Velocity = new Vector2(2),
        MaxParticleLifetimeRange = 400,
        Spread = 360,

        EmitRate = 0,
        EmitDirection = Vector2.FromDegrees(0),
        MaxLifetimeMilliseconds = 0,
        MaxParticleLifetimeMilliseconds = 180,
        VelocityRange = 1.3,
        Drag = 0.88
    };

    public static ParticleEmitter SmallExplosion = new()
    {
        Size = 1,
        Velocity = new Vector2(1.5),
        MaxParticleLifetimeRange = 250,
        Spread = 360,

        EmitRate = 0,
        EmitDirection = Vector2.FromDegrees(0),
        MaxLifetimeMilliseconds = 0,
        MaxParticleLifetimeMilliseconds = 150,
        VelocityRange = 1,
        Drag = 0.85
    };

    public static ParticleEmitter DashEmitter = new()
    {
        Parent = Game.Plr,
        Size = 1,
        MaxParticleLifetimeRange = 300,
        EmitRate = 0,

        EmitDirection = Vector2.FromDegrees(180),
        MaxLifetimeMilliseconds = -1,
        VelocityRange = 0.5,
        Drag = 0.99,
        Symbol = "#",

        Spread = 10,
        Velocity = new Vector2(3),
        MaxParticleLifetimeMilliseconds = 150
    };

    public static ParticleEmitter SmallSpark = new()
    {
        Size = 1,
        Velocity = new Vector2(2),
        MaxParticleLifetimeRange = 150,
        Spread = 50,

        EmitRate = 0,
        EmitDirection = Vector2.FromDegrees(0),
        MaxLifetimeMilliseconds = 0,
        MaxParticleLifetimeMilliseconds = 140,
        VelocityRange = 2,
        Drag = 0.95
    };


    public static ParticleEmitter PlayerThrusterEmitter = new()
    {
        Symbol = "*",
        Parent = Game.Plr,
        Size = 1,
        Velocity = new Vector2(1),
        MaxParticleLifetimeRange = 300,
        Spread = 40,

        EmitRate = 35,
        EmitDirection = Vector2.FromDegrees(180),
        MaxLifetimeMilliseconds = -1,
        MaxParticleLifetimeMilliseconds = 300,
        Drag = 0.98
    };
}