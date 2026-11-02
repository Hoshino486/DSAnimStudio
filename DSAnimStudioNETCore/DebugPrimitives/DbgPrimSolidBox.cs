using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimSolidBox : DbgPrimSolid
    {
        //public DbgPrimSolidBox(Transform location, Vector3 size, Color color)
        //    : this(location, size, color, color)
        //{

        //}

        public DbgPrimSolidBox(Transform location, Vector3 min, Vector3 max, Color color)
        {
            Transform = location;

            // 下面这些名字的 3 个字母： 
            // [T]op/[B]ottom, [F]ront/[B]ack, [L]eft/[R]ight
            var tfl = new Vector3(min.X, max.Y, max.Z);
            var tfr = new Vector3(max.X, max.Y, max.Z);
            var bfr = new Vector3(max.X, min.Y, max.Z);
            var bfl = new Vector3(min.X, min.Y, max.Z);
            var tbl = new Vector3(min.X, max.Y, min.Z);
            var tbr = new Vector3(max.X, max.Y, min.Z);
            var bbr = new Vector3(max.X, min.Y, min.Z);
            var bbl = new Vector3(min.X, min.Y, min.Z);



            // 顶面
            AddTri(tfl, tbl, tbr, color);
            AddTri(tfl, tbr, tfr, color);

            // 底面
            AddTri(bbl, bfl, bbr, color);
            AddTri(bfl, bfr, bbr, color);

            // 前面
            AddTri(bfl, tfl, tfr, color);
            AddTri(bfl, tfr, bfr, color);

            // 后面
            AddTri(bbr, tbr, tbl, color);
            AddTri(tbr, tbl, bbl, color);

            // 左面
            AddTri(bbl, tbl, tfl, color);
            AddTri(bbl, tfl, bfl, color);

            // 右侧面
            AddTri(bfr, tfr, tbr, color);
            AddTri(bfr, tbr, bbr, color);
        }

        //public DbgPrimSolidBox(Transform location, Vector3 size, Color topColor, Color bottomColor)
        //{
        //    Transform = location;
        //    NameColor = new Color((topColor.ToVector4() + bottomColor.ToVector4()) / 2);

        //    Vector3 max = size / 2;
        //    Vector3 min = -max;

        //    // 下面这些名字的 3 个字母： 
        //    // [T]上/[B]下，[F]前/[B]后，[L]左/[R]右
        //    var tfl = new Vector3(min.X, max.Y, max.Z);
        //    var tfr = new Vector3(max.X, max.Y, max.Z);
        //    var bfr = new Vector3(max.X, min.Y, max.Z);
        //    var bfl = new Vector3(min.X, min.Y, max.Z);
        //    var tbl = new Vector3(min.X, max.Y, min.Z);
        //    var tbr = new Vector3(max.X, max.Y, min.Z);
        //    var bbr = new Vector3(max.X, min.Y, min.Z);
        //    var bbl = new Vector3(min.X, min.Y, min.Z);

        //    // 顶面
        //    AddLine(tfl, tfr, topColor);
        //    AddLine(tfr, tbr, topColor);
        //    AddLine(tbr, tbl, topColor);
        //    AddLine(tbl, tfl, topColor);

        //    // 底面
        //    AddLine(bfl, bfr, bottomColor);
        //    AddLine(bfr, bbr, bottomColor);
        //    AddLine(bbr, bbl, bottomColor);
        //    AddLine(bbl, bfl, bottomColor);

        //    // 四根竖直柱子
        //    AddLine(bfl, tfl, bottomColor, topColor);
        //    AddLine(bfr, tfr, bottomColor, topColor);
        //    AddLine(bbl, tbl, bottomColor, topColor);
        //    AddLine(bbr, tbr, bottomColor, topColor);
        //}
    }
}
