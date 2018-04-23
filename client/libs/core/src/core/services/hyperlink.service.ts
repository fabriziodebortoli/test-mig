import { OnDestroy } from '@angular/core';
import { WebSocketService } from './websocket.service';
import { EventDataService } from './eventdata.service';
import { Subscription, Observable, Subject } from '../../rxjs.imports';
import * as _ from 'lodash';
import { untilDestroy } from './../../shared/commons/untilDestroy';
import { TbComponentServiceParams, TbComponentService, ComponentMediator } from '../..';

export type HyperLinkInfo = { name: string, cmpId: string, controlId: string, enableAddOnFly: boolean, mustExistData: boolean, model: any };
export type HyperLinkStyles = { color: string, textDecoration: string, cursor: string, pointerEvents: string };
type Destroyer = OnDestroy;

interface ChangeFocusEvent { source: HTMLElement; target: HTMLElement; }

//TODO: this is a orrible workaround... Structural review of "AddOnFly" behavior is needed!
function isInAddOnFlyExclusionList(e: HTMLElement): boolean {
    if (!e) return false;
    let parentOfCurrElem = e.parentElement;
    if (!parentOfCurrElem) return false;
    if (e.classList.contains('k-tabstrip-items')) return true;
    let cmpIdAttr = parentOfCurrElem.attributes ? parentOfCurrElem.attributes.getNamedItem('cmpid') : null;
    if (!cmpIdAttr) return false;
    return cmpIdAttr.value === 'ID_EXTDOC_ESCAPE' || cmpIdAttr.value === 'ID_EXTDOC_EXIT';
}

export class HyperLinkService {

    static New(wsService: WebSocketService, ext: EventDataService) { return new HyperLinkService(wsService, ext); }

