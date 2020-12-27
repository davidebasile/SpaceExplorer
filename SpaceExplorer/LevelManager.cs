using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

namespace SpaceExplorer
{
    public class LevelManager
    {
        #region Fields
        public Vector3 blockSize { get; set; }
        private string fileLevel;
        private Texture2D[] textures;
        public Block[][][] level { get; set; }
        private string levelPath = Environment.CurrentDirectory + "\\..\\..\\..\\Levels\\";
        public GraphicsDevice device;
        public ContentManager content;
        private BasicEffect basicEffect;
        public GameObject gates;
        public Avatar avatar;
        public Enemy[] enemy;
        public int maxEnemy { get; set; }
        #endregion

        #region Initialize
        public LevelManager(string fileLevel, GraphicsDevice device, ContentManager content, Texture2D[] textures)
        {

            this.blockSize = new Vector3(6, 12, 12);
            this.fileLevel = levelPath + fileLevel;
            this.textures = textures;
            this.device = device;
            this.content = content;
            this.basicEffect = new BasicEffect(device, null);

            maxEnemy = 10;
            enemy = new Enemy[maxEnemy];
            for (int i = 0; i < enemy.Length; i++)
            {
                enemy[i] = new Enemy(this);
                enemy[i].LoadContent(content);
            }

            this.gates = new GameObject();
            gates.modelString = "Models\\gates";
            gates.LoadContent(content);
            gates.offset = new Vector3(12.5f, -58f, 35.5f);

            avatar = new Avatar(this);
            avatar.LoadContent(content);

            this.level = this.CreateLevel();
            
        }


        private Block[][][] CreateLevel()
        {
            Random rand = new Random();
            StreamReader sr = File.OpenText(@fileLevel);
            string file = sr.ReadToEnd();
            string[] sections = Regex.Split(file, "\r\n[+]\r\n");
            Block[][][] block = new Block[sections.Length][][];
            int enemyCount = 0;
            for (int axis_z = 0; axis_z < sections.Length; axis_z++)
            {
                string[] lines = Regex.Split(sections[axis_z], "\r\n");
                block[axis_z] = new Block[lines.Length][];
                for (int axis_y = 0; axis_y < lines.Length; axis_y++)
                {
                    char[] c = new char[lines[axis_y].Length];
                    StringReader str = new StringReader(lines[axis_y]);
                    str.Read(c, 0, c.Length);
                    block[axis_z][axis_y] = new Block[c.Length];
                    for (int axis_x = 0; axis_x < c.Length; axis_x++)
                    {
                        switch (c[axis_x])
                        {
                            case '1':
                                {
                                    block[axis_z][axis_y][axis_x] = new Block(blockSize,
                                        new Vector3((-axis_x * blockSize.X) * 2, (-axis_y * blockSize.Y) * 2, (axis_z * blockSize.Z) * 2));
                                    if (rand.Next(2)>0)
                                        block[axis_z][axis_y][axis_x].shapeTexture = textures[3];
                                    else
                                        block[axis_z][axis_y][axis_x].shapeTexture = textures[3];
                                    break;
                                }
                            case '2':
                                {
                                    block[axis_z][axis_y][axis_x] = new Block(blockSize,
                                        new Vector3((-axis_x * blockSize.X) * 2, (-axis_y * blockSize.Y) * 2, (axis_z * blockSize.Z) * 2));
                                    block[axis_z][axis_y][axis_x].shapeTexture = textures[1];
                                    break;
                                }
                            case '>':
                                {
                                    avatar.position = new Vector3((-axis_x * blockSize.X) * 2, (-axis_y * blockSize.Y) * 2,
                                        (axis_z * blockSize.Z) * 2);
                                    avatar.isActive = true;
                                    block[axis_z][axis_y][axis_x] = null;
                                    break;
                                }
                            case 'U': 
                                {
                                    gates.position = new Vector3((-axis_x * blockSize.X) * 2, (-axis_y * blockSize.Y) * 2,
                                        (axis_z * blockSize.Z) * 2);
                                    gates.isActive = true;
                                    gates.UpdateBounding();
                                    block[axis_z][axis_y][axis_x] = null; 
                                    break; 
                                }
                            case 'E':
                                {
                                    
                                    enemy[enemyCount].position = new Vector3((-axis_x * blockSize.X) * 2, (-axis_y * blockSize.Y) * 2,
                                        (axis_z * blockSize.Z) * 2);
                                    enemy[enemyCount].UpdateBounding();
                                    enemy[enemyCount].isActive = true;
                                    enemyCount++;
                                    block[axis_z][axis_y][axis_x] = null;
                                    break;
                                }

                            case ' ': { block[axis_z][axis_y][axis_x] = null; break; }
                        }
                    }
                }
            }
            return block;
        }
        #endregion

        #region Update

        public void Update(KeyboardState key, KeyboardState prevKey)
        {
            foreach (Enemy e in enemy)
            {
                e.Update();

                for (int i = 0; i < e.bullet.Length; i++)
                    e.bullet[i].Update();
            }

            avatar.Update(key,prevKey);
        }
        #endregion

        #region Draw
        public void DrawLevel(Matrix view, Matrix projection)
        {
            basicEffect.World = Matrix.Identity;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.TextureEnabled = true;
            basicEffect.Begin();
            basicEffect.EnableDefaultLighting();
            foreach (Block[][] sections in this.level)
                foreach (Block[] lines in sections)
                    foreach (Block block in lines)
                        if (block != null)
                        {
                            basicEffect.Texture = block.shapeTexture;
                            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                            {
                                pass.Begin();
                                block.RenderShape(device);
                                pass.End();

                            }
                        }
            basicEffect.End();
            //DrawDebug();
            //enemy[0].DrawDebug();
            gates.DrawModel(view, projection);
            for (int i = 0; i < enemy.Length; i++)
            {
                enemy[i].DrawModel(view, projection);

                for (int j = 0; j < enemy[i].bullet.Length; j++)
                    enemy[i].bullet[j].DrawModel(view, projection);
            }
            
            avatar.DrawModel(view, projection);
            for (int i = 0; i < avatar.bullet.Length; i++)
                avatar.bullet[i].DrawModel(view,projection);

        }
        private void DrawDebug()
        {
            Block block = this.level[0][0][4];
            if (block != null)
            {
                Vector3 v1 = block.shapePosition - block.shapeSize;
                Vector3 v2 = block.shapePosition + block.shapeSize;
                VertexPositionColor[] uno = new VertexPositionColor[2];
                uno[0] = new VertexPositionColor(v1, Color.White);
                uno[1] = new VertexPositionColor(v2, Color.White);
                device.RenderState.PointSize = 10;
                device.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.PointList,
                    uno,
                    0,  // index of the first vertex to draw
                    2  // number of primitives
                );
            }
        }
        #endregion

    }
}
