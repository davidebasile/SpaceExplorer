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
    public class BulletAvatar : GameObject
    {
        public LevelManager levelManager { get; set; }
        public BulletAvatar(LevelManager levelManager)
        {
            this.texture2D = levelManager.content.Load<Texture2D>("Texture\\orange");
            this.levelManager = levelManager;
            //this.offset = new Vector3(-0.4f, 0.8f, 4.3f);
            this.offset = Vector3.Zero;
            this.isActive = false;
            this.modelString = "Models\\bullet";
            this.forwardSpeed = 1.5f;
        }

        public void Activate(Vector3 rotation, Vector3 position)
        {
            this.isActive = true;
            this.rotation = rotation;
            Vector3 v = new Vector3(-0.4f, 0.0f, 4.3f);
            v = Vector3.Transform(v, Matrix.CreateRotationX(this.rotation.X));
            v = Vector3.Transform(v, Matrix.CreateRotationY(this.rotation.Y));
            this.position = position+v; 
        }
        public void Update()
        {
            if (!isActive)
                return;
            int i = IsCollidedWithEnemy(levelManager);
            if (i > -1)
            {
                levelManager.enemy[i].energy -= 20;
            }
            else if (IsCollidedWithBounding(levelManager))
                this.isActive=false;
            else{
                Vector3 v = new Vector3(0, 0, forwardSpeed);
                v = Vector3.Transform(v, Matrix.CreateRotationX(this.rotation.X));
                v = Vector3.Transform(v, Matrix.CreateRotationY(this.rotation.Y));
                this.position = new Vector3(this.position.X + v.X, this.position.Y + v.Y, this.position.Z + v.Z);
            }
            this.UpdateBounding();
        }

        
    }
}