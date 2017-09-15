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

        RangeBar, RangeColumn,

        Bubble,

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
        public Variable BindedField = null;
        public string Title;

        public EnumChartType SeriesType = EnumChartType.None;

        public Color Color = Color.White;
        public bool Colored = false;
        public int Group = 0;   //for grouping stacked column/bar
        public EnumChartStyle Style = EnumChartStyle.Normal;

        /*CCategories : non riesco a fare la dichiarazione forward*/
        public Categories Parent = null;

        public Series(Categories p) { Parent = p; }
    };

    class Categories
    {
        public Variable BindedField = null;
        public string Title;

        public List<Series> Series = new List<Series>();

        public Color Color = Color.White;
        public bool Colored = false;

        /*CCategories : non riesco a fare la dichiarazione forward*/
        public Chart Parent = null;

        public Categories(Chart p) { Parent = p; }
    };

    class ChartLegend
    {
        //public AlignType Align = 0;
        public bool Hidden = false;

        public ChartLegend() { }
    };

    public class Chart : BaseRect
    {
        public string Name;
        public string Title;
        public EnumChartType ChartType = EnumChartType.None;

        List<Categories> Categories = new List<Categories>();

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
                ChartType != EnumChartType.None &&
                ChartType != EnumChartType.Scatter &&
                ChartType != EnumChartType.ScatterLine &&
                ChartType != EnumChartType.PolarLine &&
                ChartType != EnumChartType.PolarArea &&
                ChartType != EnumChartType.PolarScatter &&
                ChartType != EnumChartType.Bubble;
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

        bool IsChartFamilyRange()
        {
            return
                ChartType == EnumChartType.RangeBar ||
                ChartType == EnumChartType.RangeColumn ||

                ChartType == EnumChartType.RadarArea ||
                ChartType == EnumChartType.RadarLine ||

                ChartType == EnumChartType.Bubble;
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

            bool ok = lex.ParseTag(Token.TITLE) && lex.ParseString(out pSeries.Title);
            if (!ok)
                return false;

            if (lex.Matched(Token.TYPE))
            {
                int n = 0;
                if (!lex.ParseInt(out n))
                    return false;
                pSeries.SeriesType = (EnumChartType)n;
            }
            else pSeries.SeriesType = pSeries.Parent.Parent.ChartType;

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

            //if (!pF.IsArray() && !pF.IsColumn())
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

            //string dt = pF.DataType;
            //if (!dt.IsNumeric())
            //{
            //    lex.SetError(_TB("TODO - il campo associato alla serie deve essere numerico"));
            //    return false;
            //}

            pSeries.BindedField = pF;

            if (lex.LookAhead(Token.COLOR))
            {
                if (!lex.ParseColor(Token.COLOR, out pSeries.Color))
                    return false;
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

            if (lex.Matched(Token.GROUP))
            {
                if (!lex.ParseInt(out pSeries.Group))
                    return false;
            }

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        bool ParseCategories(WoormParser lex, Categories pCat)
        {
            if (!lex.ParseBegin())
                return false;

            if (HasCategories())
            {
                bool ok = lex.ParseTag(Token.TITLE) && lex.ParseString(out pCat.Title);
                if (!ok)
                    return false;

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

                pCat.BindedField = pF;

                if (lex.LookAhead(Token.COLOR))
                {
                    if (!lex.ParseColor(Token.COLOR, out pCat.Color))
                        return false;
                    pCat.Colored = true;
                }
                else
                    pCat.Colored = false;
            }

            while (lex.Matched(Token.CHART_SERIES))
            {
                Series pSeries = new Series(pCat);
                if (!ParseSeries(lex, pSeries))
                {
                    return false;
                }
                pCat.Series.Add(pSeries);
            }

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        bool ParseLegend(WoormParser lex)
        {
            bool ok = lex.ParseBegin() &&
                      lex.ParseTag(Token.HIDDEN) &&
                      lex.ParseBool(out Legend.Hidden);
            if (!ok)
                return false;

            //ok = lex.ParseAlign(out Legend.Align);
            //if (!ok)
            //    return false;

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            bool ok = lex.ParseTag(Token.CHART) &&
                        lex.ParseBegin() &&
                        lex.ParseTag(Token.TITLE) &&
                        lex.ParseString(out Title) &&
                        lex.ParseAlias(out this.InternalID);

            if (lex.Matched(Token.COMMA))
                ok = ok && lex.ParseID(out Name);

            int t = 0;
            ok = ok && lex.ParseRect(out this.Rect) &&
                       lex.ParseTag(Token.TYPE) &&
                       lex.ParseInt(out t);

            ChartType = (EnumChartType)t;

            ok = ok && ParseBlock(lex);

            while (lex.Matched(Token.CHART_CATEGORIES))
            {
                Categories pCat = new Categories(this);
                if (!ParseCategories(lex, pCat))
                {
                    return false;
                }
                Categories.Add(pCat);
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
                string position = "bottom";
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
            DataArray ar = GetArray(series.BindedField);
            if (ar == null)
            {
                return string.Empty;
            }

            string s = "{\"data\":[";

            bool first = true;
            for (int i = 0; i < ar.Count; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    s += ',';
                }


                s += ar.GetAt(i).ToJson();

            }

            s += "]," + series.Title.ToJson("name", false, true);

            if (series.Colored)
                s += ',' + series.Color.ToJson("color");

            s += ',' + series.SeriesType.ToJson("type");

            if (series.Group != 0)
                s += ',' + series.Group.ToJson("group");

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

            return s + '}';
        }

        //---------------------------------------------------------------------
        /**
         * Chart con le serie complesse
         */
        string ToJsonData(List<Series> seriesList, DataArray categories)
        {
            string s = "";
            int count = seriesList.Count - 1;
            foreach (Series series in seriesList)
            {
                DataArray arSeries = GetArray(series.BindedField);
                if (arSeries == null)
                {
                    return string.Empty;
                }
                if (categories.Count != arSeries.Count)
                {
                    return string.Empty;
                }


                s += "{\"data\":[";

                for (int i = 0; i < categories.Count; i++)
                {
                    if (i != 0)
                    {
                        s += ',';
                    }



                    string categoriesStr = categories.GetAt(i).ToJson("category");

                    string val = arSeries.GetAt(i).ToJson("value");
                    s += '{' + categoriesStr + ',' + val + '}';

                }

                s += "]," + series.Title.ToJson("name", false, true);

                if (series.Colored)
                    s += ',' + series.Color.ToJson("color");

                s += ',' + series.SeriesType.ToJson("type");

                if (series.Group != 0)
                    s += ',' + series.Group.ToJson("group");

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
                s += '}';
                if (count > 0)
                {
                    s += ",";
                    count--;
                }
            }

            return s;
        }

        //---------------------------------------------------------------------

        string ToJsonData(Categories cat)
        {
            DataArray ar = GetArray(cat.BindedField);
            if (ar == null)
            {
                return string.Empty;
            }

            string categories = "\"categories\":[";

            bool first = true;
            for (int i = 0; i < ar.Count; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    categories += ',';
                }

                categories += ar.GetAt(i).ToJson();
            }

            categories += ']';

            string category_axis = "\"category_axis\":{" +
                 cat.Title.ToJson("title", false, true) + ',' +
                 categories + '}';

            return category_axis;
        }
        //---------------------------------------------------------------------

        string ToJsonDataFamilyBar()
        {
            string series = "\"series\":[";
            bool first = true;

            for (int ser = 0; ser < this.Categories[0].Series.Count; ser++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    series += ',';
                }

                series += ToJsonData(this.Categories[0].Series[ser]);
            }
            series += ']';

            if (HasCategories())
                series += ',' + ToJsonData(this.Categories[0]);
            return series;
        }

        string ToJsonDataFamilyPie(List<Categories> categoriesList)
        {
            if (categoriesList == null || categoriesList.Count == 0)
            {
                return string.Empty;
            }

            string series = "[";
            int count = categoriesList.Count - 1;
            foreach (Categories cat in categoriesList)
            {
                DataArray categories = GetArray(cat.BindedField);
                series += ToJsonData(cat.Series, categories);
                if (count > 0)
                {
                    series += ",";
                    count--;
                }
            }

            series += ']';
            return series;
        }

        string ToJsonDataFamilyPie()
        {
            if (Categories[0] == null)
            {
                return string.Empty;
            }

            return "\"series\":" + ToJsonDataFamilyPie(Categories);
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
            else if (IsChartFamilyPie())
            {
                s += ToJsonDataFamilyPie();
            }
            //TODO CHART 
            //else if (IsChartFamily...())

            //---------------------------
            s += '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }
    }
}
