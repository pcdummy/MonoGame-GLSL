#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FourWaysForVFTChapter1
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Grid
    {
        private GraphicsDevice device;
        private VertexBuffer vb;
        private IndexBuffer ib;
        private VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
        private short dimension = 128;
        private int[] indices;

        private float cellSize = 4;


        public Grid(GraphicsDevice device)
        {
            this.device = device;
        }

        public float CellSize
        {
            get { return cellSize; }
            set { cellSize = value; }
        }

        public short Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }


        public void GenerateStructures()
        {

            vertices = new VertexPositionNormalTexture[(dimension + 1) * (dimension + 1)];
            indices = new int[dimension * dimension * 6];
            for (int i = 0; i < dimension + 1; i++)
            {
                for (int j = 0; j < dimension + 1; j++)
                {
                    VertexPositionNormalTexture vert = new VertexPositionNormalTexture();
                    vert.Position = new Vector3((i - dimension / 2.0f) * cellSize, 0, (j - dimension / 2.0f) * cellSize);
                    vert.Normal = Vector3.Up;
                    vert.TextureCoordinate = new Vector2((float)i / dimension, (float)j / dimension);
                    vertices[i * (dimension + 1) + j] = vert;
                }
            }

            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    indices[6 * (i * dimension + j)] = (i * (dimension + 1) + j);
                    indices[6 * (i * dimension + j) + 1] = (i * (dimension + 1) + j + 1);
                    indices[6 * (i * dimension + j) + 2] = ((i + 1) * (dimension + 1) + j + 1);

                    indices[6 * (i * dimension + j) + 3] = (i * (dimension + 1) + j);
                    indices[6 * (i * dimension + j) + 4] = ((i + 1) * (dimension + 1) + j + 1);
                    indices[6 * (i * dimension + j) + 5] = ((i + 1) * (dimension + 1) + j);
                }
            }
        }

        public void Draw()
        {
			device.SetVertexBuffer(vb);
            device.Indices = ib;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (dimension + 1) * (dimension + 1), 0, dimension * dimension * 6);
        }

        public void LoadGraphicsContent()
        {
            GenerateStructures();

			vb = new VertexBuffer (device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
			ib = new IndexBuffer(device, typeof(int), 6 * dimension * dimension, BufferUsage.WriteOnly);
            vb.SetData<VertexPositionNormalTexture>(vertices);
            ib.SetData<int>(indices);
        }
    }
}


