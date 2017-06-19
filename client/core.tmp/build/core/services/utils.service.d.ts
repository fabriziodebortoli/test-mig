import { Logger } from './logger.service';
export declare class UtilsService {
    private logger;
    constructor(logger: Logger);
    serializeData(data: any): string;
    generateGUID(): string;
    toArray(items: any): any;
    getCurrentDate: () => number;
    parseBool(str: any): boolean;
    getApplicationFromQueryString(): string;
    hexToRgba(hex: any): any;
}
