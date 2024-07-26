using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Drawing;

namespace MEngine;

public class Entity
{
    public Shape2 Shape = Shapes2D.Dot;
    public Shape2 VisibleShape = new();

    public double AirFrictionModifier = 0;
    public string Symbol = "#";
    public string Name = "Entity";
    public bool Exists = true;

    public bool ClickListenerEnabled = false;
    public delegate void ClickedEventHandler(object sender, ClickedEventArgs e);
    public event ClickedEventHandler ClickedEvent;
    public delegate void EventHandler(object sender, EventArgs e);
    public event EventHandler OnDispose;

    public bool HasPhysics = false;
    public bool HasGravity = false;
    public bool HasFriction = false;
    public bool HasWallCollision = false;
    public bool UseCustomFriction = false;
    public bool Visible = true;

    public double VelocityLowerThreshold = 0.001;
    public double LongestSizeAxis = 0;
    protected List<EntityFlag> Flags = new();

    public Vector2 Size = new(1, 1);
    public Vector2 Position = new();
    public Vector2 Velocity = new();
    public Vector2 Acceleration = new();

    public Color Color = Color.White;

    public double RotationalVelocity = new();

    public Vector2 Forward = new();
    public Vector2 Backward = new();
    public Vector2 Left = new();
    public Vector2 Right = new();

    public Vector2 LookDir = new(0, 1);

    public Entity(double x = 0, double y = 0)
    {
        Position = new Vector2(x, y);
        Runtime.Entities.Add(this);
    }
    public Entity()
    {
        Position = new Vector2(0, 0);
        Runtime.Entities.Add(this);
    }

    public void Update()
    {
        var x = false;
        var y = false;

        // Sets velocity to 0 under threshold. (ex. 0.0003 --> 0)
        if (Velocity.X < VelocityLowerThreshold && Velocity.X > -VelocityLowerThreshold)
            x = true;
        if (Velocity.Y < VelocityLowerThreshold && Velocity.Y > -VelocityLowerThreshold)
            y = true;

        if (LookDir == 0)
            LookDir = new Vector2(0, 1);

        VisibleShape.Triangles.Clear();

        foreach (VertexGroup triOrg in Shape.Triangles)
        {
            //Vector2 newVec = (LookDir == 0) ? new Vector2(0,1) : new Vector2(LookDir.X, LookDir.Y);
            VertexGroup triNew = triOrg * Size;

            VertexGroup tri = new(
                new Vector2(triNew.A.X, triNew.A.Y, this),
                new Vector2(triNew.B.X, triNew.B.Y, this),
                new Vector2(triNew.C.X, triNew.C.Y, this)
            );

            VisibleShape.AddTriangle(tri);
        }

        if (x || y)
            Velocity = new Vector2(x ? 0 : Velocity.X, y ? 0 : Velocity.Y);
        else if (Velocity == 0)
            return;

        LookDir = LookDir.Normalize();
        Forward = LookDir;
        Backward = -LookDir;
        Left = Forward.Rotate(-90d);
        Right = -Left;

        LongestSizeAxis = Math.Max(Size.X, Size.Y);
    }

    /// <summary>
    /// Gets distance of entity position from given Vector2 position.
    /// </summary>
    /// <param name="otherVector"></param>
    /// <returns>Distance as a double value.</returns>
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

    public Entity[]? GetTouchingEntities(Entity[] ignore = null)
    {
        List<Entity> touching = new();
        List<Entity> ents = new(Runtime.Entities);

        foreach (Entity e in ents)
        {
            if (e == this || e == null || !(ignore == null) && ignore.Contains(e)) continue;
            /*if (
                e.GetDistance(Position) > 
                Math.Max(
                    (Size + e.Size).X, 
                    (Size + e.Size).Y
                    )
                )
                return null;*/
            bool _continue = true;
            Vector2 offset = new(e.Position);

            Parallel.ForEach(e.VisibleShape.Triangles, tri =>
            {
                if (!_continue)
                    return;

                tri.GenerateDenominator(Position);
                Parallel.ForEach(VisibleShape.Triangles, selftri =>
                {
                    selftri.GenerateDenominator(Position);
                    if (
                        selftri.IsPosInside(tri.A + offset) ||
                        selftri.IsPosInside(tri.B + offset) ||
                        selftri.IsPosInside(tri.C + offset)
                        ||
                        tri.IsPosInside(selftri.A + offset) ||
                        tri.IsPosInside(selftri.B + offset) ||
                        tri.IsPosInside(selftri.C + offset)
                        )
                    {
                        touching.Add(e);
                        _continue = false;
                        return;
                    }
                });
            });
        }
        if (touching.Count == 0)
            return null;

        return [.. touching];
    }

