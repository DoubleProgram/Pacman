using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Threading;
using Engine;
using System.Diagnostics;

namespace Pacman
{
    enum GhostType { Pinky, Incy, Blinky, Clyde }
    enum GhostState { Frightened, Scatter, Chase, Eaten }
    struct Ghost {
        public GhostState state;
        public Direction dir;
        public Vector2 nextPosition;
        public GhostType type;
        public int y, x;
        public Square square;
        public bool NextMoveIsUpdated;
        public Direction prevDir;
        public Stopwatch InGhostTown;
        public bool isInGhostTown;
        public Ghost(Direction dir, GhostType type,int x, int y) {
            InGhostTown = new Stopwatch();
            isInGhostTown = type != GhostType.Blinky ? true : false;
            this.dir = dir;
            this.type = type;
            state = GhostState.Chase;
            NextMoveIsUpdated = false;
            prevDir = dir;
            this.x = x;
            this.y = y;
            Vector2 tempPos = Pacman.playground.getMapPosition(x, y);
            nextPosition = tempPos;
            square = new Square(
                                new Vector3(Ghosts.squareSize*1.5f), new Vector3(tempPos.X, tempPos.Y, -1f), null,
                                Ghosts.getDirTexture(type,dir)+"_0.png", new float[12] { 0, 0,1, 0,1, 1,1, 1,0, 1,0, 0 });
        }
        public string getDirTexture() => Ghosts.getDirTexture(type, dir,state);
        public void Render(){
            square.vao.Bind();
            square.Render();
            square.vao.Unbind();
        }
        public void GhostHouseAction() {
            if (Pacman.playground.map[x,y + 1].objectType == ObjectType.Door 
                || Pacman.playground.map[x, y + 1].objectType == ObjectType.Wall)
                dir = Direction.Up;  
            else if(Pacman.playground.map[x, y - 1].objectType == ObjectType.Door 
                || Pacman.playground.map[x, y - 1].objectType == ObjectType.Wall)
                dir = Direction.Down;
        }
        public void ExitGhostHouse() {
            if (y == 11 && x == 12)
                isInGhostTown = false;
        }
        public void Update(float deltaTime){
            Move(deltaTime);
            if (prevDir != dir) {
                prevDir = dir;
                if (state == GhostState.Chase || state == GhostState.Scatter)
                    square.SetAnimation(new string[] { getDirTexture() + "_0.png", getDirTexture() + "_1.png" }, 0.04f);
                else if(state == GhostState.Eaten) { 
                    square.ChangeTexture(getDirTexture() +".png");
                }
            }
        }
        Direction isOutSide() => x > Pacman.playground.map.GetLength(0) - 1 ? Direction.Right : Direction.Left;
        bool isOutSide(int x) => x > Pacman.playground.map.GetLength(0) - 1 || x < 0;
        public void Move(float deltaTime){
            if (!MoveDone) {
                SmoothMove(deltaTime);
                square.Update(deltaTime);
                return; 
            }
            if (isOutSide(x)){
                nextPosition.X = square.position.X = isOutSide() == Direction.Right ?
                    Pacman.playground.getMapPosition(x = 0, y).X :
                    Pacman.playground.getMapPosition((int)(x = Pacman.playground.map.GetLength(0) - 1), y).X;
            }
        }
        void SmoothMove(float deltaTime) {
            int frames = (state == GhostState.Eaten) ? 50 : (state == GhostState.Frightened) ? 14 : 18;
            if (state != GhostState.Eaten && (x == 0 || x == 1 || x == 2 || x == 3 || x == 24 || x == 25 || x == 26 || x == 27) && y == 14)
                frames = 8;
            for (int i = 0; i < frames; i++)
                switch (dir){
                    case Direction.Left: case Direction.Right:
                        if(Math.Round(square.position.X, 3) > Math.Round(nextPosition.X, 3)) 
                            square.position.X -= 0.01f *(deltaTime) * Ghosts.speed;
                        else if(Math.Round(square.position.X, 3) < Math.Round(nextPosition.X, 3)) 
                            square.position.X += 0.01f * (deltaTime) * Ghosts.speed; break;
                    case Direction.Up: case Direction.Down:
                        if (Math.Round(square.position.Y, 3) < Math.Round(nextPosition.Y, 3)) 
                            square.position.Y += 0.01f * (deltaTime) * Ghosts.speed;
                        else if (Math.Round(square.position.Y, 3) > Math.Round(nextPosition.Y, 3)) 
                            square.position.Y -= 0.01f * (deltaTime) * Ghosts.speed; break;
                }
        }
       public bool MoveDone {
            get { return Math.Round(nextPosition.Y, 3) == Math.Round(square.position.Y, 3) &&
                Math.Round(nextPosition.X, 3) == Math.Round(square.position.X, 3); }
        }
    }
    class Ghosts {
        public static float speed = 0.8f;
        public static float squareSize { get { return Pacman.playground.squareSize; } }
        public Ghost pinky;
        public Ghost blinky;
        public Ghost clyde;
        public Ghost incy;
        List<int> availablemoves = new List<int>();
        public static string getDirTexture(GhostType type, Direction dir,GhostState state = GhostState.Chase){
            string place = Pacman.ProjectPlace + @"\res\ghosts\";
            string ghost = "";
            if (state == GhostState.Eaten) ghost = "Eaten";
            else
                switch (type){
                    case GhostType.Blinky: ghost = "Blinky"; break;
                    case GhostType.Clyde: ghost = "Clyde"; break;
                    case GhostType.Pinky: ghost = "Pinky"; break;
                    case GhostType.Incy: ghost = "Incy"; break;
            }
            return place + ghost + ((int)dir).ToString();
        }
   
