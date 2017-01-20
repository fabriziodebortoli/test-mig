import {Component, Input, OnDestroy}  from '@angular/core';

@Component({
  selector: 'tb-accordion',
  template: '<ng-content></ng-content>'
})
export class Accordion {
  groups: Array<AccordionGroup> = [];
  
  addGroup(group: AccordionGroup): void {
    this.groups.push(group);
  }
  
  closeOthers(openGroup: AccordionGroup): void {
    this.groups.forEach((group: AccordionGroup) => {
      if (group !== openGroup) {
        group.isOpen = false;
      }
    });
  }
  
  removeGroup(group: AccordionGroup): void {
    const index = this.groups.indexOf(group);
    if (index !== -1) {
      this.groups.splice(index, 1);
    }
  }
}


@Component({
  selector: 'tb-accordion-group',
  templateUrl: './accordion.component.html',  
  styleUrls: ['./accordion.component.scss']
})
export class AccordionGroup implements OnDestroy {
  private _isOpen:boolean = false;
  
  @Input() heading: string;
  
  @Input()
  set isOpen(value: boolean) {
    this._isOpen = value;
    if (value) {
      this.accordion.closeOthers(this);
    }
  }
  
  get isOpen() {
    return this._isOpen;
  }
  
  constructor(private accordion: Accordion) {
    this.accordion.addGroup(this);
  }
  
  ngOnDestroy() {
    this.accordion.removeGroup(this);
  }
  
  toggleOpen(event: MouseEvent): void {
    event.preventDefault();
    this.isOpen = !this.isOpen;
  }

  getExpandCollapseClass()
  {
    return this.isOpen ? 'keyboard_arrow_up' : 'keyboard_arrow_down';

  }
}