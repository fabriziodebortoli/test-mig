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
    //c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\TABLE.H - EnumChartType
    //c:\development\standard\web\server\report-service\woormviewer\table.cs - EnumChartType
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
        None,
        LineSmooth   /*spline*/
    };

    class Series 
	{
	    public Variable BindedField = null;
        public string Title;

        public EnumChartType SeriesType = EnumChartType.None;

        public Color Color = Color.White;
        public bool Colored = false;
        public int Group = 0;   //for grouping stacked column/bar
        public EnumChartStyle Style = EnumChartStyle.None;

        /*CCategories : non riesco a fare la dichiarazione forward*/
        public Categories Parent = null;

        public Series (Categories p) { Parent = p; }
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
        public AlignType Align = 0;
        public bool Hidden = false;

        public ChartLegend() { }
    };

    public class Chart: BaseRect
    {
        public string Name;
        public string Title;
        public EnumChartType ChartType = EnumChartType.None;

        List<Categories> Categories = new List<Categories>();

        ChartLegend Legend;

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
                int n;
                if (!lex.ParseInt(out n))
                    return false;
                pSeries.SeriesType = (EnumChartType)n;
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

            if (lex.LookAhead(Token.BKGCOLOR))
            {
                if (!lex.ParseColor(Token.BKGCOLOR, out pSeries.Color))
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
                pSeries.Style = (EnumChartStyle) st;
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

                if (lex.LookAhead(Token.BKGCOLOR))
                {
                    if (!lex.ParseColor(Token.BKGCOLOR, out pCat.Color))
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

            ok = lex.ParseAlign(out Legend.Align);
            if (!ok)
                return false;

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            bool ok = lex.ParseTag(Token.CHART) &&
                        lex.ParseBegin() &&
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
        override public string ToJsonTemplate(bool bracket)
        {
            string name = "chart";

            string s = '\"' + name + "\":";

            s += '{' +
                base.ToJsonTemplate(false) + ',' +

  
              '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string name = "repeater";

            string s = '\"' + name + "\":";

            s += '{' +
                base.ToJsonData(false) +
              '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

    }
}
