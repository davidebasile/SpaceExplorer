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
    public class BulletEnemy : GameObject
    {
        public LevelManager levelManager { get; set; }
        public Ray ray { get; set; }
        private float step;
        public Enemy belongTo { get; set; }
        public BulletEnemy(LevelManager levelManager,Enemy belongTo)
        {
            this.texture2D = levelManager.content.Load<Texture2D>("Texture\\orange");
            this.levelManager = levelManager;
            this.offset = new Vector3(-27.9f, -19.82f, -21.2f);
            this.isActive = false;
            this.modelString = "Models\\sphere";
            this.forwardSpeed = 0.75f;
            this.belongTo = belongTo;
        }

        public void Activate(Ray ray)
        {
            this.ray = ray;
            this.position = ray.Position;
            this.UpdateBounding();
            isActive = true;
            step = 0;
        }

        public void Update()
        {

            if (!isActive)
                return;

            if (this.boundingSphere.Intersects(levelManager.avatar.boundingSphere))
            {

                //levelManager.avatar.energy--;
                isActive = false;
            }
            else if (IsCollidedWithBounding(levelManager))
                isActive=false;
            else{
                this.position = Vector3.Add(Vector3.Multiply(ray.Direction, step*forwardSpeed), ray.Position);
                step++;
                if ((Math.Abs((this.position - belongTo.position).X) > Math.Abs((levelManager.avatar.position - belongTo.position).X))||
                (Math.Abs((this.position - belongTo.position).Y) > Math.Abs((levelManager.avatar.position - belongTo.position).Y))||
                (Math.Abs((this.position - belongTo.position).Z) > Math.Abs((levelManager.avatar.position - belongTo.position).Z)))
                    isActive = false;
            }
            this.UpdateBounding();
        }

    }
}
