import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';
import { ITreeNode, ITreeOptions } from "angular-tree-component/dist/defs/api";


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

  constructor(protected layoutService: LayoutService) {
    super(layoutService);
  }

  getOptions(): any {

  }

  onExpand(item: ITreeNode) {
    //call for leaves of item.id
  }

}


//// 
 /** Input Formt
  * [
  {
    id: 1,
    name: 'root1',
    isExpanded: true,
    children: [
      {
        id: 2,
        name: 'child1'
      }, {
        id: 3,
        name: 'child2'
      }
    ]
  }
]
  */
