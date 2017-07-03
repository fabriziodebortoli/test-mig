import { Component, OnInit, Output, EventEmitter, ViewChild, OnDestroy, HostListener, ElementRef, AfterContentInit, ViewEncapsulation } from '@angular/core';
import { Subscription } from 'rxjs';

import {
  LoginSessionService, ComponentService, ComponentCreatedArgs, LocalizationService, EnumsService, SettingsService,
  LayoutService, TabberService, SidenavService, MessageDialogComponent, MessageDlgArgs, ComponentInfo, MenuService
} from '@taskbuilder/core';

import { environment } from './../../environments/environment';

import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent implements OnDestroy, AfterContentInit {

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
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private enumsService: EnumsService

  ) {
    this.subscriptions.push(sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle()));

    this.subscriptions.push(componentService.componentInfoCreated.subscribe(arg => {
      if (arg.activate) {
        this.kendoTabStripInstance.selectTab(arg.index + 2);
      }
      this.subscriptions.push(tabberService.tabSelected$.subscribe((index: number) => this.kendoTabStripInstance.selectTab(index)));

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

    this.menuService.getMenuElements();
    this.localizationService.loadLocalizedElements(true);
    this.settingsService.getSettings();
    this.enumsService.getEnumsTable();
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
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  closeTab(info: ComponentInfo) {
    event.stopImmediatePropagation();
    info.document.close();
  }

}
