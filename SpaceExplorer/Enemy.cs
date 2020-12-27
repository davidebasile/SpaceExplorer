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
    public class Enemy : GameObject
    {
        #region Field
        const int Up = 0;
        const int Down = 1;
        const int Left = 2;
        const int Right = 3;
        int isChangingDirection = -1;
        public LevelManager levelManager { get; set; }
        public BulletEnemy[] bullet { get; set; }
        public string debug;
        bool first = true;
        //VertexPositionColor[] v = new VertexPositionColor[1000];
        //int lenght = 0;
        #endregion

        #region Initialize
        public Enemy(LevelManager levelManager)
        {
            modelString = "Models\\Hermes\\Hermes";
            offset = new Vector3(15.1f, -22.1f, -4.2f);
            forwardSpeed = 0.3f;
            yawSpeed = MathHelper.ToRadians(90) / 40 ;
            radius = 1.2f;
            energy = 100f;
            this.levelManager = levelManager;
            this.bullet = new BulletEnemy[1];
            for (int i = 0; i < bullet.Length; i++)
            {
                bullet[i] = new BulletEnemy(levelManager,this);
                bullet[i].LoadContent(levelManager.content);
            }
            this.UpdateBounding();
        }
        #endregion

        #region Update
        bool isShooting = false;
        public void Update()
        {
            if (energy <= 0)
                isActive = false;
            if (!isActive)
                return;

            if (this.boundingSphere.Intersects(levelManager.avatar.boundingSphere))
            {
                levelManager.avatar.energy-=0.01f;
            }
            else if (CheckAvatarContact())
            {
                debug = "contact";
                isShooting = true;
                Shoot();
            }
            else
            {

                debug = "no contact";
                if (isShooting)
                {
                    isShooting = false;
                    this.rotation = Vector3.Forward;
                    isChangingDirection = -1;
                    Turn();
                }
                else
                {
                    if (IsCollidedWithBounding(levelManager) && !first)
                    {
                        float f = -forwardSpeed;
                        Vector3 v = new Vector3(0, 0, f);
                        v = Vector3.Transform(v, Matrix.CreateRotationX(this.rotation.X));
                        v = Vector3.Transform(v, Matrix.CreateRotationY(this.rotation.Y));
                        this.position = new Vector3(this.position.X + v.X, this.position.Y + v.Y, this.position.Z + v.Z);
                        isChangingDirection = -2;
                    }
                    else
                    {
                        if (isChangingDirection == -2 || isChangingDirection > -1)
                            Turn();
                        else
                        {
                            Vector3 v = new Vector3(0, 0, forwardSpeed);
                            v = Vector3.Transform(v, Matrix.CreateRotationX(this.rotation.X));
                            v = Vector3.Transform(v, Matrix.CreateRotationY(this.rotation.Y));
                            this.position = new Vector3(this.position.X + v.X, this.position.Y + v.Y, this.position.Z + v.Z);
                        }
                    }
                }
            }
            first = false;
            this.UpdateBounding();
        }

        /// <summary>
        /// Rotate the enemy of 90 degrees in a random direction
        /// </summary>
        public void Turn()
        {
            Random random = new Random();
            int turning;
            if (isChangingDirection < 0)
                turning = random.Next(4);
            else
                turning = isChangingDirection;
            if (turning == Up)
            {

                this.rotation = new Vector3(this.rotation.X - yawSpeed, this.rotation.Y, this.rotation.Z);
                int angle = (int)MathHelper.ToDegrees(this.rotation.X);
                if ((angle % 90 == 0) && (isChangingDirection > -1))
                {
                    isChangingDirection = -1;
                    this.rotation = new Vector3(MathHelper.ToRadians(angle % 360), this.rotation.Y, this.rotation.Z);
                }
                else
                    isChangingDirection = Up;
            }
            else if (turning == Down)
            {
                this.rotation = new Vector3(this.rotation.X + yawSpeed, this.rotation.Y, this.rotation.Z);
                int angle = (int)MathHelper.ToDegrees(this.rotation.X);
                if ((angle % 90 == 0) && (isChangingDirection > 0))
                {
                    isChangingDirection = -1;
                    this.rotation = new Vector3(MathHelper.ToRadians(angle % 360), this.rotation.Y, this.rotation.Z);
                }
                else
                    isChangingDirection = Down;
            }
            else if (turning == Left)
            {
                this.rotation = new Vector3(this.rotation.X, this.rotation.Y + yawSpeed, this.rotation.Z);
                int angle = (int)MathHelper.ToDegrees(this.rotation.Y);
                if ((angle % 90 == 0) && (isChangingDirection > 0))
                {
                    isChangingDirection = -1;
                    this.rotation = new Vector3(this.rotation.X, MathHelper.ToRadians(angle % 360), this.rotation.Z);
                }
                else
                    isChangingDirection = Left;
            }
            else if (turning == Right)
            {
                this.rotation = new Vector3(this.rotation.X, this.rotation.Y - yawSpeed, this.rotation.Z);
                int angle = (int)MathHelper.ToDegrees(this.rotation.Y);
                if ((angle % 90 == 0) && (isChangingDirection > 0))
                {
                    isChangingDirection = -1;
                    this.rotation = new Vector3(this.rotation.X, MathHelper.ToRadians(angle % 360), this.rotation.Z);
                }
                else
                    isChangingDirection = Right;
            }  
        }

        /// <summary>
        /// Check if there isn't bounding between the enemy and the avatar
        /// </summary>
        /// <returns>true if there is a contact</returns>
        public bool CheckAvatarContact()
        {
           // v[0] = new VertexPositionColor(levelManager.avatar.position, Color.White);
            Vector3 distance =levelManager.avatar.position-this.position;
            Vector3 direction = distance;
            direction.Normalize();
            if (direction.Z > 0)
            {
                Ray pickRay = new Ray(this.position, direction);
                int count = 1;
                Vector3 intersectPoint = this.position;
                //lenght = 1;
                while (Math.Abs(intersectPoint.X - this.position.X) < Math.Abs(distance.X) ||
                       Math.Abs(intersectPoint.Y - this.position.Y) < Math.Abs(distance.Y) ||
                       Math.Abs(intersectPoint.Z - this.position.Z) < Math.Abs(distance.Z))
                {
                    intersectPoint = Vector3.Add(Vector3.Multiply(pickRay.Direction, count), pickRay.Position);
                    int zz = (int)Math.Floor((intersectPoint.Z + (levelManager.blockSize.Z)) / (levelManager.blockSize.Z * 2));
                    int yy = (int)Math.Ceiling((intersectPoint.Y + (levelManager.blockSize.Y)) / (levelManager.blockSize.Y * 2 * -1));
                    int xx = (int)Math.Ceiling((intersectPoint.X + (levelManager.blockSize.X)) / (levelManager.blockSize.X * 2 * -1));
                    if (!((zz < 0) || (zz >= levelManager.level.Length) ||
                        (yy < 0) || (yy >= levelManager.level[zz].Length) ||
                        (xx < 0) || (xx >= levelManager.level[zz][yy].Length)))
                    {
                        Block select = levelManager.level[zz][yy][xx];
                        if (select != null)
                        {
                            return false;
                        }
                    }
                    //v[lenght] = new VertexPositionColor(intersectPoint, Color.White);
                   // lenght++;
                    count++;
                }
                return true;
            }
            else
                return false;
        }
    
        public void Shoot()
        {
            int i = -1;
            for (int j = 0; j < bullet.Length; j++)
            {
                if (bullet[j].isActive == false)
                {
                    i = j;
                    break;
                }
            }
            if (i != -1)
            {
                Vector3 direction = levelManager.avatar.position - this.position;
                direction.Normalize();
                Vector3 rot = new Vector3();
                rot.X = (float)Math.Atan((direction.Y / direction.Z)*-1);
                rot.Y = (float)Math.Atan(direction.X / direction.Z);
                rot.Z = this.rotation.Z;
                if (direction.Z < 0)
                {
                    rot.X *= -1;
                    rot.Y += MathHelper.Pi;
                }
                this.rotation = new Vector3(rot.X, rot.Y, rot.Z);
                bullet[i].rotation = new Vector3(rot.X, rot.Y, rot.Z); 
                Ray pickRay = new Ray(this.position, direction);
                bullet[i].Activate(pickRay);
            }
        }
        #endregion

        #region Draw
        /*
        public void DrawDebug()
        {
            if (v != null)
            {
                levelManager.device.RenderState.PointSize = 10;
                levelManager.device.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.PointList,
                    v,
                    0,  // index of the first vertex to draw
                    lenght  // number of primitives
                );
            }
        }*/
        #endregion
    }
}