using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Program;
using Boolean = System.Boolean;

namespace Painters_Algorithm
{
    public class Game: MainRenderWindow
    {
        private struct Player
        {
            public Vector2 Pos;
            public Vector2 LookDir;
            public Vector2 Right;
            public float LookAngle;
        };
        private class Boundary
        {
            public Vector2 Pos;
            public Vector2 Pos1, Pos2;
            public Texture Texture;
            public Color4 Color;
            public float Length, Radius;
            public const int TypeCircle = 1, TypeLine = 0;
            public int Type;

            public void CalcLength()
            {
                if (Type == TypeLine)
                {
                    Vector2 t = Pos1 - Pos2;
                    Length = t.Length;
                }
                else
                {
                    Length = 2 * (float)Math.PI * Radius;
                }
                
            }
        }

        private Player _player;
        private int _xsize = 48, _ysize = 48;
        private Boolean _firstMove = true;
        private Vector2 _lastPos;
        private readonly List<Boundary> _boundaries = new List<Boundary>();
        private int _textXSize = 50;
        private readonly Texture[] _texture = new Texture[9];
        
        public Game(int width, int height, string title)
            : base(width, height, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            setClearColor(new Color4(0.0f, 0.0f, 0.0f, 1.0f)); //Sets Background Color
            UseDepthTest = false; //Enables Depth Testing for 3D
            RenderLight = false; //Makes the 3D light visible
            UseAlpha = true; //Enables alpha use
            KeyboardAndMouseInput = false; //Enables keyboard and mouse input for 3D movement
            base.OnLoad(e);
            CursorVisible = false;

            _player = new Player
            {
                Pos = new Vector2(300, Height / 2f),
                Right = new Vector2(-1, 0),
                LookAngle =  3 * (float)Math.PI / 2,
            };
            
            _texture[0] = new Texture("pics/eagle.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[1] = new Texture("pics/redbrick.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[2] = new Texture("pics/purplestone.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[3] = new Texture("pics/greystone.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[4] = new Texture("pics/bluestone.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[5] = new Texture("pics/mossy.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[6] = new Texture("pics/wood.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[7] = new Texture("pics/colorstone.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _texture[8] = new Texture("pics/greystone.png", TextureMinFilter.Linear, TextureMagFilter.Linear);
            
            CreateRectangleBoundary(100f, 100f, 900f, 900f, _texture[1], Color4.White);
            CreateTriangleBoundary(500f, 400f, 400f, 600f, 600f, 600f, _texture[3], new Color4(1f, 1f, 1f, 0.25f));
            //CreateCircleBoundary(500f, 500f, 100f, _texture[3], new Color4(1f, 1f, 1f, 0.25f));

            foreach (var b in _boundaries)
            {
                b.CalcLength();
            }
        }

        private void CreateRectangleBoundary(float x1, float y1, float x2, float y2, Texture tex, Color4 color4)
        {
            _boundaries.Add(new Boundary{Pos1 = new Vector2(x1, y1), Pos2 = new Vector2(x2, y1), Texture = tex, Type = Boundary.TypeLine, Color = color4});
            _boundaries.Add(new Boundary{Pos1 = new Vector2(x2, y1), Pos2 = new Vector2(x2, y2), Texture = tex, Type = Boundary.TypeLine, Color = color4});
            _boundaries.Add(new Boundary{Pos1 = new Vector2(x2, y2), Pos2 = new Vector2(x1, y2), Texture = tex, Type = Boundary.TypeLine, Color = color4});
            _boundaries.Add(new Boundary{Pos1 = new Vector2(x1, y2), Pos2 = new Vector2(x1, y1), Texture = tex, Type = Boundary.TypeLine, Color = color4});
        }

        private void CreateTriangleBoundary(float x1, float y1,float x2, float y2,float x3, float y3, Texture tex, Color4 color4)
        {
            _boundaries.Add(new Boundary{Pos1 = new Vector2(x1, y1), Pos2 = new Vector2(x2, y2), Texture = tex, Type = Boundary.TypeLine, Color = color4});
            _boundaries.Add(new Boundary{Pos1 = new Vector2(x2, y2), Pos2 = new Vector2(x3, y3), Texture = tex, Type = Boundary.TypeLine, Color = color4});
            _boundaries.Add(new Boundary{Pos1 = new Vector2(x3, y3), Pos2 = new Vector2(x1, y1), Texture = tex, Type = Boundary.TypeLine, Color = color4});
        }

        private void CreateCircleBoundary(float x, float y, float r, Texture tex, Color4 color4 )
        {
            _boundaries.Add(new Boundary{Pos = new Vector2(x, y), Radius = r, Texture = tex, Type = Boundary.TypeCircle, Color = color4});
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Clear();

            
            //RenderFloor(_player);
            UpdateBoundaries(_boundaries);
            //ShowBoundaries2D(_boundaries);
            //ShowPlayer();

            
            base.OnRenderFrame(e);

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = Keyboard.GetState();
            const float playerSpeed = 250f;
            const float sensitivity = 0.001f;
            if (Focused)
            {
                if (input.IsKeyDown(Key.W))
                {
                    Vector2 newPos = _player.Pos + _player.LookDir * playerSpeed * (float) e.Time; // Forward
                    _player.Pos = UpdateCollisions(newPos);
                }

                if (input.IsKeyDown(Key.S))
                {
                    Vector2 newPos = _player.Pos - _player.LookDir * playerSpeed * (float) e.Time; // Backwards
                    _player.Pos = UpdateCollisions(newPos);
                }

                if (input.IsKeyDown(Key.A))
                {
                    Vector2 newPos = _player.Pos + _player.Right * (playerSpeed / 2) * (float) e.Time; // Left
                    _player.Pos = UpdateCollisions(newPos);
                   
                }

                if (input.IsKeyDown(Key.D))
                {
                    Vector2 newPos = _player.Pos - _player.Right * (playerSpeed / 2) * (float) e.Time; // Left
                    _player.Pos = UpdateCollisions(newPos);
                    
                }

                // Get the mouse state
                var mouse = Mouse.GetState();

                if (_firstMove) // this bool variable is initially set to true
                {
                    _lastPos = new Vector2(mouse.X, mouse.Y);
                    _firstMove = false;
                }
                else
                {
                    // Calculate the offset of the mouse position
                    var deltaX = mouse.X - _lastPos.X;
                    _lastPos = new Vector2(mouse.X, mouse.Y);

                    // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                    _player.LookAngle += deltaX * sensitivity;

                    if (_player.LookAngle > (float) Math.PI * 2) { _player.LookAngle -= (float) Math.PI * 2; }
                    if (_player.LookAngle < 0) { _player.LookAngle += (float) Math.PI * 2; }
                
                    _player.LookDir = new Vector2((float) Math.Cos(_player.LookAngle), (float) Math.Sin(_player.LookAngle));
                    _player.Right = new Vector2((float) Math.Sin(_player.LookAngle), (float) -Math.Cos(_player.LookAngle));
                }

                Mouse.SetPosition(1920f / 2f, 1080f / 2f);
            }
            base.OnUpdateFrame(e);
        }

        private Vector2 UpdateCollisions(Vector2 newPos)
        {
            List<Vector2> outVecs = new List<Vector2>();
            Boolean hasToCorrect = false;
            
            
            for (int i = 0; i < _boundaries.Count; i++)
            {
                if (_boundaries[i].Type == Boundary.TypeLine)
                {
                    float len = (_boundaries[i].Pos2 - _boundaries[i].Pos1).Length;
                    float dot = ( ((newPos.X - _boundaries[i].Pos1.X )*(_boundaries[i].Pos2.X - _boundaries[i].Pos1.X)) + ((newPos.Y - _boundaries[i].Pos1.Y) * (_boundaries[i].Pos2.Y - _boundaries[i].Pos1.Y)) ) / (float)Math.Pow(len,2);
                    Vector2 closest = new Vector2(_boundaries[i].Pos1.X + dot * (_boundaries[i].Pos2.X-_boundaries[i].Pos1.X), _boundaries[i].Pos1.Y + dot * (_boundaries[i].Pos2.Y - _boundaries[i].Pos1.Y));

                    if ((newPos - closest).Length < 10f && LinePoint(_boundaries[i].Pos1, _boundaries[i].Pos2, closest))
                    {
                        outVecs.Add(closest);
                        hasToCorrect = true;
                    }
                }
                else
                {
                    Vector2 lookAt = _boundaries[i].Pos - _player.Pos;
                    if (lookAt.Length < _boundaries[i].Radius) { lookAt *= -1;}
                    Vector2 closest = Vector2.Zero;
                    var a = 0f;
                    RayIntCircle(_player.Pos, lookAt, _boundaries[i].Pos, _boundaries[i].Radius, ref closest,
                        ref a);


                    if ((newPos - closest).Length < 10f)
                    {
                        outVecs.Add(closest);
                        hasToCorrect = true;
                    }
                }
            }
            
            if(!hasToCorrect) {return newPos;}

            Vector2 curPos = newPos;
            foreach (var vector in outVecs)
            {
                Vector2 u = curPos - vector;
                float d = u.Length;
                u.Normalize();
                u *= 10f - d;
                curPos = curPos + u;
            }
            
            return curPos;
            
        }

        private Boolean LinePoint(Vector2 a, Vector2 b, Vector2 p) {
            Vector2 pa = p - a;
            Vector2 pb = p - b;
            float d1 = pa.Length;
            float d2 = pb.Length;

            Vector2 ab = b - a;
            float lineLen = ab.Length;
            
            float buffer = 0.1f;

            if (d1+d2 >= lineLen-buffer && d1+d2 <= lineLen+buffer) {
                return true;
            }
            return false;
        }
        private void ShowBoundaries2D(List<Boundary> boundaries)
        {
            for (var i = 0; i < boundaries.Count; i++)
            {
                if (boundaries[i].Type == Boundary.TypeLine)
                {
                    drawLine(boundaries[i].Pos1.X, boundaries[i].Pos1.Y,boundaries[i].Pos2.X, boundaries[i].Pos2.Y, new Color4(1f,1f,1f,1f));
                }
                else
                {
                    drawEllipse(boundaries[i].Pos.X, boundaries[i].Pos.Y, boundaries[i].Radius, boundaries[i].Radius, Color4.White);
                    drawEllipse(boundaries[i].Pos.X, boundaries[i].Pos.Y, boundaries[i].Radius - 1f, boundaries[i].Radius - 1f, Color4.Black);

                }
            }
        }
        private void UpdateBoundaries(List<Boundary> boundaries)
        {
            int numRays = Width;
            Boolean intersected = false;
            List<float> angles = new List<float>();
            
            for( var x = 0; x <= Width; x++ ){
                var xAng = (float)Math.Atan( ( x - Width / 2 ) / 500f );
                xAng += +_player.LookAngle;
                if (xAng > Math.PI * 2) { xAng -= (float) Math.PI * 2; }
                if (xAng < 0) { xAng += (float) Math.PI * 2; }
                angles.Add(xAng);
            }

            for (int r = 0; r < numRays; r++)
            {
                //float ra = _player.LookAngle;
                float  ra = angles[r];
                Color4 outColor = Color4.Black;
                List<float> dists = new List<float>(); 
                Dictionary<float, int> bound = new Dictionary<float, int>();
                Dictionary<float, float> pos = new Dictionary<float, float>();

                for (var i = 0; i < boundaries.Count; i++)
                {
                    float curDist = 0f;
                    float pIl = 0f;
                    Vector2 o = Vector2.Zero;

                    if (boundaries[i].Type == Boundary.TypeLine)
                    {
                        Vector2 out1 = Vector2.Zero;
                        float k2 = 0f;

                        Boolean test = RayIntLine(boundaries[i].Pos1, boundaries[i].Pos2, _player.Pos,
                            new Vector2((float)Math.Cos(ra), (float)Math.Sin(ra)), ref out1, ref k2);
                    
                        if(!test){continue;}


                        Vector2 playerToInt = _player.Pos - out1;
                        curDist = playerToInt.Length;
                        pIl = k2;
                        o = out1;

                    }
                    else
                    {
                        Vector2 out1 = Vector2.Zero;
                        float alpha = 0f;

                        Boolean test = RayIntCircle(_player.Pos, new Vector2((float)Math.Cos(ra), (float)Math.Sin(ra)), boundaries[i].Pos,
                            _boundaries[i].Radius, ref out1, ref alpha);
                    
                        if(!test){continue;}

                        Vector2 playerToInt = _player.Pos - out1;
                        curDist = playerToInt.Length;
                        pIl = alpha / (float) Math.PI;
                        o = out1;
                    }
                    

                    //if (curDist < lowDist)
                    //{
                    //lowDist = curDist;
                    dists.Add(curDist);
                    bound.Add(curDist, i);
                    pos.Add(curDist, pIl);
                   
                    intersected = true;
                    
                    //}
                }
                if (!intersected){continue;}
                float ca = _player.LookAngle - ra; if (ca > Math.PI * 2) { ca -= (float)Math.PI * 2; } if (ca < 0) { ca += (float)Math.PI * 2; }
                dists.Sort();
                dists.Reverse();

                foreach (var d in dists)
                {
                    var  renderDist = (float)Math.Cos(ca) * d;
                    int i = bound[d];
                    var  posInLine = pos[d];

                    Texture outText = boundaries[i].Texture;
                    float bLength = boundaries[i].Length;
                    Color4 color = boundaries[i].Color;
                    
                    float lineH = ((Height / _ysize) * Height) / renderDist;
                    float lineO = (Height - lineH) / 2;
                    float realPosInLine = bLength * posInLine;
                    float texCoord = (realPosInLine / _textXSize) - (int)Math.Floor(realPosInLine / _textXSize);
                    drawTexturedLine(r, lineO, texCoord, 0, r, lineH + lineO, texCoord, 1, outText, color);
                }
                
                


               
                //drawLine(_player.Pos.X, _player.Pos.Y, outInt.X, outInt.Y, new Color4(0f, 0.3f, 0.6f, 0.5f));
            }
        }

        private Boolean RayIntLine(Vector2 a, Vector2 b, Vector2 p, Vector2 l, ref Vector2 ouVec, ref float k2)
        {
            Vector2 u = b - a;
            float k1 = (u.Y * (a.X - p.X) + (u.X * (p.Y - a.Y))) /
                       ((l.X * u.Y) - (l.Y * u.X));
            k2 = ((l.X * (p.Y - a.Y)) + (l.Y * (a.X - p.X))) /
                       (l.X * u.Y - l.Y * u.X);
            
            if(k1 < 0 || k2 < 0 || k2 > 1){ return false;}
            

            ouVec = new Vector2(p.X + (k1 * l.X), p.Y + (k1 * l.Y));
            //drawEllipse(ouVec.X, ouVec.Y, 10, 10, Color4.Aqua);
            return true;
        }

        private Boolean RayIntCircle(Vector2 p, Vector2 l, Vector2 c, float r, ref Vector2 ouVec, ref float a)
        {
            float rx = p.X - c.X;
            float ry = p.Y - c.Y;
            float alpha = l.X * rx + l.Y * ry;
            float beta = l.X * l.X + l.Y * l.Y;
            float theta = rx * rx + ry * ry - (r * r);

            float k1 = ((-2 * alpha) + (float) Math.Sqrt((2 * 2 * alpha * alpha) - (4 * beta * theta))) / (2 * beta);
            float k2 = ((-2 * alpha) - (float) Math.Sqrt((2 * 2 * alpha * alpha) - (4 * beta * theta))) / (2 * beta);
            
            float k;
            
            if(k1 < 0f)
            {
                k = k2;
            }
            else if (k2 < 0f)
            {
                k = k1;
            }
            else
            {
                k = k1 * Convert.ToInt16(k1 < k2) + k2 * Convert.ToInt16(k2 <= k1);
            }
            if (k < 0 || float.IsNaN(k)) { return false;}
            
            a = (float) Math.Acos((rx + k * l.X) / r);
            ouVec = new Vector2(p.X + (k * l.X), p.Y + (k * l.Y));
            
            //drawEllipse(ouVec.X, ouVec.Y, 10, 10, Color4.OrangeRed);

            return true;
        }
        
        

        private void ShowPlayer()
        {
            drawEllipse(_player.Pos.X, _player.Pos.Y, 10f, 10f, Color4.Blue);
            drawLine(_player.Pos.X, _player.Pos.Y, _player.Pos.X + (_player.LookDir.X * 25), _player.Pos.Y + (_player.LookDir.Y * 25), Color4.Red);
            //drawLine(_player.Pos.X, _player.Pos.Y, _player.Pos.X + (float)Math.Cos(_player.LookAngle - _player.Fov / 2) * 1000, _player.Pos.Y + (float)Math.Sin(_player.LookAngle - _player.Fov / 2) * 1000, Color4.Aqua);
            //drawLine(_player.Pos.X, _player.Pos.Y, _player.Pos.X + (float)Math.Cos(_player.LookAngle + _player.Fov / 2) * 1000, _player.Pos.Y + (float)Math.Sin(_player.LookAngle + _player.Fov / 2) * 1000, Color4.Aqua);
        }
        
         private void RenderFloor(Player player)
        {
            float scale = Math.Max(Width / 1000f, Height / 1000f);
            float planeX = player.Right.X * scale, planeY = player.Right.Y * scale; //the 2d raycaster version of camera plane
            float dirX = player.LookDir.X, dirY = player.LookDir.Y; //initial direction vector

            int size = Math.Min(Height, Width);
            
            for(int y = Height / 2; y < Height + 10f; y++)
            {
              // rayDir for leftmost ray (x = 0) and rightmost ray (x = w)
              float rayDirX0 = dirX + planeX;
              float rayDirY0 = dirY + planeY;
              float rayDirX1 = dirX - planeX;
              float rayDirY1 = dirY - planeY;

              // Current y position compared to the center of the screen (the horizon)
              int p = y - (Height / 2);

              // Vertical position of the camera.
              float posZ = 0.5f * Height;

              // Horizontal distance from the camera to the floor for the current row.
              // 0.5 is the z position exactly in the middle between floor and ceiling.
              float rowDistance = posZ / p;

              // calculate the real world step vector we have to add for each x (parallel to camera plane)
              // adding step by step avoids multiplications with a weight in the inner loop
              float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / size;
              float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / size;

              // real world coordinates of the leftmost column. This will be updated as we step to the right.
              float floorX = (player.Pos.X / (size / _xsize)) + rowDistance * rayDirX0;
              float floorY = (player.Pos.Y / (size / _ysize)) + rowDistance * rayDirY0;

              
              int cellX = (int)(floorX);
              int cellY = (int)(floorY);

              // get the texture coordinate from the fractional part
              float tx1 = ((int) (64 * (floorX - cellX))) / 64f;
              float ty1 = ((int) (64 * (floorY - cellY))) / 64f;
              
              float tx2 = ((int) (64 * ((floorX + (floorStepX * size)) - cellX))) / 64f;
              float ty2 = ((int) (64 * ((floorY + (floorStepY * size)) - cellY))) / 64f;

              Texture ceiling = _texture[6];
              Texture floor = _texture[8];
              
              //var bobOffset = (float) Math.Sin(_bobAngle) * 10f;

              
              //drawTexturedLine(0, y + bobOffset, tx1, ty1, Width, y + bobOffset, tx2, ty2, floor, new Color4(0.4f, 0.4f, 0.4f, 1f));
              //drawTexturedLine(0, Height - y - 1 + bobOffset, tx1, ty1, Width, Height - y - 1 + bobOffset, tx2, ty2, ceiling, new Color4(0.0f, 0.3f, 0.7f, 1f));
   
              drawTexturedLine(0, y, tx1, ty1, Width, y, tx2, ty2, floor, new Color4(0.4f, 0.4f, 0.4f, 1f));
              drawTexturedLine(0, Height - y - 1, tx1, ty1, Width, Height - y - 1, tx2, ty2, ceiling, new Color4(0.0f, 0.3f, 0.7f, 1f));

            }
        }

    }
}