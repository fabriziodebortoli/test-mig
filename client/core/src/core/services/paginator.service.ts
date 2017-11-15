import { BehaviorSubject, Observable, Subscription } from 'rxjs.imports';
import { Injectable, OnDestroy } from '@angular/core';

@Injectable()
export class PaginatorService implements OnDestroy {
    private _defaultClientData = {rows: [], total: 0, oldTotal: 0, prevServerPage: false, nextServerPage: true, columns: []};
    private _clientStartStartOffset = 0;
    private _clientEndOffset = 0;
    private _isCorrectlyConfigured = false;
    private _configurationChanged = new BehaviorSubject(false);
    private _clientData: BehaviorSubject<{rows: any[], total: number,
        oldTotal: number, nextServerPage: boolean, prevServerPage: boolean, columns: any[]}>;
    private _subscription: Subscription;
    private serverPage = 50;
    private serverData: {rows: any[], columns: any[]};
    private currentServerPage = 0;
    private clientPage = 10;

    public get clientData(): Observable<{rows: any[], total: number, columns: any[]}> {
        return this._clientData;
    }

    private getFreshData: (page: number, rowsPerPage: number) => Observable<any>;

    constructor() {
        this._clientData = new BehaviorSubject(this._defaultClientData);
    }

    public configure(serverPage: number, clientPage: number, f: (page: number, rowsPerPage: number) => Observable<any>) {
        this._isCorrectlyConfigured = f && (serverPage >= clientPage) && (clientPage > 0);
        if (!this._isCorrectlyConfigured) { return; }
        this.serverPage = serverPage;
        this.clientPage = clientPage;
        this.getFreshData = f;
        this._configurationChanged.next(true);
        this._subscription = this._configurationChanged.subscribe(c => {
            this._clientEndOffset = 0;
            this._clientStartStartOffset = 0;
            this._clientData.next(this._defaultClientData);
        });
    }

    private async loadNextServerPage() {
        let data = await this.getFreshData(this.currentServerPage, this.serverPage).toPromise();
        this.serverData = data as {rows: any[], columns: any[]};
        this.currentServerPage += 1;
    }

    private async loadPrevServerPage() {
        let data = await this.getFreshData(this.currentServerPage, this.serverPage).toPromise();
        this.serverData = data as {rows: any[], columns: any[]};
        this.currentServerPage -= 1;
    }

    public async nextPage(steps = 1) {
        if (!this._isCorrectlyConfigured) { return; }

        let serverPageChanged = false;
        if (this._clientData.value.nextServerPage) {
            await this.loadNextServerPage();
            serverPageChanged = true;
            this._clientEndOffset = this.clientPage * (steps);
            this._clientStartStartOffset = 0;
        } else {
            this._clientEndOffset = this._clientEndOffset + this.clientPage * steps;
            this._clientStartStartOffset = this._clientStartStartOffset + this.clientPage * (steps);
        }

        let willChangeServerPage = !this.serverData || (this._clientEndOffset >= this.serverData.rows.length);
        this._clientData.next({
            rows: this.serverData.rows.slice(this._clientStartStartOffset, this._clientEndOffset),
            total: willChangeServerPage ? this._clientData.value.total + this.serverData.rows.length :
            Math.max(this.serverData.rows.length, this._clientData.value.total),
            oldTotal: this._clientData.value.total,
            nextServerPage: willChangeServerPage,
            prevServerPage: serverPageChanged,
            columns: this.serverData.columns});
    }

    public async prevPage(steps = 1) {
        if (!this._isCorrectlyConfigured) { return; }

        let serverPageChanged = false;

        if (this._clientData.value.prevServerPage) {
            await this.loadNextServerPage();
            serverPageChanged = true;
            this._clientEndOffset = this.serverData.rows.length;
            this._clientStartStartOffset = this.serverData.rows.length - (this.clientPage * steps);
        } else {
            this._clientEndOffset = this._clientEndOffset - this.clientPage * (steps);
            this._clientStartStartOffset = this._clientStartStartOffset - this.clientPage * steps;
        }

        let willChangeServerPage = !this.serverData || ((this._clientStartStartOffset - this.clientPage)  < 0);
        this._clientData.next({
            rows: this.serverData.rows.slice(this._clientStartStartOffset, this._clientEndOffset),
            total: this._clientData.value.oldTotal,
            oldTotal: this._clientData.value.total,
            prevServerPage: willChangeServerPage,
            nextServerPage: serverPageChanged,
            columns: this.serverData.columns});
    }

    public getNumberOfClientSteps(oldSkip: number, newSkip: number): number {
        return Math.abs(Math.trunc((newSkip - oldSkip) / this.clientPage));
    }

    ngOnDestroy() {
        this._subscription.unsubscribe();
    }
}
