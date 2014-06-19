using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BilLODTerrain
{
    public class QuadTree : QuadNodeCollection, IDisposable
    {

        #region Fields

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public List<VertexPositionNormalTexture> Vertices;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public List<int> Indices;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BasicEffect _effect;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _size;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _heightFieldSpace;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private byte _depth;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private GraphicsDevice _device;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Vector2 _location;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _vertexDetail = 17f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _quadTreeDetailAtFront = 10000000f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _quadTreeDetailAtFar = 10000000f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float _nodeRelevance = 0.1f;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _loadUpdateOccurences = 10;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BuffersData _currentBufferData;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		private List<BuffersData> _disposeDatas;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		private List<BuffersData> _lastLoadedDatas;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _processIterationId;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private float[,] _heightData;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _minimalDepth;

        #endregion


        #region Properties

        /// <summary>
        /// <para>Gets the max number of children that a node can have.</para>
        /// </summary>
        public const int NodeChildsNumber = 4;

        /// <summary>
        /// <para>Gets the max number of children that a node can have.</para>
        /// </summary>
        internal const int ProcessIterationMaxValue = 256;

        /// <summary>
        /// <para>Gets the current minimal depth upond with node childs are not checked and automatically validated.</para>
        /// </summary>
        internal int MinimalDepth
        {
            get
            {
                return this._minimalDepth;
            }
        }

        /// <summary>
        /// <para>Internal value used to identify the update phase and optimize vertex array searching.</para>
        /// </summary>
        internal int ProcessIterationId
        {
            get
            {
                return this._processIterationId;
            }
        }

        /// <summary>
        /// <para>Gets or sets the heights array.</para>
        /// </summary>
        public float[,] HeightData
        {
            get
            {
                return this._heightData;
            }
            set
            {
                this._heightData = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold relevance for nodes.</para>
        /// </summary>
        public float NodeRelevance
        {
            get
            {
                return this._nodeRelevance;
            }
            set
            {
                this._nodeRelevance = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold for the vertices.</para>
        /// </summary>
        public float VertexDetail
        {
            get
            {
                return this._vertexDetail;
            }
            set
            {
                this._vertexDetail = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold for the QuadTree.</para>
        /// </summary>
        public float QuadTreeDetailAtFront
        {
            get
            {
                return this._quadTreeDetailAtFront;
            }
            set
            {
                this._quadTreeDetailAtFront = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the detail threshold for the QuadTree.</para>
        /// </summary>
        public float QuadTreeDetailAtFar
        {
            get
            {
                return this._quadTreeDetailAtFar;
            }
            set
            {
                this._quadTreeDetailAtFar = value;
            }
        }

        public BasicEffect Effect
        {
            get
            {
                return this._effect;
            }
        }

        public GraphicsDevice Device
        {
            get
            {
                return this._device;
            }
        }

        /// <summary>
        /// <para>Gets the location of the current tree.</para>
        /// </summary>
        public Vector2 Location
        {
            get
            {
                return this._location;
            }
        }

        /// <summary>
        /// <para>Gets the depth of the current tree.</para>
        /// </summary>
        public int Depth
        {
            get
            {
                return this._depth;
            }
        }

        /// <summary>
        /// <para>Gets the minimal distance between to vertex for the leaf node.</para>
        /// </summary>
        public float HeightFieldSpace
        {
            get
            {
                return this._heightFieldSpace;
            }
        }

        /// <summary>
        /// <para>Gets the size of the tree.</para>
        /// </summary>
        /// <remarks>A tree is a square.</remarks>
        public int Size
        {
            get
            {
                return this._size;
            }
        }

        public override int ChildNumber
        {
            get
            {
                return 1;
            }
        }

        #endregion


        #region Constructors

        public QuadTree(byte depth, int size, Vector2 location) 
        {
            this.ChildNumber = 1;
            this._location = location;
            this._depth = depth;
            this._size = size;
            this._heightFieldSpace = (float)(size / System.Math.Pow(2, depth-1));
			this._disposeDatas = new List<BuffersData>();
			this._lastLoadedDatas = new List<BuffersData>();
			this.Vertices = new List<VertexPositionNormalTexture>(1000);
			this.Indices = new List<int>(1000);
        }

        #endregion


        #region 3D


        public void Initialize()
        {
            this.Childs[0] = new QuadNode(null, NodeChild.NorthEast);
            this.Childs[0].Location = this.Location;
            this.Childs[0].ParentTree = this;
            this.Childs[0].Initialize();
        }

        public void Load(GraphicsDevice device)
        {
            this._device = device;
            this._effect = new BasicEffect(device);
            this._effect.EnableDefaultLighting();
            //device.DeviceReset += new EventHandler(device_DeviceReset);
           
            this._effect.LightingEnabled = false;
            this._effect.VertexColorEnabled = false;
            this.Effect.World = Matrix.Identity;
            this.Effect.TextureEnabled = true;
//            this.Effect.FogEnabled = true;
//            this.Effect.FogStart = 0;
//            this.Effect.FogEnd = 3000;
//            this.Effect.FogColor = new Vector3(107f / 255, 109f / 255, 57f / 255);
//            this.Effect.FogColor = new Vector3(148f / 255, 186f / 255, 247f / 255);

            this._minimalDepth = 4;

            for (int i = 0; i < this._loadUpdateOccurences; i++)
                UpdateQuadVertices();

            this._minimalDepth = 1;

            this.BuildQuadVerticesList();

            this._currentBufferData = this._lastLoadedDatas[0];

            //this._vertexDeclaration = new VertexDeclaration(null);
            //this.Device.VertexDeclaration = this._vertexDeclaration;
            //this.Device.Vertices[0].SetSource(this._currentBufferData.VertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);

            this.Device.Indices = this._currentBufferData.IndexBuffer;
            this.Effect.Projection = Camera.DefaultCamera.Projection;
        }

        void device_DeviceReset(object sender, EventArgs e)
        {
            //this.Device.`..VertexDeclaration = this._vertexDeclaration;
            //this.Device.Vertices[0].SetSource(this._currentBufferData.VertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);

            this.Device.Indices = this._currentBufferData.IndexBuffer;
        }

        public void Update(GameTime time)
        {
			//this._effect.FogEnabled = false;IT            
            //return;
            this._processIterationId += 1;
            if (this._processIterationId > ProcessIterationMaxValue)
                this._processIterationId = 0;

            this.UpdateQuadVertices();
            this.BuildQuadVerticesList();
            this.ClearLastLoaded();
        }

        private void ClearLastLoaded()
        {
            while (this._disposeDatas.Count > 0)
            {
                BuffersData data = this._disposeDatas[0];
                this._disposeDatas.RemoveAt(0);
                if (data.VertexBuffer != null) { 
                    data.VertexBuffer.Dispose();
                    data.VertexBuffer = null;
                }
                if (data.IndexBuffer != null)
                {
                    data.IndexBuffer.Dispose();
                    data.IndexBuffer = null;
                }
            }
        }

        /// <summary>
        /// <para>First step in quad tree update.</para>
        /// <para>This method update the quads of each</para>
        /// </summary>
        private void UpdateQuadVertices()
        {
            for (int i = 0; i < this.Childs.Length; i++)
                this.Childs[i].Update();
        }

        /// <summary>
        /// <para>Second stepin quad tree update.</para>
        /// <para>This method get all enabled vertices for all sub quad node and build two lists of vertices and and indices.</para>
        /// </summary>
        private void BuildQuadVerticesList()
        {
			this.Vertices.Clear ();
			this.Vertices = new List<VertexPositionNormalTexture> (1000);
			this.Indices.Clear ();
			this.Indices = new List<int> (1000);

            for (int i = 0; i < this.Childs.Length; i++)
                this.Childs[i].GetEnabledVertices();

            /*if (Indices.Count == 0)
                return;*/

            if (!this.Device.IsDisposed)
            {
                IndexBuffer indexBuffer;
                VertexBuffer vertexBuffer;

                vertexBuffer = new VertexBuffer(this.Device, typeof(VertexPositionNormalTexture), Vertices.Count, BufferUsage.WriteOnly);
                vertexBuffer.SetData<VertexPositionNormalTexture>(Vertices.ToArray());

                indexBuffer = new IndexBuffer(this.Device, typeof(int), Indices.Count, BufferUsage.WriteOnly);
                indexBuffer.SetData<int>(Indices.ToArray());

                BuffersData data = new BuffersData();
                data.IndexBuffer = indexBuffer;
                data.VertexBuffer = vertexBuffer;
                data.NumberOfIndices = Indices.Count / 3;
                data.NumberOfVertices = Vertices.Count;

                this._lastLoadedDatas.Add(data);
            }
         }

        public void Render()
        {

            if (_lastLoadedDatas.Count > 0)
            {
                this._disposeDatas.Add(this._currentBufferData);
                this._currentBufferData = _lastLoadedDatas[0];
                //this.Device.Vertices[0].SetSource(this._currentBufferData.VertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);
                this.Device.Indices = this._currentBufferData.IndexBuffer;
                _lastLoadedDatas.RemoveAt(0);
            }
            else
            {
                this.Device.SetVertexBuffer(this._currentBufferData.VertexBuffer);
                this.Device.Indices = this._currentBufferData.IndexBuffer;
            }


            this.Effect.View = Camera.DefaultCamera.View;

            this.Effect.CurrentTechnique.Passes[0].Apply();

//            foreach (EffectPass pass in this.Effect.CurrentTechnique.Passes)
//            {
//                pass.Apply();
                this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this._currentBufferData.NumberOfVertices, 0, this._currentBufferData.NumberOfIndices);
//                pass.Apply();
//            }

            this.Effect.CurrentTechnique.Passes[0].Apply();
        }


        #endregion


        #region Public methods

        /// <summary>
        /// <para>Gets the height of the ground at the specified position.</para>
        /// </summary>
        public float GetHeight(float x, float y)
        {
			float x2 = Math.Abs((x / 128f) % 200);
			float y2 = Math.Abs((y / 128f) % 200);

            return _heightData[(int)x2, (int)y2];//
        }

        /// <summary>
        /// <para>Gets the sub node size at the specified level depth.</para>
        /// </summary>
        public float GetNodeSizeAtLevel(int depth)
        {
            int diff = (int)((this.Depth-1) - depth);
            double result = System.Math.Pow(2, diff);
			return this._heightFieldSpace * (float)result;
        }

        #endregion


        #region IDisposable Members

        public override void Dispose()
        {
            base.Dispose();

            for(int i = 0 ; i < this._disposeDatas.Count ; i++)
            {
                BuffersData data = this._disposeDatas[i];
                data.VertexBuffer.Dispose();
                data.VertexBuffer = null;
                data.IndexBuffer.Dispose();
                data.IndexBuffer = null;
            }
            this._disposeDatas.Clear();
            this._disposeDatas = null;

            for (int i = 0; i < this._lastLoadedDatas.Count; i++)
            {
                BuffersData data = this._lastLoadedDatas[i];
                data.VertexBuffer.Dispose();
                data.VertexBuffer = null;
                data.IndexBuffer.Dispose();
                data.IndexBuffer = null;
                this._lastLoadedDatas.RemoveAt(0);
            }
            this._lastLoadedDatas.Clear();
            this._lastLoadedDatas = null;

        }

        #endregion

    }

}
