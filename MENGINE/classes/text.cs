using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MEngine;

public class UIText
{
    public string Text = "Text";
    public string Name = "TextEntity";
    public bool Exists = true;

    public delegate void EventHandler(object sender, EventArgs e);
    public event EventHandler OnDispose;

    public bool Visible = true;
    public bool Centered = true;

    protected List<EntityFlag> Flags = new();

    public Vector2 Position = new();

    public Vector2 Forward = new();
    public Vector2 Backward = new();
    public Vector2 Left = new();
    public Vector2 Right = new();

    public Vector2 LookDir = new(0, 1);
    public Color Color = Color.White;

    public UIText(double x = 0, double y = 0, bool centered = false)
    {
        Position = new Vector2(x, y);
        Runtime.TextEntities.Add(this);
        Centered = centered;
    }
    public UIText()
    {
        Position = new Vector2(0, 0);
        Runtime.TextEntities.Add(this);
    }
    public UIText(bool centered)
    {
        Position = new Vector2(0, 0);
        Runtime.TextEntities.Add(this);
        Centered = centered;
    }

    public void Update()
    {
        if (LookDir == 0)
            LookDir = new Vector2(0, 1);

        LookDir = LookDir.Normalize();
        Forward = LookDir;
        Backward = -LookDir;
        Left = Forward.Rotate(-90d);
        Right = -Left;
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

    /// <summary>
    /// Removes all entity references from the engine.
    /// </summary>
    public virtual void Dispose()
    {
        Runtime.TextEntities.Remove(this);
        Exists = false;
        OnDispose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Creates a deep copy of the Entity.
    /// </summary>
    /// <returns>The created copy.</returns>
    public UIText Copy()
    {
        UIText A = new();
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