        public Ghosts() {
            pinky = new Ghost(Direction.Down, GhostType.Pinky,13, 14);
            blinky = new Ghost(Direction.Left, GhostType.Blinky, 13, 11);
            clyde = new Ghost(Direction.Up, GhostType.Clyde, 15, 14);
            incy = new Ghost(Direction.Up, GhostType.Incy, 11, 14);
            
            pinky.InGhostTown.Start();
            clyde.InGhostTown.Start();
            incy.InGhostTown.Start();
        }
        public void Render(){
            incy.Render();
            clyde.Render();
            blinky.Render();
            pinky.Render();
        }
        public void Update(float deltaTime){
            if (timeInFrightenMode.ElapsedMilliseconds >= 7000 && !isTimeToEndFrighten){
                isTimeToEndFrighten = true;
                SetAnimation(ref blinky,GhostState.Frightened, true);
                SetAnimation(ref incy, GhostState.Frightened, true);
                SetAnimation(ref pinky, GhostState.Frightened, true);
                SetAnimation(ref clyde, GhostState.Frightened, true);
            }
            InGhostTown();
            GhostThread(ref blinky, deltaTime);
            GhostThread(ref pinky, deltaTime);
            GhostThread(ref clyde, deltaTime);
            GhostThread(ref incy, deltaTime);
        }
        public void GhostThread(ref Ghost ghost, float deltaTime) {
            ghost.Update(deltaTime);
            if (ghost.isInGhostTown && ghost.InGhostTown.IsRunning && ghost.type != GhostType.Blinky) {
                if (ghost.MoveDone){
                    ghost.GhostHouseAction();
                    DirectionMove(ref ghost);
                    ghost.nextPosition = Pacman.playground.getMapPosition(ghost.x, ghost.y);
                }
                return; 
            }
            if (ghost.isInGhostTown) ghost.ExitGhostHouse();

            if (Pacman.player.mPos.Y == ghost.y && Pacman.player.mPos.X == ghost.x) { 
                if (ghost.state == GhostState.Frightened){
                    ghost.state = GhostState.Eaten;
                    ghost.square.StopAnimation();
                    ghost.square.ChangeTexture(ghost.getDirTexture() + ".png");
                }
                else if (!Pacman.Gameover && (ghost.state == GhostState.Chase || ghost.state == GhostState.Scatter)) {
                    Pacman.player.square.SetAnimation(Pacman.player.Death(),0.12f,true);
                    Pacman.lifes--;
                    Pacman.player.Rotate(Pacman.player.dir);
                    Pacman.Died = true;
                }
            }

            if (ghost.MoveDone) {
                CheckForState();
                if (ghost.state != GhostState.Frightened)
                    ghost.dir = (Direction)TargetMove(ref ghost, ghost.isInGhostTown ? true : false);
                else ghost.dir = (Direction)RandomDir(ghost);
                DirectionMove(ref ghost);
                try { ghost.nextPosition = Pacman.playground.getMapPosition(ghost.x, ghost.y); } catch { ; } //very bad....
            }
        }

