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
    public class Avatar : GameObject
    {
        public float pitchSpeed { get; set; }
        public float pitch { get; set; }
        public LevelManager levelManager { get; set; }
        public BulletAvatar[] bullet { get; set; }

        private bool isCollidedLastUpdate = false;
        private bool enterKeyDown = false;

        public Avatar(LevelManager levelManager)
        {

            this.modelString = "Models\\Ship\\Ship";
            this.offset = new Vector3(-4, -14, 7);
            forwardSpeed = 0.1f;
            yawSpeed = 0.02f;
            pitchSpeed = 0.02f;
            pitch = 0.01f;
            radius = 0.8f;
            energy = 100f;
            this.focus = true;
            this.levelManager = levelManager;
            this.bullet = new BulletAvatar[3];
            for (int i = 0; i < bullet.Length; i++)
            {
                bullet[i] = new BulletAvatar(levelManager);
                bullet[i].LoadContent(levelManager.content);
            }
        }

        public void Update(KeyboardState key, KeyboardState prevKey)
        {
            if (energy < 0)
                isActive = false;
            if (!isActive)
                return;
            if (IsCollidedWithBounding(levelManager)&&!isCollidedLastUpdate)
            {
                this.energy -= 0.5f;   
                float f = -forwardSpeed * (1.5f);
                Vector3 v = new Vector3(0, 0, f);

                v = Vector3.Transform(v, Matrix.CreateRotationX(this.rotation.X));
                v = Vector3.Transform(v, Matrix.CreateRotationY(this.rotation.Y));

                this.position = new Vector3(this.position.X + v.X, this.position.Y + v.Y, this.position.Z + v.Z);
                isCollidedLastUpdate = true;
            }
            else
            {
                isCollidedLastUpdate = false;

                if (yawSpeed < 0.05)
                    yawSpeed *= 1.02f;

                if (key.IsKeyDown(Keys.Left))
                {
                    float rot = this.rotation.Z;
                    rot -= 0.04f;
                    if (rot < -0.7f)
                        rot = -0.7f;
                    this.rotation = new Vector3(this.rotation.X, this.rotation.Y + yawSpeed, rot);
                }
                else if (key.IsKeyDown(Keys.Right))
                {
                    float rot = this.rotation.Z;
                    rot += 0.04f;
                    if (rot > 0.7f)
                        rot = 0.7f;
                    this.rotation = new Vector3(this.rotation.X, this.rotation.Y - yawSpeed, rot);
                }
                else
                {
                    float rot = this.rotation.Z;
                    if (rot > 0.05f)
                        rot -= 0.02f;
                    else if (rot < -0.05f)
                        rot += 0.02f;
                    else rot = 0;
                    this.rotation = new Vector3(this.rotation.X, this.rotation.Y, rot);
                    yawSpeed = 0.02f;
                }

                if (pitchSpeed < 0.05)
                    pitchSpeed *= 1.02f;
                if (key.IsKeyDown(Keys.Up))
                {
                    if (pitch < 0.5f)
                        pitch += 0.02f;
                    this.rotation = new Vector3(this.rotation.X + pitchSpeed, this.rotation.Y, this.rotation.Z);
                }
                else if (key.IsKeyDown(Keys.Down))
                {
                    if (pitch > -0.5f)
                        pitch -= 0.02f;
                    this.rotation = new Vector3(this.rotation.X - pitchSpeed, this.rotation.Y, this.rotation.Z);
                }
                else
                {
                    if (pitch > 0.05f)
                        pitch -= 0.02f;
                    else if (pitch < -0.05f)
                        pitch += 0.02f;
                    pitchSpeed = 0.02f;
                }

                if (key.IsKeyDown(Keys.Space))
                {
                    Vector3 v = new Vector3(0, 0, forwardSpeed);
                    v = Vector3.Transform(v, Matrix.CreateRotationX(this.rotation.X));
                    v = Vector3.Transform(v, Matrix.CreateRotationY(this.rotation.Y));
                    this.position = new Vector3(this.position.X + v.X, this.position.Y + v.Y, this.position.Z + v.Z);

                    if (forwardSpeed < 0.7f)
                        forwardSpeed *= 1.05f;
                }
                else
                    forwardSpeed = 0.1f;
            }

            this.UpdateBullet(key,prevKey);
            this.UpdateBounding();
        }

        private void UpdateBullet(KeyboardState key, KeyboardState prevKey)
        {
            if (key.IsKeyUp(Keys.Enter))
            {
                enterKeyDown = true;
            }
            else if (enterKeyDown)
            {
                enterKeyDown = false;
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
                        bullet[i].Activate(this.rotation, this.position);

                }
            }
            for (int i = 0; i < bullet.Length; i++)
                bullet[i].Update();
        }

        public bool IsInTheGate()
        {
            return this.boundingSphere.Intersects(levelManager.gates.boundingSphere);
        }


        public new void DrawModel(Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateTranslation(this.offset) * Matrix.CreateRotationZ(this.rotation.Z) *
                  Matrix.CreateRotationX(this.rotation.X + this.pitch) * Matrix.CreateRotationY(this.rotation.Y) *
                      Matrix.CreateTranslation(this.position);
            DrawModel(world, view, projection);
        }
        
    }
}