import { Component } from '@angular/core';

@Component({
  selector: 'tb-frame-content',
  template: '<div><ng-content></ng-content></div>'
})
export class FrameContentComponent {


}

@Component({
  selector: 'tb-view-container',
  template: '<div><ng-content></ng-content></div>'
})
export class ViewContainerComponent {


}
@Component({
  selector: 'tb-dockpane-container',
  template: '<div><ng-content></ng-content></div>'
})
export class DocPaneContainerComponent {


}
