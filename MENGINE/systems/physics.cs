using System;
using System.Linq;
using System.Collections.Generic;

namespace MEngine;

public static class Physics 
{         
    public static readonly Vector2 EarthGravity = new(-9.81, 0);
	public static Vector2 Gravity = new(-9.81, 0);
	public static double AirFrictionModifier = 0.05;
	
    public static void Tick()
	{
		List<Entity> Entities = new(Runtime.Entities);
	    foreach (Entity obj in Entities)
		{
			if (obj.Exists)
			{
				obj.Update();
				Controls.Tick(obj);
				obj.Tick();
				foreach (VertexGroup tri in obj.VisibleShape.Triangles)
				{
					tri.GenerateDenominator(obj.Position);
				}
			}


            if (!obj.HasPhysics)
				continue;

		    obj.Position += obj.Velocity * 0.5 * Runtime.dTime;
			obj.LookDir = Vector2.FromDegrees(obj.LookDir.ToDegrees() + obj.RotationalVelocity);

            //if (obj.HasGravity)
            //    obj.Velocity += Gravity * dTime;
            if (obj.HasFriction)
                obj.Velocity *= 1 - (
                    (obj.UseCustomFriction ? obj.AirFrictionModifier : AirFrictionModifier)
                );
			obj.Velocity += obj.Acceleration * Runtime.dTime;

            obj.Position += obj.Velocity * 0.5 * Runtime.dTime;
			
			//if (obj.HasWallCollision)
			 //   CheckWallCollision();


		}
	}
}
