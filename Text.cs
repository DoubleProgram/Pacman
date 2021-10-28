using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Engine;
using Engine.Math;
namespace Pacman
{
   /* class Text{
        public float[] vertices ={
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
        };
        public static int vbo, vao;
        Bitmap[] textImages;
        string text;
        Vector3 position;
        public static Shader shader;
        Texture TextTexture = new Texture();
        Matrix4 model;
        public static char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L',
            'M', 'N', 'O', 'P', 'Q', 'R', 'S','T','U','V','W','X','Y','Z'
        };
        public static char[] characters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',',
            ';', ':', '$', '#', '_', '!', '"','/','?','%','&','(',')','@'
        };
        static SpriteSheet BigLettersSheet = new SpriteSheet(@"\res\BigLetters.bmp", 110, 120);
        static SpriteSheet SmallLettersSheet = new SpriteSheet(@"\res\SmallLetters.bmp", 110, 120);
        static SpriteSheet CharactersSheet = new SpriteSheet(@"\res\Characters.bmp", 110, 120);
        public Text(string text,Vector3 position,Vector3 color){
            this.color = color;
            this.text = text;
            this.position = position;
            shader = new Shader(Pacman.ProjectPlace +@"\TextShader.vert", Pacman.ProjectPlace+ @"\TextShader.frag");
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            textImages = CreateText();
            Rotate();
        }
        Vector3 Scale = new Vector3(30f, 30f, 2f);
        Vector3 color = new Vector3(1.0f, 1.0f, 1.0f);
        public Text(string text,Vector3 position,Vector3 color,Vector3 Scale):this(text,position,color){
            this.Scale = Scale;
        }
        void Rotate(){
            for (int i = 0; i < textImages.Length; i++) {
                if (textImages[i] == null) continue;
                textImages[i].RotateFlip(RotateFlipType.Rotate180FlipX);
            }
        }
        Bitmap[] CreateText(){
            Bitmap[] images = new Bitmap[text.Length];
            int? Spritelocation = null;
            int? LoopTrough(char[] letters, char letter){
                for (int i = 0; i < letters.Length; i++)
                    if (letter == letters[i]) return Spritelocation = i;
                return Spritelocation = null;
            }
            for (int i = 0; i < text.Length; i++){
                if (LoopTrough(letters, text[i]).HasValue) 
                    images[i] = BigLettersSheet.getImage((int)Spritelocation);
                char[] smallLetters = new char[letters.Length];
                for(int s = 0; s<smallLetters.Length; s++)
                    smallLetters[s] = Convert.ToChar(letters[s].ToString().ToLower());
                if (LoopTrough(smallLetters, text[i]) != null) 
                    images[i] = SmallLettersSheet.getImage((int)Spritelocation);
                if (LoopTrough(characters, text[i]).HasValue) 
                    images[i] = CharactersSheet.getImage((int)Spritelocation);
            }
            return images;
        }

        public void ChangeText(string text) {
            this.text = text;
            textImages = CreateText();
            Rotate();
        }
        Matrix4 Textprojection;
        public void Render(){
            Console.Write(position);
            shader.Use();
            Textprojection = Matrix4.CreateOrthographic(Game.WIDTH, Game.HEIGHT, (float)Game.WIDTH / Game.HEIGHT, 45);
            shader.SetMatrix4(ref Textprojection, "projection");
            TextTexture.Use();
            shader.SetVectorToUniform("color", color);//color, shader.GetUniformLocation("color"));
            float positionx = position.X;
            //GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            for (int i = textImages.Length-1; i >= 0; i--){
                if (textImages[i] == null) { positionx -= 0.7f; continue; };
                TextTexture.Create(textImages[i]);//new Texture(textImages[i]);
                //
                model = Matrix4.Identity;
                model = Matrices.Transformation(Vector3.Zero,Scale);      //new Vector3(positionx, position.Y, position.Z), Scale.X, Scale.Y, Scale.Z);
                shader.SetMatrix4(ref model, "model");
                positionx -= 0.8f;
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            }
            //GL.Disable(EnableCap.Blend);
        }
    }*/
}
