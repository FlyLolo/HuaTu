﻿using SVGHelper;
using SVGHelper.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawWork
{

   
    public class DrawRectangleObject : DrawObject
    {
        #region 字段

        private const string Tag = "rect";

        public RectangleF Rectangle
        {
            get => rectangle;
        }

        protected RectangleF rectangle;

        #endregion 字段

        #region 构造器

        public DrawRectangleObject() 
        {
            SetRectangleF(0, 0, 1, 1);
            Initialize();
        }

        public DrawRectangleObject(float x, float y, float width, float height)
        {
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = width;
            rectangle.Height = height;
            Initialize();
        }

        #endregion 构造器

        #region 属性

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get
            {
                return 8;
            }
        }

        public Rectangle Rect
        {
            get
            {
                var rect = new Rectangle();
                rect.X = (int)(rectangle.X / Zoom);
                rect.Y = (int)(rectangle.Y / Zoom);
                rect.Width = (int)(rectangle.Width / Zoom);
                rect.Height = (int)(rectangle.Height / Zoom);
                return rect;
            }
            set
            {
                rectangle = value;
            }
        }

        public float Height
        {
            get
            {
                return rectangle.Height;
            }
            set
            {
                rectangle.Height = value;
            }
        }

        protected RectangleF RectangleF
        {
            get
            {
                return rectangle;
            }
            set
            {
                rectangle = value;
            }
        }

        protected RectangleF ParentAndRectangleF
        {
            get
            {
                if(Parent != null)
                {
                    var point = Parent.GetCenter();
                    return new RectangleF(rectangle.X + point.X, rectangle.Y + point.Y, rectangle.Width,
                        rectangle.Height);
                }else if (ParentDrawObject != null)
                {
                    var point = ParentDrawObject.GetCenter();
                    return new RectangleF(rectangle.X + point.X, rectangle.Y + point.Y, rectangle.Width,
                        rectangle.Height);
                }

                return rectangle;

            }

        }

        public override PointF ParentPointF
        {
            get => parentPointF;
            set
            {
                parentPointF = value;
               
            }
        }

        public float Width
        {
            get
            {
                return rectangle.Width;
            }
            set
            {
                rectangle.Width = value;
            }
        }

        #endregion 属性

        #region 段瑞

       

        protected bool hasRotation = false;//是否在旋转
        protected bool hasMove = false;

        protected PointF fixedCenter = default;//没有旋转前固定旋转点
        #endregion

        #region 函数

        public static DrawRectangleObject Create(SVGRect svg)
        {
            try
            {
                var dobj = new DrawRectangleObject(ParseSize(svg.X, Dpi.X),
                    ParseSize(svg.Y, Dpi.Y),
                    ParseSize(svg.Width, Dpi.X),
                    ParseSize(svg.Height, Dpi.Y));
                dobj.Rotate(ParseAngle(svg.Transform));
                dobj.SetStyleFromSvg(svg);
                dobj.Name = svg.ShapeName;

                return dobj;
            }
            catch (Exception ex)
            {
                SVGErr.Log("DrawRectangle", "CreateRectangle:", ex.ToString(), SVGErr._LogPriority.Info);
                return null;
            }
        }

        public static RectangleF GetNormalizedRectangle(float x1, float y1, float x2, float y2)
        {
            if (x2 < x1)
            {
                float tmp = x2;
                x2 = x1;
                x1 = tmp;
            }

            if (y2 < y1)
            {
                float tmp = y2;
                y2 = y1;
                y1 = tmp;
            }

            return new RectangleF(x1, y1, x2 - x1, y2 - y1);
        }

        public static RectangleF GetNormalizedRectangle(PointF p1, PointF p2)
        {
            return GetNormalizedRectangle(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static RectangleF GetNormalizedRectangle(RectangleF r)
        {
            return GetNormalizedRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
        }

        //public static string GetRectangleXmlStr(Color stroke, bool isFill, Color fill, float strokewidth, RectangleF rect, SizeF scale, String shapeName,float angle,PointF center)
        //{
        //    string s = "<";
        //    s += Tag;
        //    s += GetStringStyle(stroke, fill, strokewidth, scale);//GetStrStyle(scale);
        //    s += GetRectStringXml(rect, scale, shapeName);
        //    s += GetTransformXML(angle, center);
        //    s += " />" + Environment.NewLine;
        //    return s;
        //}
        /// <summary>
        /// 获取旋转角度
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public virtual string GetTransformXML(float angle, PointF center)
        {
            return $" transform=\"rotate({-angle}, {center.X} {center.Y})\"";
        }

        public override float GetAngle()
        {
            return _angle;
        }

        public static string GetRectStringXml(RectangleF rect, SizeF scale, String shapeName)
        {
            string s = "";
            float x = rect.X / scale.Width;
            float y = rect.Y / scale.Height;
            float w = rect.Width / scale.Width;
            float h = rect.Height / scale.Height;

            s += " x = \"" + x.ToString(CultureInfo.InvariantCulture) + "\"";
            s += " y = \"" + y.ToString(CultureInfo.InvariantCulture) + "\"";
            s += " width = \"" + w.ToString(CultureInfo.InvariantCulture) + "\"";
            s += " height = \"" + h.ToString(CultureInfo.InvariantCulture) + "\"";
            //添加旋转
            
            //Added by Ajay
            s += " ShapeName = \"" + shapeName + "\"";
            return s;
        }

        public override string GetXmlEnd()
        {
            return "<\\rect>" + base.GetXmlEnd();
        }


        public new DrawRectangleObject GetWorldDrawObject()
        {
            PointF parents = new PointF(Parent.Rectangle.X, Parent.Rectangle.Y);
            DeviceDrawObjectBase par = Parent;

            while (par.Parent != null)
            {
                par = par.Parent;
                parents.X += par.Rectangle.X;
                parents.Y += par.Rectangle.Y;
            }


            if (Parent != null)
            {
                ////获取父物体的世界坐标
                //var parentPosition = new PointF(Parent.Rectangle.X, Parent.Rectangle.Y);
                ////应用缩放 获取缩放比
                //var zoomw = Parent.Width / Parent.ViewBox_w;
                //var zoomh = Parent.Height / Parent.ViewBox_h;

                //吴悠修改
                //获取父物体的世界坐标
                var parentPosition = new PointF(parents.X, parents.Y);
                //应用缩放 获取缩放比
                var zoomw = par.Width / par.ViewBox_w;
                var zoomh = par.Height / par.ViewBox_h;

                var worldDrawObj =  new DrawRectangleObject(rectangle.X + parentPosition.X, rectangle.Y + parentPosition.Y,
                    zoomw * rectangle.Width, zoomh * rectangle.Height);
                worldDrawObj._angle = Parent.GetAngle();

                return worldDrawObj;


            }
            return this;
        }

        /// <summary>
        /// Draw rectangle
        /// </summary>
        /// <param name="g"></param>
        public override void Draw(Graphics g)
        {

            try
            {

                if (hasRotation)
                {
                    fixedCenter = GetCenter();
                    hasRotation = false;
                }

                //if (hasMove)
                //{
                //    fixedCenter = GetCenter();
                //    hasMove = false;
                //}

             
                RectangleF r = GetNormalizedRectangle(ParentAndRectangleF);

                PointF center = new PointF(fixedCenter.X + parentPointF.X, fixedCenter.Y + parentPointF.Y);

                if (Parent != null)
                {
                    var worldObj = GetWorldDrawObject();

                    center = Parent.GetCenter();

                    r = GetNormalizedRectangle(worldObj.Rectangle);
                }
                //先设定到中心位置 旋转
                //PointF center = GetCenter();
                g.TranslateTransform(center.X, center.Y);
                g.RotateTransform(-_angle);
                //设置画笔起始位置\
                g.TranslateTransform(-center.X, -center.Y);
                //PointF start = RotatePoint(center, new PointF(RectangleF.X,RectangleF.Y), _angle);
                //g.TranslateTransform(start.X, start.Y);




                
                if (Fill != Color.Empty)
                {
                    Brush brush = new SolidBrush(Fill);
                    g.FillRectangle(brush, r);
                }
                Pen pen = new Pen(Stroke, StrokeWidth);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                
                g.ResetTransform();
                pen.Dispose();
            }
            catch (Exception ex)
            {
                SVGErr.Log("DrawRectangle", "Draw", ex.ToString(), SVGErr._LogPriority.Info);
            }
        }

        public override void Dump()
        {
            base.Dump();

            Trace.WriteLine("rectangle.X = " + rectangle.X.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Y = " + rectangle.Y.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Width = " + rectangle.Width.ToString(CultureInfo.InvariantCulture));
            Trace.WriteLine("rectangle.Height = " + rectangle.Height.ToString(CultureInfo.InvariantCulture));
        }

        public override PointF GetHandle(int handleNumber)
        {
            float x, xCenter, yCenter;

            xCenter = rectangle.X + rectangle.Width / 2;
            yCenter = rectangle.Y + rectangle.Height / 2;
            x = rectangle.X;
            float y = rectangle.Y;

            //PointF center = new PointF(xCenter, yCenter);
            PointF center = fixedCenter;
            var root = GetRoot();
            if(root != null)
                    center = new PointF(fixedCenter.X + root.rectangle.X + root.rectangle.Width / 2, fixedCenter.Y + root.rectangle.Y + root.rectangle.Height / 2);

            PointF temp = default(PointF);
            switch (handleNumber)
            {
                case 1:
                    x = rectangle.X;
                    y = rectangle.Y;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
                case 2:
                    x = xCenter;
                    y = rectangle.Y;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
                case 3:
                    x = rectangle.Right;
                    y = rectangle.Y;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
                case 4:
                    x = rectangle.Right;
                    y = yCenter;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
                case 5:
                    x = rectangle.Right;
                    y = rectangle.Bottom;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
                case 6:
                    x = xCenter;
                    y = rectangle.Bottom;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
                case 7:
                    x = rectangle.X;
                    y = rectangle.Bottom;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
                case 8:
                    x = rectangle.X;
                    y = yCenter;
                    temp = new PointF(x, y);
                    return RotatePoint(center, temp, _angle);
            }
            
            return new PointF(x, y);
        }

        
        public override PointF GetKnobPoint()
        {
            float x, xCenter, yCenter;
            PointF center = fixedCenter;
            var root = GetRoot();
            if (root != null)
                center = new PointF(fixedCenter.X + root.rectangle.X + root.rectangle.Width / 2, fixedCenter.Y + root.rectangle.Y + root.rectangle.Height / 2);

            xCenter = rectangle.X + rectangle.Width / 2;
            yCenter = rectangle.Y + rectangle.Height / 2;
            x = xCenter;
            float y = rectangle.Y - 40;
            return RotatePoint(center, new PointF(x, y), _angle);
        }

        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return Cursors.Default;
            }
        }

        public override string GetXmlStr(SizeF scale,bool noAnimation = true)
        {
            //  <rect x="1" y="1" width="1198" height="398"
            //		style="fill:none; stroke:blue"/>

            string s = "<";
            s += Tag;
            s += GetStrStyle(scale);
            s += GetRectStringXml(RectangleF, scale, Name);
            s += GetTransformXML(_angle, fixedCenter);//添加变化
            s += noAnimation ? " />" : " >";
            s += Environment.NewLine;
            return s;
        }

        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override int HitTest(PointF point)
        {
            if (Selected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (PointInObject(point))
                return 0;

            return -1;
        }

        public override int HitAroundTest(PointF point)
        {
            float distance = float.MaxValue;
            int minHandle = -1;
            for (int i = 2; i <= HandleCount; i += 2)
            {
               var pointi = GetHandle(i);
               var x = point.X - pointi.X;
                var y = point.Y - pointi.Y;
                var disi = x * x + y * y;
                if (distance > disi)
                {
                    minHandle = i;
                    distance = disi;
                }
            }

            return minHandle;
        }

        public override bool HitKnobTest(PointF point)
        {
            if (Selected)
            {
                return GetKnobRectangle().Contains(point);
            }

            return false;
        }

        public override bool IntersectsWith(RectangleF rect)
        {
            try
            {
                return RectangleF.IntersectsWith(rect);
            }
            catch (Exception ex)
            {
                SVGErr.Log("DrawRectangle", "Intersect", ex.ToString(), SVGErr._LogPriority.Info);
                return false;
            }
        }

        public override void Move(float deltaX, float deltaY)
        {
            rectangle.X += deltaX;
            rectangle.Y += deltaY;

            fixedCenter.X += deltaX;
            fixedCenter.Y += deltaY;

            OnHandleMove?.Invoke(-1);
        }

        public override void MoveHandleTo(PointF point, int handleNumber)
        {
            float left = rectangle.Left;
            float top = rectangle.Top;
            float right = rectangle.Right;
            float bottom = rectangle.Bottom;

            float x, xCenter, yCenter;

            xCenter = rectangle.X + rectangle.Width / 2;
            yCenter = rectangle.Y + rectangle.Height / 2;
            x = rectangle.X;
            float y = rectangle.Y;

            //PointF center = new PointF(xCenter, yCenter);
            PointF center = fixedCenter;

            PointF temp = default(PointF);

            PointF toRectangleMousePoint = default;//将鼠标位置转换到矩形坐标系

            switch (handleNumber)
            {
                case 1:
                    toRectangleMousePoint = RotatePointReverse(center, point, _angle);

                    left = toRectangleMousePoint.X;
                    top = toRectangleMousePoint.Y;
                    break;
                case 2:
                    x = xCenter;
                    y = rectangle.Y;
                    temp = new PointF(x, y);
                    toRectangleMousePoint = GetLocalMousePoint(point, center, temp);
                    top = toRectangleMousePoint.Y;
                    break;
                case 3:
                    toRectangleMousePoint = RotatePointReverse(center, point, _angle);
                    right = toRectangleMousePoint.X;
                    top = toRectangleMousePoint.Y;
                    break;
                case 4:
                    x = right;
                    y = yCenter;
                    temp = new PointF(x, y);
                    toRectangleMousePoint = GetLocalMousePoint(point, center, temp);
                    right = toRectangleMousePoint.X;
                    break;
                case 5:
                    toRectangleMousePoint = RotatePointReverse(center, point, _angle);
                    right = toRectangleMousePoint.X;
                    bottom = toRectangleMousePoint.Y;
                    break;
                case 6:
                    x = xCenter;
                    y = bottom;
                    temp = new PointF(x, y);
                    toRectangleMousePoint = GetLocalMousePoint(point, center, temp);
                    bottom = toRectangleMousePoint.Y;
                    break;
                case 7:
                    toRectangleMousePoint = RotatePointReverse(center, point, _angle);
                    left = toRectangleMousePoint.X;
                    bottom = toRectangleMousePoint.Y;
                    break;
                case 8:
                    x = left;
                    y = yCenter;
                    temp = new PointF(x, y);
                    toRectangleMousePoint = GetLocalMousePoint(point, center, temp);
                    left = toRectangleMousePoint.X;
                    break;
            }

            SetRectangleF(left, top, right - left, bottom - top);

            OnHandleMove?.Invoke(handleNumber);
        }
        /// <summary>
        /// 获取光标从世界坐标到矩形局部坐标的转换
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <param name="center"></param>
        /// <param name="handleLocalPoint"></param>
        /// <returns></returns>
        protected PointF GetLocalMousePoint(PointF mousePoint, PointF center, PointF handleLocalPoint)
        {
            var temp2 = RotatePoint(center, handleLocalPoint, _angle);
            var interactPoint = PointForPointToABLine(mousePoint.X, mousePoint.Y, temp2.X, temp2.Y, center.X, center.Y);//鼠标位置到中心和handle线段的垂直交点，作为新的handle
            var toRectangleMousePoint = RotatePointReverse(center, interactPoint, _angle);
            return toRectangleMousePoint;
        }
        public override void RotateKnobTo(PointF point)
        {
            base.RotateKnobTo(point);
        }

        public override void Normalize()
        {
            rectangle = GetNormalizedRectangle(rectangle);
        }

        public override void Resize(SizeF newscale, SizeF oldscale)
        {
            PointF p = RecalcPoint(RectangleF.Location, newscale, oldscale);
            var ps = new PointF(RectangleF.Width, RectangleF.Height);
            ps = RecalcPoint(ps, newscale, oldscale);
            RectangleF = new RectangleF(p.X, p.Y, ps.X, ps.Y);
            RecalcStrokeWidth(newscale, oldscale);
        }

        protected override bool PointInObject(PointF point)
        {
            return rectangle.Contains(point);
        }

        public void SetRectangleF(float x, float y, float width, float height)
        {
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = width;
            rectangle.Height = height;
        }
        #region 段瑞旋转
        

        

        protected override PointF RotatePoint(PointF center, PointF p1, float angle)
        {
            return base.RotatePoint(center, p1, angle);
        }

        public override void Rotate(float angle)
        {
            hasRotation = true;


            //rectangle = RotatePoint()
            //_rectangleR.Location = RotatePoint(_rectangleR.GetCenter(), _rectangleR.Location, angle);
            //_rectangleR.RightBottom = RotatePoint(_rectangleR.GetCenter(), _rectangleR.RightBottom, angle);
            _angle -= angle;



        }

        public override PointF GetCenter()
        {
            float x, xCenter, yCenter;

            xCenter = rectangle.X + rectangle.Width / 2;
            yCenter = rectangle.Y + rectangle.Height / 2;
            return new PointF(xCenter, yCenter);
        }

        public override void Rotate(float angle, PointF center)
        {

        }

        public override void Rotate(float angle, float x, float y)
        {
            PointF center = new PointF(x, y);

        }

        #endregion
        #endregion 函数


        long interval = 1000;
        long nextTime = DateTime.Now.Ticks;
        public override void Update()
        {
          
           
        }
    }
}
