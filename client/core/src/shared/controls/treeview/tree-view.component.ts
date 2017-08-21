import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from './../control.component';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { ITreeNode, ITreeOptions } from "angular-tree-component/dist/defs/api";
import { TreeComponent, TreeModel, TreeNode } from "angular-tree-component";


@Component({
  selector: 'tb-treeview',
  templateUrl: './tree-view.component.html',
  styleUrls: ['./tree-view.component.scss']
})
export class TreeViewComponent extends ControlComponent implements OnInit {

  @ViewChild('tree') treeComponent: TreeComponent;

  @Input('readonly') readonly = false;
  @Input('border') border = false;
  @Input('hScroll') hScroll = false;
  @Input('disableDragDrop') disableDragDrop = false;
  @Input('hasButtons') hasButtons = false;
  @Input('hasLines') hasLines = false;
  @Input('linesAtRoot') linesAtRoot = false;
  @Input('alwaysShowSelection') alwaysShowSelection = false;

  options: ITreeOptions;



  constructor(protected layoutService: LayoutService) {
    super(layoutService);
  }

  ngOnInit() {
    this.options = {
      useVirtualScroll: this.hScroll,
      allowDrag: !this.disableDragDrop,
      allowDrop: !this.disableDragDrop
    }
  }




  onExpand(item: any) {
    let node: TreeNode = item['node'];
    if (!node.isExpanded) {
      return;
      //node.collapse();
    }

    else {
      //call for leaves of item.id
    }
  }

}


//// 
 /** Input Formt
  * [
  {
    id: 1,
    name: 'root1',
    isExpanded: true,
    hasChildren:true,
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
