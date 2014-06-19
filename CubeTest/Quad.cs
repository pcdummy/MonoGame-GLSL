using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeDemo
{
    public class Quad
    {
        VertexPositionNormalTexture[] vertices;
        short[] indices;
        Vector3 normal;
        Vector3 upperLeft;
        Vector3 upperRight;
        Vector3 lowerLeft;
        Vector3 lowerRight;

        public VertexPositionNormalTexture[] Vertices
        {
            get { return vertices; }
        }

        public short[] Indices
        {
            get { return indices; }
        }

        public Quad(Vector3 ul, Vector3 ur, Vector3 ll, Vector3 lr)
        {
            vertices = new VertexPositionNormalTexture[4];
            indices = new short[6];
            normal = Vector3.Cross(ul - ur, lr - ur);
            normal.Normalize();

            upperLeft = ul;
            upperRight = ur;
            lowerLeft = ll;
            lowerRight = lr;

            FillVertices();
        }

        private void FillVertices()
        {
            // Fill in texture coordinates to display full texture
            // on quad
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);

            // Provide a normal for each vertex
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal = normal;
            }

            // Set the position and texture coordinate for each
            // vertex
            vertices[0].Position = upperLeft;
            vertices[0].TextureCoordinate = textureUpperLeft;

            vertices[1].Position = upperRight;
            vertices[1].TextureCoordinate = textureUpperRight;

            vertices[2].Position = lowerLeft;
            vertices[2].TextureCoordinate = textureLowerLeft;

            vertices[3].Position = lowerRight;
            vertices[3].TextureCoordinate = textureLowerRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 3;
        }
    }
}