        #region state
        Stopwatch timeInScatterMode = new Stopwatch();
        Stopwatch timeInFrightenMode = new Stopwatch();
        Stopwatch timeInChaseMode = new Stopwatch();
        bool isTimeToEndFrighten = false;
        public void CheckForState() {
            if (!timeInScatterMode.IsRunning && !timeInFrightenMode.IsRunning && !timeInChaseMode.IsRunning) 
                timeInChaseMode.Start();

            if (timeInChaseMode.ElapsedMilliseconds >= 20000) {
                timeInScatterMode = new Stopwatch();
                timeInScatterMode.Start();
                timeInChaseMode = new Stopwatch();
                SetMode(GhostState.Scatter);
            }

            else if (timeInScatterMode.IsRunning && timeInScatterMode.ElapsedMilliseconds >= 7000){
                timeInScatterMode.Stop();
                SetMode(GhostState.Chase);
            }
            else if (timeInFrightenMode.IsRunning){
                if (timeInFrightenMode.ElapsedMilliseconds >= 10000) {
                    timeInFrightenMode = new Stopwatch();
                    isTimeToEndFrighten = false;
                    SetMode(GhostState.Chase);
                    timeInChaseMode.Start();
                    StopAnimation();
                }
                else {
                    RandomDir(blinky); RandomDir(pinky); RandomDir(clyde); RandomDir(incy);
                }
            }
        }
        public void SetMode(GhostState state) {
            if (state == GhostState.Frightened) { //thats an excuse because we assign the Frightened from outside...
                if (timeInFrightenMode.IsRunning) { timeInFrightenMode.Restart(); }
                else {
                    timeInFrightenMode = new Stopwatch();
                    timeInFrightenMode.Start();
                }
                SetAnimation(ref blinky, GhostState.Frightened);
                SetAnimation(ref incy, GhostState.Frightened);
                SetAnimation(ref clyde, GhostState.Frightened);
                SetAnimation(ref pinky, GhostState.Frightened);
                isTimeToEndFrighten = false;
                timeInScatterMode = new Stopwatch();
                timeInChaseMode.Stop();
            }
            if (blinky.state != GhostState.Eaten)
                SetMode(ref blinky, state);
            if (pinky.state != GhostState.Eaten)
                SetMode(ref pinky, state);
            if (clyde.state != GhostState.Eaten)
                SetMode(ref clyde, state);
            if (incy.state != GhostState.Eaten)
                SetMode(ref incy, state);
        }
        int RandomDir(Ghost ghost){
            availablemoves = new List<int>();
            AvailableMoves(ghost);
            if (availablemoves.Count == 0) return (int)ghost.dir;
            Random rnd = new Random();
            return availablemoves[rnd.Next(0,availablemoves.Count)];
        }

        public void StopAnimation() {
            SetAnimation(ref blinky,GhostState.Chase);
            SetAnimation(ref incy, GhostState.Chase);
            SetAnimation(ref clyde, GhostState.Chase);
            SetAnimation(ref pinky, GhostState.Chase);
        }

        public void SetMode(ref Ghost ghost,GhostState state){
            if (state == GhostState.Frightened && ghost.state != GhostState.Eaten) { 
                ghost.state = state; 
                SetOppositeDir(ref ghost); 
            }
            else if (state == GhostState.Scatter) {
                if (ghost.state != GhostState.Eaten
                        && ghost.state != GhostState.Frightened){
                    ghost.state = state;
                    SetOppositeDir(ref ghost);
                }
            }
            else if(state == GhostState.Eaten) { ghost.state = state; SetOppositeDir(ref ghost); }
            else ghost.state = state; //Chase mode;
        }

        void SetOppositeDir(ref Ghost ghost){
            if (ghost.dir == Direction.Down) ghost.dir = Direction.Up;
            else if (ghost.dir == Direction.Up) ghost.dir = Direction.Down;
            else if (ghost.dir == Direction.Right) ghost.dir = Direction.Left;
            else ghost.dir = Direction.Right;
        }
    #endregion

        void InGhostTown(){
            if (pinky.InGhostTown.ElapsedMilliseconds >= 2000 && pinky.InGhostTown.IsRunning)
                pinky.InGhostTown.Stop();
            if (incy.InGhostTown.ElapsedMilliseconds >= 6000 && incy.InGhostTown.IsRunning)
                incy.InGhostTown.Stop();
            if (clyde.InGhostTown.ElapsedMilliseconds >= 10000 && clyde.InGhostTown.IsRunning)
                clyde.InGhostTown.Stop();
        }
      
