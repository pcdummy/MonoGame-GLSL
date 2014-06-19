using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System.ComponentModel;
using System.Globalization;

namespace BilLODTerrain
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {

        #region Fields

        private BackgroundWorker _worker;
		private QuadTree _terrain;
        private GraphicsDeviceManager _graphics;
        private Camera _camera;
        private int _lastMouseX, _lastMouseY;
        private bool _keyTest = false, _wire = false;
        private DateTime _last = DateTime.Now;
        private int _fps;

        #endregion


        #region Constructors

        public Game()
        {
            this._graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.Window.AllowUserResizing = true;
            this.IsMouseVisible = true;

            this._graphics.DeviceReset += _graphics_DeviceReset;

			this.Exiting += Game_Exiting;

            this.InitializeThread();
            this.InitializeTerrain();
        }

        void _graphics_DeviceReset(object sender, EventArgs e)
        {
			this._graphics.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
        }

        private void InitializeTerrain()
        {
            //set the depth of the tree
			byte treeDepth = 8;
            //set the size of the terrain part represented by the root quad tree node. (width + height)
            int landSize = (int)25599;
            //create a new quadtree with the specified depth, land size and at location (0,0)
			this._terrain = new QuadTree(treeDepth, landSize, Vector2.Zero);
        }

        private void InitializeThread()
        {
            this._worker = new BackgroundWorker();
            this._worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            this._worker.WorkerSupportsCancellation = true;
        }

        #endregion


        #region Terrain asynchronous management

        void Game_Exiting(object sender, EventArgs e)
        {
            this._worker.CancelAsync();
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_worker.CancellationPending)
            {
				Thread.Sleep(1000);
                this._terrain.Update(null);
            }
        }

        #endregion


        #region Game Life

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsFixedTimeStep = false;

            this.InitializeCamera();

 
            // TODO: Add your initialization logic here
            float quadFrontTreeDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[Properties.Resources.ChildFrontTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float quadFarTreeDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[Properties.Resources.ChildFarTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float vertexDetail = float.Parse(System.Configuration.ConfigurationManager.AppSettings[Properties.Resources.VertexTestThreshold], CultureInfo.GetCultureInfo("en-us"));
            float nodeRelevance = float.Parse(System.Configuration.ConfigurationManager.AppSettings[Properties.Resources.ChildRelevanceThreshold], CultureInfo.GetCultureInfo("en-us"));

            this._terrain.NodeRelevance = nodeRelevance;
            this._terrain.QuadTreeDetailAtFront = quadFrontTreeDetail;
            this._terrain.QuadTreeDetailAtFar = quadFarTreeDetail;
            this._terrain.VertexDetail = vertexDetail;
            this.LoadGround(this._terrain);

            this._terrain.Initialize();
            this._terrain.Load(this._graphics.GraphicsDevice);

            var texture = BitmapUtil.GetTexture2DFromFile (_graphics.GraphicsDevice, "Content/tile-47.png");
            this._terrain.Effect.Texture = texture;

            this._worker.RunWorkerAsync();

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            this._graphics.GraphicsDevice.RasterizerState = rs;

            base.Initialize();
        }

        private void InitializeCamera()
        {
            this._camera = new Camera(new Vector3(414.852f, 6672.64f, 586.15f), new Vector3(-0.7220212f, 0.4439481f, -0.5306558f));
            this._camera.Right = new Vector3(-0.5922151f, -1.225318E-07f, 0.80578f);
            this._camera.UpVector = new Vector3(0.3577244f, 0.8960525f, 0.2629128f);
            Camera.DefaultCamera = _camera;

            MouseState currentMouseState = Mouse.GetState();
            _lastMouseX = currentMouseState.X;
            _lastMouseY = currentMouseState.Y;
            _camera.Update();
        }

        #endregion


        #region Heights

        /// <summary>
        /// <para>Loads the ground with the asset named "heightmap".</para>
        /// </summary>
        /// <param name="content"></param>
        public void LoadGround(QuadTree tree)
        {
            string heightMapTextureName = System.Configuration.ConfigurationManager.AppSettings[Properties.Resources.HeightGrayScaleImage];
			using (var heightMap = BitmapUtil.GetTexture2DFromFile(_graphics.GraphicsDevice, "Content/winteress_heightmap.bmp"))
            {
                LoadHeightData(tree, heightMap);
            }
        }

        private void LoadHeightData(QuadTree tree, Texture2D heightMap)
        {
            Color[] heightMapColors = new Color[heightMap.Width * heightMap.Height];
            heightMap.GetData(heightMapColors);

            tree.HeightData = new float[heightMap.Width, heightMap.Height];
            for (int x = 0; x < heightMap.Width; x++)
            {
                for (int y = 0; y < heightMap.Height; y++)
                {
                    tree.HeightData [x, y] = heightMapColors [x + y * heightMap.Width].R * 8f;
                }
            }
        }


        #endregion


        #region Update and show


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            Camera.ElapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState currentKeyboardState = Keyboard.GetState();

            var distance = (currentKeyboardState.IsKeyDown(Keys.LeftShift) ? 35 : (currentKeyboardState.IsKeyDown(Keys.LeftAlt) ? 0.1f : 1));

            if (currentKeyboardState.IsKeyDown(Keys.W))
				_camera.Walk(-960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.S))
				_camera.Walk(960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.A))
				_camera.Strafe(-960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.D))
				_camera.Strafe(960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.F))
				_camera.Fly(960 * distance);
            if (currentKeyboardState.IsKeyDown(Keys.L))
				_camera.Land(960 * distance);

            if (currentKeyboardState.IsKeyDown(Keys.PageUp))
                if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
                    this._terrain.QuadTreeDetailAtFront *= 1.01f;
                else
                    this._terrain.QuadTreeDetailAtFar *= 1.01f;

            if (currentKeyboardState.IsKeyDown(Keys.PageDown))
                if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
                    this._terrain.QuadTreeDetailAtFront *= 0.99f;
                else
                    this._terrain.QuadTreeDetailAtFar *= 0.99f;
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();
            if (currentKeyboardState.IsKeyDown(Keys.Q))
            {
				if (!_keyTest)
				{
					if (!_wire)
					{
						_graphics.GraphicsDevice.RasterizerState = new RasterizerState { 
							FillMode = FillMode.WireFrame, 
							CullMode = _graphics.GraphicsDevice.RasterizerState.CullMode
						};
						_wire = true;
					}
					else
					{
						_graphics.GraphicsDevice.RasterizerState = new RasterizerState { 
							FillMode = FillMode.Solid, 
							CullMode = _graphics.GraphicsDevice.RasterizerState.CullMode
						};
						_wire = false;
					}
                }
                _keyTest = true;
            }
            else
            {
                _keyTest = false;
            }

            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                _camera.RotateByMouse(new Vector3(_lastMouseX - currentMouseState.X, currentMouseState.Y - _lastMouseY, 0));

            }
            _lastMouseX = currentMouseState.X;
            _lastMouseY = currentMouseState.Y;
            _camera.Update();

            if ((DateTime.Now - _last).TotalMilliseconds >= 1000)
            {
                this.Window.Title = string.Concat(this._fps + " fps, cam : "+_camera.Position.X+" "+_camera.Position.Y+" "+_camera.Position.Z + " keys : "+Keyboard.GetState().ToString());
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
            this._graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            this._terrain.Render();
 
            base.Draw(gameTime);
        }

        #endregion
    }
}
