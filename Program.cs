using System;
using Engine;
using Engine.Math;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace Pacman
{
    class Program {
        static void Main(string[] args) {
            Pacman game = new Pacman(545, 610, "Pacman") { CursorVisible = false };
            game.Run(60f);
        }
    }
    class Pacman : Game{
        public static Player player;
        public static Ghosts ghosts;
        public static int score;
        public static string ProjectPlace { get { return @"C:\Users\Slava\source\repos\Pacman"; } }
        public static Map playground;
        public static bool Died;
        public static bool Gameover;
        public static int lifes = 3;
        public static gameObject[,] map { get { return playground.map; } }
        
        public Pacman(int width, int height,string text) : base(width, height, GraphicsMode.Default, text){}
        protected override void OnLoad(EventArgs e){
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            camera = new Camera(new Vector3(0, 0, 0), ProjectionStyle.Perspective);
            playground = new Map();
            player = new Player(new Vector2(13,23));
            ghosts = new Ghosts();
            base.OnLoad(e);
        }
        protected override void OnUpdateFrame(FrameEventArgs e){
          //  if (!isFocused) { /*ghosts.Stop();*/ return; }
            if (Gameover == true || playground.dots == 0){
                Console.Write(playground.dots.ToString());
                Thread.Sleep(1500);
                Gameover = false;
                Died = false;
                if(lifes == 0 || playground.dots == 0) {
                    score = 0;
                    playground = new Map();
                    lifes = 3;
                }
                player = new Player(new Vector2(13, 23));
                ghosts = new Ghosts();
               
            }
            camera.Update(WIDTH, HEIGHT);
            player.Update((float)e.Time);
             if(!Died) 
                ghosts.Update((float)e.Time);
            if (Keyboard.GetState().IsKeyDown(Key.Escape)) { Exit(); } 
            base.OnUpdateFrame(e);
        }
        protected override void OnRenderFrame(FrameEventArgs e){
            if (!CanRenderFrame((float)e.Time)) return;
            GL.Clear(ClearBufferMask.ColorBufferBit);
            if(!Died)Title = "~Pacman~            Score: " + score + " Lifes: "+ (lifes-1).ToString();
            playground.Render();
            ghosts.Render();
            player.Render();
            SwapBuffers();
            base.OnRenderFrame(e);
        }
        float DeltaUpdate = 1f / 60f;
        float RealTime = 0f;
        bool CanRenderFrame(float time){
            RealTime += time;
            if (RealTime < DeltaUpdate) return false;
            RealTime = 0f;
            return true;
        }
        protected override void OnResize(EventArgs e){
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }
        protected override void OnClosing(CancelEventArgs e){
            base.OnClosing(e);
        }
    }
    enum ObjectType { Pacman,Ghost,Wall,Coin,Air,Fruit,Powerpellet,Door };
    struct gameObject{
        public ObjectType objectType;
        public Square square;
    }
    class Map {
        public float squareSize = 0.024f;
        public int dots = 0;
        public Square texturedMap;
        public Vector2 getMapPosition(int y, int x) => new Vector2(map[y,x].square.position.X, map[y, x].square.position.Y);
        public gameObject[,] map;
        VAO Wallvao = null; VAO Coinvao = null; VAO Powervao = null;
       
        public Map(){
            //-0.19f, -0.1625f
            texturedMap = new Square(new Vector3(0.024f*28 ,0.024f*31 ,0), new Vector3(-0.024f* 0.0546875f, -0.024f* 0.0546875f, -1),null, @"C:\Users\Slava\source\repos\Pacman\res\MapPic.png",
                new float[12] { 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0 });
            map = new gameObject[28, 31];
            Color clr;
            Vector2 position = new Vector2(-0.38f,- 0.325f);
            using (Bitmap bitmap = new Bitmap(@"C:\Users\Slava\source\repos\Pacman\res\map.bmp", true))
                for (int i = 0; i < 28; i++){
                    for (int j = 0; j < 31; j++) {
                        clr = bitmap.GetPixel(i, j);
                        if (clr == Color.FromArgb(65, 75, 235)) { map[i, j].objectType = ObjectType.Wall; }
                        else if (clr == Color.FromArgb(0, 0, 0)) { map[i, j].objectType = ObjectType.Air; }
                        else if (clr == Color.FromArgb(255, 184, 151)) { map[i, j].objectType = ObjectType.Coin; dots++; }
                        else if (clr == Color.FromArgb(248, 180, 1)) { 
                            map[i, j].objectType = ObjectType.Powerpellet;
                            map[i, j].square = new Square(new Vector3(squareSize / 2), new Vector3(position.Y + squareSize * i, position.X + squareSize * (31 - j), -1f), null,
                                Pacman.ProjectPlace+@"\res\PacDot.png", new float[12] { 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0 });
                            continue;
                        }
                        else if(clr == Color.FromArgb(255, 0, 0)) 
                            map[i, j].objectType = ObjectType.Door;
                        float R = clr.R / 255.0f;
                        float G = clr.G / 255.0f;
                        float B = clr.B / 255.0f;
                        map[i, j].square = new Square(new float[18]{R, G, B,
                                                                    R, G, B,
                                                                    R, G, B,
                                                                    R, G, B,
                                                                    R, G, B,
                                                                    R, G, B},
                                            map[i, j].objectType == ObjectType.Coin ? new Vector3(squareSize / 5) : map[i, j].objectType == ObjectType.Powerpellet ? new Vector3(squareSize / 2) 
                                            : new Vector3(squareSize),
                                            new Vector3(position.Y + squareSize * i, position.X + squareSize * (31 - j), -1.0f));
                        if (map[i, j].objectType == ObjectType.Wall) map[i, j].square.SetVao(ref Wallvao);
                        if (map[i, j].objectType == ObjectType.Coin) map[i, j].square.SetVao(ref Coinvao);
                        if (map[i, j].objectType == ObjectType.Powerpellet) map[i, j].square.SetVao(ref Powervao);
                    }
                }
        }
        public void Render(){
            texturedMap.vao.Bind();
            texturedMap.Render();
            texturedMap.vao.Unbind();
            Square prevSquare = map[0,0].square;
            for (int i = 0; i < 28; i++)
                  for (int j = 0; j < 31; j++){
                    //  if (map[i, j].objectType != ObjectType.Air) continue;
                    if (map[i, j].objectType != ObjectType.Coin 
                        && map[i, j].objectType != ObjectType.Powerpellet) continue;
                    if (prevSquare.vao.vaoID != map[i, j].square.vao.vaoID){
                          prevSquare.vao.Unbind();
                          map[i, j].square.vao.Bind();
                      }
                      map[i, j].square.Render();
                      prevSquare = map[i, j].square;
                  }
             // map[27, 30].square.vao.Unbind();
        }
    }
    class Square:Engine.Object {
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector3 scale;
        int vertices;
        Texture texture;
        public Square(Vector3 scale, Vector3 position, string path){
            texture = !path.isNull() ? new Texture(path) : new Texture();
            this.scale = scale;
            this.position = position;
            simpleShape = SimpleShape.Rectangle;
        }

        public Square(Vector3 scale, Vector3 position,VAO _vao = null, string path = null, float[] textureCoords = default): this( 
            scale,position,path) {
            string vertexsource = @" 
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec2 aTex;  
        out vec3 Color;
        out vec2 TexCoord;
        uniform mat4 transform;
        uniform mat4 projection;
        void main(){
            TexCoord = aTex;
            gl_Position = projection * transform * vec4(aPos,1.0f);       
        }";
            string fragmentsource = @"
        #version 330 core
        out vec4 FragColor;
        in vec2 TexCoord;
        uniform sampler2D Texture;
        void main(){
           FragColor = texture(Texture,TexCoord);
        }";
            shader = new Shader(vertexsource, fragmentsource);
            vertexdata = GetSimpleShape();

            vertices = vertexdata.vectors.Length / 3;
            if (_vao != null) vao = _vao;
            else{
                vao = new VAO();
                vbo = new VBO[3];
                vbo[0] = new VBO(vertexdata.vectors, 3, 0, 0);
                vbo[1] = new VBO(textureCoords, 2, 0, 1);
                if (!texture.isNull()){
                    vao.Bind();
                    vao.StoreData(vbo[0]);
                    vao.StoreData(vbo[1]);
                    vao.Unbind();
                }
            }
        }
      
        public void SetVao(ref VAO _vao){
            if (_vao == null){
                _vao = new VAO();
                vbo = new VBO[3];
                vbo[0] = new VBO(vertexdata.vectors, 3, 0, 0);
                vbo[1] = new VBO(colorInfo, 3, 0, 1);
                _vao.Bind();
                _vao.StoreData(vbo[0]);
                _vao.StoreData(vbo[1]);
                _vao.Unbind();
            }
            vao = _vao;
        }
        float[] colorInfo;
        public Square(float[] colorInfo,Vector3 scale,Vector3 position):this(scale,position,null){
            this.colorInfo = colorInfo;
            string vertexsource = @" 
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aColor;
        out vec3 Color;
        uniform mat4 transform;
        uniform mat4 projection;
        void main(){
            Color = aColor;
            gl_Position = projection * transform * vec4(aPos,1.0f);       
        }";
            string fragmentsource = @"
        #version 330 core
        out vec4 FragColor;
        in vec3 Color;
        void main(){
            FragColor = vec4(Color,1.0f); 
        }";
             
            shader = new Shader(vertexsource, fragmentsource);
            vertexdata = GetSimpleShape();
            vertices = vertexdata.vectors.Length / 3;
        }
        #region BringThatIntoAnimationClass
        public string[] textures; 
        float timeToAnimate;
        int currentTex = 0;
        public string[] GetTextue { get { return textures; } }
        bool repeatOnce = false;
        public void SetAnimation(string[] paths,float time,bool repeatonce = false){         
            textures = paths; timeToAnimate = time; this.repeatOnce = repeatonce;
        }
        public void StopAnimation() => textures = null;
        public void StopAnimation(int initialPic){
            ChangeTexture(textures[initialPic]); textures = null; 
        }
        public void StopAnimation(string initialPic) {
            ChangeTexture(initialPic); textures = null;
        }
        //public void StopAnimation() => textures = null;

        public void ChangeTexture(string path) { texture.Create(path); }
        #endregion
        public override void Render(Engine.Object obj = null){
            if (!isRenderable) return;
            shader.Use();

            if (!texture.isNull()){
                texture.Use();
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            Matrix4 model = Matrix4.Identity;
            model *= Matrices.Transformation(rotation, scale);
            model *= Matrix4.CreateTranslation(position);
            shader.SetMatrix4(ref model, "transform");
            shader.SetMatrix4(ref camera.projection, "projection");
            GL.DrawArrays(type, 0, vertices);
            if (!texture.isNull()) GL.Disable(EnableCap.Blend);           
        }
        bool renderable = true;
        public override bool isRenderable { get => renderable; set => renderable = value; }
        float RealTime;
        public override void Update(float deltaTime){
            if(textures != null){
                RealTime += deltaTime;
                if (RealTime >= timeToAnimate) {
                    RealTime = 0;
                    currentTex++;
                    if (currentTex < textures.Length) ChangeTexture(textures[currentTex]);
                    else { if (repeatOnce) { 
                            textures = null; return; } currentTex = 0; ChangeTexture(textures[0]); }
                }
            }
        }
        public override Light light => throw new NotImplementedException();
        public override Camera camera { get => Game.camera; set => throw new NotImplementedException(); }
    }
}   