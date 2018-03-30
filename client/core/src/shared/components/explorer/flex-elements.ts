import { ElementRef } from '@angular/core';
import { arrayFrom, tryArrayFrom } from '../../../shared/commons/u';
import { Maybe } from '../../commons/monads/maybe';

export class FlexElements {
    static create(flexContainer: any, selectedSelector: string, elementsSelector: string): FlexElements {
        return new FlexElements(flexContainer, selectedSelector, elementsSelector);
    }

    private constructor(private flexContainer, private selectedSelector, private elementsSelector) { }

    selectedElement() {
        const selected = this.flexContainer.querySelector(this.selectedSelector);
        return selected && Maybe.some(this.toFlexElement(selected, 0)) || Maybe.none();
    }

    toFlexElement = (nativeElement, idx) =>
        [nativeElement, nativeElement.getBoundingClientRect(), idx]

    all = () =>
        tryArrayFrom(this.flexContainer.querySelectorAll(this.elementsSelector))
            .map(this.toFlexElement)

    allBut = but =>
        this.all().filter(item => item[0] !== but[0])

    inRowOf = el =>
        this.allBut(el)
            .filter(i => i[1].y === el[1].y)
            .sort(this.byDistanceFrom(el))

    inColumnOf = el =>
        this.allBut(el)
            .filter(i => i[1].x === el[1].x)
            .sort(this.byDistanceFrom(el))

    leftOf = el =>
        Maybe.from(this.inRowOf(el).filter(i => i[1].x < el[1].x))
            .map(els => els.length && els[0])

    rightOf = el =>
        Maybe.from(this.inRowOf(el).filter(i => i[1].x > el[1].x))
            .map(els => els.length && els[0])

    above = el =>
        Maybe.from(this.inColumnOf(el).filter(i => i[1].y < el[1].y))
            .map(els => els.length && els[0])

    below = el =>
        Maybe.from(this.inColumnOf(el).filter(i => i[1].y > el[1].y))
            .map(els => els.length && els[0])

    first = () => {
        const elements = this.all();
        return Maybe.from(elements.length && elements[0]);
    }

    rightOfSelected = () => this.selectedElement().flatMapOr(this.rightOf, this.first);
    leftOfSelected = () => this.selectedElement().flatMapOr(this.leftOf, this.first);
    aboveSelected = () => this.selectedElement().flatMapOr(this.above, this.first);
    belowSelected = () => this.selectedElement().flatMapOr(this.below, this.first);
    distanceSquared = (a, b) => (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    byDistanceFrom = c => (a, b) => this.distanceSquared(c[1], a[1]) - this.distanceSquared(c[1], b[1]);
}
