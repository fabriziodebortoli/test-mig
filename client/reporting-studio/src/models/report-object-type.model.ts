export enum ReportObjectType { textrect, fieldrect, table, graphrect, sqrrect, chart, repeater, cell, link}

export function ReportObjectTypeDecorator(constructor: Function) {
    constructor.prototype.ReportObjectType = ReportObjectType;
}