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

namespace CubeDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private Camera camera;
        private int lastMouseX, lastMouseY;

        // FPS Calc.
        private DateTime _last = DateTime.Now;
        private int _fps;

        private bool keyTest = false, wire = false;

        BasicEffect basicEffect;

        Cube simpleCube;

        public Game1()
        {
            new GraphicsDeviceManager(this);

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
            camera = new Camera(new Vector3(-1052322f, 380897.8f, -11846.68f), new Vector3(-0.7220212f, 0.4439481f, -0.5306558f));
            camera.Right = new Vector3(-0.5922151f, -1.225318E-07f, 0.80578f);
            camera.Right = new Vector3(-0.05377903f, -2.466447E-07f, 0.988522f);
            camera.UpVector = new Vector3(0.3577244f, 0.8960525f, 0.2629128f);
            Camera.DefaultCamera = camera;

            MouseState currentMouseState = Mouse.GetState();
            lastMouseX = currentMouseState.X;
            lastMouseY = currentMouseState.Y;
            camera.Update();

            GraphicsDevice.RasterizerState = new RasterizerState { 
                FillMode = FillMode.Solid, 
                CullMode = CullMode.None
            };

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
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;

            var cubeTexture = Content.Load<Texture2D>("Content\\Textures\\SOTCTexture");
            basicEffect.Texture = cubeTexture;

            simpleCube = new Cube(GraphicsDevice, new Vector3(0), new Vector3(500000), cubeTexture);
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
                Exit();

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
                        GraphicsDevice.RasterizerState = new RasterizerState { 
                            FillMode = FillMode.WireFrame, 
                            CullMode = CullMode.None
                        };
                        wire = true;
                    }
                    else
                    {
                        GraphicsDevice.RasterizerState = new RasterizerState { 
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
                camera.RotateByMouse(new Vector3(lastMouseX - currentMouseState.X, currentMouseState.Y - lastMouseY, 0));

            }
            lastMouseX = currentMouseState.X;
            lastMouseY = currentMouseState.Y;
            camera.Update();

            if ((DateTime.Now - _last).TotalMilliseconds >= 1000)
            {
                Window.Title = string.Concat(_fps + " fps, cam : "+ camera.Position.X+" "+camera.Position.Y+" "+camera.Position.Z + " keys : "+Keyboard.GetState().ToString());
                Window.Title = string.Format("{0} fps, cam: {1},{2},{3}, cam right: {4},{5},{6}, cam up: {7},{8},{9}", _fps, camera.Position.X, camera.Position.Y, camera.Position.Z, camera.Right.X, camera.Right.Y, camera.Right.Z, camera.UpVector.X, camera.UpVector.Y, camera.UpVector.Z);
                _fps = 0;
                _last = DateTime.Now;
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.Indices = simpleCube.IndexBuffer;
            GraphicsDevice.SetVertexBuffer(simpleCube.VertexBuffer);

            basicEffect.View = camera.View;
            basicEffect.World = Matrix.Identity;
            basicEffect.Projection = camera.Projection;

            basicEffect.CurrentTechnique.Passes [0].Apply ();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, simpleCube.VerticesLength, 0, simpleCube.IndicesLength);

            base.Draw(gameTime);
        }
    }
}
