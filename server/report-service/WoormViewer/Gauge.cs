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
using System.Diagnostics;

//using Microarea.RSWeb.Temp;

namespace Microarea.RSWeb.Objects
{
    public enum EnumGaugeType
    //ATTENZIONE: tenere allineato in: 
    //c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\Gauge.h - EnumGaugeType
    //c:\development\standard\web\server\report-service\woormviewer\Gauge.cs - EnumGaugeType
    //c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - GaugeType
    //------
    {
        None,
        Linear, Radial, Arc,
        Wrong
    }

    enum EnumGaugeStyle
    //ATTENZIONE: tenere allineato in: 
    //c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\Chart.h - EnumChartType
    //c:\development\standard\web\server\report-service\woormviewer\table.cs - EnumChartType
    //c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - ChartType
    //------
    {
        Arrow = 1,
        Bar
    };

    class GaugePointer
    {
        public Gauge Parent = null;

        public Variable BindedField = null;
        public double Transparent = 1;
        public Color Color = Color.Blue;
        public bool Colored = false;
        public EnumGaugeStyle Style = EnumGaugeStyle.Arrow;

        public GaugePointer(Gauge p) { Parent = p; }

        public string ToJson()
        {
            string s = "{";
            s += "\"color\":" + Color.ToJson() + ",";
            s += "\"transparent\":" + Transparent.ToJson() + ",";
            s += "\"style\":" + (int)Style + "}";
            return s;
        }
        public string ToJsonData()
        {
            string s = "{";

            s += BindedField.Data.ToJson("value");

            return s + '}';
        }
    };

    class GaugeRangeColor
    {
        public Gauge Parent = null;

        public double From = 0;
        public double To = 0;

        public Color Color = Color.White;
        public bool Colored = false;

        public GaugeRangeColor(Gauge p) { Parent = p; }

        public string ToJson()  
        {
            string s = "{";
            s += "\"color\":" + Color.ToJson() + ",";
            s += "\"from\":" + From.ToJson() + ",";
            s += "\"to\":" + To.ToJson() + "}";
            return s;
        }
    };

    public class Gauge : BaseRect
    {
        public string Name;
        public EnumGaugeType GaugeType = EnumGaugeType.None;

        List<GaugePointer> pointers = new List<GaugePointer>();
        List<GaugeRangeColor> ranges = new List<GaugeRangeColor>();

        public double Min = 0;
        public double Max = 0;

        public double MinorUnit = 0;
        public double MajorUnit = 0;

        public bool Landscape = false;

        //------------------------------------------------------------------------------
        public Gauge(WoormDocument document)
            : base(document)
        {
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
        bool ParsePointer(WoormParser lex, GaugePointer pointer)
        {
            if (!lex.ParseBegin())
                return false;

            bool ok;

            if (lex.Matched(Token.DATASOURCE))
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
                pointer.BindedField = pF;
            }

            if (lex.Matched(Token.STYLE))
            {
                int st = 0;
                if (!lex.ParseInt(out st))
                    return false;
                pointer.Style = (EnumGaugeStyle)st;
            }

            if (lex.LookAhead(Token.COLOR))
            {
                if (!lex.ParseColor(Token.COLOR, out pointer.Color))
                {
                    return false;
                }
                pointer.Colored = true;
            }
            else
                pointer.Colored = false;

            if (lex.Matched(Token.TRANSPARENT))
            {
                if (!lex.ParseDouble(out pointer.Transparent))
                    return false;
            }

            return lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        bool ParseRangeColor(WoormParser lex, GaugeRangeColor range)
        {
            bool ok = lex.ParseTag(Token.ROUNDOPEN) &&
                    lex.ParseDouble(out range.From) && lex.ParseTag(Token.COMMA) &&
                    lex.ParseDouble(out range.To) && lex.ParseTag(Token.COMMA) &&
                    lex.ParseColor(Token.COLOR, out range.Color) &&
                    lex.ParseTag(Token.ROUNDCLOSE);
            return ok;
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            bool ok = lex.ParseTag(Token.GAUGE) && lex.ParseID(out Name) &&
                        lex.ParseBegin() &&
                        lex.ParseAlias(out this.InternalID);

            int t = 0;
            ok = ok &&
                lex.ParseTag(Token.TYPE) && lex.ParseInt(out t) &&
                lex.ParseRect(out this.Rect);

            GaugeType = (EnumGaugeType)t;

            ok = ok && ParseBlock(lex);

            ok = lex.ParseTag(Token.GAUGE_SCALE) &&
                lex.ParseTag(Token.ROUNDOPEN) &&
                lex.ParseDouble(out Min) && lex.ParseTag(Token.COMMA) &&
                lex.ParseDouble(out Max) && lex.ParseTag(Token.COMMA) &&
                lex.ParseDouble(out MinorUnit) && lex.ParseTag(Token.COMMA) &&
                lex.ParseDouble(out MajorUnit) &&
                lex.ParseTag(Token.ROUNDCLOSE);
            if (!ok)
                return false;

            Landscape = lex.Matched(Token.LANDSCAPE);

            while (lex.Matched(Token.GAUGE_RANGE_COLOR))
            {
                GaugeRangeColor r = new GaugeRangeColor(this);
                if (!ParseRangeColor(lex, r))
                {
                    return false;
                }
                ranges.Add(r);
            }

            while (lex.Matched(Token.GAUGE_POINTER))
            {
                GaugePointer pointer = new GaugePointer(this);
                if (!ParsePointer(lex, pointer))
                {
                    return false;
                }
                pointers.Add(pointer);
            }

            if (!lex.ParseEnd())
                return false;

            return true;
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser ofile)
        {
            //TODO unparse GAUGE

            //---- Template Override
            BaseRect tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----
            //TODO 

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
            string name = "gauge";
            string s = '\"' + name + "\":";

            s += '{' +
               base.ToJsonTemplate(false) +
               ',' + GaugeType.ToJson("gaugeType");

            if (this.GaugeType == EnumGaugeType.Linear)
                s += ',' + (Landscape ? false : true).ToJson("vertical");//  <kendo-lineargauge [pointer]="{ value: value }" [scale]="{ vertical: true }">

            s += ',' + this.MinorUnit.ToJson("monirUnit");
            s += ',' + this.MajorUnit.ToJson("majorUnit");
            s += ',' + this.Min.ToJson("min");
            s += ',' + this.Max.ToJson("max");


            s += ',' + " \"ranges\":[";
            for (int i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];
                s += range.ToJson() + ((i < ranges.Count - 1) ? "," : "");
            }

            s += "]";

            s += ',' + " \"pointers\":[";
            for (int i = 0; i < pointers.Count; i++)
            {
                var point = pointers[i];
                s += point.ToJson() + ((i < pointers.Count - 1) ? "," : "");
            }

            s += "]";

            s += '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        

        //---------------------------------------------------------------------
        public override string ToJsonData(bool bracket)
        {
            string name = "gauge";

            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            s += '{' + base.ToJsonData(false)+'}';


            if (pointers.Count > 0)
                s += ", pointers:[";
            for(int i = 0; i < pointers.Count; i++)
            {
                var pointer = pointers[i];
                s += pointer.ToJsonData();

                if (i < pointers.Count - 1)
                    s += ',';
            }
           
            if (pointers.Count > 0)
                s += "]";

            if (bracket)
                s = '{' + s + '}';

            return s;
        }
    }
}
