using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace MEngine;

public class Vector2
{         
    protected double _x; protected double _y;
	
	public double X
	{
		get { return _x; }
	}
	
	public double Y
	{
		get { return _y; }
	}
	
	public double this[int index]
	{
		get {
		    return index == 0 ? _x : _y;
		}
	}
	
	public Vector2(double x = 0, double y = 0, Entity referenceEntity = null)
	{
	    _x = x;
		_y = y;

		if (referenceEntity == null)
			return;

		if (referenceEntity.LookDir == 0)
			referenceEntity.LookDir = new Vector2(0, 1);

		Vector2 newVector = referenceEntity.Right * x + referenceEntity.Forward * y;

        _x = newVector.X;
		_y = newVector.Y;
    }

    public Vector2(Vector2 other)
    {
        _x = other.X;
        _y = other.Y;
    }

    public Vector2(double x)
    {
        _x = x;
        _y = x;
    }
    public double GetDistance(Vector2 otherVector)
    {
        double deltaX = X - otherVector.X;
        double deltaY = Y - otherVector.Y;

        double distanceSquared = deltaX * deltaX + deltaY * deltaY;
        return Math.Sqrt(distanceSquared);
    }

    public double Magnitude { get { return Math.Sqrt(_x * _x + _y * _y); } }

    public Vector2 ToPixels()
	{
		return new Vector2(X * 16d, Y * 16d); // font size is 8 on x and 16 pixels on y
	}

    public Vector2 FromPixels()
    {
        return new Vector2(X / 16d, Y / 16d); // font size is 8 on x and 16 pixels on y
    }

    public Vector2 Round()
	{
		return new Vector2(Math.Round(X), Math.Round(Y));
	}
    public Vector2 Floor()
    {
        return new Vector2(Math.Floor(X), Math.Floor(Y));
    }
    public Vector2 Ceiling()
    {
        return new Vector2(Math.Ceiling(X), Math.Ceiling(Y));
    }

    public Vector2 LookAt(Vector2 other)
	{
		return -(this - other).Normalize();
	}

    public Vector2 Normalize()
    {
		var magnitude = Magnitude;
        if (magnitude > 0)
        {
		    return new Vector2(
			_x / magnitude,
            _y / magnitude
			);
        }
		return new Vector2(0,0);
	}

    public static Vector2 FromDegrees(double angle)
    {
        double radians = angle * Math.PI / 180.0;

        return new Vector2(
			Math.Cos(radians), 
			Math.Sin(radians)
			).Normalize();
    }
    public double ToDegrees()
    {
        double radians = Math.Atan2(Y, X);
        double degrees = radians * 180.0 / Math.PI;
        return degrees;
    }

    public static Vector2 FromRadians(double angle)
    {
        return new Vector2(
            Math.Cos(angle),
            Math.Sin(angle)
            ).Normalize();
    }

    public static Vector2 operator* (Vector2 vectorA, Vector2 vectorB)
	{
	    Vector2 newVector = new();
		newVector._x =
		    vectorA._x * vectorB._x;
		newVector._y =
		    vectorA._y * vectorB._y;
		return newVector;
	}
	
	public static Vector2 operator+ (Vector2 vectorA, Vector2 vectorB)
	{
	    Vector2 newVector = new();
		newVector._x =
		    vectorA._x + vectorB._x;
		newVector._y =
		    vectorA._y + vectorB._y;
		return newVector;
	}

    public static Vector2 operator +(Vector2 vectorA, double num)
    {
        Vector2 newVector = new();
        newVector._x =
            vectorA._x + num;
        newVector._y =
            vectorA._y + num;
        return newVector;
    }

    public static Vector2 operator- (Vector2 vectorA, Vector2 vectorB)
	{
	    Vector2 newVector = new();
		newVector._x =
		    vectorA._x - vectorB._x;
		newVector._y =
		    vectorA._y - vectorB._y;
		return newVector;
	}

    public static Vector2 operator -(Vector2 vectorA, double num)
    {
        Vector2 newVector = new();
        newVector._x =
            vectorA._x - num;
        newVector._y =
            vectorA._y - num;
        return newVector;
    }

    public static Vector2 operator -(Vector2 vectorA)
    {
        Vector2 newVector = new();
        newVector._x =
            -vectorA._x;
        newVector._y =
            -vectorA._y;
        return newVector;
    }

    public static bool operator ==(Vector2 vectorA, Vector2 vectorB)
	{
		if (ReferenceEquals(vectorA, vectorB)) return true;
		if (ReferenceEquals(vectorA, null)) return false;
        if (ReferenceEquals(null, vectorB)) return false;

        if (vectorA._x == vectorB._x &&
		    vectorA._y == vectorB._y)
			return true;
		return false;
	}

    public static bool operator ==(Vector2 vectorA, double num)
    {
        if (vectorA._x == num &&
            vectorA._y == num)
            return true;
        return false;
    }
    public static bool operator !=(Vector2 vectorA, double num)
    {
        if (vectorA._x != num ||
            vectorA._y != num)
            return true;
        return false;
    }

    public static bool operator !=(Vector2 vectorA, Vector2 vectorB)
	{
		if (ReferenceEquals(vectorA, vectorB)) return false;
		if (ReferenceEquals(vectorA, null)) return false;

	    if (vectorA._x != vectorB._x ||
		    vectorA._y != vectorB._y)
			return true;
		return false;
	}
	
	public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Vector2 vec2 = (Vector2)obj;
        return (this == vec2);
    }

    public override int GetHashCode()
    {
        return Tuple.Create(_x, _y).GetHashCode();
    }
	
	
	public static Vector2 operator* (Vector2 vectorA, long num)
	{
	    Vector2 newVector = new();
		newVector._x =
		    vectorA._x * num;
		newVector._y =
		    vectorA._y * num;
		return newVector;
	}
	
	public static Vector2 operator* (Vector2 vectorA, float num)
	{
	    Vector2 newVector = new();
		newVector._x =
		    vectorA._x * num;
		newVector._y =
		    vectorA._y * num;
		return newVector;
	}
	
	public static Vector2 operator* (Vector2 vectorA, double num)
	{
	    Vector2 newVector = new();
		newVector._x =
		    vectorA._x * num;
		newVector._y =
		    vectorA._y * num;
		return newVector;
	}

    public static Vector2 operator* (Vector2 vectorA, decimal num)
	{
	    Vector2 newVector = new();
		newVector._x =
		    (double)(Convert.ToDecimal(vectorA._x) * num);
		newVector._y =
            (double)(Convert.ToDecimal(vectorA._y) * num);
		return newVector;
    }


    public Vector2 Rotate(double angleDegrees)
    {
        double angleRadians = (double)(-angleDegrees * Math.PI / 180.0);
        double cos = (double)Math.Cos(angleRadians);
        double sin = (double)Math.Sin(angleRadians);

        double x = this.X * cos - this.Y * sin;
        double y = this.X * sin + this.Y * cos;
		return new Vector2(x, y);
    }
}


