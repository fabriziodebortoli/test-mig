import { LocalizationService } from './../core/services/localization.service';
import { Component, OnInit, Output, EventEmitter, ViewChild, OnDestroy, HostListener, ElementRef, AfterContentInit, ViewEncapsulation } from '@angular/core';

import { Subscription } from 'rxjs';

import { environment } from 'environments/environment';

import { MessageDlgArgs } from './../shared/models/message-dialog.model';
import { ComponentInfo } from './../shared/models/component-info.model';

import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";
import { MessageDialogComponent } from './../shared/containers/message-dialog/message-dialog.component';

import { InfoService } from './../core/services/info.service';
import { UtilsService } from './../core/services/utils.service';
import { ComponentInfoService } from './../core/services/component-info.service';
import { EnumsService } from './../core/services/enums.service';
import { TabberService } from './../core/services/tabber.service';
import { LayoutService } from './../core/services/layout.service';
import { ComponentService } from './../core/services/component.service';
import { TaskbuilderService } from './../core/services/taskbuilder.service';
import { SidenavService } from './../core/services/sidenav.service';
import { SettingsService } from './../menu/services/settings.service';
import { LoadingService } from './../core/services/loading.service';

import { MenuService } from './../menu/services/menu.service';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers: [ComponentInfoService]
})
export class HomeComponent implements OnDestroy, AfterContentInit, OnInit {

  @ViewChild('sidenav') sidenav;
  subscriptions: Subscription[] = [];

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('tabberContainer') tabberContainer: ElementRef;
  @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;
  viewHeight: number;

  isDesktop: boolean;

  constructor(
    public sidenavService: SidenavService,
    public taskbuilderService: TaskbuilderService,
    public componentService: ComponentService,
    public layoutService: LayoutService,
    public tabberService: TabberService,
    public menuService: MenuService,
    public localizationService: LocalizationService,
    public settingsService: SettingsService,
    public enumsService: EnumsService,
    public infoService: InfoService, 
    public loadingService: LoadingService 
  ) {

    this.loadingService.setLoading(true, "connecting...");

    this.isDesktop = infoService.isDesktop;

    this.subscriptions.push(sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle()));

    this.subscriptions.push(componentService.componentInfoCreated.subscribe(arg => {
      if (arg.activate) {
        this.kendoTabStripInstance.selectTab(arg.index + 2);
      }
    }));

    this.subscriptions.push(tabberService.tabSelected$.subscribe((index: number) => this.kendoTabStripInstance.selectTab(index)));

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
    this.localizationService.loadLocalizedElements();
    this.settingsService.getSettings();
    this.enumsService.getEnumsTable();

    // sottoscrivo la connessione TB e WS e, se non attiva, la apro tramite il servizio TaskbuilderService
    this.subscriptions.push(this.taskbuilderService.connected.subscribe(connected => {
        this.loadingService.setLoading(!connected, connected ?  "" : "connecting...");
    }));

  }

  ngOnInit() {
    this.taskbuilderService.openConnection();
  }

  ngAfterContentInit() {
    setTimeout(() => this.calcViewHeight(), 0);

    this.layoutService.detectDPI();
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
    this.loadingService.setLoading(false);

    this.taskbuilderService.dispose();
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  closeTab(info: ComponentInfo) {
    event.stopImmediatePropagation();
    info.document.close();
  }

  onContextMenu() {
    if (environment.production) {
      return false;
    }
  }

}
