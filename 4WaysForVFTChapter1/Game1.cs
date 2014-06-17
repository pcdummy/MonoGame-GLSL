#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace FourWaysForVFTChapter1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager content;
        Grid grid;
        
        Effect gridEffect;
        Texture2D displacementTexture, sandTexture, grassTexture, rockTexture, snowTexture;

        // FPS Calc.
        private DateTime _last = DateTime.Now;
        private int _fps;

        // Camera Stuff
        private Camera camera;
        private int _lastMouseX, _lastMouseY;

        private bool keyTest = false, wire = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);

            Window.AllowUserResizing = true;
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            grid = new Grid(graphics.GraphicsDevice);
            grid.CellSize = 4;
            grid.Dimension = 128;

            this.camera = new Camera(new Vector3(414.852f, 6672.64f, 586.15f), new Vector3(-0.7220212f, 0.4439481f, -0.5306558f));
            this.camera.Right = new Vector3(-0.5922151f, -1.225318E-07f, 0.80578f);
            this.camera.UpVector = new Vector3(0.3577244f, 0.8960525f, 0.2629128f);
            Camera.DefaultCamera = camera;

            MouseState currentMouseState = Mouse.GetState();
            _lastMouseX = currentMouseState.X;
            _lastMouseY = currentMouseState.Y;
            camera.Update();

            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
		protected override void LoadContent()
        {
            grid.LoadGraphicsContent();
			// gridEffect = content.Load<Effect>("Shaders\\VTFDisplacement");
			gridEffect = new Effect (
				graphicsDevice: GraphicsDevice,
                effectCode: File.ReadAllText ("Content/Shaders/GridTerrain.glfx"),
				effectName: "GridTerrain"
			);

            displacementTexture = content.Load<Texture2D>("Content/Textures/heightmap.png");
            sandTexture = content.Load<Texture2D>("Content/Textures/sand.png");
            grassTexture = content.Load<Texture2D>("Content/Textures/grass.png");
            rockTexture = content.Load<Texture2D>("Content/Textures/rock.png");
            snowTexture = content.Load<Texture2D>("Content/Textures/snow.png");
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            Camera.ElapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState currentKeyboardState = Keyboard.GetState();

            float distance;
            distance = (currentKeyboardState.IsKeyDown(Keys.LeftShift) ? 35 : (currentKeyboardState.IsKeyDown(Keys.LeftAlt) ? 0.1f : 1));

            if (currentKeyboardState.IsKeyDown(Keys.W))
                camera.Walk(-960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.S))
                camera.Walk(960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.A))
                camera.Strafe(-960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.D))
                camera.Strafe(960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.F))
                camera.Fly(960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.L))
                camera.Land(960 * distance);

            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();
            if (currentKeyboardState.IsKeyDown(Keys.Q))
            {
                if (!keyTest)
                {
                    if (!wire)
                    {
                        graphics.GraphicsDevice.RasterizerState = new RasterizerState { 
                            FillMode = FillMode.WireFrame, 
                            CullMode = CullMode.None
                        };
                        wire = true;
                    }
                    else
                    {
                        graphics.GraphicsDevice.RasterizerState = new RasterizerState { 
                            FillMode = FillMode.Solid, 
                            CullMode = CullMode.None
                        };
                        wire = false;
                    }
                }
                keyTest = true;
            }
            else
            {
                keyTest = false;
            }

            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                camera.RotateByMouse(new Vector3(_lastMouseX - currentMouseState.X, currentMouseState.Y - _lastMouseY, 0));

            }
            _lastMouseX = currentMouseState.X;
            _lastMouseY = currentMouseState.Y;
            camera.Update();

            if ((DateTime.Now - _last).TotalMilliseconds >= 1000)
            {
                this.Window.Title = string.Concat(this._fps + " fps, cam : "+ camera.Position.X+" "+camera.Position.Y+" "+camera.Position.Z + " keys : "+Keyboard.GetState().ToString());
                _fps = 0;
                this._last = DateTime.Now;
            }
            else
                _fps++;

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            gridEffect.Parameters["Heightmap"].SetValue(displacementTexture);

            gridEffect.Parameters["SandSampler"].SetValue(sandTexture);
            gridEffect.Parameters["GrassSampler"].SetValue(grassTexture);
            gridEffect.Parameters["RockSampler"].SetValue(rockTexture);
            gridEffect.Parameters["SnowSampler"].SetValue(snowTexture);

            gridEffect.Parameters["World"].SetValue(Matrix.Identity);
            gridEffect.Parameters["View"].SetValue(camera.View);
            gridEffect.Parameters["Projection"].SetValue(camera.Projection);
            gridEffect.Parameters["MaxHeight"].SetValue(128f);
            gridEffect.Parameters["TexelSize"].SetValue(1.0f / 256.0f);
            gridEffect.Parameters["TextureSize"].SetValue(256.0f);


			gridEffect.CurrentTechnique.Passes [0].Apply ();
            foreach (EffectPass pass in gridEffect.CurrentTechnique.Passes)
            {
				pass.Apply();
                grid.Draw();
				pass.Apply();
            }
			gridEffect.CurrentTechnique.Passes [0].Apply ();

            base.Draw(gameTime);
        }
    }
}
