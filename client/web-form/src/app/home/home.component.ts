import { TabberService } from './../core/tabber.service';
import { EventDataService } from './../core/eventdata.service';
import { MessageDialogComponent, MessageDlgArgs } from './../shared/containers/message-dialog/message-dialog.component';
import { Subscription } from 'rxjs';
import { ComponentInfo } from './../shared/models/component.info';
import { LayoutService } from 'app/core/layout.service';

import { MenuService } from '../menu/services/menu.service';

import { Component, OnInit, Output, EventEmitter, ViewChild, OnDestroy, HostListener, ElementRef, AfterContentInit, ViewEncapsulation } from '@angular/core';

import { environment } from './../../environments/environment';

import { ComponentService, ComponentCreatedArgs } from './../core/component.service';
import { LoginSessionService } from './../core/login-session.service';
import { SidenavService } from './../core/sidenav.service';
import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent implements OnInit, OnDestroy, AfterContentInit {

  @ViewChild('sidenav') sidenav;
  subscriptions: Subscription[] = [];

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('tabberContainer') tabberContainer: ElementRef;
  @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;
  viewHeight: number;

  private appName = environment.appName;
  private companyName = environment.companyName;

  constructor(
    private sidenavService: SidenavService,
    private loginSession: LoginSessionService,
    private componentService: ComponentService,
    private layoutService: LayoutService,
    private tabberService: TabberService,
    private menuService: MenuService

  ) {
    this.subscriptions.push(sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle()));

    this.subscriptions.push(componentService.componentInfoCreated.subscribe(arg => {
      if (arg.activate) {
        this.kendoTabStripInstance.selectTab(arg.index + 2);
      }
       this.subscriptions.push(tabberService.tabSelected$.subscribe((index:number) => this.kendoTabStripInstance.selectTab(index)));

    }));

    this.subscriptions.push(componentService.componentInfoRemoved.subscribe(cmp => {
      this.kendoTabStripInstance.selectTab(0);
    }));

    this.subscriptions.push(componentService.componentCreationError.subscribe(reason => {
      const args = new MessageDlgArgs();
      args.ok = true;
      args.text = reason;

      this.messageDialog.open(args);
    }));
  }

  ngOnInit() {

  }

  ngAfterContentInit() {
    setTimeout(() => this.calcViewHeight(), 0);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.calcViewHeight();
  }
  calcViewHeight() {
    this.viewHeight = this.tabberContainer ? this.tabberContainer.nativeElement.offsetHeight - 31 : 0;
    this.layoutService.setViewHeight(this.viewHeight);
    // console.log("viewHeight", this.viewHeight);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  closeTab(info: ComponentInfo) {
    event.stopImmediatePropagation();
    info.document.close();
  }

}