    private _elementInfo: { element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string } }
    public get elementInfo(): { element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string } } {
        if (!this._elementInfo) {
            let e = this.getElementLogic();
            if (e) {
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
        }
        return this._elementInfo;
    }

    private _workingValue: any;
    public get workingValue(): any { return this._workingValue; }
    public set workingValue(value: any) { this._workingValue = value; }

    private _isAttachedComponentEnabled: boolean;
    public get isAttachedComponentEnabled(): boolean { return this._isAttachedComponentEnabled; }
    public set isAttachedComponentEnabled(value: boolean) { this._isAttachedComponentEnabled = value; }


    private _workingType: string;
    public get workingType(): string { return this._workingType; }
    public set workingType(value: string) { this._workingType = value; }

    onBackToFocus: (oldValue: any, value: any) => void = (oldValue, value) => { };
    onNextControl: (any) => void = value => { };
    private shouldAddOnFly = (focusEvent: ChangeFocusEvent) => true;
    private getElementLogic: () => HTMLElement = () => null;
    private oldElementVaue: any;

    private controlFocusChanged$(destroyer: Destroyer): Observable<ChangeFocusEvent> {
        return Observable.fromEvent<FocusEvent>(this.elementInfo.element, 'blur', { capture: false }).pipe(untilDestroy(destroyer))
            .map(fe => ({ source: fe.target as HTMLElement, target: fe.relatedTarget as HTMLElement }));
    }

    private getHotLinkButtonFocusChanged$(destroyer: OnDestroy, hotLinkButton: HTMLElement): Observable<ChangeFocusEvent> {
        return hotLinkButton ? Observable.fromEvent<FocusEvent>(hotLinkButton, 'blur', { capture: false }).pipe(untilDestroy(destroyer))
            .map(fe => ({ source: fe.target as HTMLElement, target: fe.relatedTarget as HTMLElement }))
            :
            Observable.never<{ source: HTMLElement, target: HTMLElement }>();
    }

    start(destroyer: Destroyer,
        getElementLogic: () => HTMLElement,
        hotLinkButton: HTMLElement,
        info: HyperLinkInfo,
        slice$: Observable<{ value: any, enabled: boolean, selector: any, type: string }>,
        onBackToFocus: (oldValue: any, value: any) => void,
        onNextControl: (any) => void,
        onControlFocusLost?: () => void,
        customShouldAddOnFlyPredicate?: (focusedElem: HTMLElement) => boolean): void {
        if (!getElementLogic || !info) throw Error('You should provide a value form "getElement" and "info"');
        this.withGetElementLogic(getElementLogic)
            .withHyperLinkLogic(destroyer, info, slice$)
            .withAddOnFlyLogic(destroyer, info, slice$, hotLinkButton, onBackToFocus, onNextControl, onControlFocusLost, customShouldAddOnFlyPredicate);
    }

    stop() {

    }

    private withHyperLinkLogic(destroyer: Destroyer, info: HyperLinkInfo, slice$: Observable<{ value: any, enabled: boolean, selector: any, type: string }>): HyperLinkService {
        if (slice$)
            slice$.pipe(untilDestroy(destroyer)).subscribe(x => this.shouldEnableHyperLink(x) ? this.enableHyperLink(info) : this.disableHyperLink(x));
        return this;
    }

    private withGetElementLogic(getElementLogic: () => HTMLElement): HyperLinkService {
        this.getElementLogic = getElementLogic;
        return this;
    }

    private withAddOnFlyLogic(destroyer: Destroyer, info: HyperLinkInfo,
        slice$: Observable<{ value: any, enabled: boolean, selector: any, type: string }>,
        hotLinkButton: HTMLElement,
        onBackToFocus: (oldValue: any, value: any) => void,
        onNextControl: (any) => void,
        onControlFocusLost?: () => void,
        customShouldAddOnFlyPredicate?: (focusedElem: HTMLElement) => boolean): HyperLinkService {
        if (slice$)
            slice$.do(x => this.isAttachedComponentEnabled = x.enabled ? true : false)
                .filter(x => !x.enabled).pipe(untilDestroy(destroyer)).subscribe(x => this.oldElementVaue = x.value);
        this.onBackToFocus = onBackToFocus;
        this.onNextControl = onNextControl;
        this.shouldAddOnFly = (focusEvent: ChangeFocusEvent) => 
            this.isAttachedComponentEnabled && 
            this.elementInfo.element &&
            this.elementInfo.element.contains(focusEvent.source) &&
            (!hotLinkButton || !hotLinkButton.contains(focusEvent.target)) &&
            !isInAddOnFlyExclusionList(focusEvent.target) &&
            (!customShouldAddOnFlyPredicate || customShouldAddOnFlyPredicate(focusEvent.target));

        this.controlFocusChanged$(destroyer)
            .do(_ => { if (onControlFocusLost) onControlFocusLost(); })
            .merge(this.getHotLinkButtonFocusChanged$(destroyer, hotLinkButton))
            .filter(x => this.shouldAddOnFly(x))
            .subscribe(_ => this.existData(info));
        return this;
    }

    private shouldEnableHyperLink(info: { value: any, enabled: boolean }): boolean {
        return !info.enabled && info.value;
    }

    private get styles(): HyperLinkStyles {
        return { color: 'blue', textDecoration: 'underline', cursor: 'pointer', pointerEvents: 'all' };
    }

    private wsService: WebSocketService;
    private eventDataService: EventDataService;
    private constructor(wsService: WebSocketService,
        eventDataService: EventDataService,

    ) {
        this.eventDataService = eventDataService;
        this.wsService = wsService;
    }

    private async follow(p: HyperLinkInfo): Promise<void> {
        await this.wsService.openHyperLink(p.name, p.cmpId);
    }

    private existData = (info: HyperLinkInfo) => {
        this.moveToNextControl$(info, this.workingValue, this.workingType)
            .subscribe(nextControl => nextControl.existData ? 
            this.onNextControl(nextControl.value) : 
            this.giveBackFocus(this.workingValue));
    }


    private moveToNextControl$ = (info: HyperLinkInfo, currentValue: any, currentType: string): Observable<{existData: boolean, value: string}> =>
        this.wsService.existData(info.name, info.cmpId, info.controlId, currentValue, currentType);

    private giveBackFocus(value: any) {
        if(this.elementInfo && this.elementInfo.element)
            this.elementInfo.element.focus();
        if (this.onBackToFocus) 
            this.onBackToFocus(this.oldElementVaue, value);
    }

    private enableHyperLink(info: HyperLinkInfo): void {
        if (this.elementInfo) {
            this.elementInfo.element.style.textDecoration = this.styles.textDecoration;
            this.elementInfo.element.style.color = this.styles.color;
            this.elementInfo.element.style.cursor = this.styles.cursor;
            this.elementInfo.element.style.pointerEvents = this.styles.pointerEvents;
            if (this.elementInfo.clickSubscription) this.elementInfo.clickSubscription.unsubscribe();
            this.elementInfo.clickSubscription = Observable.fromEvent(document, 'click', { capture: true })
                .filter(e => (e as any) && this.elementInfo.element && this.elementInfo.element.contains((e as any).target))
                .subscribe(e => this.follow(info));
        }
    }

    private disableHyperLink(info: { value: any, type: string }): void {
        this.workingValue = info.value;
        if (this.elementInfo) {
            this.elementInfo.element.style.textDecoration = this.elementInfo.initInfo.textDecoration;
            this.elementInfo.element.style.color = this.elementInfo.initInfo.color;
            this.elementInfo.element.style.cursor = this.elementInfo.initInfo.cursor;
            this.elementInfo.element.style.pointerEvents = this.elementInfo.initInfo.pointerEvents;
            if (this.elementInfo.clickSubscription)
                this.elementInfo.clickSubscription.unsubscribe();
        }
    }
}