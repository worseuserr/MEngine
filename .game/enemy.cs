using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace MEngine;

public class Enemy : Entity
{
    public static bool SpawnEnemies()
    {
        if (Game.Enemies.Count > 9)
            return false;

        Enemy enemy = new();
        enemy.Position = Game.Plr.Position + Vector2.FromRadians(new Random().NextInt64(180)) * 50;

        return false;
    }


    public ParticleEmitter Thruster;
    public double Health = 20;
    public List<Entity> DamagersToExclude = new();

    public Enemy()
    {
        Thruster = new()
        {
            Symbol = "*",
            Parent = this,
            Velocity = new Vector2(1),
            MaxParticleLifetimeRange = 100,
            Spread = 40,

            EmitRate = 10,
            EmitDirection = Vector2.FromDegrees(180),
            MaxLifetimeMilliseconds = -1,
            MaxParticleLifetimeMilliseconds = 200,
            Drag = 0.94
        };

        SetFlag("Enemy", 1);
        HasPhysics = true;
        Size = new Vector2(2,3);
        Shape = new Shape2(EnemyShips.Small);

        Health = new Random().NextInt64(10) + 10;

        Game.Enemies.Add(this);
        OnDispose += Disposed;
    }

    public void Disposed(object sender, EventArgs e)
    {
        Game.Enemies.Remove(this);
        Thruster.Dispose();
    }

    public void TakeDamage(float amount)
    {
        //_debug.Variable = amount;
        Health -= amount;
        if (Health < 0)
        {
            Death();
        }
    }

    public void Death()
    {
        FX.SmallExplosion.Position = Position;
        FX.SmallExplosion.Emit(40);
        Game.ExplosionSound.Play();
        Dispose();
    }

    public void ExcludeDamager(Entity damager)
    {
        DamagersToExclude.Add(damager);
        Thread t = new Thread(() => { Thread.Sleep(400); 
            if (damager.Exists)
                DamagersToExclude.Remove(damager);
        
        });
        t.Start();
    }
}

public static class EnemyUtil
{
    public static void ProcessEnemies(List<Enemy> enemies)
    {
        foreach (Enemy enemy in enemies)
        {
            Entity[] touching = enemy.GetTouchingEntities([.. enemies, .. enemy.DamagersToExclude]);
            if (!(touching == null) && touching.Length > 0)
            {
                CheckForDamagers(enemy, touching);
            }


            enemy.Velocity *= 0.95;

            enemy.LookDir = Util.Lerp(enemy.LookDir,
            enemy.Position.LookAt(Game.Plr.Position)
            , 0.3);


            //ai
            double minX = 20;
            double maxX = 20;
            double minY = 20;
            double maxY = 10;

            var distance = Game.Plr.GetDistance(enemy.Position);
            if (distance < 15)
            {
                enemy.Velocity = Game.Plr.Position.LookAt(enemy.Position) * (3 * 15 - distance);
            }

            if (!enemy.HasFlag("LastPosCheck"))
                enemy.SetFlag("LastPosCheck", Game.lastCheck);

            if (Game.lastCheck > enemy.GetFlag("LastPosCheck").Value + 10)
            {

                enemy.Velocity += new Vector2(
                    new Random().NextDouble() * (maxX * 2) - (maxX * 2 - minX),
                    new Random().NextDouble() * (maxY * 2) - (maxY * 2 - minY),
                    enemy
                    );
                enemy.SetFlag("LastPosCheck", Game.lastCheck);
            }

            if (Game.lastCheck > 5000)
                Game.lastCheck = 0;
        }
    }

    static void CheckForDamagers(Enemy enemy, Entity[] touching)
    {
        foreach (Entity entity in touching)
        {
            if (entity == null || !entity.Exists) continue;

            var damager = entity.GetFlag("Damager");
            if (damager != null && !entity.HasFlag(Game.EnemyTag))
            {
                if (entity.HasFlag("SmallSpark"))
                {
                    Game.SmallHitSound.Play();
                    FX.SmallSpark.Position = enemy.Position;
                    FX.SmallSpark.EmitDirection = ((Bullet)entity).LastPos.LookAt(enemy.Position);
                    FX.SmallSpark.Emit(20);
                }

                ((Bullet)entity).MaxLifeTime = 0;
                enemy.TakeDamage((float)damager.Value / 10);
                enemy.ExcludeDamager(entity);
            }
        }
    }
}

public static class EnemyShips
{
    public static readonly Shape2 Small = new(
        new VertexGroup(
            new Vector2(0,1),
            new Vector2(-0.5,0),
            new Vector2(0.5,0)
            ),
        new VertexGroup(
            new Vector2(-0.75, 0.5),
            new Vector2(-1,-1),
            new Vector2(0,-0.5)
            ),
        new VertexGroup(
            new Vector2(0.75, 0.5),
            new Vector2(1, -1),
            new Vector2(0, -0.5)
            )
        );
}
