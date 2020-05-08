using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.IO;

//  
//  assets
//  https://pixel-frog.itch.io/kings-and-pigs
//  

namespace KingsGame
{
	public class KingsGame : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch sprite;
		static IContent content;

		int[] fps = new int[] { 0, 0 };
		ulong _fps_f = 0;
		Thread fpsc;

		static float speed = 800F;

		SpriteFont font;
		static int[] key = new int[] { 0, 0, 0, 0, 0, 0 };
		static int[] _key = new int[] { 0, 0, 0, 0, 0, 0 };

		Player player;

		static byte width;
		static byte height;
		static int TexSi = 32;
		static int scale = 3;
		static int currMap = 0;
		class IContent : System.IDisposable
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
				if (c.ReadInt32() != 0) throw err;
				var d = c.ReadInt32();
				_textures = new string[d];
				for (var _a = 0; _a < d; _a++)
				{
					_textures[_a] = c.ReadString();
					if (c.ReadUInt16() != _a) throw err;
				}
				if (c.ReadInt32() != 1) throw err;
				d = c.ReadInt32();
				size = new byte[d, 2];
				for (var _a = 0; _a < d; _a++)
				{
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
				for (var _a = 0; _a < d; _a++)
				{
					var _b = width * height;
					for (var _c = 0; _c < _b; _c++) maps[_a, _c] = c.ReadUInt16();
					if (c.ReadUInt16() != _a) throw err;
				}
				if (c.ReadInt32() != 4) throw err;
				if (c.ReadInt32() != d) throw err;
				decoration = new dec[d];
				for (var _a = 0; _a < d; _a++)
				{
					var _b = c.ReadInt32();
					decoration[_a] = new dec() { d = new byte[_b, 2], t = new ushort[_b] };
					for (var _c = 0; _c < _b; _c++)
					{
						decoration[_a].t[_c] = c.ReadUInt16();
						for (var _d = 0; _d < 2; _d++) decoration[_a].d[_c, _d] = c.ReadByte();
					}
					if (c.ReadUInt16() != _a) throw err;
				}
				c.ReadString();
				c.Dispose();
				b.Dispose();
				System.GC.Collect();
			}
			public void save(string p)
			{
				var a = new MemoryStream();
				var b = new BinaryWriter(a);
				b.Write("IC:IContent;");
				b.Write(0);
				b.Write(_textures.Length);
				for (var _a = 0; _a < _textures.Length; _a++)
				{
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
				var a = scale * TexSi;
				var b = new byte[] { size[tmap[t, 3], 0], size[tmap[t, 3], 1] };
				sprite.Draw(textures[tmap[t, 0]],
					new Rectangle((manual ? x * scale : (x * a)) + (((int)tmap[t, 4] - 120) * scale), (manual ? y * scale : (y * a)) + (((int)tmap[t, 5] - 120) * scale), b[0] * scale, b[1] * scale),
					new Rectangle(tmap[t, 1] * TexSi, tmap[t, 2] * TexSi, b[0], b[1]),
					Color.White, 0, Vector2.One, effects, 0);
			}
			public void drawMap(SpriteBatch sprite, int a, SpriteFont font)
			{
				for (int _a = 0; _a < width * height; _a++)
				{
					int y = _a / width, x = _a - (y * width);
					_draw(sprite, (ushort)x, (ushort)y, maps[a, _a]);
				}
				for (var _a = 0; _a < decoration[a].t.Length; _a++) _draw(sprite, decoration[a].d[_a, 0], decoration[a].d[_a, 1], decoration[a].t[_a]);
			}
			#region IDisposable Support
			private bool disposedValue = false; // To detect redundant calls

			protected virtual void Dispose(bool disposing)
			{
				if (disposedValue) return;
				disposedValue = true;
				for (var _a = 0; _a < textures.Length; _a++) textures[_a].Dispose();
				if (!disposing) return;
				size = null;
				tmap = null;
				maps = null;
				width = 0;
				height = 0;
				_textures = null;
				decoration = null;
			}
			public void Dispose() => Dispose(true);
			#endregion
		}
		public KingsGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			width = 12;
			height = 6;
			player = new Player();
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
		static byte[] readData(string a)
		{
			var b = File.ReadAllBytes(a);
			uint c = 0;
			var e = new byte[b.Length - 4];
			for (var _a = 0; _a < b.Length - 4; _a++) c += ((c << 2) - (e[_a] = b[_a]));
			if ((uint)(b[b.Length - 1] << 24 | b[b.Length - 2] << 16 | b[b.Length - 3] << 8 | b[b.Length - 4]) != c) throw new System.Exception("File Damaged");
			return e;
		}
		protected override void Initialize()
		{
			graphics.PreferredBackBufferWidth = scale * TexSi * width;
			graphics.PreferredBackBufferHeight = scale * TexSi * height;
			graphics.ApplyChanges();
			player.init();
			base.Initialize();
		}
		protected override void LoadContent()
		{
			sprite = new SpriteBatch(GraphicsDevice);
			content.load(Content);
			font = Content.Load<SpriteFont>("font");
			fpsc = new Thread(new ThreadStart(() =>
			{
				while (true)
				{
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
			content.Dispose();
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
			for (var _a = 0; _a < key.Length; _a++)
			{
				if (key[_a] != _key[_a] && key[_a] == 1)
				{
					if (_a == 0) graphics.ToggleFullScreen();
				}
				_key[_a] = key[_a];
			}
			player.update(gameTime);
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
			object[] resize(object[] d, int a, int b)
			{
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
		class Player
		{
			public ushort[] Idle = new ushort[] { 117, 118, 119, 120, 121, 122, 123 };
			public ushort[] Run = new ushort[] { 109, 110, 111, 112, 113, 114, 115, 116 };
			public ushort[] Attack = new ushort[] { 109, 132, 133, 134, 109 };
			public ushort[] Dead = new ushort[] { 128, 129, 130, 131 };
			public ushort[] WalkOut = new ushort[] { 144, 145, 146, 147, 148, 149, 150, 151 };
			public ushort[] WalkIn = new ushort[] { 136, 137, 138, 139, 140, 141, 142, 143 };
			public ushort[] Fall = new ushort[] { 124 };
			public ushort[] Jump = new ushort[] { 132 };
			public ushort[] Hit = new ushort[] { 126, 127 };
			public ushort[] Ground = new ushort[] { 125 };
			public ushort positionX;
			public ushort positionY;
			public float posX;
			public float posY;
			//public float desX;
			public float desY;
			public int state = 0;
			public int _state = -1;
			public byte view = 0;
			public byte inputBlock = 0;
			public float blockRev = 0;
			public const double MSpeed = 0.04;
			public double start;
			public bool allowFall = true;
			public sbyte aniJump = -1;
			public ushort playerWidth = 38;
			public Player()
			{
				positionX = 3;
				positionY = 4;
				posX = positionX * TexSi;
				posY = positionY * TexSi;
				view = 1;
			}
			public void init()
			{
			}
			public void update(GameTime t)
			{
				positionX = (ushort)(posX / TexSi);
				positionY = (ushort)(posY / TexSi);
				if (state == 5 && blockRev > 0)
				{
					blockRev -= (float)t.ElapsedGameTime.TotalMilliseconds;
					if (blockRev <= 0)
					{
						state = 0;
						inputBlock = 0;
						aniJump = -1;
					}
					return;
				}
				if ((inputBlock & 1) == 0)
				{
					var _a = blockR(positionX, positionY);
					_a[0] = (ushort)(++_a[0] * TexSi);
					_a[1] = (ushort)(_a[1] * TexSi - playerWidth);
					if ((key[3] == 1 || key[4] == 1) && key[4] != view) view = (byte)key[4];
					if (state == 0 || state == 1) state = key[3] == 1 || key[4] == 1 ? 1 : 0;
					if (key[3] == 1)
					{
						posX -= (float)(MSpeed * t.ElapsedGameTime.TotalMilliseconds * (1000 / speed));
						if (posX < _a[0]) posX = _a[0];
					}
					if (key[4] == 1)
					{
						posX += (float)(MSpeed * t.ElapsedGameTime.TotalMilliseconds * (1000 / speed));
						if (posX > _a[1]) posX = _a[1];
					}
				}
				if ((inputBlock & 2) == 0)
				{
					if (key[1] == 1 || key[2] == 1)
					{
						state = key[1] == 1 ? 3 : 4;
						inputBlock |= 2;
						blockRev = speed;
						if (key[1] == 1)
						{
							var _a = 1.4f;
							if (positionY - 1 >= 0 && content.maps[currMap, _map(positionX, (ushort)(positionY - 1))] <= 47) _a -= 0.4f;
							desY = ((float)positionY - _a) * TexSi;
							allowFall = false;
							aniJump = (sbyte)(positionY);
						}
					}
				}
				if ((inputBlock & 4) == 0)
				{
					if (key[5] == 1)
					{
						inputBlock = (byte)(inputBlock | 4);
						blockRev = speed;
						state = 2;
					}
				}
				if (inputBlock != 0 && blockRev > 0)
				{
					blockRev -= (float)t.ElapsedGameTime.TotalMilliseconds;
					if (blockRev <= 0)
					{
						//inputBlock = 0;
						//state = 0;
						blockRev = 0;
						allowFall = true;
						if (state == 2)
						{
							inputBlock = (byte)(inputBlock & 0xFB);
							state = 0;
						}
					}
					if (state == 3)
					{
						if (posY > desY) posY -= (float)(MSpeed * t.ElapsedGameTime.TotalMilliseconds * 5);
						if (posY <= desY)
						{
							allowFall = true;
							posY = desY;
						}
					}
				}
				if (allowFall)
				{
					ushort drop = _block((ushort)(aniJump > 0 ? aniJump : (posY / TexSi + 1)), positionX);
					ushort _drop = _block((ushort)(aniJump > 0 ? aniJump : (posY / TexSi + 1)), (ushort)(positionX + 1));
					if (_drop < drop) drop = _drop;
					var _a = (int)(drop * TexSi - posY);
					if (_a != 0)
					{
						var __a = (float)(MSpeed / 4 * 5 * t.ElapsedGameTime.TotalMilliseconds);
						posY += __a < _a ? __a : _a;
						state = (int)(drop * TexSi - posY) > 0 ? 4 : 5;
						if (state == 5) posY = drop * TexSi;
						blockRev = speed / 4;
					}
				}
			}
			public ushort _block(ushort a, ushort b)
			{
				while (a <= height)
				{
					var __a = content.maps[currMap, _map(b, a++)];
					if (__a < 47) break;
				}
				return (ushort)(a - 2);
			}
			public ushort[] blockR(ushort b, ushort y)
			{
				var c = new ushort[] { 0, 0 };
				for (var _a = b; _a >= 0; _a--)
				{
					if (content.maps[currMap, _map(_a, y)] < 47)
					{
						c[0] = _a;
						break;
					}
				}
				for (var _a = b; _a < width; _a++)
				{
					if (content.maps[currMap, _map(_a, y)] < 47)
					{
						c[1] = _a;
						break;
					}
				}
				return c;
			}
			public int _map(ushort x, ushort y) => y * width + x;
			public void draw(GameTime t, SpriteBatch sprite, SpriteFont font)
			{
				ushort[] frames;
				switch (state)
				{
					case 1: frames = Run; break;
					case 2: frames = Attack; break;
					case 3: frames = Jump; break;
					case 4: frames = Fall; break;
					case 5: frames = Ground; break;
					case 6: frames = Hit; break;
					case 7: frames = Dead; break;
					case 8: frames = WalkOut; break;
					case 9: frames = WalkIn; break;
					default: frames = Idle; break;
				}
				if (_state != state)
				{
					start = t.TotalGameTime.TotalMilliseconds;
					_state = state;
				}
				var _a = blockR(positionX, positionY);
				content._draw(sprite, (ushort)posX, (ushort)posY,
					frames[(int)(((t.TotalGameTime.TotalMilliseconds - start) / (speed / frames.Length)) % frames.Length)],
					true, view == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
				sprite.DrawString(font, positionX + ", " + positionY + '\n' + content.maps[currMap, _map(positionX, positionY)].ToString() + "\n" + _a[0].ToString() + ", " + _a[1].ToString(), new Vector2(posX * scale, posY * scale), Color.Aqua);
			}
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

			content.drawMap(sprite, currMap, font);
			player.draw(gameTime, sprite, font);

			sprite.DrawString(font, logs, new Vector2(10, 10), Color.White);
			sprite.End();
			fps[1]++;
			_fps_f++;
			base.Draw(gameTime);
		}
	}
}
