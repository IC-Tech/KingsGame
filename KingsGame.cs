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
		static bool[] key = new bool[7];
		static bool[] _key = new bool[7];

		Player player;

		static byte width;
		static byte height;
		const int TexSi = 32;
		const int scale = 3;

		static int currMap = 0;
		static map Map;

		class Animations
		{
			public static ushort[] Idle = new ushort[] { 117, 118, 119, 120, 121, 122, 123 };
			public static ushort[] Run = new ushort[] { 109, 110, 111, 112, 113, 114, 115, 116 };
			public static ushort[] Attack = new ushort[] { 109, 132, 133, 134, 109 };
			public static ushort[] Dead = new ushort[] { 128, 129, 130, 131 };
			public static ushort[] WalkOut = new ushort[] { 144, 145, 146, 147, 148, 149, 150, 151 };
			public static ushort[] WalkIn = new ushort[] { 136, 137, 138, 139, 140, 141, 142, 143 };
			public static ushort[] Fall = new ushort[] { 124 };
			public static ushort[] Jump = new ushort[] { 132 };
			public static ushort[] Hit = new ushort[] { 126, 127 };
			public static ushort[] Ground = new ushort[] { 125 };
			public static ushort[] DoorOpen = new ushort[] { 94, 152, 153, 154, 155 };
			public static ushort[] DoorClose = new ushort[] { 155, 154, 153, 152, 94 };
		}
		public struct dec
		{
			public ushort[] t;
			public byte[,] d;
		}
		public struct map
		{
			public dec decoration;
			public ushort[] back;
			public byte[] start;
			public byte[] end;
			public byte[,] pigs;
		}
		class IContent : System.IDisposable
		{
			public byte[,] size;
			public byte[,] tmap;
			public byte width;
			public byte height;
			public string[] _textures;
			public Texture2D[] textures;
			public map[] maps;
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
				tmap = new byte[d, 8];
				for (var _a = 0; _a < d; _a++)
				{
					for (var _b = 0; _b < 8; _b++)
					{
						tmap[_a, _b] = c.ReadByte();
					}
				}
				if (c.ReadInt32() != 3) throw err;
				width = c.ReadByte();
				height = c.ReadByte();
				d = c.ReadInt32();
				maps = new map[d];
				for (var _a = 0; _a < d; _a++)
				{
					var _b = width * height;
					maps[_a] = new map()
					{
						start = new byte[] { c.ReadByte(), c.ReadByte() },
						end = new byte[] { c.ReadByte(), c.ReadByte() },
						back = new ushort[_b]
					};
					for (var _c = 0; _c < _b; _c++)
						maps[_a].back[_c] = c.ReadUInt16();
					_b = c.ReadInt32();
					maps[_a].decoration = new dec()
					{
						t = new ushort[_b],
						d = new byte[_b, 2]
					};
					for (var _c = 0; _c < _b; _c++)
					{
						maps[_a].decoration.t[_c] = c.ReadUInt16();
						for (var _d = 0; _d < 2; _d++)
							maps[_a].decoration.d[_c, _d] = c.ReadByte();
					}
					maps[_a].pigs = new byte[_b = c.ReadInt32(), 6];
					for (var _c = 0; _c < _b; _c++)
						for (var _d = 0; _d < 4; _d++)
							maps[_a].pigs[_c, _d] = c.ReadByte();

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
				for (var _a = 0; _a < c; _a++) for (var _b = 0; _b < 8; _b++) b.Write(tmap[_a, _b]);
				b.Write(3);
				b.Write(width);
				b.Write(height);
				b.Write(c = maps.Length);
				for (var _a = 0; _a < c; _a++)
				{
					for (var _b = 0; _b < 2; _b++) b.Write(maps[_a].start[_b]);
					for (var _b = 0; _b < 2; _b++) b.Write(maps[_a].end[_b]);
					for (var _b = 0; _b < width * height; _b++) b.Write(maps[_a].back[_b]);
					b.Write(maps[_a].decoration.t.Length);
					for (var _b = 0; _b < maps[_a].decoration.t.Length; _b++)
					{
						b.Write(maps[_a].decoration.t[_b]);
						for (var _c = 0; _c < 2; _c++) b.Write(maps[_a].decoration.d[_b, _c]);
					}
					var _d = maps[_a].pigs.GetLength(0);
					b.Write(_d);
					for (var _b = 0; _b < _d; _b++)
					{
						for (var _c = 0; _c < 4; _c++)
						{
							b.Write(maps[_a].pigs[_b, _c]);
						}
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
					new Rectangle(
						(manual ? x * scale : (x * a)) + ((tmap[t, 4] - 120) * scale) + ((effects == SpriteEffects.FlipHorizontally ? tmap[t, 6] - 120 : 0) * scale),
						(manual ? y * scale : (y * a)) + ((tmap[t, 5] - 120) * scale) + ((effects == SpriteEffects.FlipHorizontally ? tmap[t, 7] - 120 : 0) * scale),
						b[0] * scale, b[1] * scale),
					new Rectangle(tmap[t, 1] * TexSi, tmap[t, 2] * TexSi, b[0], b[1]),
					Color.White, 0, Vector2.One, effects, 0);
			}
			public void drawMap(SpriteBatch sprite, SpriteFont font)
			{
				for (int _a = 0; _a < maps[currMap].back.Length; _a++)
				{
					int y = _a / width, x = _a - (y * width);
					_draw(sprite, (ushort)x, (ushort)y, maps[currMap].back[_a]);
				}
				_draw(sprite, maps[currMap].start[0], maps[currMap].start[1], Animations.DoorOpen[0]);
				_draw(sprite, maps[currMap].end[0], maps[currMap].end[1], Animations.DoorOpen[0]);
				for (var _a = 0; _a < maps[currMap].decoration.t.Length; _a++) _draw(sprite, maps[currMap].decoration.d[_a, 0], maps[currMap].decoration.d[_a, 1], maps[currMap].decoration.t[_a]);
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
			Map = content.maps[currMap];
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
			key[0] = kstate.IsKeyDown(Keys.F11);
			key[1] = kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W);
			key[2] = kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S);
			key[3] = kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A);
			key[4] = kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D);
			key[5] = kstate.IsKeyDown(Keys.Space);
			key[6] = kstate.IsKeyDown(Keys.E) || kstate.IsKeyDown(Keys.Enter);
			for (var _a = 0; _a < key.Length; _a++)
			{
				if (key[_a] != _key[_a] && key[_a])
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
			public ushort positionX;
			public ushort positionY;
			public float posX;
			public float posY;
			//public float desX;
			public float desY;
			public int state = 0;
			public int _state = -1;
			public bool view;
			public byte inputBlock = 0;
			public float blockRev = 0;
			public const double MSpeed = 0.04;
			public double start;
			public bool allowFall = true;
			public sbyte aniJump = -1;
			public Player()
			{
				positionX = 3;
				positionY = 4;
				posX = positionX * TexSi;
				posY = positionY * TexSi;
				view = true;
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
					_a[1] = (ushort)(--_a[1] * TexSi);
					if ((key[3] || key[4]) && key[4] != view) view = key[4];
					if (state == 0 || state == 1) state = key[3] || key[4] ? 1 : 0;
					if (key[3])
					{
						posX -= (float)(MSpeed * t.ElapsedGameTime.TotalMilliseconds * (1000 / speed));
						if (posX < _a[0]) posX = _a[0];
					}
					if (key[4])
					{
						posX += (float)(MSpeed * t.ElapsedGameTime.TotalMilliseconds * (1000 / speed));
						if (posX > _a[1]) posX = _a[1];
					}
				}
				if ((inputBlock & 2) == 0)
				{
					if (key[1] || key[2])
					{
						state = key[1] ? 3 : 4;
						inputBlock |= 2;
						blockRev = speed;
						if (key[1])
						{
							var _a = 1.4f;
							if (positionY - 1 >= 0 && Map.back[_map(positionX, (ushort)(positionY - 1))] <= 47) _a -= 0.4f;
							desY = ((float)positionY - _a) * TexSi;
							allowFall = false;
							aniJump = (sbyte)(positionY);
						}
					}
				}
				if ((inputBlock & 4) == 0)
				{
					if (key[5])
					{
						inputBlock = (byte)(inputBlock | 4);
						blockRev = speed;
						state = 2;
					}
				}
				if ((inputBlock & 8) == 0)
				{
					if (key[6])
					{
						var _a = Map.end[0] - (int)(posX / TexSi);
						if ((_a == 0 || _a == -1) && Map.end[1] - (int)(posY / TexSi) == -1)
						{

						}
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
					if ((posX - (TexSi * (int)(posX / TexSi))) != 0 && _drop < drop) drop = _drop;
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
					var __a = Map.back[_map(b, a++)];
					if (__a < 47) break;
				}
				return (ushort)(a - 2);
			}
			public ushort[] blockR(ushort b, ushort y)
			{
				var c = new ushort[] { 0, 0 };
				for (var _a = b; _a >= 0; _a--)
				{
					if (Map.back[_map(_a, y)] < 47)
					{
						c[0] = _a;
						break;
					}
				}
				for (var _a = b; _a < width; _a++)
				{
					if (Map.back[_map(_a, y)] < 47)
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
					case 1: frames = Animations.Run; break;
					case 2: frames = Animations.Attack; break;
					case 3: frames = Animations.Jump; break;
					case 4: frames = Animations.Fall; break;
					case 5: frames = Animations.Ground; break;
					case 6: frames = Animations.Hit; break;
					case 7: frames = Animations.Dead; break;
					case 8: frames = Animations.WalkOut; break;
					case 9: frames = Animations.WalkIn; break;
					default: frames = Animations.Idle; break;
				}
				if (_state != state)
				{
					start = t.TotalGameTime.TotalMilliseconds;
					_state = state;
				}
				var _a = blockR(positionX, positionY);
				content._draw(sprite, (ushort)posX, (ushort)posY,
					frames[(int)(((t.TotalGameTime.TotalMilliseconds - start) / (speed / frames.Length)) % frames.Length)],
					true, view ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
				sprite.DrawString(font, positionX + ", " + positionY + '\n' + Map.back[_map(positionX, positionY)].ToString() + "\n" + _a[0].ToString() + ", " + _a[1].ToString(), new Vector2(posX * scale, posY * scale), Color.Aqua);
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

			content.drawMap(sprite, font);
			player.draw(gameTime, sprite, font);

			sprite.DrawString(font, logs, new Vector2(10, 10), Color.White);
			sprite.End();
			fps[1]++;
			_fps_f++;
			base.Draw(gameTime);
		}
	}
}
