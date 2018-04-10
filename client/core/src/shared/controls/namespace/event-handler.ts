import { Observable, Subscription } from 'rxjs';
import { untilDestroy } from './../../../shared/shared.module';
import { NamespaceComponent } from './namespace.component';
import { Store } from './../../../core/services/store.service';
import { createSelector } from './../../../shared/shared.module';

const enableLogicClass = 'image-link-enabled';
const disabledLogicClass = '';

const canEnableLogic: (x: { enabled: boolean, value: any }) => boolean = x => !x.enabled && x.value;
const canDisableLogic: (x: { enabled: boolean, value: any }) => boolean = x => x.enabled || x.value;
const toClass: (x: {enabled: boolean, value: any}) => string = x => canEnableLogic(x) ? enableLogicClass : disabledLogicClass;
const click: (component: NamespaceComponent) => void = (component) => component.onClick();
const documentClick: () => Observable<MouseEvent> = () => Observable.fromEvent<MouseEvent>(document, 'click', { capture: true });
const isInElement: (evt: MouseEvent, elem: any) => boolean = (evt, elem) => elem && elem.contains(evt.target);

export class EventHandler {

    static Attach(component: NamespaceComponent) { new EventHandler(component); }
    private clickSubscription: Subscription;
    private detachClickHandling() { if (this.clickSubscription) this.clickSubscription.unsubscribe(); }
    private attachClickHandling(component: any) {
        this.detachClickHandling();
        this.clickSubscription = documentClick().filter(e => isInElement(e, component.textArea.nativeElement))
            .map(_ => component).subscribe(click);
    }

    private constructor(component: NamespaceComponent) {

        let compSelector = createSelector(
            _ => component.model && component.model.value,
            _ => component.model && component.model.enabled,
            (value, enabled) => ({ value, enabled })
        );

        let nestedSelector = (component as any).store.select(compSelector).pipe(untilDestroy(component));
        nestedSelector.map(toClass).subscribe(_class => component.controlClass = _class);
        nestedSelector.filter(canDisableLogic).subscribe(_ => this.detachClickHandling());
        nestedSelector.filter(canEnableLogic).subscribe(_ => this.attachClickHandling(component));
    }
}