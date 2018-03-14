import { Injectable, OnDestroy, NgZone } from '@angular/core';
import { WebSocketService } from './websocket.service';
import { EventDataService } from './eventdata.service';
import { Subscription, Observable, Subject } from '../../rxjs.imports';
import * as _ from 'lodash';
import { untilDestroy } from './../../shared/commons/untilDestroy';

export type HyperLinkInfo = {name: string, cmpId: string, enableAddOnFly: boolean, mustExistData: boolean, model: any};
export type HyperLinkStyles = {color: string, textDecoration: string, cursor: string, pointerEvents: string};

interface ChangeFocusEvent { source: HTMLElement; target: HTMLElement }

//TODO: this is a orrible workaround... Structural review of "AddOnFly" behavior is needed!
function isInAddOnFlyExclusionList(e: HTMLElement): boolean {
    if(!e) return true;
    let parentOfCurrElem = e.parentElement;
    if(!parentOfCurrElem) return false;
    if(e.classList.contains('k-tabstrip-items')) return true;
    let cmpIdAttr = parentOfCurrElem.attributes ? parentOfCurrElem.attributes.getNamedItem('cmpid') : null;
    if(!cmpIdAttr) return false;
    return cmpIdAttr.value === 'ID_EXTDOC_ESCAPE' || cmpIdAttr.value ==='ID_EXTDOC_EXIT';
}

@Injectable()
export class HyperLinkService implements OnDestroy {

    private _elementInfo: {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }}
    public get elementInfo(): {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }} {
        if(!this._elementInfo) {
            let e = this.getElementLogic();
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
        }
        return this._elementInfo;
    }

    private currentValue: any;
    private currentType: number;
    onBackToFocus: (oldValue: any, value: any) => void = (oldValue, value) => { };
    onAfterAddOnFly: (any) => void = value => { };
    private shouldAddOnFly = (focusEvent: ChangeFocusEvent) => true;
    private getElementLogic: () => HTMLElement = () => null;
    private oldElementVaue: any;

    private get focusChanged$(): Observable<ChangeFocusEvent> {
        return Observable.fromEvent<FocusEvent>(this.elementInfo.element, 'blur', {capture: false}).pipe(untilDestroy(this))
        .map(fe => ({source: fe.target as HTMLElement, target: fe.relatedTarget as HTMLElement}));
    }

    start (getElementLogic: () => HTMLElement, 
           info: HyperLinkInfo, 
           slice$: Observable<{ value: any, enabled: boolean, selector: any, type: number }>,
           onBackToFocus: (oldValue: any, value: any) => void,
           onAfterAddOnFly: (any) => void,
           customShouldAddOnFlyPredicate?: (focusedElem: HTMLElement) => boolean): void {
        if (!getElementLogic || !info) throw Error('You should provide a value form "getElement" and "info"');       
        this.withGetElementLogic(getElementLogic)
            .withHyperLinkLogic(info, slice$)
            .withAddOnFlyLogic(info, slice$, onBackToFocus, onAfterAddOnFly, customShouldAddOnFlyPredicate);
    }

    private withHyperLinkLogic(info: HyperLinkInfo, slice$: Observable<{ value: any, enabled: boolean, selector: any, type: number }>) : HyperLinkService {
        if(slice$)
            slice$.pipe(untilDestroy(this)).subscribe(x => this.shouldEnableHyperLink(x) ? this.enableHyperLink(info) : this.disableHyperLink(x));
        return this;
    }

    private withGetElementLogic(getElementLogic: () => HTMLElement): HyperLinkService {
        this.getElementLogic = getElementLogic;
        return this;
    }

    private withAddOnFlyLogic(info: HyperLinkInfo, 
        slice$: Observable<{ value: any, enabled: boolean, selector: any, type: number }>,
        onBackToFocus: (oldValue: any, value: any) => void,
        onAfterAddOnFly: (any) => void,
        customShouldAddOnFlyPredicate?: (focusedElem: HTMLElement) => boolean): HyperLinkService {
        if (slice$) slice$.filter(x => !x.enabled).pipe(untilDestroy(this)).subscribe(x => this.oldElementVaue = x.value);
        this.onBackToFocus = onBackToFocus;
        this.onAfterAddOnFly = onAfterAddOnFly;
        this.shouldAddOnFly = (focusEvent: ChangeFocusEvent) => 
            this.elementInfo.element.contains(focusEvent.source) 
            && !isInAddOnFlyExclusionList(focusEvent.target)
            && (!customShouldAddOnFlyPredicate || customShouldAddOnFlyPredicate(focusEvent.target));
        this.focusChanged$.filter(x => this.shouldAddOnFly(x)).subscribe(_ => this.addOnFly(info)); 
        return this;
    }

    private shouldEnableHyperLink(info: {value: any, enabled: boolean}): boolean {
        return !info.enabled && info.value;   
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
            .subscribe(ok => ok ? this.afterAddOnFly(info).subscribe(s => this.updateCmpValue(s)) : this.giveBackFocus(this.currentValue));

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
        this.giveBackFocus(value);
    }

    private giveBackFocus(value: any) {
        this.elementInfo.element.focus();
        if (this.onBackToFocus) this.onBackToFocus(this.oldElementVaue, value);
    }

    private enableHyperLink(info: HyperLinkInfo): void {
        if(this.elementInfo) {
            this.elementInfo.element.style.textDecoration = this.styles.textDecoration; 
            this.elementInfo.element.style.color = this.styles.color;
            this.elementInfo.element.style.cursor = this.styles.cursor;
            this.elementInfo.element.style.pointerEvents = this.styles.pointerEvents;
            this.elementInfo.clickSubscription = Observable.fromEvent(document, 'click', { capture: true })
              .filter(e => (e as any) && this.elementInfo.element && this.elementInfo.element.contains((e as any).target))
              .subscribe(e => this.follow(info));
        }
    }

    private disableHyperLink(info: {value: any, type: number}) : void {
        this.currentValue = info.value;
        this.currentType = info.type
        if(this.elementInfo) {
            this.elementInfo.element.style.textDecoration = this.elementInfo.initInfo.textDecoration; 
            this.elementInfo.element.style.color = this.elementInfo.initInfo.color;
            this.elementInfo.element.style.cursor = this.elementInfo.initInfo.cursor;
            this.elementInfo.element.style.pointerEvents = this.elementInfo.initInfo.pointerEvents;
            if(this.elementInfo.clickSubscription)
                this.elementInfo.clickSubscription.unsubscribe()
        }
    }

    ngOnDestroy(): void { }
}