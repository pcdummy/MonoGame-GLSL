using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VertexExample
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const string TEXTURE_NAME = "Green-gel-x";  // http://upload.wikimedia.org/wikipedia/commons/9/99/Green-gel-x.png
        const int TOP_LEFT = 0;
        const int TOP_RIGHT = 1;
        const int BOTTOM_RIGHT = 2;
        const int BOTTOM_LEFT = 3;
        RasterizerState WIREFRAME_RASTERIZER_STATE = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

        GraphicsDeviceManager graphics;
        BasicEffect effect;
        Texture2D texture;
        VertexPositionColorTexture[] vertexData;
        int[] indexData;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            effect = new BasicEffect(graphics.GraphicsDevice);

            SetUpVertices(Color.White);
            SetUpCamera();
            SetUpIndices();

            base.Initialize();
        }

        private void SetUpVertices(Color color)
        {
            const float HALF_SIDE = 200.0f;
            const float Z = 0.0f;

            vertexData = new VertexPositionColorTexture[4];
            vertexData[TOP_LEFT] = new VertexPositionColorTexture(new Vector3(-HALF_SIDE, HALF_SIDE, Z), color, new Vector2(0, 0));
            vertexData[TOP_RIGHT] = new VertexPositionColorTexture(new Vector3(HALF_SIDE, HALF_SIDE, Z), color, new Vector2(1, 0));
            vertexData[BOTTOM_RIGHT] = new VertexPositionColorTexture(new Vector3(HALF_SIDE, -HALF_SIDE, Z), color, new Vector2(1, 1));
            vertexData[BOTTOM_LEFT] = new VertexPositionColorTexture(new Vector3(-HALF_SIDE, -HALF_SIDE, Z), color, new Vector2(0, 1));
        }

        private void SetUpIndices()
        {
            indexData = new int[6];
            indexData[0] = TOP_LEFT;
            indexData[1] = BOTTOM_RIGHT;
            indexData[2] = BOTTOM_LEFT;

            indexData[3] = TOP_LEFT;
            indexData[4] = TOP_RIGHT;
            indexData[5] = BOTTOM_RIGHT;
        }

        private void SetUpCamera()
        {
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreateOrthographic(Window.ClientBounds.Width, Window.ClientBounds.Height, -1.0f, 1.0f);
        }

        protected override void LoadContent()
        {
            texture = Content.Load<Texture2D>(TEXTURE_NAME);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw textured box
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;  // vertex order doesn't matter
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;    // use alpha blending
            GraphicsDevice.DepthStencilState = DepthStencilState.None;  // don't bother with the depth/stencil buffer

            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;
            effect.Texture = texture;
            effect.TextureEnabled = true;
            effect.DiffuseColor = Color.White.ToVector3();
            effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, 4, indexData, 0, 2);

            // Draw wireframe box
            GraphicsDevice.RasterizerState = WIREFRAME_RASTERIZER_STATE;    // draw in wireframe
            GraphicsDevice.BlendState = BlendState.Opaque;                  // no alpha this time

            effect.TextureEnabled = false;
            effect.DiffuseColor = Color.Black.ToVector3();
            effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, 4, indexData, 0, 2);

            base.Draw(gameTime);
        }
    }
}