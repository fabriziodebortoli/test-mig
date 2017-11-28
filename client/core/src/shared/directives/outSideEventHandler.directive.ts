import { Directive, Input, Output, NgZone, ElementRef, OnInit, OnDestroy, EventEmitter } from '@angular/core';


@Directive({
    selector: '[outSideEventHandler]'
  })
export class OutSideEventHandlerDirective implements OnInit, OnDestroy {
    @Input() event = 'click';
    // tslint:disable-next-line:no-output-rename
    @Output('outSideEventHandler') emitter = new EventEmitter();

    private _handler: Function;
    constructor(private _ngZone: NgZone, private el: ElementRef) {}

    ngOnInit() {
        this._ngZone.runOutsideAngular(() => {
        const nativeElement = this.el.nativeElement;
        this._handler = $event => {
            this.emitter.emit($event);
        }

        nativeElement.addEventListener(this.event, this._handler, false);
        });
    }

    ngOnDestroy(): void {
        this.el.nativeElement.removeEventListener(this.event, this._handler);
    }
}
