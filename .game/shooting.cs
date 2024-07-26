using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEngine;

public class Bullet : Entity
{
    public Vector2 LastPos = new();
    //public Entity hitbox;
    public int LifeTime = 0;
    public int MaxLifeTime = 500;

    public Bullet(Vector2 position, Vector2 velocity, float damage, string owner)
    {
        SetFlag(owner, 1);
        SetFlag("Damager", (short)(damage * 10));
        /*hitbox = new() {
            Position = position,
            Shape = new Shape2(Shapes2D.Triangle),
            Size = new Vector2(1),
            Visible = false,
        };
        hitbox.SetFlag("Damager", (short)(damage * 10));
        hitbox.SetFlag(owner, 1);*/

        Velocity = velocity;
        Shape = new Shape2(Shapes2D.Triangle);
        Size = new Vector2(1);
        Position = position;
        LastPos = Position;
        HasPhysics = false;
    }

    public override void Tick()
    {
        LifeTime += (int)(Runtime.dTime * 1000);
        if (LifeTime > MaxLifeTime)
        {
            //hitbox.Dispose();
            Dispose();
            //hitbox = null;
            return;
        }

        LookDir = Velocity.Normalize();
        var _LastPos = Position;
        Position += Velocity * Runtime.dTime;

        //hitbox.Position = (LastPos + Position) * 0.5;
        //hitbox.LookDir = LookDir;
        LastPos= _LastPos;
    }
}