using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
//using System.Runtime.Serialization;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.WoormWebControl;
using Microarea.Common.Hotlink;

//using Microarea.RSWeb.Temp;

namespace Microarea.RSWeb.Objects
{
    public enum EnumChartType
    //ATTENZIONE: tenere allineato in: 
    //c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\Chart.h - EnumChartType
    //c:\development\standard\web\server\report-service\woormviewer\Chart.cs - EnumChartType
    //c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - ChartType
    //------
    {
        None,
        Bar, BarStacked, BarStacked100,
        Column, ColumnStacked, ColumnStacked100,
        Area, AreaStacked, AreaStacked100, Line,

        Funnel, Pie, Donut, DonutNested,

        RangeBar, RangeColumn, RangeArea,

        Bubble, BubbleScatter,

        Scatter, ScatterLine,

        PolarLine, PolarArea, PolarScatter,
        RadarLine, RadarArea,

        Wrong,
        //mancano nei BCGP
        VerticalLine, VerticalArea,
        //unsupported nei Kendo UI
        Pyramid
        //, RadarScatter,
        //versioni 3D di bar,column,area  
    }

    enum EnumChartStyle
    //ATTENZIONE: tenere allineato in: 
    //c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\Chart.h - EnumChartType
    //c:\development\standard\web\server\report-service\woormviewer\table.cs - EnumChartType
    //c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - ChartType
    //------
    {
        Normal,
        Smooth,  /*spline*/
        Step
    };

    class Series
    {
        public List<Variable> BindedFields = new List<Variable>();
        public string Title;

        public EnumChartType SeriesType = EnumChartType.None;
        public double Transparent = 1;
        public Color Color = Color.White;
        public bool Colored = false;
        public int Group = 0;   //for grouping stacked column/bar
        public EnumChartStyle Style = EnumChartStyle.Normal;
        public bool ShowLabel = false;

    };

    class Categories
    {
        public Variable BindedField = null;
        public string Title;

        public Color Color = Color.White;
        public bool Colored = false;

        /*CCategories : non riesco a fare la dichiarazione forward*/
        public Chart Parent = null;

        public Categories(Chart p) { Parent = p; }
    };

    class ChartLegend
    {
        public string Align = "top";
        public bool Hidden = false;

        public ChartLegend() { }
    };

    public class Chart : BaseRect
    {
        public string Name;
        public string Title;
        public EnumChartType ChartType = EnumChartType.None;

        Categories Categories = null;
        List<Series> Series = new List<Series>();
        Variable ColorVar=null;
        ChartLegend Legend = new ChartLegend();

        //------------------------------------------------------------------------------
        public Chart(WoormDocument document)
            : base(document)
        {
        }

        //---------------------------------------------------------------------
        bool HasCategories()
        {
            return
                ChartType == EnumChartType.None ||
                ChartType == EnumChartType.Bar ||
                ChartType == EnumChartType.BarStacked ||
                ChartType == EnumChartType.BarStacked100 ||
                ChartType == EnumChartType.Column ||
                ChartType == EnumChartType.ColumnStacked ||
                ChartType == EnumChartType.ColumnStacked100 ||
                ChartType == EnumChartType.Area ||
                ChartType == EnumChartType.AreaStacked ||
                ChartType == EnumChartType.AreaStacked100 ||
                ChartType == EnumChartType.Line ||
                ChartType == EnumChartType.Funnel ||
                ChartType == EnumChartType.Pie ||
                ChartType == EnumChartType.Donut ||
                ChartType == EnumChartType.DonutNested ||
                ChartType == EnumChartType.RadarArea ||
                ChartType == EnumChartType.RadarLine ||            
                ChartType == EnumChartType.Bubble ||
                ChartType == EnumChartType.RangeArea ||
                ChartType == EnumChartType.RangeBar ||
                ChartType == EnumChartType.RangeColumn;
        }

