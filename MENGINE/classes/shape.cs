using System;
using System.Linq;
using System.Collections.Generic;

namespace MEngine;

public class Shape2
{         
    public List<VertexGroup> Triangles = new(){};
	public void AddTriangle(VertexGroup Triangle)
	{
	    Triangles.Add(Triangle);
	}
	
	public Shape2(params VertexGroup[] triangles)
	{
	    if (triangles.Length < 1)
		    return;
			
		foreach (VertexGroup triangle in triangles)
		{
		    Triangles.Add(triangle);
		}
	}

    public Shape2(Shape2 shape)
    {
        foreach (VertexGroup triangle in shape.Triangles)
        {
            Triangles.Add(triangle);
        }
    }

    public static Shape2 FromDefault(Shape2 defaultShape)
    {
		Shape2 output = new();
        foreach (VertexGroup triangle in defaultShape.Triangles)
        {
            output.Triangles.Add(triangle);
        }
		return output;
    }
}

public class VertexGroup
{
    public Vector2 A;
	public Vector2 B;
	public Vector2 C;
	
	public VertexGroup(Vector2 VertexA, Vector2 VertexB, Vector2 VertexC)
	{
	    A = VertexA;
		B = VertexB;
		C = VertexC;
	}

	private double denominator = 0;
	private VertexGroup TrueTri = null;
	public void GenerateDenominator(Vector2 Offset)
	{
        if (Offset == null)
            Offset = new Vector2(0, 0);

        TrueTri = new(A + Offset, B + Offset, C + Offset);
        denominator = (TrueTri.B.Y - TrueTri.C.Y) * (TrueTri.A.X - TrueTri.C.X) + (TrueTri.C.X - TrueTri.B.X) * (TrueTri.A.Y - TrueTri.C.Y);
    }
	public bool IsTTnull()
	{
		if (TrueTri == null)
			return true;
		return false;
	}

    /// <summary>
	/// Checks if a Vector2 position is inside of a triangle. Offset parameter is the relative offset to an entity. Should be <i>Entity.Position</i> in most cases.
	/// </summary>
	/// <param name="Position"></param>
	/// <param name="Offset"></param>
	/// <returns>True if Position + Offset is inside of triangle. Otherwise false.</returns>
	public bool IsPosInside(Vector2 Position)
    {
		if (TrueTri == null)
			return false;

        double a = ((TrueTri.B.Y - TrueTri.C.Y) * (Position.X - TrueTri.C.X) + (TrueTri.C.X - TrueTri.B.X) * (Position.Y - TrueTri.C.Y)) / denominator;
        double b = ((TrueTri.C.Y - TrueTri.A.Y) * (Position.X - TrueTri.C.X) + (TrueTri.A.X - TrueTri.C.X) * (Position.Y - TrueTri.C.Y)) / denominator;
        double c = 1 - a - b;

        return a >= 0 && a <= 1 && b >= 0 && b <= 1 && c >= 0 && c <= 1;
    }
	public static VertexGroup operator *(VertexGroup tri, float num)
	{
		return new VertexGroup(tri.A * (double)num, tri.B * (double)num, tri.C * (double)num);
	}

    public static VertexGroup operator *(VertexGroup tri, Vector2 vec)
    {
        return new VertexGroup(
			tri.A * vec, 
			tri.B * vec, 
			tri.C * vec);
    }

    public static VertexGroup operator *(VertexGroup tri, double num)
    {
        return new VertexGroup(tri.A * num, tri.B * num, tri.C * num);
    }
}

public static class Shapes2D
{	
    public static readonly Shape2 Triangle = new(
	    new VertexGroup(
		    new Vector2(-1, -1),	// Bottom left
			new Vector2(0,1),	   // Top middle
		    new Vector2(1,-1)	   // Bottom right
		)
	);
	
	public static readonly Shape2 Dot = new(
	    new VertexGroup(
		    new Vector2(0,0),
			new Vector2(0,0),
			new Vector2(0,0)
		)
	);
	
	public static readonly Shape2 Square = new(
	    new VertexGroup(
		    new Vector2(-1, -1),  // Bottom left
			new Vector2(-1,1),	// Top left
		    new Vector2(1,-1)	 // Bottom right
		),
		new VertexGroup(
		    new Vector2(-1, 1),   // Top left
			new Vector2(1,1),	 // Top right
		    new Vector2(1,-1)	 // Bottom right
		)
	);
}
