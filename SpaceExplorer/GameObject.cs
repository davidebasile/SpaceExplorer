using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace SpaceExplorer
{
    public class GameObject
    {
        #region Fields
        public Vector3 offset { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public Model model { get; set; }
        public Texture2D texture2D { get; set; }
        public BoundingSphere boundingSphere { get; set; }
        public bool focus { get; set; }
        public float forwardSpeed { get; set; }
        public float yawSpeed { get; set; }
        public float radius { get; set; }
        public bool isActive { get; set; }
        public string modelString { get; set; }
        public float energy { get; set; }
        #endregion

        #region Inizialize
        public void LoadContent(ContentManager content, Vector3 pos)
        {
            LoadContent(content);
            position = pos;
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>(modelString);
            focus = false;
            BoundingSphere boundingSphere = model.Meshes[0].BoundingSphere;
            for (int i = 1; i < model.Meshes.Count; i++)
                boundingSphere = BoundingSphere.CreateMerged(boundingSphere, model.Meshes[i].BoundingSphere);
            boundingSphere.Radius *= radius;
            this.boundingSphere = boundingSphere;
        }
        #endregion

        #region Update
        public void UpdateBounding()
        {
             boundingSphere = new BoundingSphere(new Vector3(position.X, position.Y, position.Z), boundingSphere.Radius);
        }

        public bool IsCollidedWithBounding(LevelManager levelManager)
        {
            Block[][][] level = levelManager.level;
            Vector3 blockSize = levelManager.blockSize;
            Vector3 coord = new Vector3();
            coord.X = (this.position.X / (blockSize.X * 2)) * -1;
            coord.Y = (this.position.Y / (blockSize.Y * 2)) * -1;
            coord.Z = this.position.Z / (blockSize.Z * 2);
            Block b = new Block(Vector3.Zero, Vector3.Zero);
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                    for (int z = -1; z < 2; z++)
                    {
                        int zz = (int)coord.Z + i;
                        int yy = (int)coord.Y + j;
                        int xx = (int)coord.X + z;
                        if (!((zz < 0) || (zz >= level.Length) ||
                            (yy < 0) || (yy >= level[zz].Length) ||
                            (xx < 0) || (xx >= level[zz][yy].Length)))
                        {
                            b = level[zz][yy][xx];
                            if (b != null)
                            {
                                if ((this.boundingSphere.Intersects(b.boundingBox)))
                                    return true;
                            }
                        }
                    }
            return false;
        }


        public int IsCollidedWithEnemy(LevelManager levelManager)
        {
            Enemy[] enemy = levelManager.enemy;
            for (int i = 0; i < enemy.Length; i++)
            {
                if ((this.boundingSphere.Intersects(enemy[i].boundingSphere))&&enemy[i].isActive)
                {
                    this.isActive = false;
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region Draw
        public void DrawBoundingSphere(Matrix view, Matrix projection, GameObject boundingSphereModel)
        {
            UpdateBounding();
            Matrix scaleMatrix = Matrix.CreateScale(boundingSphere.Radius);
            Matrix translateMatrix = Matrix.CreateTranslation(boundingSphere.Center);
            Matrix worldMatrix = scaleMatrix * translateMatrix;

            foreach (ModelMesh mesh in boundingSphereModel.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldMatrix;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }

        public void DrawModel(Matrix world, Matrix view, Matrix projection)
        {
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;
                    if (texture2D != null)
                    {
                        effect.Texture = texture2D;
                        effect.TextureEnabled = true;
                    }
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }

        }

        public void DrawModel(Matrix view, Matrix projection)
        {
            if (this.isActive)
            {
                Matrix rotation = Matrix.CreateRotationZ(this.rotation.Z)
                    * Matrix.CreateRotationX(this.rotation.X) * Matrix.CreateRotationY(this.rotation.Y);
                Matrix world = Matrix.CreateTranslation(this.offset) * rotation *
                          Matrix.CreateTranslation(this.position);
                DrawModel(world, view, projection);
            }
        }
        #endregion
    }
}
