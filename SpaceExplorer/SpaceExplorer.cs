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
    
    public enum GameState { Loading, Instructions, Running, Won, Lost }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceExplorer : Microsoft.Xna.Framework.Game
    {
        #region Fields
        struct mouse
        {
            public MouseState pos;
            public bool isPressed;
            public float offset;
            public int selectedIndex;
            public float distance;
        }
        struct keyboard
        {
            public KeyboardState previousKeystate;
            public KeyboardState currentKeyState;
        }
        struct background
        {

            public Texture2D stars;
            public Texture2D loading;
        }

        GameState currentGameState = GameState.Running;
        GraphicsDeviceManager graphics;
        SpriteBatch foregroundBatch;
        SpriteFont statsFont;
        Keys cameraStateKeyDown = Keys.Zoom;
        string debug;
        Vector3 thirdPersonReference = new Vector3(0, 4.4f, -10);
        Vector3 firstPersonReference = new Vector3(0, 2.3f, -4.7f);
        Vector3 cameraReference;
        Vector3 cameraRotation = new Vector3(0, 2.22f, 0);
        Matrix view;
        Matrix projection;
        float aspectRatio;
        GameObject[] worldObject;
        GameObject boundingSphere;
        mouse currentMouse = new mouse();
        keyboard currentKeyboard= new keyboard();
        background currentBackground = new background();
        LevelManager levelManager;
        GameObject cameraLookAt;
        int enemyCount;
        #endregion

        #region Initialize
        public SpaceExplorer()
        {
            graphics = new GraphicsDeviceManager(this);
            
            //this.graphics.PreferredBackBufferWidth = 768;
            //this.graphics.PreferredBackBufferHeight = 1024;
            //this.graphics.IsFullScreen = true;
           
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            worldObject = new GameObject[2];
            boundingSphere = new GameObject();
            foregroundBatch = new SpriteBatch(GraphicsDevice);
            IsMouseVisible = true;
            currentMouse.isPressed = false;
            for (int i = 0; i < worldObject.Length; i++)                
                worldObject[i] = new GameObject();
            cameraReference = thirdPersonReference;
            enemyCount = 0;
            base.Initialize();
        }

        protected override void LoadContent()
        {   
            Texture2D[] textures = new Texture2D[5];
            textures[0] = Content.Load<Texture2D>("Texture\\texture1");
            textures[1] = Content.Load<Texture2D>("Texture\\texture2");
            textures[2] = Content.Load<Texture2D>("Texture\\rock");
            textures[3] = Content.Load<Texture2D>("Texture\\shiphull");
            textures[4] = Content.Load<Texture2D>("Texture\\shiphull2");
            levelManager = new LevelManager("Level3.txt", GraphicsDevice, Content, textures);
            worldObject[0] = levelManager.avatar;
            cameraLookAt = levelManager.avatar;

            foreach (GameObject wo in worldObject)
                wo.UpdateBounding();

            boundingSphere.model = Content.Load<Model>("Models/sphere1uR");

            currentBackground.stars = Content.Load<Texture2D>("Texture/stars");
            
            currentBackground.loading = Content.Load<Texture2D>("Texture/background_loading"); 
            statsFont = Content.Load<SpriteFont>("SpriteFont1");

            aspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
            (float)graphics.GraphicsDevice.Viewport.Height;
        }

        protected override void UnloadContent()
        {

        }
        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
        {

            currentKeyboard.previousKeystate = currentKeyboard.currentKeyState;
            currentKeyboard.currentKeyState = Keyboard.GetState();

            // Allows the game to exit
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.Escape)||levelManager.avatar.IsInTheGate()||!levelManager.avatar.isActive)
                this.Exit();

            if (currentGameState == GameState.Loading)
            {
                if ((currentKeyboard.previousKeystate.IsKeyDown(Keys.Space))&&(currentKeyboard.currentKeyState.IsKeyUp(Keys.Space)))
                    currentGameState = GameState.Running;
                else if ((currentKeyboard.previousKeystate.IsKeyDown(Keys.Enter)) && (currentKeyboard.currentKeyState.IsKeyUp(Keys.Enter)))
                    currentGameState = GameState.Instructions;
            }
            else if (currentGameState == GameState.Instructions)
            {
                if ((currentKeyboard.previousKeystate.IsKeyDown(Keys.Enter)) && (currentKeyboard.currentKeyState.IsKeyUp(Keys.Enter)))
                    currentGameState = GameState.Loading;
            }
            else if (currentGameState == GameState.Running)
            {
                levelManager.Update(currentKeyboard.currentKeyState, currentKeyboard.previousKeystate);
                UpdateCamera();
                UpdateMouse();
            }

            base.Update(gameTime);
        }

        protected void UpdateCamera()
        {

            float rotation_speed = 5f / 50f;
            float forwardSpeed = 1.5f;

            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.Tab))
                cameraStateKeyDown = Keys.Tab;
            else if (currentKeyboard.currentKeyState.IsKeyDown(Keys.Back))
                cameraStateKeyDown = Keys.Back;
            else if (cameraStateKeyDown!=Keys.Zoom)
            {
                if (cameraStateKeyDown.Equals(Keys.Tab))
                {
                    if (cameraReference.Equals(thirdPersonReference))
                    {
                        cameraReference = firstPersonReference;
                        cameraRotation = new Vector3(0, 1.8f, 0);
                    }
                    else
                    {
                        cameraReference = thirdPersonReference;
                        cameraRotation = new Vector3(0, 2.22f, 0);
                    }
                }
                else if (cameraStateKeyDown.Equals(Keys.Back))
                {
                    bool found = false;
                    for (int i = enemyCount; i < levelManager.enemy.Length; i++)
                    {
                        if (levelManager.enemy[i].isActive)
                        {
                            found = true;
                            enemyCount = i+1;
                            cameraLookAt = levelManager.enemy[i];
                            break;
                        }
                    }
                    if (!found)
                    {
                        enemyCount = 0;
                        cameraLookAt = levelManager.avatar;
                    }
                }
                cameraStateKeyDown = Keys.Zoom;
            }

            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.W))
                cameraRotation.Y += rotation_speed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.S))
                cameraRotation.Y -= rotation_speed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.A))
                cameraRotation.X -= rotation_speed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.D))
                cameraRotation.X += rotation_speed;

            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.O))
                cameraReference.Y += forwardSpeed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.L))
                cameraReference.Y -= forwardSpeed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.H))
                cameraReference.X -= forwardSpeed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.K))
                cameraReference.X += forwardSpeed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.U))
                cameraReference.Z += forwardSpeed;
            if (currentKeyboard.currentKeyState.IsKeyDown(Keys.J))
                cameraReference.Z -= forwardSpeed;
        }

        protected void UpdateMouse()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && !currentMouse.isPressed)
            {
                currentMouse.isPressed = true;
                currentMouse.pos = mouseState;
                Ray pickRay = GetPickRay();
                currentMouse.selectedIndex = -1;
                currentMouse.distance = float.MaxValue;
                for (int i = 0; i < worldObject.Length; i++)
                {
                    worldObject[i].focus = false;
                    Nullable<float> result = pickRay.Intersects(worldObject[i].boundingSphere);
                    if (result.HasValue == true)
                    {
                        if (result.Value < currentMouse.distance)
                        {
                            currentMouse.selectedIndex = i;
                            currentMouse.distance = result.Value;
                            Vector3 IntersectPoint = Vector3.Add(Vector3.Multiply(pickRay.Direction, currentMouse.distance), pickRay.Position);
                            currentMouse.offset = IntersectPoint.Y - worldObject[currentMouse.selectedIndex].position.Y;
                        }
                    }
                }
                if (currentMouse.selectedIndex > -1)
                {
                    worldObject[currentMouse.selectedIndex].focus = true;
                }
            }
            else if (currentMouse.isPressed && mouseState.LeftButton == ButtonState.Released)
                currentMouse.isPressed = false;
            else if (mouseState.LeftButton == ButtonState.Pressed && currentMouse.isPressed && currentMouse.selectedIndex != -1)
            {
                if (currentMouse.pos.Y != mouseState.Y)
                {
                    Ray pickRay = GetPickRay();
                    Vector3 IntersectPoint = Vector3.Add(Vector3.Multiply(pickRay.Direction, currentMouse.distance), pickRay.Position);
                    worldObject[currentMouse.selectedIndex].position = new Vector3(worldObject[currentMouse.selectedIndex].position.X,
                        IntersectPoint.Y-currentMouse.offset,
                        worldObject[currentMouse.selectedIndex].position.Z);
                    currentMouse.pos = mouseState;
                }
            }
        }

        protected Ray GetPickRay()
        {
            MouseState mouseState = Mouse.GetState();

            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;

            Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
            Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource,
                projection, view, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource,
                projection, view, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            return pickRay;
        }
        #endregion

        #region Draw
        protected override void Draw(GameTime gameTime)
        {

            graphics.GraphicsDevice.Clear(Color.Black);

            if (currentGameState == GameState.Running)
            {
                Matrix transformation = Matrix.CreateRotationX(cameraLookAt.rotation.X) * Matrix.CreateRotationY(cameraLookAt.rotation.Y);
                Vector3 transformedReference = Vector3.Transform(cameraReference, transformation);
                Vector3 cameraPosition = cameraLookAt.position + transformedReference;
                Vector3 up = Vector3.Transform(Vector3.Up, transformation);
                
                transformedReference = Vector3.Transform(cameraRotation, transformation);

                Vector3 cameraLookatView = Vector3.Transform(cameraLookAt.position, Matrix.CreateTranslation(transformedReference));

                view = Matrix.CreateLookAt(cameraPosition, cameraLookatView,up );
                projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f,400.0f);             
                
                DrawBackground();
                levelManager.DrawLevel(view,projection);

                DrawText();
                
                GraphicsDevice.RenderState.FillMode = FillMode.WireFrame; 
                for (int i = 0; i < worldObject.Length; i++)
                {
                    if (worldObject[i].focus)
                         worldObject[i].DrawBoundingSphere(view, projection, boundingSphere);
                }
                GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            }
            else if (currentGameState == GameState.Loading)
                DrawSplashScreen("", "Press Enter to view instructions or Space to begin!");

            else if (currentGameState == GameState.Instructions)
                DrawSplashScreen("INSTRUCTION \r\n\r\nW : Up\r\nS : Down\r\nA : left\r\nD : right\r\nMouse click : action\r\n\r\npress Enter to return", "");
            base.Draw(gameTime);
        }

        protected void DrawBackground()
        {
            foregroundBatch.Begin();
            foregroundBatch.Draw(currentBackground.stars, new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), Color.White);
            foregroundBatch.End();
            resetGraphicsDevice();
            
            
        }

        protected void DrawText()
        {
            foregroundBatch.Begin();

            debug = " avatar energy : " + levelManager.avatar.energy;
            for (int i=0;i<levelManager.enemy.Length;i++)
                if (levelManager.enemy[i] != null && levelManager.enemy[i].isActive)
                    debug+= "\r\nenemy "+i+" energy " +levelManager.enemy[i].energy+ " position "+levelManager.enemy[i].position.ToString() 
                             +" " + levelManager.enemy[i].debug;
            // Draw the string
            foregroundBatch.DrawString(statsFont, debug,new Vector2(20, 20), Color.Orange, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
            foregroundBatch.End();
            resetGraphicsDevice();
            
        }

        private void DrawSplashScreen(string msg1, string msg2)
        {
            foregroundBatch.Begin();
            foregroundBatch.Draw(currentBackground.loading, new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), Color.White);
            float xOffsetText, yOffsetText;
            Vector2 viewportSize = new Vector2(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Vector2 strCenter;

            graphics.GraphicsDevice.Clear(Color.DarkBlue);

            xOffsetText = yOffsetText = 0;
            Vector2 strInstructionsSize =
                statsFont.MeasureString(msg1);
            Vector2 strPosition;
            strCenter = new Vector2(strInstructionsSize.X / 2,
                strInstructionsSize.Y / 2);

            yOffsetText = (viewportSize.Y / 2 - strCenter.Y);
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            foregroundBatch.DrawString(statsFont, msg1,
                strPosition, Color.White);

            strInstructionsSize =
                statsFont.MeasureString(msg2);
            strCenter = new Vector2(strInstructionsSize.X / 2,
                strInstructionsSize.Y / 2);
            yOffsetText =
                ((viewportSize.Y * 7/8) - strCenter.Y) + statsFont.LineSpacing;
            xOffsetText = (viewportSize.X / 2 - strCenter.X);
            strPosition = new Vector2((int)xOffsetText, (int)yOffsetText);

            foregroundBatch.DrawString(statsFont, msg2,
                strPosition, Color.LightGray);
            foregroundBatch.End();

            //re-enable depth buffer after sprite batch disablement
            resetGraphicsDevice();
        }

        private void resetGraphicsDevice()
        {
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Mirror;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Mirror;
            GraphicsDevice.SamplerStates[0].AddressW = TextureAddressMode.Mirror;
        }

        #endregion

        #region EntryPoint
        static void Main(string[] args)
        {
            using (SpaceExplorer game = new SpaceExplorer())
            {
                game.Run();
            }
        }
        #endregion
    }
}
