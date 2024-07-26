using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace MEngine;

public class RenderHandler 
{         
    // Console for now
	private Type window = null;
	private string sDefaultChar = " ";
	private Vector2 windowSize = new();
    public int SafeZoneExtraLines = 2;
	 
	public RenderHandler(Type window)
	{
       this.window = window;
	}

    private static List<Particle> particles = new();

    public void Tick(int consoleX, int consoleY)
	 {
		string Output = "";

        var SizeField = window.GetField("Size", BindingFlags.Public | BindingFlags.Static);
        if (SizeField == null)
            SizeField = typeof(Window).GetField("Size", BindingFlags.Public | BindingFlags.Static);

        Vector2 Center = ConsoleUtil.Center;
        var CenterField = window.GetField("Center", BindingFlags.Public | BindingFlags.Static);
        if (CenterField == null)
            CenterField = typeof(Window).GetField("Center", BindingFlags.Public | BindingFlags.Static);
        Vector2 wCenter = (Vector2)CenterField.GetValue(null);

        var DebugField = window.GetField("bShowDebugConsole", BindingFlags.Public | BindingFlags.Static);
        if (DebugField == null)
            DebugField = typeof(Window).GetField("bShowDebugConsole", BindingFlags.Public | BindingFlags.Static);
        bool Debug = (bool)DebugField.GetValue(null);

        Vector2 vec = ((Vector2)SizeField.GetValue(null)).Round();
        windowSize = new Vector2(
            Math.Clamp(vec.X * 2, 3, 300), 
            Math.Clamp(vec.Y, 3, 50));

        Console.SetWindowSize((int)windowSize.X+1, (int)windowSize.Y + (Debug ? SafeZoneExtraLines + 2 : SafeZoneExtraLines));

        string[] lines = new string[(int)windowSize.Y];

        Vector2 offset = new Vector2(
            Math.Round((consoleX-Center.X)/ 8d) - wCenter.X * 2
            ,
            Math.Round((consoleY-Center.Y) / 16d) - wCenter.Y
        );

        particles.Clear();
        RenderCache.GenerateParticleCache(windowSize.X, windowSize.Y);
        RenderCache.GenerateEntityCache(windowSize.X, windowSize.Y);

        
        EntityBuffer.Clear();
        foreach (Entity entity in Runtime.Entities)
        {
            if (entity != null && entity.Exists && entity.Visible)
            {
                foreach (VertexGroup tri in entity.VisibleShape.Triangles)
                {
                    tri.GenerateDenominator(entity.Position);
                }

                EntityBuffer.Add(entity);
            }
        }

        UILayer.Clear();
        UILayer = GenerateUI();

        Parallel.For(0, (int)windowSize.Y,
            index => {
				var iy = ((int)windowSize.Y / 2 - index);
				lines[index] = RenderLine(iy, windowSize, offset);
            });

        Console.SetCursorPosition(0, 0);

        foreach (string line in lines)
		{
			Output += line + "\n";
		}

        Console.Write(Output + "\b"); 

        if (Debug)
            _debug.RenderText();
    }

    public Dictionary<Vector2, string> UILayer = new();
    private List<Entity> EntityBuffer = new List<Entity>();

    private string RenderLine(int iy, Vector2 Size, Vector2 offset)
	{
		string line = "";

        for (double ix = 0d - Size.X / 2d; ix < Size.X - Size.X / 2d; ix++)
	    {
            bool IsFound = false;

            (line, IsFound) = EntityScan(ix, iy, line, IsFound, offset);
			
			if (!IsFound)
			    line += sDefaultChar;
	    }
        return line;
	}
	
	 private (string, bool) EntityScan(double ix, double iy, string line, bool IsFound, Vector2 offset)
	 {
        //particles.Clear();

        Vector2 pos = new(
                (ix + offset.X) * 0.5d
                ,
                iy + offset.Y
            );

        Vector2 altpos = new(
                ix + offset.X
                ,
                iy + offset.Y
            );

        foreach (KeyValuePair<Vector2, string> kv in UILayer)
        {
            if (kv.Value == null || kv.Key == null) continue;

            if (kv.Key == altpos)
            {
                line += kv.Value;
                return (line, true);
            }
        }

        var entities = RenderCache.GetChunkAt(pos, RenderCache.EntityChunks);

        if (entities != null)
        {
            for (int i=0; i<entities.EData.Count; i++)
            {
                Entity entity = entities.EData[i];
                /*
                double distance = entity.GetFastDistance(entity.Position.X - x, y);
                if (distance > 1.5*entity.LongestSizeAxis)
                    continue;
                */

                foreach (VertexGroup triangle in entity.VisibleShape.Triangles)
                {
                    if (!triangle.IsPosInside(pos))
                        continue;

                    line += entity.Symbol;
                    return (line, true);
                }
            }
        }

        var cache = RenderCache.GetChunkAt(pos, RenderCache.ParticleChunks);

        if (cache == null)
            return (line, false);

        particles = new(cache.PData);
        if (particles.Count == 0)
            return (line, false);

        bool IsFoundP = false;
        foreach (Particle p in particles)
        {
            if (p != null && pos.Round() == p.Position.Round())
            {
                line += p.Symbol;
                IsFoundP = true;
                try
                {
                    cache.PData.Remove(p);
                } catch { continue; }
                break;
            }
        }
        if (IsFoundP)
        {
            //cache.PData.Remove(pp);
            return (line, true);
        }

        return (line, false);
     }
    
