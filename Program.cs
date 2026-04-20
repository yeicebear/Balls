using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;

/// i just want to go to sleep man
class Program
{
    const float max_hp = 250f;
    const float drainSpeed = 40f; 
    const float SLOW_MO_SCALE = 0.15f;
    const float launch_force_base = 2.54554f;
    const float GRAV = 800f;

    static float hp = max_hp;
    static Vector2 pos = new Vector2(400, 200);
    static Vector2 vel = Vector2.Zero;
    static bool dragging = false;
    static Vector2 dragStart = Vector2.Zero;
    static float ts = 1.0f; /// time scale lol
    static float powerrr = launch_force_base;

    static List<Enemy> guys = new List<Enemy>();
    static float spawn_t = 0f;
    static float rotation_stuff = 0f;

    struct Enemy
    {
        public Vector2 b_pos;
        public float off;
        public float speed;
        public float r;
        public float amp;
        public bool is_moving;
    }

    static void Main()
    {
        Raylib.InitWindow(1280, 720, "Dani's Balls.");
        Raylib.SetTargetFPS(60);

        /// load the stupid texture
        Texture2D globTex = Raylib.LoadTexture("assets/glob.png");
        
        Camera2D cam = new Camera2D();
        cam.Offset = new Vector2(640, 360);
        cam.Rotation = 0.0f;
        cam.Zoom = 1.0f;
        cam.Target = pos;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            float time = (float)Raylib.GetTime();

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                dragging = true;
                dragStart = Raylib.GetMousePosition();
                // Console.WriteLine("DEBUG: click");
            }

            if (dragging)
            {
                ts = SLOW_MO_SCALE;
                hp -= drainSpeed*dt;
                
                if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    vel = (dragStart - Raylib.GetMousePosition()) * powerrr;
                    dragging = false;
                    ts = 1.0f;
                }
            }
            else {
                ts = 1.0f;
            }

            if (hp <= 0) break; /// rip player

            float s_dt = dt * ts;
            vel.Y += GRAV*s_dt;
            pos += vel * s_dt;
            vel.X *= 0.99f;

            /// fix the rotation juice thing
            float target_rot = vel.X * 0.1f;
            rotation_stuff = rotation_stuff + (target_rot - rotation_stuff) * (10f * dt);

            cam.Target = Vector2.Lerp(cam.Target, pos, 5f * dt);

            /// why is floor logic so annoying
            if (pos.Y > 500)
            {
                if (pos.X > 300 && pos.X < 500)
                {
                    pos.Y = 500;
                    vel.Y = 0;
                    powerrr = launch_force_base;
                }
                else {
                    hp -= 100 * dt; 
                }
            }

            spawn_t += dt * ts;
            if (spawn_t > 1.0f)
            {
                for (int i = 0; i < 3; i++) 
                {
                    bool m = Raylib.GetRandomValue(0, 100) > 75;
                    guys.Add(new Enemy {
                        b_pos = new Vector2(cam.Target.X + Raylib.GetRandomValue(-800, 800), cam.Target.Y - Raylib.GetRandomValue(300, 700)),
                        off = (float)Raylib.GetRandomValue(0, 1000) / 100f,
                        speed = m ? (float)Raylib.GetRandomValue(100, 250) : 0f,
                        r = 15f,
                        amp = m ? (float)Raylib.GetRandomValue(50, 150) : 0f,
                        is_moving = m
                    });
                }
                spawn_t = 0;
            }

            for (int i = guys.Count - 1; i >= 0; i--)
            {
                Enemy e = guys[i];
                
                if (e.is_moving) {
                    e.b_pos.Y += e.speed * s_dt;
                }

                float sine_wave = e.is_moving ? (float)Math.Sin(time * 3f + e.off) * e.amp : 0f;
                Vector2 cur_p = new Vector2(e.b_pos.X + sine_wave, e.b_pos.Y);

                if (Raylib.CheckCollisionCircles(pos, 15, cur_p, e.r))
                {
                    powerrr += 1.5f; /// buff launch
                    hp += 75;
                    if(hp > max_hp) hp = max_hp;
                    vel.Y = -800f; /// jump up
                    guys.RemoveAt(i);
                    // Console.WriteLine("DEBUG: hit guy lol");
                }
                else if (e.b_pos.Y > cam.Target.Y + 600) {
                    guys.RemoveAt(i);
                }
                else {
                    guys[i] = e;
                }
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(15, 15, 15, 255));

            Raylib.BeginMode2D(cam);

            Raylib.DrawRectangle(-10000, 520, 20000, 800, Color.Red);
            Raylib.DrawRectangle(300, 500, 200, 20, Color.DarkGray);

            if (dragging)
            {
                Vector2 w_mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cam);
                Vector2 w_start = Raylib.GetScreenToWorld2D(dragStart, cam);
                Raylib.DrawLineEx(pos, pos + (w_start - w_mouse), 3f, Color.Yellow);
            }

            foreach (var g in guys)
            {
                float w = g.is_moving ? (float)Math.Sin(time * 3f + g.off) * g.amp : 0f;
                Raylib.DrawCircleV(new Vector2(g.b_pos.X + w, g.b_pos.Y), g.r, Color.Orange);
            }

            /// i think the scale is right... i hope
            float s = 30f / globTex.Width;
            Raylib.DrawTextureEx(globTex, pos - new Vector2(15, 15), rotation_stuff, s, Color.White);

            Raylib.EndMode2D();

            /// health bar and stuff
            Raylib.DrawRectangle(20, 20, (int)hp * 2, 15, Color.Lime);
            Raylib.DrawText($"Launch: {powerrr:F1}", 20, 45, 20, Color.White);

            if (pos.Y > 500 && (pos.X < 300 || pos.X > 500))
            {
                Raylib.DrawText("LAVA BRUH", 350, 250, 40, Color.Yellow);
            }

            Raylib.EndDrawing();
        }

        /// clean up the mess *we* made. . . :pleading_face: type sh-
        Raylib.UnloadTexture(globTex);
        Raylib.CloseWindow();
    }
}