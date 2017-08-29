export enum ChartType 
//ATTENZIONE: tenere allineato in: 
//c:\development\Standard\TaskBuilder\Framework\TbWoormViewer\TABLE.H - EnumChartType
//c:\development\standard\web\server\report-service\woormviewer\table.cs - EnumChartType
//c:\development\Standard\web\client\reporting-studio\src\models\chart-type.model.ts - ChartType
//------
{
    None,
    Bar, BarStacked, BarStacked100,
    Column, ColumnStacked, ColumnStacked100,
    Area, AreaStacked, AreaStacked100,
    Line, 
    Pie, Donut, DonutNested,
    Funnel, 
    Wrong,
    //mancano nei BCGP
    VerticalLine, VerticalArea,
    //unsupported nei Kendo UI
    Pyramid
    //versioni 3D di bar,column,area  
}