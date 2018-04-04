export enum ChartType 
//ATTENZIONE: tenere allineato in: 
//c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\chart.h - EnumChartType
//c:\development\standard\web\server\report-service\woormviewer\table.cs - EnumChartType
//c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - ChartType
//------
{
    None,

    Bar, BarStacked, BarStacked100,
    Column, ColumnStacked, ColumnStacked100,
    Area, AreaStacked, AreaStacked100,Line, 

    Funnel, Pie, Donut, DonutNested, 

    RangeBar, RangeColumn, RangeArea,

    Bubble, BubbleScatter,
     
    Scatter, ScatterLine, 
    PolarLine, PolarArea, PolarScatter,
    RadarLine, RadarArea, 

    Wrong,

    //solo Kendo UI
    VerticalLine, VerticalArea, RadarColumn,
    //solo BCGP
    Pyramid, RadarScatter
    //versioni 3D di bar,column,area  
}
