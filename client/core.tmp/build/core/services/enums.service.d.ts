import { HttpService } from './http.service';
export declare class EnumsService {
    private httpService;
    private enumsTable;
    private getEnumsTableSubscription;
    constructor(httpService: HttpService);
    getEnumsTable(): void;
    getEnumsItem(storedValue: string): any;
    getItemsFromTag(tag: string): any;
    dispose(): void;
}
