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
		Thread fpsc;

		Texture2D select;
		Vector2 selectPos = new Vector2(0, 0);
		Array selArr = new Array();

		SpriteFont font;
		int[] key = new int[] { 0, 0, 0, 0, 0, 0};
		int[] _key = new int[] { 0, 0, 0, 0, 0, 0};
		ushort[] map;
		byte[,] textmap;
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
			public lay[] layers;
			public struct lay
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
				textures = new Texture2D[d];
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
				layers = new lay[d];
				for (var _a = 0; _a < d; _a++)
				{
					var _b = c.ReadInt32();
					layers[_a] = new lay() { d = new byte[_b, 2], t = new ushort[_b] };
					for(var _c = 0; _c < _b; _c++)
					{
						layers[_a].t[_c] = c.ReadUInt16();
						for(var _d = 0; _d < 2; _d++) layers[_a].d[_c, _d] = c.ReadByte();
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
				b.Write(c = layers.GetLength(0));
				for (var _a = 0; _a < c; _a++)
				{
					var _c = layers[_a].t.GetLength(0);
					b.Write(_c);
					for (var _b = 0; _b < _c; _b++)
					{
						b.Write(layers[_a].t[_b]);
						for (var _d = 0; _d < 2; _d++) b.Write(layers[_a].d[_b, _d]);
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
				for (var _a = 0; _a < _textures.Length; _a++) textures[_a] = Content.Load<Texture2D>(_textures[_a]);
			}
			public void drawMap(SpriteBatch sprite, int a, SpriteFont font)
			{
				var b = TexSc * TexSi;
				var c = new byte[2];
				var d = 0;
				for (int _a = 0; _a < width * height; _a++) {
					int y = _a / width, x = _a - (y * width);
					var _b = maps[a, _a];
					sprite.Draw(textures[tmap[_b, 0]], new Rectangle(x * b + tmap[_b, 4] * TexSc, y * b + tmap[_b, 5] * TexSc, size[tmap[_b, 3], 0] * TexSc, size[tmap[_b, 3], 1] * TexSc), new Rectangle(tmap[_b, 1] * size[tmap[_b, 3], 0], tmap[_b, 2] * size[tmap[_b, 3], 1], size[tmap[_b, 3], 0], size[tmap[_b, 3], 1]), Color.White);
				}
				for (var _a=0; _a < layers[a].t.Length; _a++) {
					var _c = layers[a];
					var _b = new byte[2] { size[tmap[_c.t[_a], 3], 0], size[tmap[_c.t[_a], 3], 1] };
					sprite.Draw(textures[tmap[_c.t[_a], 0]], new Rectangle(_c.d[_a, 0] * b + tmap[_c.t[_a], 4] * TexSc, _c.d[_a, 1] * b + tmap[_c.t[_a], 5] * TexSc, _b[0] * TexSc, _b[1] * TexSc),
						new Rectangle(tmap[_c.t[_a], 1] * _b[0], tmap[_c.t[_a], 2] * _b[1], _b[0], _b[1]), Color.White);
				}
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
					if (!kstate.IsKeyDown(Keys.LeftControl)) {
						if (_a == 1) selectPos.Y -= TexSi * TexSc;
						else if (_a == 2) selectPos.Y += TexSi * TexSc;
						else if (_a == 3) selectPos.X -= TexSi * TexSc;
						else if (_a == 4) selectPos.X += TexSi * TexSc;
						else if (_a == 5) selArr.add(new int[] { (int)selectPos.X / TexSi, (int)selectPos.Y / TexSi });
					}
					else
					{
						var _b = TexSi * TexSc;
						_b = (int)((selectPos.Y / _b) * width + (selectPos.X / _b));
						if (_a == 1) content.maps[0, _b] -= 1;
						else if (_a == 2) content.maps[0, _b] += 1;
						else if (_a == 3) content.maps[0, _b] -= 10;
						else if (_a == 4) content.maps[0, _b] += 10;
					}
					if (kstate.IsKeyDown(Keys.LeftShift) && kstate.IsKeyDown(Keys.S))
					{
						content.save("new.idata");
					}
				}
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
			string logs = "FPS: " + fps[0];
			graphics.GraphicsDevice.Clear(Color.WhiteSmoke);
			logs += "\nSize: " + width + ", " + height;
			logs += "\nGT: " + gameTime.ElapsedGameTime.Milliseconds;
			sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			content.drawMap(sprite, 0, font);
			sprite.Draw(select, selectPos, Color.White);
			sprite.DrawString(font, logs, new Vector2(10, 10), Color.White);
			sprite.End();
			fps[1]++;
			base.Draw(gameTime);
		}
	}
}