        bool IsChartFamilyBar()
        {
            return
                ChartType == EnumChartType.Bar ||
                ChartType == EnumChartType.BarStacked ||
                ChartType == EnumChartType.BarStacked100 ||
                ChartType == EnumChartType.Column ||
                ChartType == EnumChartType.ColumnStacked ||
                ChartType == EnumChartType.ColumnStacked100 ||
                ChartType == EnumChartType.Area ||
                ChartType == EnumChartType.AreaStacked ||
                ChartType == EnumChartType.AreaStacked100 ||
                ChartType == EnumChartType.Line;
        }

        bool IsChartFamilyPie()
        {
            return
                ChartType == EnumChartType.Pie ||
                ChartType == EnumChartType.Funnel ||
                ChartType == EnumChartType.Donut ||
                 ChartType == EnumChartType.DonutNested;
        }

        bool IsChartFamilyPolar()
        {
            return
                ChartType == EnumChartType.PolarArea ||
                ChartType == EnumChartType.PolarLine ||
                ChartType == EnumChartType.PolarScatter;
        }
        bool IsChartFamilyRadar()
        {
            return
                ChartType == EnumChartType.RadarArea ||
                ChartType == EnumChartType.RadarLine;
        }

        bool IsChartFamilyRange()
        {
            return
                ChartType == EnumChartType.RangeArea ||
                ChartType == EnumChartType.RangeBar ||
                ChartType == EnumChartType.RangeColumn;
        }

        bool IsChartFamilyBubble()
        {
            return            
                ChartType == EnumChartType.Bubble ||
                ChartType == EnumChartType.BubbleScatter;
        }

        bool IsChartFamilyScatter()
        {
            return
                ChartType == EnumChartType.Scatter ||
                ChartType == EnumChartType.ScatterLine;
        }
        //------------------------------------------------------------------------------
        protected override bool ParseProp(WoormParser lex, bool block)
        {
            bool ok = true;

            switch (lex.LookAhead())
            {
                case Token.PEN: ok = lex.ParsePen(BorderPen); break;
                case Token.BORDERS: ok = lex.ParseBorders(Borders); break;

                case Token.HIDDEN:
                    {
                        Token[] stopTokens =
                        {
                            Token.SEP, Token.END,
                            Token.BKGCOLOR, Token.PEN, Token.BORDERS, Token.TRANSPARENT,
                            Token.TOOLTIP, Token.STYLE, Token.TEMPLATE, Token.DROPSHADOW,
                            Token.ANCHOR_COLUMN_ID, Token.ANCHOR_PAGE_LEFT, Token.ANCHOR_PAGE_RIGHT,
                            Token.LAYER
                        };
                        ok = ParseHidden(lex, stopTokens);
                        break;
                    }

                case Token.TOOLTIP:
                    {
                        Token[] stopTokens =
                                    {
                                        Token.SEP
                                    };
                        ok = ParseTooltip(lex, stopTokens);
                        break;
                    }

                case Token.DROPSHADOW:
                    {
                        lex.SkipToken();
                        if (!lex.ParseInt(out DropShadowHeight))
                            return false;
                        if (!lex.ParseColor(Token.COLOR, out DropShadowColor))
                            return false;
                        break;
                    }

                case Token.LAYER:
                    lex.SkipToken();
                    ok = lex.ParseInt(out this.Layer);
                    break;

                case Token.TRANSPARENT:
                    lex.SkipToken();
                    break;

                case Token.END:
                    return ok;

                default:
                    if (block)
                    {
                        lex.SetError(WoormViewerStrings.BadGeneralProperties);
                        ok = false;
                    }
                    break;
            }

            return ok;
        }