    public Dictionary<Vector2, string> GenerateUI()
    {
        Dictionary<Vector2, string> dict = new();

        foreach (UIText text in Runtime.TextEntities)
        {
            if (text == null || !text.Exists || !text.Visible) continue;

            for (int i = 0; i < text.Text.Length; i++)
            {

                var letter = text.Text[i];

                Vector2 pos = (
                    text.Position * new Vector2(2, 1) 
                    + (text.Centered ? new Vector2(-(text.Text.Length * 0.5) + 1, 0) : new Vector2(0, 0))) 
                    + text.Right * i;

                pos = new Vector2(double.Ceiling(pos.X), double.Round(pos.Y));
                    pos.Round();

                if (dict.ContainsKey(pos))
                {
                    continue;
                }

                dict.Add(pos, letter.ToString());
            }
        }

        return dict;
    }
}

internal static class RenderCache
{
    public static int ParticleCacheSize = 8;
    public static List<CacheChunk> ParticleChunks = new();

    public static int EntityCacheSize = 8;
    public static List<CacheChunk> EntityChunks = new();

    public static CacheChunk GetChunkAt(Vector2 p, List<CacheChunk> cache)
    {
        //Vector2 p = new(Pos.X + ConsoleUtil.X, Pos.Y + ConsoleUtil.Y);
        double x = p.X;
        double y = p.Y;
        foreach (CacheChunk chunk in cache)
        {
            //if (chunk.Data.Count == 0)
            //    continue;
            if (chunk.XStart <= x && chunk.XEnd > x &&
                chunk.YStart <= y && chunk.YEnd > y)
                return chunk;
        }
        return null;
        //throw new Exception($"Position nowhere in chunks. X: {p.X}, Y: {p.Y}");
    }

    public static void GenerateParticleCache(double xMax, double yMax)
    {
        ParticleChunks.Clear();

        for (int ix = (int)-(xMax / 2d); ix < xMax / 2; ix += ParticleCacheSize)
        {
            for (int iy = (int)-(yMax / 2d); iy < yMax / 2; iy += ParticleCacheSize)
            {
                ParticleChunks.Add(
                    new CacheChunk()
                    {
                        XStart = ix + ConsoleUtil.X / 16,
                        XEnd = ix + ConsoleUtil.X / 16 + ParticleCacheSize,
                        YStart = iy + ConsoleUtil.Y / 16,
                        YEnd = iy + ConsoleUtil.Y / 16 + ParticleCacheSize,
                        PData = new()
                    }
                    );
            }
        }

        foreach (Particle p in ParticleSystem.ActiveParticles)
        {
            CacheChunk selectedChunk = GetChunkAt(p.Position, ParticleChunks);

            if (selectedChunk == null)
                continue;

            selectedChunk.PData.Add(p);
        }
    }

    public static void GenerateEntityCache(double xMax, double yMax)
    {
        EntityChunks.Clear();

        for (int ix = (int)-(xMax / 2d); ix < xMax / 2; ix += EntityCacheSize)
        {
            for (int iy = (int)-(yMax / 2d); iy < yMax / 2; iy += EntityCacheSize)
            {
                EntityChunks.Add(
                    new CacheChunk()
                    {
                        XStart = ix + ConsoleUtil.X / 16,
                        XEnd = ix + ConsoleUtil.X / 16 + EntityCacheSize,
                        YStart = iy + ConsoleUtil.Y / 16,
                        YEnd = iy + ConsoleUtil.Y / 16 + EntityCacheSize,
                        EData = new()
                    }
                    );
            }
        }

        foreach (CacheChunk chunk in EntityChunks)
        {
            var en = new List<Entity>(Runtime.Entities);
            foreach (Entity e in en)
            {
                double distance = e.GetDistance(chunk.XStart + EntityCacheSize/2, chunk.YStart + EntityCacheSize / 2);

                if (distance > EntityCacheSize * e.LongestSizeAxis)
                    continue;
                //throw new Exception();

                chunk.EData.Add(e);
            }
        }
    }
}
internal class CacheChunk
{
    public double XStart;
    public double XEnd;
    public double YStart;
    public double YEnd;

    public List<Particle> PData;
    public List<Entity> EData;
}



/* 

 for (int ix = (int)-(xMax/2d); ix < xMax/2; ix += ParticleCacheSize)
        {
            for (int iy = (int)-(yMax/2d); iy < yMax/2; iy += ParticleCacheSize)
            {
                ParticleChunks.Add(
                    new ParticleCacheChunk()
                    {
                        XStart = ix + ConsoleUtil.X/16,
                        XEnd = ix + ConsoleUtil.X/16 + ParticleCacheSize,
                        YStart = iy + ConsoleUtil.Y/16,
                        YEnd = iy + ConsoleUtil.Y/16 + ParticleCacheSize,
                        Data = new()
                    }
                    );
            }
        }


        Parallel.For((int)-(xMax / 2d), (int)(xMax / 2 / ParticleCacheSize), _ix => { 
            int ix = _ix * ParticleCacheSize;
            Parallel.For((int)-(yMax / 2d), (int)(yMax / 2 / ParticleCacheSize), _iy =>
            {
                int iy = _iy * ParticleCacheSize;
                ParticleChunks.Add(
                    new ParticleCacheChunk()
                    {
                        XStart = ix + ConsoleUtil.X / 16,
                        XEnd = ix + ConsoleUtil.X / 16 + ParticleCacheSize,
                        YStart = iy + ConsoleUtil.Y / 16,
                        YEnd = iy + ConsoleUtil.Y / 16 + ParticleCacheSize,
                        Data = new()
                    }
                    );
            });
        });
*/