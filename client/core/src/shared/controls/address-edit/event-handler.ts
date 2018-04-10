import { Observable, Subscription } from 'rxjs';
import { untilDestroy } from './../../../shared/shared.module';
import { AddressEditComponent } from './address-edit.component';

const canAttach: (x: {enabled: boolean, value: any}) => boolean = x => !x.enabled && x.value;
const canDetach: (x: {enabled: boolean, value: any}) => boolean = x => x.enabled;
const toClass: (x: {enabled: boolean}) => string = x => x.enabled ? '' : 'map-disabled';
const click: (component: AddressEditComponent) => void = (component) => component.showMap();
const documentClick: () => Observable<MouseEvent> = () => Observable.fromEvent<MouseEvent>(document, 'click', { capture: true });
const isInElement: (evt: MouseEvent, elem: any) => boolean = (evt, elem) => elem && elem.contains(evt.target);

export class EventHandler {
    static Attach(component: AddressEditComponent) { new EventHandler(component); }
    private clickSubscription: Subscription;
    private detachClickHandling() { if(this.clickSubscription) this.clickSubscription.unsubscribe(); }
    private attachClickHandling(component: any) {
        if(this.clickSubscription) this.clickSubscription.unsubscribe();
        this.clickSubscription = documentClick().filter(e => isInElement(e, component.textArea.nativeElement))
        .map(_ => component).subscribe(click);
    }
    
    private constructor(component: AddressEditComponent) {
        let nestedSelector = (component as any).store.select(component.nestedSelector).pipe(untilDestroy(component));
        nestedSelector.map(toClass).subscribe(_class => component.controlClass = _class);
        nestedSelector.filter(canDetach).subscribe(_ => this.detachClickHandling());
        nestedSelector.filter(canAttach).subscribe(_ => this.attachClickHandling(component));
    }
}