        //------------------------------------------------------------------------------
        bool ParseSeries(WoormParser lex, Series pSeries)
        {
            if (!lex.ParseBegin())
                return false;


            bool ok;
            if (lex.Matched(Token.TITLE))
            {
                ok = lex.ParseString(out pSeries.Title);
                if (!ok)
                    return false;
            }

            if (lex.Matched(Token.TYPE))
            {
                int n = 0;
                if (!lex.ParseInt(out n))
                    return false;
                pSeries.SeriesType = (EnumChartType)n;
            }
            else
            {
                pSeries.SeriesType = ChartType;
            }

            while (lex.Matched(Token.DATASOURCE))
            {
                string sVarName = string.Empty;
                ok = lex.ParseID(out sVarName);
                if (!ok)
                    return false;

                Variable pF = Document.SymbolTable.Find(sVarName);
                if (pF == null)
                {
                    lex.SetError("TODO - il campo associato alla serie non esiste");
                    return false;
                }
                pSeries.BindedFields.Add(pF);
            }

            if (lex.Matched(Token.GROUP))
            {
                if (!lex.ParseInt(out pSeries.Group))
                    return false;
            }

            if (lex.Matched(Token.TRANSPARENT))
            {
                if (!lex.ParseDouble(out pSeries.Transparent))
                    return false;
            }

            if (lex.LookAhead(Token.COLOR))
            {
                if (!IsChartFamilyPie())
                {
                    if (!lex.ParseColor(Token.COLOR, out pSeries.Color))
                        return false;
                }
                else
                {
                    string colorId = "";
                    ok = lex.ParseTag(Token.COLOR);                  
                    ok=lex.ParseID(out colorId);
                    Variable pF = Document.SymbolTable.Find(colorId);
                    if (pF == null)
                    {
                        lex.SetError("TODO - il campo associato al colore non esiste");
                        return false;
                    }
                    ColorVar = pF;
                }
                
                pSeries.Colored = true;
            }
            else
                pSeries.Colored = false;

            if (lex.Matched(Token.STYLE))
            {
                int st = 0;
                if (!lex.ParseInt(out st))
                    return false;
                pSeries.Style = (EnumChartStyle)st;
            }

            if (lex.Matched(Token.LABEL))
            {
                if (!lex.ParseBool(out pSeries.ShowLabel))
                    return false;
            }

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        bool ParseCategories(WoormParser lex)
        {
            if (!lex.ParseBegin())
                return false;

            Categories = new Categories(this);

            bool ok = false;

            if (HasCategories())
            {
                if (lex.Matched(Token.TITLE)) {
                   ok= /*lex.ParseTag(Token.TITLE) &&*/ lex.ParseString(out Categories.Title);
                    if (!ok)
                        return false;
                }

                string sVarName = string.Empty;
                ok = lex.ParseTag(Token.DATASOURCE) && lex.ParseID(out sVarName);
                if (!ok)
                    return false;
                Variable pF = Document.SymbolTable.Find(sVarName);
                if (pF == null)
                {
                    lex.SetError("TODO - il campo associato alla serie non esiste");
                    return false;
                }
                Categories.BindedField = pF;




                //if (!pF->IsArray() && !pF->IsColumn())
                //{
                //    lex.SetError(_TB("TODO - il campo associato alla serie non è un array/colonna"));
                //    return false;
                //}

                //if (pF->IsColumn())
                //{
                //    if (pF->IsColTotal() || pF->IsSubTotal())
                //    {
                //        lex.SetError(_TB("TODO - il campo associato alla serie non può essere il totale/subtotale di una colonna"));
                //        return false;
                //    }
                //}



                if (lex.LookAhead(Token.COLOR))
                {
                    if (!lex.ParseColor(Token.COLOR, out Categories.Color))
                        return false;
                    Categories.Colored = true;
                }
                else
                    Categories.Colored = false;
            }

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        bool ParseLegend(WoormParser lex)
        {
            bool ok = lex.ParseBegin();
            if (!ok)
                return false;
            AlignType t;
            ok = lex.ParseAlign(out t);
            if (!ok)
                return false;
            if ((int)t == 1)
                Legend.Align = "top";
            if ((int)t == 2)
                Legend.Align = "bottom";
            if ((int)t == 3)
                Legend.Align = "left";
            if ((int)t == 4)
                Legend.Align = "right";

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            bool ok = lex.ParseTag(Token.CHART) && lex.ParseID(out Name) &&
                        lex.ParseBegin() &&                     
                        lex.ParseAlias(out this.InternalID) &&
                        lex.ParseTag(Token.TITLE) &&
                        lex.ParseString(out Title);

           /* if (lex.Matched(Token.COMMA))
                ok = ok && lex.ParseID(out Name); */

            int t = 0;
            ok = ok &&
                lex.ParseTag(Token.TYPE) && lex.ParseInt(out t) && 
                lex.ParseRect(out this.Rect) ;

            ChartType = (EnumChartType)t;

            ok = ok && ParseBlock(lex);

            /*if*/
            if (lex.Matched(Token.CHART_CATEGORIES))
            {
                ParseCategories(lex);          
            }

            while (lex.Matched(Token.CHART_SERIES))
            {
                Series pSeries = new Series();
                if (!ParseSeries(lex, pSeries))
                {
                    return false;
                }
                Series.Add(pSeries);
            }

            if (lex.Matched(Token.CHART_LEGEND))
            {
                if (!ParseLegend(lex))
                {
                    return false;
                }
            }

            if (!lex.ParseEnd())
                return false;

            return true;
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser ofile)
        {
            //TODO CHART

            //---- Template Override
            BaseRect tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----
            //TODO CHART

            UnparseProp(ofile);

            //----
            Default = tempDefault;
            return true;
        }

        //------------------------------------------------------------------------------
        public override void ClearData()
        {
        }

        //------------------------------------------------------------------------------
        public override string ToJsonTemplate(bool bracket)
        {
            string name = "chart";
            string s = '\"' + name + "\":";

            s += '{' +
               base.ToJsonTemplate(false) + ',' +
               ChartType.ToJson("chartType");

            s += ',' + this.Title.ToJson("title", false, true);

            if (!Legend.Hidden)
            {
                string position = Legend.Align;
                string orientation = "horizontal";

                s += ",\"legend\":{" + position.ToJson("position", false, true) + ',' +
                     orientation.ToJson("orientation", false, true) + '}';
            }

            s += '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //---------------------------------------------------------------------
        DataArray GetArray(Variable field)
        {
            if (field == null)
            {
                return null;
            }

            DataArray ar = null;

            if (field.IsColumn2)
            {
                Column col = Document.Objects.FindColumn(field.Id);
                if (col == null)
                    return null;

                ar = col.GetColumnData();
            }
            else if (field.WoormType == "Array" || field.WoormType == "DataArray")
            {
                ar = field.Data as DataArray;
                if (ar == null)
                    return null;
            }
            return ar; ;
        }
        //---------------------------------------------------------------------

        string ToJsonData(Series series)
        {

            DataArray ar = GetArray(series.BindedFields[0]);
            if (ar == null)
            {
                return string.Empty;
            }

            string s = "{\"data\":[";

            int count = ar.Count - 1;
            for (int i = 0; i < ar.Count; i++)
            {

                s += ar.GetAt(i).ToJson();
                if (count > 0)
                {
                    s += ',';
                    count--;
                }

            }

            s += "]," + series.Title.ToJson("name", false, true);

            if (series.Colored)
                s += ',' + series.Color.ToJson("color");

            s += ',' + series.SeriesType.ToJson("type");

            if (series.Group != -1)
                s += ',' + series.Group.ToJson("group");

            s += ',' + series.Transparent.ToJson("transparent");

            switch (series.Style)
            {
                case EnumChartStyle.Normal:
                    s += ',' + "normal".ToJson("style");
                    break;
                case EnumChartStyle.Smooth:
                    s += ',' + "smooth".ToJson("style");
                    break;
                case EnumChartStyle.Step:
                    s += ',' + "step".ToJson("style");
                    break;
            }

            s += ',' + series.ShowLabel.ToJson("label");

            return s + '}';
        }
           
        //---------------------------------------------------------------------
        string ToJsonDataFamilyBar()
        {
            string series = "\"series\":[";

            int count = Series.Count - 1;
            for (int ser = 0; ser < Series.Count; ser++)
            {

                series += ToJsonData(Series[ser]);
                if (count > 0)
                {
                    series += ',';
                    count--;
                }
            }
            series += ']';

            DataArray ar = GetArray(Categories.BindedField);
            if (ar == null)
            {
                return string.Empty;
            }

            string categories = "\"categories\":[";

            count = ar.Count - 1;
            for (int i = 0; i < ar.Count; i++)
            {
                categories += ar.GetAt(i).ToJson();
                if (count > 0)
                {
                    categories += ',';
                    count--;
                }
            }

            categories += ']';

            string category_axis = "\"category_axis\":{" +
                 Categories.Title.ToJson("title", false, true) + ',' +
                 categories + '}';

            series += ',' + category_axis;

            return series;
        }

        //---------------------------------------------------------------------
        string ToJsonDataFamilyPieRadar()
        {
            if (Categories == null)
            {
                return string.Empty;
            }

            string series = "[";

            DataArray categories = GetArray(Categories.BindedField);


            int count = Series.Count - 1;
            foreach (Series seriesItem in Series)
            {
                DataArray arSeries = GetArray(seriesItem.BindedFields[0]);
                if (arSeries == null)
                {
                    return string.Empty;
                }
                if (categories.Count != arSeries.Count)
                {
                    return string.Empty;
                }


                series += "{\"data\":[";

                for (int i = 0; i < categories.Count; i++)
                {
                    if (i != 0)
                    {
                        series += ',';
                    }

                    string categoriesStr = categories.GetAt(i).ToJson("category");

                    string val = arSeries.GetAt(i).ToJson("value");
                    series += '{' + categoriesStr + ',' + val + '}';

                }

                series += "]," + seriesItem.Title.ToJson("name", false, true);

                if (seriesItem.Colored)
                    series += ',' + seriesItem.Color.ToJson("color");

                series += ',' + seriesItem.SeriesType.ToJson("type");

                if (seriesItem.Group != 0)
                    series += ',' + seriesItem.Group.ToJson("group");

                series += ',' + seriesItem.Transparent.ToJson("transparent");

                switch (seriesItem.Style)
                {
                    case EnumChartStyle.Normal:
                        series += ',' + "normal".ToJson("style");
                        break;
                    case EnumChartStyle.Smooth:
                        series += ',' + "smooth".ToJson("style");
                        break;
                    case EnumChartStyle.Step:
                        series += ',' + "step".ToJson("style");
                        break;
                }
                series += ',' + seriesItem.ShowLabel.ToJson("label");

                series += '}';
                if (count > 0)
                {
                    series += ",";
                    count--;
                }
            }

            series += ']';

            return "\"series\":" + series;
        }

        string ToJsonDataFamilyPolarRange()
        {
            if (Series.Count == 0)
            {
                return string.Empty;
            }

            string series = "[";

            int count = Series.Count - 1;
            DataArray categories = HasCategories() ? GetArray(Categories.BindedField) : null;

            foreach (Series seriesItem in Series)
            {              
                DataArray axesX = GetArray(seriesItem.BindedFields[0]);
                DataArray axesY = GetArray(seriesItem.BindedFields[1]);

                if (axesX == null || axesY == null)
                {
                    return string.Empty;
                }
                if (axesX.Count != axesY.Count)
                {
                    return string.Empty;
                }


                series += "{\"data\":[";

                for (int i = 0; i < axesX.Count; i++)
                {
                    if (i != 0)
                    {
                        series += ',';
                    }

                    string x = axesX.GetAt(i).ToJson("x");

                    string y = axesY.GetAt(i).ToJson("y");
                    string ss = x + ',' + y;

                    if (HasCategories())
                    {
                        ss += ',' + categories.GetAt(i).ToJson("category");
                    }

                    series += '{' + ss + '}';
                }

                series += "]," + seriesItem.Title.ToJson("name", false, true);

                if (seriesItem.Colored)
                    series += ',' + seriesItem.Color.ToJson("color");

                series += ',' + seriesItem.SeriesType.ToJson("type");

                if (seriesItem.Group != 0)
                    series += ',' + seriesItem.Group.ToJson("group");

                series += ',' + seriesItem.Transparent.ToJson("transparent");

                switch (seriesItem.Style)
                {
                    case EnumChartStyle.Normal:
                        series += ',' + "normal".ToJson("style");
                        break;
                    case EnumChartStyle.Smooth:
                        series += ',' + "smooth".ToJson("style");
                        break;
                    case EnumChartStyle.Step:
                        series += ',' + "step".ToJson("style");
                        break;
                }
                series += ',' + seriesItem.ShowLabel.ToJson("label");

                series += '}';
                if (count > 0)
                {
                    series += ",";
                    count--;
                }
            }
        
            series += ']';

            return "\"series\":" + series;
        }


        string ToJsonDataFamilyBubbleScatter()
        {
            if (Series.Count == 0)
            {
                return string.Empty;
            }

            string series = "[";

            int count = Series.Count - 1;
            DataArray categories = HasCategories() ? GetArray(Categories.BindedField) : null;

            foreach (Series seriesItem in Series)
            {              
                DataArray axesX = GetArray(seriesItem.BindedFields[0]);
                DataArray axesY = GetArray(seriesItem.BindedFields[1]);
                DataArray size = IsChartFamilyBubble() ? GetArray(seriesItem.BindedFields[2]) : null;

                if (axesX == null || axesY == null)
                {
                    return string.Empty;
                }
                if (axesX.Count != axesY.Count)
                {
                    return string.Empty;
                }


                series += "{\"data\":[";

                for (int i = 0; i < axesX.Count; i++)
                {
                    if (i != 0)
                    {
                        series += ',';
                    }

                    string x = axesX.GetAt(i).ToJson("x");

                    string y = axesY.GetAt(i).ToJson("y");
                    string ss = x + ',' + y;

                    if (HasCategories())
                    {
                        ss += ',' + categories.GetAt(i).ToJson("category");
                    }
                    if (size != null)
                    {
                        ss += "," + size.GetAt(i).ToJson("size");
                    }

                    series += '{' + ss + '}';
                }

                series += "]," + seriesItem.Title.ToJson("name", false, true);

                if (seriesItem.Colored)
                    series += ',' + seriesItem.Color.ToJson("color");

                series += ',' + seriesItem.SeriesType.ToJson("type");

                if (seriesItem.Group != 0)
                    series += ',' + seriesItem.Group.ToJson("group");

                series += ',' + seriesItem.Transparent.ToJson("transparent");

                switch (seriesItem.Style)
                {
                    case EnumChartStyle.Normal:
                        series += ',' + "normal".ToJson("style");
                        break;
                    case EnumChartStyle.Smooth:
                        series += ',' + "smooth".ToJson("style");
                        break;
                    case EnumChartStyle.Step:
                        series += ',' + "step".ToJson("style");
                        break;
                }

                series += ',' + seriesItem.ShowLabel.ToJson("label");

                series += '}';
                if (count > 0)
                {
                    series += ",";
                    count--;
                }
            }
        
            series += ']';

            return "\"series\":" + series;
        }
        //---------------------------------------------------------------------
        public override string ToJsonData(bool bracket)
        {
            string name = "chart";

            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            s += '{' + base.ToJsonData(false) + ',';

            //---------------------------
            if (IsChartFamilyBar())
            {
                s += ToJsonDataFamilyBar();
            }
            else if (IsChartFamilyPie() || IsChartFamilyRadar())
            {
                s += ToJsonDataFamilyPieRadar();
            }
            else if (IsChartFamilyPolar() || IsChartFamilyRange())
            {
                s += ToJsonDataFamilyPolarRange();
            }
            else if (IsChartFamilyScatter() || IsChartFamilyBubble())
            {
                s += ToJsonDataFamilyBubbleScatter();
            }
            else
            {
                s = s.Left(s.Length - 1);
            }

            //---------------------------
            s += '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }
    }
}
