using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireSphere : DbgPrimWire
    {
        private static DbgPrimGeometryData GeometryData = null;

        public DbgPrimWireSphere(Transform location, Color color)
        {
            KeepBuffersAlive = true;
            
            Transform = location;

            if (GeometryData != null)
            {
                SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
            }
            else
            {
                int numVerticalSegments = 11;
                int numSidesPerSegment = 12;

                float radius = 1;

                var topPoint = Vector3.Up * radius;
                var bottomPoint = Vector3.Down * radius;
                var points = new Vector3[numVerticalSegments, numSidesPerSegment];

                for (int i = 0; i < numVerticalSegments; i++)
                {
                    for (int j = 0; j < numSidesPerSegment; j++)
                    {
                        float horizontalAngle = (1.0f * j / numSidesPerSegment) * MathHelper.TwoPi;
                        float verticalAngle = ((1.0f * (i + 1) / (numVerticalSegments + 1)) * MathHelper.Pi) - MathHelper.PiOver2;
                        float altitude = (float)Math.Sin(verticalAngle);
                        float horizontalDist = (float)Math.Cos(verticalAngle);
                        points[i, j] = new Vector3((float)Math.Cos(horizontalAngle) * horizontalDist, altitude, (float)Math.Sin(horizontalAngle) * horizontalDist) * radius;
                    }
                }

                for (int i = 0; i < numVerticalSegments; i++)
                {
                    for (int j = 0; j < numSidesPerSegment; j++)
                    {
                        // 在底部，我们必须把每一个都连到底部那个点上
                        if (i == 0)
                        {
                            AddLine(points[i, j], bottomPoint, color);
                        }
                        // 在顶部，我们必须把每一个点都连到顶部
                        // Note: this isn't "else if" because with 2 segments, 
                        // 对于唯一的那一环，这两条都成立
                        if (i == numVerticalSegments - 1)
                        {
                            AddLine(points[i, j], topPoint, color);
                        }

                        // 画出从这一环向上连的竖线 
                        // 水平环连到上面那一环
                        // 因为我们要做连接 
                        // (current) -> (the one above current)
                        // 最后一个就不需要这么做了。
                        if (i < numVerticalSegments - 1)
                        {
                            AddLine(points[i, j], points[i + 1, j], color);
                        }


                        // 画出把各个点水平连起来的线
                        //---- 如果到了末尾，就要绕回来， 
                        //---- 否则就简单连到下一个
                        if (j == numSidesPerSegment - 1)
                        {
                            AddLine(points[i, j], points[i, 0], color);
                        }
                        else
                        {
                            AddLine(points[i, j], points[i, j + 1], color);
                        }
                    }
                }

                FinalizeBuffers(true);

                GeometryData = new DbgPrimGeometryData()
                {
                    VertBuffer = VertBuffer,
                    IndexBuffer = IndexBuffer,
                };
            }



        }
    }
}