        void SetAnimation(ref Ghost ghost,GhostState state,bool frightenToEnd = false){
            if(frightenToEnd ) {
                if(ghost.state == GhostState.Frightened)
                    ghost.square.SetAnimation(new string[] { Pacman.ProjectPlace + @"\res\ghosts\Frightened_1.png", Pacman.ProjectPlace + @"\res\ghosts\Frightened1_0.png",
                        Pacman.ProjectPlace + @"\res\ghosts\Frightened_1.png" }, 0.04f);
                return;
            }
            switch (state){
                case GhostState.Chase:
                    if (ghost.state != GhostState.Eaten)
                        ghost.square.SetAnimation(new string[] { ghost.getDirTexture() + "_0.png", ghost.getDirTexture() + "_1.png" }, 0.04f); 
                    break;
                case GhostState.Frightened: 
                    if(ghost.state != GhostState.Eaten) 
                        ghost.square.SetAnimation(new string[] { Pacman.ProjectPlace + @"\res\ghosts\Frightened_0.png", Pacman.ProjectPlace + @"\res\ghosts\Frightened_1.png" }, 0.04f); 
                    break;
            }
        }

        int Hypotenuse(Vector2 ghostmove, Vector2 target){
            Vector2 diff = ghostmove - target;
            Vector2 vec2 = ghostmove;
            int xplier, yplier;
            xplier = diff.X > 0 ? -1 : 1;
            yplier = diff.Y > 0 ? -1 : 1;
            while (vec2 != target){
                if (vec2.X != target.X) vec2.X += xplier;
                if (vec2.Y != target.Y) vec2.Y += yplier;
            }
            vec2 -= ghostmove;
            if (vec2.Y < 0) vec2.Y *= -1;
            if (vec2.X < 0) vec2.X *= -1;
            int hypotenuse = (int)(vec2.Y * vec2.Y + vec2.X * vec2.X);
            return hypotenuse;
        }
        
        int TargetMove(ref Ghost ghost,bool targeted) {
            availablemoves = new List<int>();
            AvailableMoves(ghost); // availablemoves for the ghost as next step.
            if (availablemoves.Count == 0) return (int)ghost.dir; //it should happen when the ghost is going outside left or Right
            int listsize = availablemoves.Count;
            Vector2 target = new Vector2();
            Vector2 nxtPosMove;
            int[] hypmoves = new int[listsize]; // all hypotenuse moves
            hypmoves[0] = 0;
            for (int i = 0; i < listsize; i++) {
                nxtPosMove = new Vector2(ghost.x, ghost.y);
                ChangePos((Direction)availablemoves[i], ref nxtPosMove, 1);
                target = Pacman.player.mPos;
                if (targeted) { hypmoves[i] = Hypotenuse(nxtPosMove, new Vector2(13, 11)); continue; }
                switch (ghost.type) {
                    case GhostType.Blinky:
                        if (ghost.state == GhostState.Scatter) target = new Vector2(28, 0);
                        if (ghost.state == GhostState.Eaten) target = new Vector2(14, 13);
                        
                        hypmoves[i] = Hypotenuse(nxtPosMove, target); 
                        break;

                    case GhostType.Pinky:
                        if (ghost.state == GhostState.Scatter) target = new Vector2(0, 0);
                        if (ghost.state == GhostState.Eaten) target = new Vector2(14, 13);
                        
                        else if (Pacman.player.dir == Direction.Up){
                            target.X -= 4;
                            target.Y += 4;
                        }
                        else ChangePos(Pacman.player.dir, ref target, 4);
                        hypmoves[i] = Hypotenuse(nxtPosMove, target);
                        break;
                    
                    case GhostType.Clyde:
                        if (ghost.state == GhostState.Scatter){
                            target = new Vector2(0, 30);
                            hypmoves[i] = Hypotenuse(nxtPosMove, target);
                        }
                        else if (ghost.state == GhostState.Eaten) {
                            target = new Vector2(14, 13);
                            hypmoves[i] = Hypotenuse(nxtPosMove, target);
                        }
                        else {
                            Vector2 vec = nxtPosMove - target;
                            if (vec.Y < 0) vec.Y *= -1;
                            if (vec.X < 0) vec.X *= -1;
                            if (vec.Y > 8 && vec.X > 8) hypmoves[i] = Hypotenuse(nxtPosMove, target);
                            else {
                                target = new Vector2(0, 30);
                                hypmoves[i] = Hypotenuse(nxtPosMove, target);
                            }
                        }
                        break;

                    case GhostType.Incy:
                        if (ghost.state == GhostState.Scatter) target = new Vector2(28, 30);
                        if (ghost.state == GhostState.Eaten) target = new Vector2(14, 13);
                        else { 
                            if (Pacman.player.dir == Direction.Up){
                                target.X -= 2;
                                target.Y += 2;
                            }
                            else ChangePos(Pacman.player.dir, ref target, 2);
                            Vector2 distance = new Vector2(blinky.x - target.X, blinky.y - target.Y);
                            target -= distance;
                        }
                        hypmoves[i] = Hypotenuse(nxtPosMove, target);
                        break;
                }
            }
            if (ghost.state == GhostState.Eaten && ghost.x == target.X && ghost.y == target.Y) { 
                ghost.state = GhostState.Chase;
                return 1;
            }
            if (ghost.state == GhostState.Chase && Pacman.playground.map[ghost.x, ghost.y].objectType == ObjectType.Door) return 1;
            int prev = hypmoves[0];
            int prev_i = 0;
            for (int i = 1; i < listsize; i++)
                if (hypmoves[i] < prev) { prev = hypmoves[i]; prev_i = i; }
                else if (hypmoves[i] == prev) {
                    //Logger.Console(Log.Error, "Overflow:" + GivePriorityDir(i, prev_i).ToString() + " ghost: " + ghost.type);
                    prev = hypmoves[prev_i = GivePriorityDir(i, prev_i)];
                    // here is the problem, the thing is that the GivePriorityDir does return a overflow value... //STILL NOT SURE IF THAT IS WORKING RIGHT!!!!!!
                }
            return availablemoves[prev_i];
        }
        void AvailableMoves(Ghost ghost){
            if (ghost.dir != Direction.Down && CanMove(Direction.Up, ghost.y, ghost.x,ghost.state))
                availablemoves.Add(1);
            if (ghost.dir != Direction.Up && CanMove(Direction.Down, ghost.y, ghost.x,ghost.state))
                availablemoves.Add(3);
            if (ghost.dir != Direction.Left && CanMove(Direction.Right, ghost.y, ghost.x, ghost.state))
                availablemoves.Add(4);
            if (ghost.dir != Direction.Right && CanMove(Direction.Left, ghost.y, ghost.x, ghost.state))
                availablemoves.Add(2);
        }

