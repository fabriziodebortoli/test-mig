import { Injectable, OnDestroy, NgZone } from '@angular/core';
import { WebSocketService } from './websocket.service';
import { EventDataService } from './eventdata.service';
import { Subscription, Observable, Subject } from '../../rxjs.imports';
import * as _ from 'lodash';
import { untilDestroy } from './../../shared/commons/untilDestroy';

export type HyperLinkInfo = {name: string, cmpId: string, enableAddOnFly: boolean, mustExistData: boolean, model: any};
export type HyperLinkStyles = {color: string, textDecoration: string, cursor: string, pointerEvents: string};

@Injectable()
export class HyperLinkService implements OnDestroy {

    private _elementInfo: {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }}
    public get elementInfo(): {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }} {
        return this._elementInfo;
    }

    private currentValue: any;
    private currentType: number;
    onBackToFocus: () => void;
    onAfterAddOnFly: (any) => void;
    start (e: HTMLElement, 
           info: HyperLinkInfo, 
           slice$: Observable<{ value: any, enabled: boolean, selector: any, type: number }>,
           onBackToFocus: () => void,
           onAfterAddOnFly: (any) => void): void {
        if (!e || !info) return;
        if (slice$) {
            slice$.pipe(untilDestroy(this)).subscribe(x => {
                if(!x.enabled && x.value) { 
                  this.enableHyperLink();
                  this.elementInfo.clickSubscription = Observable.fromEvent(document, 'click', { capture: true })
                    .filter(e => (e as any) && this.elementInfo.element.contains((e as any).target))
                    .subscribe(e => this.follow(info));
                } else {
                  this.currentValue = x.value;
                  this.currentType = x.type
                  
                  this.disableHyperLink();
                  if(this.elementInfo && this.elementInfo.clickSubscription)
                    this.elementInfo.clickSubscription.unsubscribe();
                }
              });
        }


        Observable.fromEvent(e, 'blur', { capture: false }).pipe(untilDestroy(this)).subscribe(e => this.addOnFly(info));            

        let oldColor = e.style.color;
        let oldDecoration = e.style.textDecoration;
        let oldCursor = e.style.cursor;
        let oldPointerEvents = e.style.pointerEvents;
        this._elementInfo = { 
            element: e,
            initInfo: {
            color: oldColor,
            textDecoration: oldDecoration,
            cursor: oldCursor,
            pointerEvents: oldPointerEvents
            }
        };

        this.onBackToFocus = onBackToFocus;
        this.onAfterAddOnFly = onAfterAddOnFly;
    }

    private get styles(): HyperLinkStyles {
        return {color: 'blue', textDecoration: 'underline', cursor: 'pointer', pointerEvents: 'all'};
    }

    constructor(private wsService: WebSocketService,
                private eventDataService: EventDataService){ }

    private async follow(p: HyperLinkInfo): Promise<void> {
        await this.wsService.openHyperLink(p.name, p.cmpId);
    }

    private addOnFly = (info: HyperLinkInfo) => 
        this.userChoice(info, this.currentValue, this.currentType)
            .subscribe(ok => ok ? this.afterAddOnFly(info).subscribe(this.updateCmpValue) : this.giveBackFocus());

    /**
    * Creates an observable that emits a single boolean (the user choice) after the user press the "yes | no"
    * button of the "add on fly" dialog. Then completes.
    * @param info the info needed by the "Hyper Link" behavior
    * @param currentValue key value to add on fly 
    * @param currentType model type
    */
    private userChoice = (info: HyperLinkInfo, currentValue: any, currentType: number): Observable<boolean> =>
        (currentValue === '' || currentValue === undefined) ? Observable.never<boolean>() :
        Observable.fromPromise(this.wsService.queryHyperLink(info.name, info.cmpId, currentValue, currentType)).take(1).map(_ => false)
        .concat(this.eventDataService.closeMessageDialog.take(1).map(res => res['yes'])).skip(1);

    
    private afterAddOnFly = (info: HyperLinkInfo): Observable<string> => 
        this.wsService.addOnFly.filter(info => info.name === info.name).take(1).map(msg => msg.value);

    private updateCmpValue(value:any) {
        if (this.onAfterAddOnFly) this.onAfterAddOnFly(value);
    }

    private giveBackFocus() {
        this.elementInfo.element.focus();
        if (this.onBackToFocus) this.onBackToFocus();
    }

    private enableHyperLink(): void {
        if(this.elementInfo) {
            this.elementInfo.element.style.textDecoration = this.styles.textDecoration; 
            this.elementInfo.element.style.color = this.styles.color;
            this.elementInfo.element.style.cursor = this.styles.cursor;
            this.elementInfo.element.style.pointerEvents = this.styles.pointerEvents;    
        }
    }

    private disableHyperLink() : void {
        if(this.elementInfo) {
            this.elementInfo.element.style.textDecoration = this.elementInfo.initInfo.textDecoration; 
            this.elementInfo.element.style.color = this.elementInfo.initInfo.color;
            this.elementInfo.element.style.cursor = this.elementInfo.initInfo.cursor;
            this.elementInfo.element.style.pointerEvents = this.elementInfo.initInfo.pointerEvents;
        }
    }

    ngOnDestroy(): void { }
}