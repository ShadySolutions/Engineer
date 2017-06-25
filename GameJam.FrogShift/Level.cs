﻿using Engineer.Engine;
using Engineer.Mathematics;
using Engineer.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameJam.FrogShift
{
    public class Level
    {
        public static void Create(Scene2D CScene, ExternRunner Runner)
        {
            SeqGen SG = new SeqGen();

            List<int> LilipadList = SG.GenerateSequence(99);
            List<int> LilipadArtList = SG.GenerateArtIndexSequence(100, 2);

            LilipadList.Insert(0, 1);

            DrawnSceneObject Back = GameLogic.CreateStaticSprite("Back", global::GameJam.FrogShift.Properties.Resources.pozadina2, new Vertex(0, 0, 0), new Vertex(1920, 850, 0));
            CScene.Data["Back"] = Back;
            CScene.AddSceneObject(Back);

            DrawnSceneObject Back2 = GameLogic.CreateStaticSprite("Back2", global::GameJam.FrogShift.Properties.Resources.podvodom, new Vertex(0, 850, 0), new Vertex(1920, 850, 0));
            CScene.Data["Back2"] = Back2;
            CScene.AddSceneObject(Back2);

            DrawnSceneObject WaterSurface = GameLogic.CreateStaticSprite("WaterSurface", global::GameJam.FrogShift.Properties.Resources.voda1, new Vertex(0, 825, 0), new Vertex(1920, 50, 0));
            CScene.Data["WaterSurface"] = WaterSurface;
            //((Sprite)(WaterSurface.Representation)).SpriteSets[0].Sprite.Add(global::GameJam.FrogShift.Properties.Resources.voda2);
            //((Sprite)(WaterSurface.Representation)).SpriteSets[0].Sprite.Add(global::GameJam.FrogShift.Properties.Resources.voda3);
            CScene.AddSceneObject(WaterSurface);

            for (int i = 0; i < LilipadList.Count; i++)
            {
                DrawnSceneObject Floor = GameLogic.CreateStaticSprite("Floor" + i, ResourceManager.Images["lokvanj"+LilipadArtList[i]], new Vertex(LilipadList[i] * 180, 830, 0), new Vertex(200, 30, 0));
                CScene.AddSceneObject(Floor);                
                ((List<DrawnSceneObject>)(CScene.Data["Colliders"])).Add(Floor);
            }

            DrawnSceneObject Water = GameLogic.CreateStaticSprite("Water", global::GameJam.FrogShift.Properties.Resources.voda, new Vertex(0, 850, 0), new Vertex(1920, 2000, 0));
            CScene.Data["Water"] = Water;
            CScene.AddSceneObject(Water);
        }
    }
}
