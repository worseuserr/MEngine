using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace MEngine;

/* TODO
 * ------------------------------------------ GAME
 * Create bosses
 * Create healthbar
 * Create shoot modes
 * Create drones
 * 
 * -- later
 * fix entity.copy()
 */
public class Game : Window
{
	public static new float Framerate = 1000;
	public static new bool bShowDebugConsole = true;
	public static new RenderHandler Renderer = new(typeof(Game));
	public static new Vector2 Size = new(80, 80);

	public static double Speed = 40;
	public static double CurrentSpeed = 0;

	public static Entity Plr = Player.CreatePlr();

	public static new Vector2 Center = new(0, 0);
	public static Vector2 DesiredWindowSize = new(Size.X, Size.Y);

	public static string EnemyTag = "Enemy_Team";
	public static string FriendlyTag = "Friendly_Team";

	public static Sound DashSound = SoundSystem.CreateSound(new SoundData 
	{
		FileName = "dash.wav",
		IsLooped = false,
		Volume = 0.6f
	});

	public static Sound ExplosionSound = SoundSystem.CreateSound(new SoundData
	{
		FileName = "explosion.wav",
		IsLooped = false,
		Volume = 0.6f
	});

    public static Sound PlayerShootSound1 = SoundSystem.CreateSound(new SoundData
    {
        FileName = "playershoot1.wav",
        IsLooped = false,
        Volume = 0.4f
    });

    public static Sound SmallHitSound = SoundSystem.CreateSound(new SoundData
    {
        FileName = "smallhit.wav",
        IsLooped = false,
        Volume = 0.3f
    });

    //												DASH
    public static bool IsDashOnCooldown = false;
	public static double dashDistance = 10;
	public static int dashCooldown = 100;

	public static Vector2 cursorPos = new();
	private static bool ScreenFollow = true;

	public static List<Enemy> Enemies = new();
	public static List<Bullet> Bullets = new();


	//									INIT
	public static void Init()
	{
		_debug.WindowTick();
		//ConsoleUtil.SetConsolePosition(Center, bCenter: true);
        TopLeftButton.ClickedEvent += OnTopLeftButtonClick;

		Enemy enemy1 = new();
		enemy1.Position = new Vector2(20,0);

		time.Start();

        Time.EachInterval(2500, Enemy.SpawnEnemies);
    }


    public class CDEntity : Entity
	{
		public bool Cooldown = false;
	}

    public static CDEntity TopLeftButton = new()
	{
        Position = new Vector2(10, 0),
		Size = new Vector2(2, 2),
		LookDir = new Vector2(0, 0),
		HasPhysics = false,
        ClickListenerEnabled = true,
        Shape = new Shape2(Shapes2D.Square),
		Name = "ExplodeButton",
	};

    public static void OnTopLeftButtonClick(object sender, ClickedEventArgs args)
    {
        if (TopLeftButton.Cooldown)
			return;

		FX.SmallExplosion.Position = Plr.Position;
        FX.SmallExplosion.Emit(40);
		ExplosionSound.Play();

        Thread t = new(() => {
            TopLeftButton.Cooldown = true;
			Thread.Sleep(200);
			if (TopLeftButton.Exists)
			{
                TopLeftButton.Cooldown = false;
			}
		});
		t.Start();
    }


	public static bool CDClick = false;
	public static int ClickCooldown = 400;

	public static Stopwatch time = new();

