using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ZedGraph;

namespace ZedGraphDemo
{
    public partial class ZedGraphDemo : Form
    {
        public ZedGraphDemo()
        {
            InitializeComponent();
           
            InitPlot();

            AddGraph();
          //  zedGraph.PointValueEvent += ZedGraph_PointValueEvent;

           zedGraph.MouseMove += ZedGraph_MouseMove;
           zedGraph.MouseLeave += ZedGraph_MouseLeave;
        }

        private string ZedGraph_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {
            string info = "";
            if (zedGraph.GraphPane.CurveList.Count > 0)
            {//有曲线
                if (  iPt > 0)
                {
                    foreach (CurveItem item in zedGraph.GraphPane.CurveList)
                    {
                        info += "(X:" + item.Points[iPt].X.ToString() + " , Y:"
                            + item.Points[iPt].Y.ToString() + ")\r\n";
                    }
              }

            }
            return info;
        }


        //鼠标离开时，将游标隐藏
        private void ZedGraph_MouseLeave(object sender, EventArgs e)
        {
            using (Graphics graphics = zedGraph.CreateGraphics())
            {//在zedGraph上创建画布
                zedGraph.Refresh();

                using (Pen pen = new Pen(Color.Red, 2))
                {//创建画笔并设置样式
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    //画竖直线
                    graphics.DrawLine(pen,0, 0, 0, 0);

                }
            }
        }


