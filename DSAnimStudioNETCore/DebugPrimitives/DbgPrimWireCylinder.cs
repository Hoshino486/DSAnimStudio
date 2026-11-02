using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireCylinder : DbgPrimWire
    {
        public DbgPrimWireCylinder(Transform location, float range, float height, int numSegments, Color color)
        {
            Transform = location;

            float top = height / 2;
            float bottom = -top;

            for (int i = 0; i < numSegments; i++)
            {
                float angle = (1.0f * i / numSegments) * MathHelper.TwoPi;
                float x = (float)Math.Cos(angle) * range;
                float z = (float)Math.Sin(angle) * range;
                //最后一个要绕回到第一个
                if (i == numSegments - 1)
                {
                    float x_next = (float)Math.Cos(0);
                    float z_next = (float)Math.Sin(0);
                    // 创建一条绕回第一个点的线
                    //---- Top
                    AddLine(new Vector3(x, top, z), new Vector3(x_next, top, z_next), color);
                    //---- Bottom
                    AddLine(new Vector3(x, bottom, z), new Vector3(x_next, bottom, z_next), color);
                }
                else
                {
                    float angle_next = (1.0f * (i + 1) / numSegments) * MathHelper.TwoPi;
                    float x_next = (float)Math.Cos(angle_next);
                    float z_next = (float)Math.Sin(angle_next);

                    // 创建一条连到环中下一个点的线
                    //---- Top
                    AddLine(new Vector3(x, top, z), new Vector3(x_next, top, z_next), color);
                    //---- Bottom
                    AddLine(new Vector3(x, bottom, z), new Vector3(x_next, bottom, z_next), color);
                }

                // 从上到下建一根柱子
                AddLine(new Vector3(x, bottom, z), new Vector3(x, top, z), color);
            }
        }
    }
}