	public static void Tick()
	{
		_debug.WindowTick();

        if (Controls.IsKeyDown("Escape"))
			ExitWindow();

        Vector2 cursorDir = Plr.Position.ToPixels().LookAt(Controls.GetCursorPosition(bCenter: true));

		// Screen follow player
		if (ScreenFollow)
		{
			ConsoleUtil.MoveTo(
				Util.Lerp(
					ConsoleUtil.GetConsolePosition(true),
					Plr.Position.ToPixels() + new Vector2(cursorDir.X * 50, cursorDir.Y * 50)
				, 0.1
				)
				);
		}

        Vector2 oldVel = Plr.Velocity.Normalize();

		if (Controls.IsKeyDown("D") || Controls.IsKeyDown("A")
			|| Controls.IsKeyDown("S")  || Controls.IsKeyDown("W"))
            Plr.Velocity = new Vector2(0, 0);

        if (Controls.IsKeyDown("D"))
		{
			CurrentSpeed = Speed;
		    Plr.Velocity = new Vector2(1, Plr.Velocity.Y);
		} 
		if (Controls.IsKeyDown("A"))
		{
            CurrentSpeed = Speed;
            Plr.Velocity = new Vector2(-1, Plr.Velocity.Y);
		}
		if (Controls.IsKeyDown("S"))
		{
            CurrentSpeed = Speed;
            Plr.Velocity = new Vector2(Plr.Velocity.X, -1);
		}
		if (Controls.IsKeyDown("W"))
		{
            CurrentSpeed = Speed;
            Plr.Velocity = new Vector2(Plr.Velocity.X, 1);
		}

		Plr.Velocity = Util.Lerp(oldVel, Plr.Velocity.Normalize(), 0.5);
		Plr.Velocity *= CurrentSpeed;
		CurrentSpeed *= 1 - 5 * Runtime.dTime;

		if (Controls.IsKeyDown("Spacebar") && !IsDashOnCooldown) 
		{
			Dash(dashDistance);
		}

		if (Controls.IsLeftMouseButtonDown() && !CDClick)
		{
			//_debug.Variable += 1;
			PlayerShootSound1.Play();

			Bullet bullet = new(
				Plr.Position, 
				Plr.Position.LookAt(
					Controls.GetCursorPosition(true).FromPixels()
					) * 100, 
				12, FriendlyTag);
			bullet.SetFlag("SmallSpark");

			CDClick = true;
			Thread t = new(() => { Thread.Sleep(ClickCooldown); CDClick = false; });
			t.Start();
		}

        Plr.LookDir = Util.Lerp(Plr.LookDir,
			new Vector2(cursorDir.X, cursorDir.Y)
			, 0.3);

		Size = Util.Lerp(Size, DesiredWindowSize, 0.2);
		TopLeftButton.Position = new Vector2(ConsoleUtil.X-ConsoleUtil.Width*0.4, ConsoleUtil.Y+ConsoleUtil.Height*0.35).FromPixels();

		List<Enemy> enemies = new(Enemies);

		EnemyUtil.ProcessEnemies(enemies);
        lastCheck++;

		/*if (time.ElapsedMilliseconds > 2500 && Enemies.Count < 9)
		{
			
		}*/
	}

	public static short lastCheck = 0; 
	public static void PostRenderTick()
	{
    }

	public static void Dash(double distance)
	{
        IsDashOnCooldown = true;

		Time.Delay(5, () => {
            IsDashOnCooldown = false;
		});

		DashSound.Play();

		Vector2 normalizedVel = Plr.Velocity.Normalize();

        Entity trail = new();
		trail.Name = "Trail";
        trail.Shape = new Shape2(Shapes2D.Triangle);
        trail.HasPhysics = false;
        trail.Size = Plr.Size * new Vector2(2, distance / 2 -1);

        FX.DashEmitter.MaxParticleLifetimeMilliseconds = 150;
        FX.DashEmitter.Velocity = new Vector2(2);
        FX.DashEmitter.Spread = 10;
        FX.DashEmitter.Emit(10);

        Plr.Position = Plr.Position + Plr.LookDir * distance;
		trail.Position = Plr.Position + Plr.LookDir * ((-distance / 2) - 9);
        trail.LookDir = Plr.LookDir;

        FX.DashEmitter.MaxParticleLifetimeMilliseconds = 250;
        FX.DashEmitter.Velocity = new Vector2(1);
        FX.DashEmitter.Spread = 55;
        FX.DashEmitter.Emit(20);

        Thread decay = new(() =>
		{
			while (trail.Exists) 
			{
				trail.Size *= new Vector2(0.7, 1);

				if (trail.Size.X < 0.1)
					trail.Dispose();

				Thread.Sleep((int)(1d / Runtime.currentFPS * 1000d));
			}
		});
		decay.Start();
    }
	public static void Main()
	{ Runtime.Initialize(typeof(Game)); }
}

