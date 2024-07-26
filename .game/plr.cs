using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEngine;

public static class Player
{
    public static Entity CreatePlr()
    {
        Entity Plr = new();
        Plr.Visible = true;
        Plr.Name = "Player";
        Plr.Shape = new Shape2(Shapes2D.Triangle);
        Plr.Size = new Vector2(3, 3);
        Plr.Symbol = "#";
        Plr.HasPhysics = true;
        Plr.HasGravity = false;
        Plr.HasFriction = false;

        Plr.Shape.Triangles[0].A += new Vector2(0, 0.5);
        Plr.Shape.Triangles[0].B += new Vector2(0, 0.5);
        Plr.Shape.Triangles[0].C += new Vector2(0, 0.5);

        Plr.Shape.AddTriangle(
            new VertexGroup(
                new Vector2(-1, -0.5),
                new Vector2(0, -0.5),
                new Vector2(-1, -1.5)
                )
            );
        Plr.Shape.AddTriangle(
            new VertexGroup(
                new Vector2(1, -0.5),
                new Vector2(0, -0.5),
                new Vector2(1, -1.5)
                )
            );

        return Plr;
    }

    public static Entity CreateShip()
    {
        Entity Ship = new();
        Ship.Shape = new Shape2(Shapes2D.Triangle);
        Ship.Size = new Vector2(1, 2);
        Ship.Symbol = ">";
        Ship.HasPhysics = true;
        Ship.HasGravity = false;
        Ship.HasFriction = false;
        Ship.Name = "Ship1";

        return Ship;
    }
}