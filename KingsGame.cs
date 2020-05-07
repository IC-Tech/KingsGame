using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.IO;

//assets
//https://pixel-frog.itch.io/kings-and-pigs

namespace KingsGame
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class KingsGame : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch sprite;
		Texture2D wall;
		IContent content;

		int[] fps = new int[] { 0, 0 };
		ulong _fps_f = 0;
		Thread fpsc;

		Texture2D select;
		Vector2 selectPos = new Vector2(0, 0);
		Array selArr = new Array();

		float speed = 800F;

		SpriteFont font;
		int[] key = new int[] { 0, 0, 0, 0, 0, 0};
		int[] _key = new int[] { 0, 0, 0, 0, 0, 0};

		float PlayerPos = 0;
		int PlayerView = 1;

		static byte width;
		static byte height;
		static int TexSi;
		static int TexSc;
		class IContent
		{
			public byte[,] size;
			public byte[,] tmap;
			public ushort[,] maps;
			public byte width;
			public byte height;
			public string[] _textures;
			public Texture2D[] textures;
			public dec[] decoration;
			public struct dec
			{
				public ushort[] t;
				public byte[,] d;
			}
			public IContent(string a)
			{
				var err = new System.Exception("data corrupted");
				var b = new MemoryStream(readData(a));
				var c = new BinaryReader(b);
				c.ReadString();
				if(c.ReadInt32() != 0) throw err;
				var d = c.ReadInt32();
				_textures = new string[d];
				for (var _a = 0; _a < d; _a++) {
					_textures[_a] = c.ReadString();
					if (c.ReadUInt16() != _a) throw err;
				}
				if (c.ReadInt32() != 1) throw err;
				d = c.ReadInt32();
				size = new byte[d, 2];
				for (var _a = 0; _a < d; _a++) {
					size[_a, 0] = c.ReadByte();
					size[_a, 1] = c.ReadByte();
				}
				if (c.ReadInt32() != 2) throw err;
				d = c.ReadInt32();
				tmap = new byte[d, 6];
				for (var _a = 0; _a < d; _a++)
				{
					for (var _b = 0; _b < 6; _b++)
					{
						tmap[_a, _b] = c.ReadByte();
					}
				}
				if (c.ReadInt32() != 3) throw err;
				width = c.ReadByte();
				height = c.ReadByte();
				d = c.ReadInt32();
				maps = new ushort[d, width * height];
				for (var _a = 0; _a < d; _a++) {
					var _b = width * height;
					for(var _c =0; _c < _b; _c++) maps[_a, _c] = c.ReadUInt16();
					if (c.ReadUInt16() != _a) throw err;
				}
				if (c.ReadInt32() != 4) throw err;
				if (c.ReadInt32() != d) throw err;
				decoration = new dec[d];
				for (var _a = 0; _a < d; _a++)
				{
					var _b = c.ReadInt32();
					decoration[_a] = new dec() { d = new byte[_b, 2], t = new ushort[_b] };
					for(var _c = 0; _c < _b; _c++)
					{
						decoration[_a].t[_c] = c.ReadUInt16();
						for(var _d = 0; _d < 2; _d++) decoration[_a].d[_c, _d] = c.ReadByte();
					}
					if (c.ReadUInt16() != _a) throw err;
				}
				c.ReadString();
				c.Dispose();
				b.Dispose();
				System.GC.Collect();
			}
			public void save(string p) {
				var a = new MemoryStream();
				var b = new BinaryWriter(a);
				b.Write("IC:IContent;");
				b.Write(0);
				b.Write(_textures.Length);
				for (var _a = 0; _a < _textures.Length; _a++) {
					b.Write(_textures[_a]);
					b.Write((ushort)_a);
				}
				b.Write(1);
				var c = size.GetLength(0);
				b.Write(c);
				for (var _a = 0; _a < c; _a++)
				{
					b.Write(size[_a, 0]);
					b.Write(size[_a, 1]);
				}
				b.Write(2);
				b.Write(c = tmap.GetLength(0));
				for (var _a = 0; _a < c; _a++) for (var _b = 0; _b < 6; _b++) b.Write(tmap[_a, _b]);
				b.Write(3);
				b.Write(width);
				b.Write(height);
				b.Write(c = maps.GetLength(0));
				for (var _a = 0; _a < c; _a++)
				{
					for (var _b = 0; _b < width * height; _b++) b.Write(maps[_a, _b]);
					b.Write((ushort)_a);
				}
				b.Write(4);
				b.Write(c = decoration.GetLength(0));
				for (var _a = 0; _a < c; _a++)
				{
					var _c = decoration[_a].t.GetLength(0);
					b.Write(_c);
					for (var _b = 0; _b < _c; _b++)
					{
						b.Write(decoration[_a].t[_b]);
						for (var _d = 0; _d < 2; _d++) b.Write(decoration[_a].d[_b, _d]);
					}
					b.Write((ushort)_a);
				}
				b.Write("END;");
				writeData(p, a.GetBuffer(), (int)a.Length);
				b.Dispose();
				a.Dispose();
			}
			public void load(Microsoft.Xna.Framework.Content.ContentManager Content)
			{
				textures = new Texture2D[_textures.Length];
				for (var _a = 0; _a < _textures.Length; _a++) textures[_a] = Content.Load<Texture2D>(_textures[_a]);
			}
			public void _draw(SpriteBatch sprite, ushort x, ushort y, ushort t, bool manual = false, SpriteEffects effects = SpriteEffects.None)
			{
				var a = TexSc * TexSi;
				var b = new byte[] { size[tmap[t, 3], 0], size[tmap[t, 3], 1] };
				sprite.Draw(textures[tmap[t, 0]],
					new Rectangle((manual ? x * TexSc : (x * a)) + tmap[t, 4] * TexSc, (manual ? y * TexSc : (y * a)) + tmap[t, 5] * TexSc, b[0] * TexSc, b[1] * TexSc), 
					new Rectangle(tmap[t, 1] * TexSi, tmap[t, 2] * TexSi, b[0], b[1]),
					Color.White, 0, Vector2.One, effects, 0);
			}
			public void drawMap(SpriteBatch sprite, int a, SpriteFont font)
			{
				for (int _a = 0; _a < width * height; _a++) {
					int y = _a / width, x = _a - (y * width);
					_draw(sprite, (ushort)x, (ushort)y, maps[a, _a]);
				}
				for (var _a=0; _a < decoration[a].t.Length; _a++) _draw(sprite, decoration[a].d[_a, 0], decoration[a].d[_a, 1], decoration[a].t[_a]);
			}
		}
		static Rectangle Tex(int a, int b) => new Rectangle(a * TexSi, b * TexSi, TexSi, TexSi);
		public KingsGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			width = 12;
			height = 6;
			TexSi = 32;
			TexSc = 3;
			content = new IContent("game.idata");
		}
		static void writeData(string b, byte[] a, int d = -1)
		{
			d = d < 0 ? a.Length : d;
			uint c = 0;
			for (var _a = 0; _a < d; _a++) c += ((c << 2) - a[_a]);
			var f = new FileStream(b, FileMode.Create);
			f.Write(a, 0, d);
			f.Write(new byte[] { (byte)c, (byte)(c >> 8), (byte)(c >> 16), (byte)(c >> 24) }, 0, 4);
			f.Dispose();
		}
		static byte[] readData(string a) {
			var b = File.ReadAllBytes(a);
			uint c = 0;
			var e = new byte[b.Length - 4];
			for(var _a=0; _a<b.Length -4; _a++) c += ((c << 2) - (e[_a] = b[_a]));
			if ((uint)(b[b.Length - 1] << 24 | b[b.Length - 2] << 16 | b[b.Length - 3] << 8 | b[b.Length - 4]) != c) throw new System.Exception("File Damaged");
			return e;
		}
		protected override void Initialize()
		{
			graphics.PreferredBackBufferWidth = TexSc * TexSi * width;
			graphics.PreferredBackBufferHeight = TexSc * TexSi * height;
			graphics.ApplyChanges();
			base.Initialize();
		}
		protected override void LoadContent()
		{
			sprite = new SpriteBatch(GraphicsDevice);
			content.load(Content);
			wall = Content.Load<Texture2D>("wall");
			font = Content.Load<SpriteFont>("font");
			select = new Texture2D(GraphicsDevice, TexSi * TexSc, TexSi * TexSc);
			Color[] d = new Color[TexSi * TexSi * TexSc * TexSc];
			for (var _a = 0; _a < TexSi * TexSi * TexSc * TexSc; _a++) d[_a] = new Color(0, 0, 0, 100);
			select.SetData(d);
			fpsc = new Thread(new ThreadStart(() =>
			{
				while (true) {
					Thread.Sleep(1000);
					fps[0] = fps[1];
					fps[1] = 0;
				}
			}));
			fpsc.Start();
		}
		protected override void UnloadContent()
		{
			sprite.Dispose();
			wall.Dispose();
			fpsc.Abort();
			base.UnloadContent();
		}
		protected override void Update(GameTime gameTime)
		{
			var kstate = Keyboard.GetState();
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || kstate.IsKeyDown(Keys.Escape)) Exit();
			key[0] = kstate.IsKeyDown(Keys.F11) == true ? 1 : 0;
			key[1] = kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W) == true ? 1 : 0;
			key[2] = kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S) == true ? 1 : 0;
			key[3] = kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A) == true ? 1 : 0;
			key[4] = kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D) == true ? 1 : 0;
			key[5] = kstate.IsKeyDown(Keys.Space) == true ? 1 : 0;
			for (var _a=0; _a<key.Length; _a++)
			{
				if(key[_a] != _key[_a] && key[_a] == 1) {
					if (_a == 0) graphics.ToggleFullScreen();
				}
				if (_a == 3 && key[_a] == 1) PlayerPos -= (float)(0.04 * gameTime.ElapsedGameTime.TotalMilliseconds * (1000 / speed));
				if (_a == 4 && key[_a] == 1) PlayerPos += (float)(0.04 * gameTime.ElapsedGameTime.TotalMilliseconds * (1000 / speed));
				_key[_a] = key[_a];
			}
			base.Update(gameTime);
		}
		class Array
		{
			public const int sp = 10;
			public object[] data;
			public int len;
			public Array()
			{
				len = 0;
				data = new object[sp];
			}
			object[] resize(object[] d, int a, int b) {
				object[] _a = new object[a];
				for (int _b = 0; _b < b; _b++) _a[_b] = d[_b];
				return _a;
			}
			public int add(object a)
			{
				if (len == data.Length - 1) data = resize(data, len + 10, len);
				data[len] = a;
				return ++len;
			}
			public void set(int b, object a) => data[b] = a;
			public object[] get() => resize(data, len, len);
		}
		protected override void Draw(GameTime gameTime)
		{
			string logs = "FPS: " + (1000 / (int)(gameTime.ElapsedGameTime.TotalMilliseconds < 1 ? 1000 : gameTime.ElapsedGameTime.TotalMilliseconds)).ToString() + ", " + fps[0];
			logs += "\nFrames: " + _fps_f.ToString();
			logs += "\nSize: " + width + ", " + height;
			logs += "\nGT: " + gameTime.ElapsedGameTime.TotalMilliseconds;
			logs += "\nLT: " + System.DateTime.Now.ToLongTimeString();
			graphics.GraphicsDevice.Clear(Color.WhiteSmoke);
			sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			content.drawMap(sprite, 0, font);
			var anime = key[3] == 1 || key[4] == 1 ? new ushort[] { 116, 117, 118, 119, 120, 121, 122, 123 } : new ushort[] { 95, 95, 95, 95, 95, 96, 97, 98, 99, 100, 101 };
			var a = (int)((gameTime.TotalGameTime.TotalMilliseconds / (speed / anime.Length)) % anime.Length);
			logs += "\na: " + a; 
			content._draw(sprite, (ushort)(32 + PlayerPos), (ushort)(3 * TexSi), anime[a], true, PlayerView == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
			sprite.DrawString(font, logs, new Vector2(10, 10), Color.White);
			sprite.End();
			fps[1]++;
			_fps_f++;
			base.Draw(gameTime);
		}
	}
}
