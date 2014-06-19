using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace CubeDemo
{
    public class Cube
    {
        public IndexBuffer IndexBuffer { get; private set; }
        public VertexBuffer VertexBuffer { get; private set; }

        public int IndicesLength { get; private set; }
        public int VerticesLength { get; private set; }

        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[24];
        int[] indices = new int[36];

        private Texture2D texture;
        public Texture2D Texture
        {
            get { return texture; }
        }

        public Cube(GraphicsDevice device, Vector3 pos, Vector3 size, Texture2D texture)
        {
            this.texture = texture;

            Vector3 s = size * 0.5f;
            Quad[] quads = new Quad[6];

            // front
            quads[0] = new Quad(pos + s * new Vector3(-1, 1, 1), pos + s * new Vector3(1, 1, 1), pos + s * new Vector3(-1, -1, 1), pos + s * new Vector3(1, -1, 1));
            // top
            quads[1] = new Quad(pos + s * new Vector3(-1, 1, -1), pos + s * new Vector3(1, 1, -1), pos + s * new Vector3(-1, 1, 1), pos + s * new Vector3(1, 1, 1));
            // right
            quads[2] = new Quad(pos + s * new Vector3(1, 1, 1), pos + s * new Vector3(1, 1, -1), pos + s * new Vector3(1, -1, 1), pos + s * new Vector3(1, -1, -1));
            // bottom
            quads[3] = new Quad(pos + s * new Vector3(-1, -1, 1), pos + s * new Vector3(1, -1, 1), pos + s * new Vector3(-1, -1, -1), pos + s * new Vector3(1, -1, -1));
            // left
            quads[4] = new Quad(pos + s * new Vector3(-1, 1, -1), pos + s * new Vector3(-1, 1, 1), pos + s * new Vector3(-1, -1, -1), pos + s * new Vector3(-1, -1, 1));
            // back
            quads[5] = new Quad(pos + s * new Vector3(1, 1, -1), pos + s * new Vector3(-1, 1, -1), pos + s * new Vector3(1, -1, -1), pos + s * new Vector3(-1, -1, -1));

            int vlen = quads[0].Vertices.Length;
            int ilen = quads[0].Indices.Length;

            for (int q = 0; q < 6; q++)
            {
                for (int v = 0; v < vlen; v++)
                {
                    vertices[vlen * q + v] = quads[q].Vertices[v];
                }
                for (int i = 0; i < ilen; i++)
                {
                    indices[ilen * q + i] = q * vlen + quads[q].Indices[i];
                }
            }

            IndicesLength = indices.Length;
            VerticesLength = vertices.Length;

            IndexBuffer = new IndexBuffer(device, typeof(int), IndicesLength, BufferUsage.WriteOnly);
            VertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, VerticesLength, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);
            IndexBuffer.SetData(indices);
        }
    }
}
