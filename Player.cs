using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace Pacman{
    enum Direction { Up = 1, Down = 3, Left = 2, Right = 4 }
    class Player {
        Vector2 nextPosition = new Vector2(0, 0);
        public Direction dir = Direction.Left;
        Direction tempdir = Direction.Left;
        public Square square;
        
        public Vector2 mPos;
        public Vector2 GetMapPosition { get { return mPos; } }
        float squareSize { get { return Pacman.playground.squareSize; } }
        
        public Player(Vector2 mapPosition) {
            this.mPos = mapPosition; 
            Vector2 tempPos = Pacman.playground.getMapPosition((int)mPos.X, (int)mPos.Y);
            square = new Square(
                                new Vector3(squareSize*1.5f), new Vector3(tempPos.X, tempPos.Y, -1f),null,
                                Pacman.ProjectPlace+ @"\res\pacman\pacman1.png", new float[12] {
                                           0.0f, 0.0f,
                                           1.0f, 0.0f,
                                           1.0f, 1.0f,
                                           1.0f, 1.0f, 
                                           0.0f, 1.0f,
                                           0.0f, 0.0f,
                                           });
            nextPosition = new Vector2(square.position.X, square.position.Y);
            square.SetAnimation(new string[3] {Pacman.ProjectPlace+ @"\res\pacman\pacman1.png",
                Pacman.ProjectPlace+@"\res\pacman\pacman2.png",Pacman.ProjectPlace+@"\res\pacman\pacman3.png"},0.04f);
        }
   
        public void Render(){
            square.vao.Bind();
            square.Render();
            square.vao.Unbind();
        }
        bool isOutSide(int x) => x > Pacman.playground.map.GetLength(0)-1 || x < 0;
        Direction isOutSide() => mPos.X > Pacman.playground.map.GetLength(0)-1 ? Direction.Right : Direction.Left;
        
        bool CanMove(float x,float y){
            if (isOutSide((int)x)) return true;
            if (Pacman.playground.map[(int)x, (int)y].objectType == ObjectType.Door) return false;
            if (Pacman.playground.map[(int)x, (int)y].objectType == ObjectType.Wall) return false;
            return true;
        }
        void GetInput(){
            if (Keyboard.GetState().IsKeyDown(Key.A)) tempdir = Direction.Left; 
            else if (Keyboard.GetState().IsKeyDown(Key.W)) tempdir = Direction.Up;
            else if (Keyboard.GetState().IsKeyDown(Key.S)) tempdir = Direction.Down;
            else if (Keyboard.GetState().IsKeyDown(Key.D)) tempdir = Direction.Right;
        }
        void Move(float deltaTime){
            
            GetInput();
            if (!MoveDone) { 
                SmoothMove(deltaTime);
                square.Update(deltaTime);
                return; 
            }
            if (isOutSide((int)mPos.X)) {
                nextPosition.X = square.position.X = isOutSide() == Direction.Right ?
                    Pacman.playground.getMapPosition((int)(mPos.X = 0), (int)mPos.Y).X : 
                    Pacman.playground.getMapPosition((int)(mPos.X = Pacman.playground.map.GetLength(0) - 1), (int)mPos.Y).X;
            }
            square.position = new Vector3(nextPosition.X, nextPosition.Y, -1.0f);
          
            if (Pacman.map[(int)mPos.X, (int)mPos.Y].objectType == ObjectType.Coin) { 
                Pacman.map[(int)mPos.X, (int)mPos.Y].square.isRenderable = false;
                Pacman.playground.map[(int)mPos.X, (int)mPos.Y].objectType = ObjectType.Air;
                Pacman.playground.dots -= 1;
                Pacman.score += 10;
            }
            if (Pacman.map[(int)mPos.X, (int)mPos.Y].objectType == ObjectType.Powerpellet) { 
                Pacman.map[(int)mPos.X, (int)mPos.Y].square.isRenderable = false;
                Pacman.playground.map[(int)mPos.X, (int)mPos.Y].objectType = ObjectType.Air;
                Pacman.ghosts.SetMode(GhostState.Frightened);
            }

            if (!MoveDirection(tempdir)) MoveDirection(dir);
            else dir = tempdir;
        }
        public string[] Death(){
            string[] death = new string[12];
            for (int i = 1; i < 13; i++)
                death[i-1] = Pacman.ProjectPlace + @"\res\pacman\Death" + i + ".png";
            return death;
        }
        bool MoveDirection(Direction dir){
              switch (dir){
                case Direction.Left: 
                    if (!CanMove(mPos.X - 1, mPos.Y)) return false;
                    square.rotation.Z = 180;
                    mPos.X -= 1;
                    nextPosition.X -= squareSize;
                    break;
                case Direction.Up: 
                    if(!CanMove(mPos.X, mPos.Y - 1)) return false;
                    square.rotation.Z = 90;
                    mPos.Y -= 1; 
                    nextPosition.Y += squareSize; 
                    break;
                case Direction.Down: 
                    if(!CanMove(mPos.X, mPos.Y + 1)) return false;
                    square.rotation.Z = -90;
                    mPos.Y += 1;
                    nextPosition.Y -= squareSize; 
                    break;
                case Direction.Right: 
                    if(!CanMove(mPos.X + 1, mPos.Y)) return false; 
                    mPos.X += 1;
                    nextPosition.X += squareSize;
                    square.rotation.Z = 0;
                    break;
            }
            return true;
        } // ghost door: x:14,y:13     15,12
        public void Rotate(Direction dir){
            if(dir == Direction.Left) square.rotation.Z = 180;
            else if (dir == Direction.Right) square.rotation.Z = 0;
            else if (dir == Direction.Down) square.rotation.Z = -90;
            else if (dir == Direction.Up) square.rotation.Z = 90;
        }
        void SmoothMove(float deltaTime) {
            for(int i = 0; i< 18; i++)
            switch (dir){
                case Direction.Left: case Direction.Right:
                        if(Math.Round(square.position.X, 3) > Math.Round(nextPosition.X, 3)) 
                            square.position.X -= 0.01f *( deltaTime);
                        else if(Math.Round(square.position.X, 3) < Math.Round(nextPosition.X, 3)) 
                            square.position.X += 0.01f * (deltaTime); break;
                case Direction.Up: case Direction.Down:
                    if (Math.Round(square.position.Y, 3) < Math.Round(nextPosition.Y, 3)) 
                        square.position.Y += 0.01f * (deltaTime);
                    else if (Math.Round(square.position.Y, 3) > Math.Round(nextPosition.Y, 3)) 
                        square.position.Y -= 0.01f * (deltaTime); break;
            }
        }
        bool MoveDone {
            get { return Math.Round(nextPosition.Y, 3) == Math.Round(square.position.Y, 3) &&
                Math.Round(nextPosition.X, 3) == Math.Round(square.position.X, 3); }
        }

        public void Update(float deltaTime){
            if (square.textures == null) Pacman.Gameover = true;
            else if (Pacman.Died) { Rotate(dir); square.Update(deltaTime); }
            else Move(deltaTime);
        }
    }
}
