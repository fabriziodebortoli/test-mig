import { BehaviorSubject, Observable, Subscription, Subject } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone } from '@angular/core';
import * as _ from 'lodash';

export type ClientPage = {key: string, rows: any[], total: number, oldTotal: number,
    columns: any[], ignore: boolean};
export type ServerNeededParams = { model?: any, customFilters?: any, customSort?: any };

@Injectable()
export class PaginatorService implements OnDestroy {
    private get defaultClientData() { return {serverData: {}, key: '', rows: [], total: 0, oldTotal: 0, columns: [], ignore : true }; }
    private getFreshData: (page: number, rowsPerPage: number, srvParams: ServerNeededParams) => Observable<any>;
    private clientStartOffset = 0;
    private clientEndOffset = 0;
    private isCorrectlyConfigured = false;
    private configurationChanged = new BehaviorSubject(false);
    private displayedClientPages = 10;
    private get serverPage(): number { return this.clientPage * this.displayedClientPages; }
    private serverData: any;
    private currentServerPageNumber = -1;
    private clientPage = 10;
    private sizeOfLastServer: number = -1;
    private higherServerPage = 0;
    private lookAheadServerPageCache: {page: number, data: any} = {page: -1, data: null};
    private prevSkip = -1;
    private _skip = 0;
    private get skip(): number { return this._skip; }
    private set skip(value: number) {
        this.prevSkip = this._skip;
        this._skip = value;
    }

    private lastServNeededParams: ServerNeededParams = { model: {value: ''}, customFilters: '' };
    private queryTrigger$: Observable<ServerNeededParams>;
    private needSrvParamTriggerSub: Subscription;

    private get noDataClientPage(): ClientPage {
        let p: ClientPage = _.cloneDeep(this.defaultClientData);
        p.columns = this._clientData.value.columns,
        p.ignore = false;
        p.key = this._clientData.value.key;
        p.total = this._clientData.value.total;
        p.oldTotal = this._clientData.value.oldTotal;
        p.rows = [];
        return p;
    }

    private  get willChangeServerPage() {
        return this.clientEndOffset >= this.serverData.rows.length &&
        ((this.sizeOfLastServer === -1) ||
            (this.sizeOfLastServer >= 0 && this.currentServerPageNumber < this.higherServerPage));
    }


    private _serverData$: Subject<any> = new Subject<any>();
    public get serverData$(): Observable<any> {
        return this._serverData$.asObservable();
    }

    private _clientData: BehaviorSubject<ClientPage> = new BehaviorSubject(this.defaultClientData);
    public get clientData(): Observable<ClientPage> {
        return this._clientData.asObservable().filter(x => !x.ignore);
    }

    public get currentPage() {
        return this.currentServerPageNumber;
    }

    public get isJustInitialized(): boolean{
        return this.clientStartOffset === 0 && this.currentServerPageNumber === -1;
    }

    public get isFirstPage(): boolean {
        return this.clientStartOffset === 0 && this.currentServerPageNumber === 0;
    }

    public get thereAreData(): boolean {
        return true;
    }

    public get noMorePages(): boolean {
        return this.sizeOfLastServer !== -1;
    }

    public waiting$ = new Subject<boolean>().distinctUntilChanged();

    constructor(private ngZone: NgZone) {
        this.configurationChanged.subscribe(c => {
            this.clientEndOffset = 0;
            this.clientStartOffset = 0;
            this._clientData.next(this.defaultClientData);
        });
     }

    private isNext(prevSkip: number, skip: number): boolean {
        if (prevSkip === -1) { return true }
        return prevSkip < skip;
      }

    private getNumberOfClientSteps(oldSkip: number, newSkip: number): number {
        if (oldSkip === newSkip) { return 0 };
        if (oldSkip === -1) { return 1 };
        let max = Math.max(oldSkip, newSkip);
        let min = Math.min(oldSkip, newSkip);
        return Math.abs(Math.trunc((max - min) / this.clientPage));
    }

    private async getNewTotal(): Promise<number> {
        if (this.sizeOfLastServer !== -1) {
            return this.higherServerPage * this.serverPage + this.sizeOfLastServer;
        } else {
            if (this.willChangeServerPage) {
                let nextPageIdx = this.currentServerPageNumber + 1;
                let data = await this.loadServerPage(nextPageIdx, this.serverPage, this.lastServNeededParams);
                if (this.lookAheadServerPageCache.page !== nextPageIdx) {
                    this.lookAheadServerPageCache = {page: nextPageIdx, data: data}
                    if (!data || !data.rows || data.rows.length === 0) {
                        this.sizeOfLastServer = 0;
                        return this._clientData.value.total;
                    } else { return (this.higherServerPage + 1) * this.serverPage + data.rows.length; }
                }
                return (this.higherServerPage + 1) * this.serverPage + data.rows.length;
            } else {
                return (this.higherServerPage + 1) * this.serverPage;
            }
        }
    }

    private async loadServerPage(currentServerPage: number, serverPage: number, otherParams: ServerNeededParams): Promise<any> {
        if (currentServerPage === this.lookAheadServerPageCache.page) {
            return this.lookAheadServerPageCache.data;
        }
        (this.waiting$ as Subject<boolean>).next(true);
        let data = await this.ngZone.runOutsideAngular(() =>
            this.getFreshData(currentServerPage, serverPage, otherParams).toPromise());
        this._serverData$.next(data);
        return data;
    }