        //鼠标移动
        private void ZedGraph_MouseMove(object sender, MouseEventArgs e)
        {

            ShowPonitByDraw(e);
        }

       
        //自定义绘制游标
        private void ShowPonitByDraw(MouseEventArgs e)
        {
            if (zedGraph.GraphPane.Chart.Rect.Contains(e.Location) == false)
            {
                return;
            }

            using (Graphics graphics = zedGraph.CreateGraphics())
            {//在zedGraph上创建画布
                zedGraph.Refresh();

                using (Pen pen = new Pen(Color.Red, 2))
                {//创建画笔并设置样式
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    //画竖直线
                    graphics.DrawLine(pen, e.X, zedGraph.GraphPane.Chart.Rect.Top, e.X, zedGraph.GraphPane.Chart.Rect.Bottom);
                   
                    if (zedGraph.GraphPane.CurveList.Count <= 0)
                    {
                      
                        return;
                    }

                    //找最近的一个点
                    zedGraph.GraphPane.FindNearestPoint(e.Location, out CurveItem nearCurve, out int nearIndex);

                    if (nearCurve == null || nearIndex < 0)
                    {
                        return;
                    }

                    string tempMax = "";
                    List<string> infoList = new List<string>();

                    foreach (CurveItem curve in zedGraph.GraphPane.CurveList)
                    {
                        //曲线名称 + 坐标点
                        string tmp = curve.Points[nearIndex].Y.ToString();
                        //填充到8个长度
                        tmp =  tmp.PadLeft(8);
                        tmp = tmp.Insert(0, curve.Label.Text + ": " );

                        infoList.Add(tmp);
                        if (tmp.Length > tempMax.Length)
                        {//记录最大的长度字符串
                            tempMax = tmp;
                        }
                    }


                    //文本绘制的一些字体和画刷配置
                    Font font = new Font("Arial", 10, System.Drawing.FontStyle.Regular, GraphicsUnit.World);

                    //得到一个字体绘制的大小
                    SizeF tempSizeF = graphics.MeasureString(tempMax, font, (int)font.Size);

                    //根据字符长度计算矩形的宽度 10是颜色矩形框的宽度
                    float rectWidth = tempSizeF.Width * tempMax.Length;
                    //高度
                    float rectHeight = (infoList.Count + 1) * 18 + 5;
                    //背景颜色框的左上角点的坐标，偏移2个像素
                    Point point = new Point(e.X + 2, e.Y + 2);

                    #region 计算左上角坐标 让背景矩形框在曲线的矩形框范围之内
                    if (point.X + rectWidth > zedGraph.GraphPane.Chart.Rect.Right)
                    {
                        point.X = (int)(point.X - rectWidth - 2);
                    }

                    if (point.Y + rectHeight > zedGraph.GraphPane.Chart.Rect.Bottom)
                    {
                        point.Y = (int)(point.Y - rectHeight - 2);
                    }
                    #endregion

                    pen.Color = Color.White;
                    //绘制背景矩形框
                    Rectangle rectBg = new Rectangle(point, new Size((int)rectWidth, (int)rectHeight));
                    graphics.DrawRectangle(pen, rectBg);
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb(70, 70, 70)), rectBg);

                    //颜色框的大小
                    Size colorSize = new Size(10, 10);
                    //绘制文本的颜色
                    SolidBrush textBrush = new SolidBrush(Color.Red);

                    //绘制文本内容 时间
                    int time = 0;
                    //"时间(ms):"
                    string timeStr = "时间： " + nearCurve[nearIndex].X.ToString();
                    graphics.DrawString(timeStr , font, textBrush,
                                  new Point(point.X + 20, point.Y + 5 + time * 16));

                    for (int m = 0; m < infoList.Count; m++)
                    {
                        time++;
                        //绘制每条曲线的颜色小矩形框 
                        Rectangle rect1 = new Rectangle(new Point(point.X + 5, point.Y + 5 + time * 16), colorSize);
                        graphics.DrawRectangle(new Pen(zedGraph.GraphPane.CurveList[m].Color), rect1);
                        graphics.FillRectangle(new SolidBrush(zedGraph.GraphPane.CurveList[m].Color), rect1);

                        //绘制文本内容
                        graphics.DrawString(infoList[m], font, textBrush,
                                      new Point(point.X + 20, point.Y + 5 + time * 16));
                    }

                }
            }
        }



        //初始化图表样式
        private void InitPlot()
        {
            //去掉外边框
            this.zedGraph.GraphPane.Border.IsVisible = false;

            //设置黑色
            this.zedGraph.GraphPane.Fill = new ZedGraph.Fill(Color.Black);

            //设置曲线区域的矩形框的颜色  
            this.zedGraph.GraphPane.Chart.Fill = new ZedGraph.Fill(Color.Black);

            //设置绘制曲线区域的矩形框的边框颜色 
              this.zedGraph.GraphPane.Chart.Border.Color = Color.FromArgb(150, 150, 150);


            //设置缩放和显示点
            zedGraph.IsEnableZoom = false;
            zedGraph.IsShowPointValues = false;

            //设置图例
            this.zedGraph.GraphPane.Legend.Fill = new Fill(Color.Black);
            this.zedGraph.GraphPane.Legend.FontSpec.FontColor = Color.White;
            

            //灰色
            Color axisColor = Color.FromArgb(150, 150, 150);
            float dashLength = 4f;

            #region X轴
            //设置网格线 主网格线
            this.zedGraph.GraphPane.XAxis.MajorGrid.IsVisible = true;
            this.zedGraph.GraphPane.XAxis.MajorGrid.Color = axisColor;
            this.zedGraph.GraphPane.XAxis.MajorGrid.DashOn = dashLength;
            this.zedGraph.GraphPane.XAxis.MajorGrid.DashOff = dashLength;
            this.zedGraph.GraphPane.XAxis.MajorGrid.PenWidth = 0.1f;
            //子网格线 不可见
            this.zedGraph.GraphPane.XAxis.MinorGrid.IsVisible = false;
            
            //刻度
            //设置主刻度的长度
            this.zedGraph.GraphPane.XAxis.MajorTic.Size = 10f;
            //主刻度颜色 
            this.zedGraph.GraphPane.XAxis.MajorTic.Color = axisColor;

            //隐藏X轴正上方的刻度
            this.zedGraph.GraphPane.XAxis.MajorTic.IsOpposite = false;
            this.zedGraph.GraphPane.XAxis.MinorTic.IsOpposite = false;

            //朝外
            this.zedGraph.GraphPane.XAxis.MajorTic.IsInside = false;
            this.zedGraph.GraphPane.XAxis.MinorTic.IsInside = false;

           
            //设置刻度文本颜色
            this.zedGraph.GraphPane.XAxis.Scale.FontSpec.FontColor = axisColor;
            //设置X轴标题颜色
            this.zedGraph.GraphPane.XAxis.Title.FontSpec.FontColor = axisColor;
            this.zedGraph.GraphPane.XAxis.Title.Text = "时间(ms)";
            //设置X轴颜色
            this.zedGraph.GraphPane.XAxis.Color = axisColor;

            #endregion

            #region Y轴
            //设置网格线 主网格线
            this.zedGraph.GraphPane.YAxis.MajorGrid.IsVisible = true;
            this.zedGraph.GraphPane.YAxis.MajorGrid.Color = axisColor;
            this.zedGraph.GraphPane.YAxis.MajorGrid.DashOn = dashLength;
            this.zedGraph.GraphPane.YAxis.MajorGrid.DashOff = dashLength;
            this.zedGraph.GraphPane.YAxis.MajorGrid.PenWidth = 0.1f;
            //设置子网格线不可见
            this.zedGraph.GraphPane.YAxis.MinorGrid.IsVisible = false;


            Color ycolor = Color.Yellow;

            //刻度
            //设置主刻度的长度
            this.zedGraph.GraphPane.YAxis.MajorTic.Size = 10f;
            //主刻度颜色 
            this.zedGraph.GraphPane.YAxis.MajorTic.Color = ycolor;
            //设置对面的Y轴刻度不可见
            this.zedGraph.GraphPane.YAxis.MajorTic.IsOpposite = false;

            //朝内
            this.zedGraph.GraphPane.YAxis.MajorTic.IsOutside = false;
            this.zedGraph.GraphPane.YAxis.MinorTic.IsOutside = false;


            //设置Y轴颜色
            this.zedGraph.GraphPane.YAxis.Color = ycolor;
            //设置刻度文本颜色
            this.zedGraph.GraphPane.YAxis.Scale.FontSpec.FontColor = ycolor;
            //设置Y轴标题颜色
            this.zedGraph.GraphPane.YAxis.Title.FontSpec.FontColor = ycolor;
            this.zedGraph.GraphPane.YAxis.Title.Text = "速度";
            this.zedGraph.GraphPane.YAxis.Title.Gap = 0.05f;
            this.zedGraph.GraphPane.YAxis.Title.FontSpec.StringAlignment = StringAlignment.Near;
            this.zedGraph.GraphPane.YAxis.Title.FontSpec.Angle = 90;


            #endregion

            #region Y2Axis轴
            //显示Y2Axis
            zedGraph.GraphPane.Y2Axis.IsVisible = true;

            //设置主刻度
            zedGraph.GraphPane.Y2Axis.MajorTic.Size = 10f;
            zedGraph.GraphPane.Y2Axis.MajorTic.IsOutside = false;
            zedGraph.GraphPane.Y2Axis.MajorTic.Color = Color.Blue;

            //设置颜色 蓝色
            zedGraph.GraphPane.Y2Axis.Scale.FontSpec.FontColor = Color.Blue;
             zedGraph.GraphPane.Y2Axis.Title.FontSpec.FontColor = Color.Blue;

            //设置对面的刻度不可见
             zedGraph.GraphPane.Y2Axis.MajorTic.IsOpposite = false;
             zedGraph.GraphPane.Y2Axis.MinorTic.IsOpposite = false;


            //隐藏网格线
             zedGraph.GraphPane.Y2Axis.MajorGrid.IsVisible = false;

             zedGraph.GraphPane.Y2Axis.Scale.Align = AlignP.Inside;   //align the Y2 axis labels so they are flush to the axis
             zedGraph.GraphPane.Y2Axis.Scale.Min = 1.5;
             zedGraph.GraphPane.Y2Axis.Scale.Max = 3;
             zedGraph.GraphPane.Y2Axis.Scale.MaxAuto = true;
            #endregion
        }


        /// <summary>
        /// 添加曲线
        /// </summary>
        private void AddGraph()
        {
            PointPairList vlist = new PointPairList();
            PointPairList dlist = new PointPairList();
          

            for (int i = 0; i < 30; i++)
            {
                double time = (double)i;
                double acceleration = 2.0;
                double velocity = acceleration * time;
                double distance = acceleration * time * time / 2.0;
                double energy = 100.0 * velocity * velocity / 2.0;

               
                vlist.Add(time, velocity);
                dlist.Add(time, distance);
            }

            //添加速度曲线
            LineItem myCurve = zedGraph.GraphPane.AddCurve("速度", vlist, Color.Yellow, SymbolType.Diamond);
            myCurve.Symbol.Fill = new Fill(Color.White);   // fill the symbols with white

            //添加加速度曲线
            myCurve = zedGraph.GraphPane.AddCurve("路程", dlist, Color.Blue, SymbolType.Circle);
            myCurve.Symbol.Fill = new Fill(Color.White);   //fill the symbols with white
            myCurve.IsY2Axis = true;

            zedGraph.AxisChange();
        }

       
    }
}
