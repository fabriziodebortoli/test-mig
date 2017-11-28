import { BehaviorSubject, Observable } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone } from '@angular/core';

export type ClientPage = {key: string, rows: any[], total: number, oldTotal: number, columns: any[]};
export type ServerPage = {key: string, rows: any[], columns: any[]};
export type ServerNeededParams = { model?: any, customFilters?: any };

@Injectable()
export class PaginatorService implements OnDestroy {
    private get defaultClientData() { return {key: '', rows: [], total: 0, oldTotal: 0, columns: []}; }
    private getFreshData: (page: number, rowsPerPage: number, srvParams: ServerNeededParams) => Observable<any>;
    private clientStartOffset = 0;
    private clientEndOffset = 0;
    private isCorrectlyConfigured = false;
    private configurationChanged = new BehaviorSubject(false);
    private displayedServerPages = 2;
    private get serverPage(): number { return this.clientPage * this.displayedServerPages; }
    private serverData: ServerPage;
    private currentServerPage = -1;
    private clientPage = 10;
    private lastServerPageRowsLength = -1;
    private higherServerPage = 0;
    private _lookAheadServerPageCache: {page: number, data: ServerPage} = {page: -1, data: null};
    private prevSkip = -1;
    private _skip = 0;
    private get skip(): number { return this._skip; }
    private set skip(value: number) {
        this.prevSkip = this._skip;
        this._skip = value;
    }

    private lastServNeededParams: ServerNeededParams;
    private srvNeededParamsTrigger$: Observable<ServerNeededParams>;

    private  get willChangeServerPage() {
        return this.clientEndOffset >= this.serverData.rows.length &&
        (this.lastServerPageRowsLength < 0 ||
            (this.lastServerPageRowsLength >= 0 && this.currentServerPage < this.higherServerPage));
    }

    private _clientData: BehaviorSubject<ClientPage> = new BehaviorSubject(this.defaultClientData);
    public get clientData(): Observable<ClientPage> {
        return this._clientData.asObservable();
    }

    constructor(private ngZone: NgZone) {
        this.ngZone.runOutsideAngular(() => {
            this.configurationChanged.subscribe(c => {
                this.clientEndOffset = 0;
                this.clientStartOffset = 0;
                this._clientData.next(this.defaultClientData);
            });
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
        if (this.lastServerPageRowsLength >= 0) {
            return (this.higherServerPage + 1) * this.serverPage + this.lastServerPageRowsLength;
        } else {
            if (this.willChangeServerPage) {
                let data = await this.loadServerPage(this.currentServerPage + 1, this.serverPage, this.lastServNeededParams);
                return (this.higherServerPage + 1) * this.serverPage + data.rows.length;
            } else {
                return (this.higherServerPage + 1) * this.serverPage;
            }
        }
    }

    private async loadServerPage(currentServerPage: number, serverPage: number, otherParams: ServerNeededParams): Promise<ServerPage> {
        if (currentServerPage === this._lookAheadServerPageCache.page) {
            return this._lookAheadServerPageCache.data;
        }
        let data = this.ngZone.runOutsideAngular(async () =>
            await this.getFreshData(currentServerPage, serverPage, otherParams).toPromise() as ServerPage);
        this._lookAheadServerPageCache = {page: currentServerPage, data: data};
        return data;
    }

    private getServerPage(skip: number): number {
        if (skip === 0) { return 0; }
        return Math.trunc(skip / this.serverPage);
    }

    private async nextPage(prevSkip: number, skip: number, take: number) {
        if (!this.isCorrectlyConfigured || this.getNumberOfClientSteps(prevSkip, skip) <= 0) { return; }
        this.clientStartOffset = skip === 0 ? 0 : skip % this.serverPage;
        this.clientEndOffset = this.clientStartOffset + take;
        let newServerPage = this.getServerPage(skip);
        let newServerPageNeeded = this.currentServerPage !== newServerPage;
        if (newServerPageNeeded) {
            let data = await this.loadServerPage(newServerPage, this.serverPage, this.lastServNeededParams);
            if (!data || !data.rows) { return; }
            if (data.rows.length > 0) {
                this.higherServerPage = newServerPage;
                this.currentServerPage = newServerPage;
                if (data.rows.length < this.serverPage) {
                    this.lastServerPageRowsLength = data.rows.length;
                }
            }

            this.serverData = data;
        }

        let newTotal = await this.getNewTotal();
        this._clientData.next({
            key: this.serverData.key,
            rows: this.serverData.rows.slice(this.clientStartOffset, this.clientEndOffset),
            total: newTotal,
            oldTotal: this._clientData.value.total,
            columns: this.serverData.columns});
    }

    private async prevPage(prevSkip: number, skip: number, take: number) {
        let clientSteps = this.getNumberOfClientSteps(prevSkip, skip);
        if (!this.isCorrectlyConfigured || clientSteps <= 0) { return; }

        this.clientStartOffset = skip === 0 ? 0 : skip % this.serverPage;
        this.clientEndOffset = this.clientStartOffset + take;
        let newServerPage = this.getServerPage(skip);
        let newServerPageNeeded = this.currentServerPage !== newServerPage;
        if (newServerPageNeeded) {
            let data = await this.loadServerPage(newServerPage, this.serverPage, this.lastServNeededParams);
            if (!data || !data.rows || data.rows.length === 0) { return; }
            this.serverData = data;
            this.currentServerPage = newServerPage;
        }

        this._clientData.next({
            key: this.serverData.key,
            rows: this.serverData.rows.slice(this.clientStartOffset, this.clientEndOffset),
            total: this._clientData.value.total,
            oldTotal: this._clientData.value.total,
            columns: this.serverData.columns});
    }

    public configure(displayedClientPages: number, rowsPerPage: number,
        srvNeededParamsTrigger$: Observable<ServerNeededParams>,
        f: (page: number, rowsPerPage: number, srvParams: ServerNeededParams) => Observable<any>) {
        this.isCorrectlyConfigured = f && (displayedClientPages >= 1) && (rowsPerPage > 0);
        if (!this.isCorrectlyConfigured) { return; }
        this.srvNeededParamsTrigger$ = srvNeededParamsTrigger$;
        this.displayedServerPages = displayedClientPages;
        this.clientPage = rowsPerPage;
        this.getFreshData = f;
        if (this.srvNeededParamsTrigger$) {
            this.ngZone.runOutsideAngular(() => {
                this.srvNeededParamsTrigger$.subscribe((e) => {
                    this.lastServNeededParams = e;
                    this.firstPage();
                });
            });
        }
        this.configurationChanged.next(true);
    }

    public getClientPageIndex(index: number): number {
        return index % this.clientPage;
    }

    private reset() {
        this.currentServerPage = -1;
        this._lookAheadServerPageCache = { page: -1, data: null };
        this.higherServerPage = 0;
        this.lastServerPageRowsLength = -1;
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
        this.configurationChanged.complete();
        this._clientData.complete();
    }
}