    private getServerPage(skip: number): number {
        if (skip === 0) { return this.currentServerPageNumber === -1 ? 0 : Math.min(0, this.currentServerPageNumber -1); }
        return Math.trunc(skip / this.serverPage);
    }

    private async nextPage(prevSkip: number, skip: number, take: number) {
        if (!this.isCorrectlyConfigured || this.getNumberOfClientSteps(prevSkip, skip) <= 0) { return; }
        this.clientStartOffset = skip === 0 ? 0 : skip % this.serverPage;
        this.clientEndOffset = this.clientStartOffset + take;
        let newServerPage = this.getServerPage(skip);
        let newServerPageNeeded = this.currentServerPageNumber !== newServerPage;
        if (newServerPageNeeded) {
            let data = await this.loadServerPage(newServerPage, this.serverPage, this.lastServNeededParams);
            if (!data || !data.rows || data.rows.length === 0) {
                if (this.currentServerPageNumber < 0) { 
                    this._clientData.next(this.noDataClientPage); 
                    this.currentServerPageNumber = newServerPage;
                    this.sizeOfLastServer = 0;
                }
                (this.waiting$ as Subject<boolean>).next(false);
                return;
            }
            if (data.rows.length > 0) {
                this.higherServerPage = newServerPage;
                this.currentServerPageNumber = newServerPage;
                if (data.rows.length < this.serverPage) {
                    this.sizeOfLastServer = data.rows.length;
                }
            }

            this.serverData = data;
        }

        let newTotal = await this.getNewTotal();
        this._clientData.next({
            key: this.serverData.key ?  this.serverData.key : '',
            rows: this.serverData.rows.slice(this.clientStartOffset, this.clientEndOffset),
            total: newTotal,
            oldTotal: this._clientData.value.total,
            columns: this.serverData.columns,
            ignore: false });
        (this.waiting$ as Subject<boolean>).next(false);
    }

    private async prevPage(prevSkip: number, skip: number, take: number) {
        let clientSteps = this.getNumberOfClientSteps(prevSkip, skip);
        if (!this.isCorrectlyConfigured || clientSteps <= 0) { return; }

        this.clientStartOffset = skip === 0 ? 0 : skip % this.serverPage;
        this.clientEndOffset = this.clientStartOffset + take;
        let newServerPage = this.getServerPage(skip);
        let newServerPageNeeded = this.currentServerPageNumber !== newServerPage;
        if (newServerPageNeeded) {
            this.lookAheadServerPageCache = { page: -1, data: null };
            this.sizeOfLastServer = -1;
            let data = await this.loadServerPage(newServerPage, this.serverPage, this.lastServNeededParams);
            if (!data || !data.rows || data.rows.length === 0) { return; }
            this.serverData = data;
            this.currentServerPageNumber = newServerPage;
        }

        this._clientData.next({
            key: this.serverData.key ?  this.serverData.key : '',
            rows: this.serverData.rows.slice(this.clientStartOffset, this.clientEndOffset),
            total: this._clientData.value.total,
            oldTotal: this._clientData.value.total,
            columns: this.serverData.columns,
            ignore: false });
            (this.waiting$ as Subject<boolean>).next(false);
    }


    public start(displayedClientPages: number,
                rowsPerPage: number,
                queryTrigger$: Observable<ServerNeededParams>,
                f: (page: number, rowsPerPage: number, srvParams: ServerNeededParams) => Observable<any>,
                intialServerParams?: ServerNeededParams) {
        this.isCorrectlyConfigured = f && (displayedClientPages >= 1) && (rowsPerPage > 0);
        if (!this.isCorrectlyConfigured) { return; }
        this.queryTrigger$ = queryTrigger$;
        this.displayedClientPages = displayedClientPages;
        this.clientPage = rowsPerPage;
        this.getFreshData = f;
        if (this.queryTrigger$) {
            this.needSrvParamTriggerSub = this.queryTrigger$.subscribe((e) => {
                this.lastServNeededParams = e;
                this.firstPage();
            });
        }
        this.configurationChanged.next(true);
        if (intialServerParams) { 
            this.lastServNeededParams = intialServerParams;
            this.firstPage();
        }
    }

    public stop() {
        this.reset();
        this._clientData.complete();
        this._clientData = new BehaviorSubject(this.defaultClientData);
        (this.waiting$ as Subject<boolean>).complete();
        this.waiting$ = new Subject<boolean>().distinctUntilChanged();
        this.configurationChanged.complete();
        this.configurationChanged = new BehaviorSubject(false);
        if (this.needSrvParamTriggerSub) { this.needSrvParamTriggerSub.unsubscribe(); }
        
    }

    public getClientPageIndex(index: number): number {
        return index % this.clientPage;
    }

    private reset() {
        this.currentServerPageNumber = -1;
        this.lookAheadServerPageCache = { page: -1, data: null };
        this.higherServerPage = 0;
        this.sizeOfLastServer = -1;
        this.clientStartOffset = 0;
        this.clientEndOffset = 0;
        this.skip = 0;
        this.prevSkip = -1;
    }

    public async firstPage() {
        this.reset();
        await this.nextPage(-1, 0, this.clientPage);
    }

    public async pageChange(skip: number, take: number) {
        if (this.skip === skip) { return ; }
        this.skip = skip;
        if (this.isNext(this.prevSkip, skip)) {
            await this.nextPage(this.prevSkip, skip, take)
        } else {
            await this.prevPage(this.prevSkip, skip, take)
        }
    }

    ngOnDestroy() {
        this.stop();
    }
}