        int GivePriorityDir(int one, int two){
            if (availablemoves[one] < availablemoves[two]) return one;
            return two;
        }

        void DirectionMove(ref Ghost ghost)=>ChangePos(ghost.dir, ref ghost.x,ref ghost.y, 1);
        
        void ChangePos(Direction dir, ref Vector2 vec2,int increase){
             switch(dir){
                case Direction.Right: vec2.X += increase; break;
                case Direction.Left: vec2.X -= increase;  break;
                case Direction.Down: vec2.Y += increase; break;
                case Direction.Up: vec2.Y -= increase; break;
            }
        }
        void ChangePos(Direction dir,ref int x,ref int y, int increase){
            Vector2 vec2 = new Vector2(x,y);
            ChangePos(dir, ref vec2, increase);
            x = (int)vec2.X; y = (int)vec2.Y;
        }

        bool isObstacle(int y, int x, GhostState state) => Pacman.playground.map[x, y].objectType == ObjectType.Wall;
        
        bool isOutside(int y, int x) => x >= 28 || x <= 0 || y >= 31 || y <= 0;
        bool CanMove(Direction dir, int y, int x,GhostState state){
            int dy = 0, dx = 0; //destination
            if (dir == Direction.Up) { 
                if (!isOutside(y + dy, x + dx) && state != GhostState.Eaten){
                    if ((x == 15 && y == 23) || (x == 12 && y == 23))
                        return false;
                    if ((x == 15 && y == 11) || (x == 12 && y == 11))
                        return false;
                }
                dy = -1;
            }
            else if (dir == Direction.Down) { 
                dy = 1;
                if (!isOutside(y+dy,x+dx) && state != GhostState.Eaten && 
                    Pacman.playground.map[x + dx, y + dy].objectType == ObjectType.Door) return false;
            }
            else if (dir == Direction.Left) dx = -1;
            else if (dir == Direction.Right) dx = 1;
            return !isOutside(y + dy, x + dx) && !isObstacle(y + dy, x + dx,state);
        }
    }
}