    /// <summary>
    /// Removes all entity references from the engine.
    /// </summary>
    public virtual void Dispose()
	{
	    Runtime.Entities.Remove(this);
        Exists = false;
        OnDispose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Creates a deep copy of the Entity.
    /// </summary>
    /// <returns>The created copy.</returns>
    public Entity Copy()
	{
        Entity A = new();
        PropertyInfo[] properties = A.GetType().GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var property in properties)
        {
            if (property.CanWrite)
                property.SetValue(A, property.GetValue(this));
        }
        return A;
    }

    // called in physics.cs
    public virtual void Tick()
    {
    }

    /// <summary>
    /// Invokes entity's ClickedEvent, regardless of Entity.ClickedEventListener state.
    /// </summary>
    /// <param name="clickPosition">Vector2 position of the click.</param>
    /// <param name="bLeftClick">Whether the click was of the left mouse button. Otherwise, right mouse button.</param>
    public void InvokeClickedEvent(Vector2 clickPosition, bool bLeftClick)
    {
        ClickedEventArgs args = new();
        args.Position = clickPosition;
        args.Entity = this;
        args.bLeftClick = bLeftClick;
        ClickedEvent?.Invoke(this, args);
    }

    /// <summary>
    /// Sets or adds a flag with specified value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns><b>True</b> if flag already existed and was overwritten; <b>False</b> if a new flag was created.</returns>
    public bool SetFlag(string key, short value = 0)
    {
        EntityFlag flag = Flags.Find(flag => (flag.Key == key));
        if (flag == null)
        {
            flag = new EntityFlag(key, value);
            Flags.Add(flag);
            return false;
        }

        flag.Value = value;
        return true;
    }

    /// <summary>
    /// Sets or adds a flag with given EntityFlag.
    /// </summary>
    /// <param name="flag">EntityFlag instance, EntityFlag.Value <b>must not be null.</b></param>
    /// <returns><b>True</b> if flag already existed and was overwritten; <b>False</b> if a new flag was created.</returns>
    public bool SetFlag(EntityFlag flag)
    {
        if (flag == null)
            throw new ArgumentException("Flag value cannot be null.");

        EntityFlag _flag = Flags.Find(_flag => (_flag.Key == flag.Key));
        if (_flag == null)
        {
            _flag = new EntityFlag(flag.Key, flag.Value);
            Flags.Add(_flag);
            return false;
        }

        _flag.Value = flag.Value;
        return true;
    }

    /// <summary>
    /// Checks if entity has a flag.
    /// </summary>
    /// <param name="key"></param>
    /// <returns><b>True</b> if the entity has the specified flag; otherwise <b>False.</b></returns>
    /// <remarks>Note: If you wish to get the flag itself, use Entity.GetFlag().</remarks>
    public bool HasFlag(string key)
    {
        EntityFlag flag = Flags.Find(flag => (flag.Key == key));

        if (flag == null)
            return false;

        return true;
    }

    /// <summary>
    /// Get specified EntityFlag.
    /// </summary>
    /// <param name="key"></param>
    /// <returns><b>EntityFlag instance</b> if found; otherwise <b>null.</b></returns>
    public EntityFlag? GetFlag(string key)
    {
        EntityFlag flag = Flags.Find(flag => (flag.Key == key));

        if (flag == null)
            return null;

        return flag;
    }

    /// <summary>
    /// Get specified EntityFlag.
    /// </summary>
    /// <param name="key"></param>
    /// <returns><b>True</b> if found and removed; otherwise <b>False.</b></returns>
    public bool RemoveFlag(string key)
    {
        EntityFlag flag = Flags.Find(flag => (flag.Key == key));

        if (flag == null)
            return false;

        Flags.Remove(flag);
        return true;
    }
}

public class ClickedEventArgs : EventArgs
{
    public Entity Entity { get; set; }
    public Vector2 Position { get; set; }
    public bool bLeftClick { get; set; }
}

public class EntityFlag
{
    public string Key { get; set; }
    public short? Value {  get; set; }
    public EntityFlag(string key, short? value = null)
    {
        Key = key;
        Value = value;
    }
}