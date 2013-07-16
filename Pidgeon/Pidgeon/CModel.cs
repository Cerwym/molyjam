using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pidgeon
{
    public class CModel
    {
        public bool Lighting;
        public Model Model;
        public Matrix World;

        public Matrix Translation;

        public List<BoundingBox> boundingBoxes;

        protected BasicEffect boxEffect;

        public float Width;

        public CModel(Model model, GraphicsDevice graphicsDevice, Matrix world, BasicEffect effect)
        {
            Lighting = true;
            World = world;
            Model = model;

            UpdateCollisions();

            boxEffect = effect;
        }
        public void UpdateCollisions()
        {
            boundingBoxes = new List<BoundingBox>();
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix meshTransform = transforms[mesh.ParentBone.Index];
                boundingBoxes.Add(BuildBoundingBox(mesh, meshTransform * World));
            }
        }
        protected BoundingBox BuildBoundingBox(ModelMesh mesh, Matrix meshTransform)
        {
            // Create initial variables to hold min and max xyz values for the mesh
            Vector3 meshMax = new Vector3(float.MinValue);
            Vector3 meshMin = new Vector3(float.MaxValue);
 
            foreach (ModelMeshPart part in mesh.MeshParts)
            {   
                // The stride is how big, in bytes, one vertex is in the vertex buffer
                // We have to use this as we do not know the make up of the vertex
                int stride = part.VertexBuffer.VertexDeclaration.VertexStride;
 
                VertexPositionNormalTexture[] vertexData = new VertexPositionNormalTexture[part.NumVertices];
                part.VertexBuffer.GetData(part.VertexOffset * stride, vertexData, 0, part.NumVertices, stride);
 
                // Find minimum and maximum xyz values for this mesh part
                Vector3 vertPosition = new Vector3();
 
                for (int i = 0; i < vertexData.Length; i++)
                {
                    vertPosition = vertexData[i].Position;
 
                    // update our values from this vertex
                    meshMin = Vector3.Min(meshMin, vertPosition);
                    meshMax = Vector3.Max(meshMax, vertPosition);
                }
            }
 
            // transform by mesh bone matrix
            meshMin = Vector3.Transform(meshMin, meshTransform);
            meshMax = Vector3.Transform(meshMax, meshTransform);

            if (meshMin.X > meshMax.X)
            {
                float temp = meshMin.X;
                meshMin.X = meshMax.X;
                meshMax.X = temp;
            }

            if (meshMin.Y > meshMax.Y)
            {
                float temp = meshMin.Y;
                meshMin.Y = meshMax.Y;
                meshMax.Y = temp;
            }

            if (meshMin.Z > meshMax.Z)
            {
                float temp = meshMin.Z;
                meshMin.Z = meshMax.Z;
                meshMax.Z = temp;
            }
            Width = meshMax.Z - meshMin.Z;
            
            // Create the bounding box
            BoundingBox box = new BoundingBox(meshMin, meshMax);
            return box;
        }

        public void Draw(Matrix view, Matrix projection, GraphicsDevice graphicsDevice)
        {
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (Lighting)
                    {
                        effect.EnableDefaultLighting();
                        effect.LightingEnabled = true;
                    }
                    effect.World = transforms[mesh.ParentBone.Index] * World;

                    // Use the matrices provided by the chase camera
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }

        public virtual void DrawCollisions(ChaseCamera camera, GraphicsDevice graphicsDevice)
        {
            // Initialize an array of indices for the box. 12 lines require 24 indices
            short[] bBoxIndices = {
                0, 1, 1, 2, 2, 3, 3, 0, // Front edges
                4, 5, 5, 6, 6, 7, 7, 4, // Back edges
                0, 4, 1, 5, 2, 6, 3, 7 // Side edges connecting front and back
            };

            // Use inside a drawing loop
            foreach (BoundingBox box in boundingBoxes)
            {
                Vector3[] corners = box.GetCorners();
                VertexPositionColor[] primitiveList = new VertexPositionColor[corners.Length];

                // Assign the 8 box vertices
                for (int i = 0; i < corners.Length; i++)
                {
                    primitiveList[i] = new VertexPositionColor(corners[i], Color.White);
                }

                boxEffect.World = Matrix.Identity;
                boxEffect.View = camera.View;
                boxEffect.Projection = camera.Projection;
                boxEffect.TextureEnabled = false;

                // Draw the box with a LineList
                foreach (EffectPass pass in boxEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.LineList, primitiveList, 0, 8,
                        bBoxIndices, 0, 12);
                }
            }
        }

        public bool CheckCollisionWith(CModel model)
        {
            foreach (BoundingBox box in boundingBoxes)
            {
                foreach (BoundingBox check in model.boundingBoxes)
                {
                    if(box.Intersects(check))
                        return true;
                }
            }
            return false;
        }
    }
}
