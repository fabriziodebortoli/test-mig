import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-treeview',
  templateUrl: './tree-view.component.html',
  styleUrls: ['./tree-view.component.scss']
})
export class TreeViewComponent extends ControlComponent {
  @Input('readonly') readonly = false;

  @Input('border') border = false;
  @Input('hScroll') hScroll = false;
  @Input('disableDragDrop') disableDragDrop = false;
  @Input('hasButtons') hasButtons = false;
  @Input('hasLines') hasLines = false;
  @Input('linesAtRoot') linesAtRoot = false;
  @Input('alwaysShowSelection') alwaysShowSelection = false;

}
