import {
  Component, OnInit, Output, EventEmitter, ViewChild, OnDestroy, HostListener,
  ElementRef, AfterContentInit, ViewEncapsulation, ComponentFactoryResolver, ChangeDetectorRef
} from '@angular/core';

import { Subscription } from 'rxjs';

import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";
import { TabStripTabComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip-tab.component";

import {
  ComponentInfoService, TbComponent, MessageDialogComponent, ComponentInfo, SidenavService, TaskBuilderService,
  ComponentService, LayoutService, TabberService, SettingsService, EnumsService, FormattersService,
  InfoService, LoadingService, ThemeService, EventManagerService, ActivationService, AuthService, TbComponentService,
  Logger, MessageDlgArgs
} from '@taskbuilder/core';

import { MenuService } from './../../services/menu.service';
import { SettingsContainerComponent } from './../../settings/settings-container/settings-container.component';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  providers: [ComponentInfoService]
})
export class HomeComponent extends TbComponent implements OnDestroy, AfterContentInit, OnInit {

  subscriptions: Subscription[] = [];
  @ViewChild('sidenav') sidenav;

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('tabberContainer') tabberContainer: ElementRef;
  @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;

  @ViewChild('dashBoardTabStrip') dashBoardTabStrip: TabStripTabComponent;
  @ViewChild('menuTabStrip') menuTabStrip: TabStripTabComponent;
  @ViewChild('settingsTabStrip') settingsTabStrip: TabStripTabComponent;

  viewHeight: number;
  isDesktop: boolean;
  sidenavPinned: boolean = localStorage.getItem('sidenavPinned') == 'true';
  sidenavOpened: boolean = localStorage.getItem('sidenavOpened') == 'true';
  settingsPageComponent: ComponentInfo = null;

  constructor(
    public sidenavService: SidenavService,
    public taskbuilderService: TaskBuilderService,
    public componentService: ComponentService,
    public layoutService: LayoutService,
    public tabberService: TabberService,
    public menuService: MenuService,
    public settingsService: SettingsService,
    public enumsService: EnumsService,
    public formattersService: FormattersService,
    public infoService: InfoService,
    public loadingService: LoadingService,
    public resolver: ComponentFactoryResolver,
    public themeService: ThemeService,
    public eventManagerService: EventManagerService,
    public activationService: ActivationService,
    private authService: AuthService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    public logger: Logger
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
    this.initialize();
  }

  initialize() {

    this.loadingService.setLoading(true, this._TB('connecting...'));

    this.isDesktop = this.infoService.isDesktop;

    this.subscriptions.push(this.settingsService.settingsPageOpenedEvent.subscribe((opened) => { this.openSettings(opened); }));

    this.subscriptions.push(this.componentService.componentInfoCreated.subscribe(arg => {
      if (arg.activate) {
        this.kendoTabStripInstance.selectTab(arg.index + 1);
      }
    }));

    this.subscriptions.push(this.eventManagerService.loggingOff.subscribe(() => {
      //sulla logout, se serve, chiudo i settings
      this.logger.debug("loggedOff");
      this.closeSettings();
      //eventualmente altre chiusure??? TODOLUCA
    }));

    this.subscriptions.push(this.tabberService.tabSelected$.subscribe((index: number) => this.kendoTabStripInstance.selectTab(index)));

    this.subscriptions.push(this.tabberService.tabMenuSelected$.subscribe(() => {
      //TODOLUCA, serve qualcosa che permetta la seleziona di tab by name o id, e non index
      this.kendoTabStripInstance.tabs.forEach(tab => tab.active = false);
      this.menuTabStrip.active = true;
    }));

    this.subscriptions.push(this.componentService.componentInfoRemoved.subscribe(cmp => {
      
      let idx = this.kendoTabStripInstance.tabs._results.findIndex((t) => t.active);
      this.kendoTabStripInstance.selectTab(idx===this.kendoTabStripInstance.tabs.length-1 ? --idx : idx);
      
    }));

    this.subscriptions.push(this.componentService.componentCreationError.subscribe(reason => {
      const args = new MessageDlgArgs();
      args.ok = true;
      args.text = reason;

      this.messageDialog.open(args);
    }));

    this.menuService.getMenuElements();
    this.settingsService.getSettings();
    this.enumsService.getEnumsTable();
    this.activationService.getModules();
    this.formattersService.loadFormattersTable();
    //in modalità web non aspetto che tbloader venga su
    if (!this.isDesktop) {
      this.loadingService.setLoading(false);
    }
  }

  ngOnInit() {
    super.ngOnInit();
    let sub = this.taskbuilderService.openTbConnectionAndShowDiagnostic().subscribe(ok => {
      sub.unsubscribe();
      if (this.isDesktop) {
        this.loadingService.setLoading(false);
        if (!ok) {
          this.authService.logout();
        }
      }
    });
  }
  ngAfterContentInit() {
    setTimeout(() => this.calcViewHeight(), 0);

    this.subscriptions.push(this.sidenavService.sidenavPinned$.subscribe((pinned) => this.sidenavPinned = pinned));
    this.subscriptions.push(this.sidenavService.sidenavOpened$.subscribe((opened) => {
      this.sidenavOpened = opened;

      if (opened) {
        this.sidenav.open();
      }
      else {
        this.sidenav.close();
      }

    }));

    this.layoutService.detectDPI();
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.calcViewHeight();
  }
  calcViewHeight() {
    // this.logger.debug("screen.height", screen.height);
    this.viewHeight = this.tabberContainer ? this.tabberContainer.nativeElement.offsetHeight - 31 : screen.height;
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

  getIcon(info: ComponentInfo) {
    //qua andrebbe differenziata in base a report, document, settings ecc
    return 'tb-inscription';
  }

  onContextMenu() {
    return !this.infoService.isDesktop;
  }

  closeSettings() {
    this.logger.debug("closeSettings", this.settingsPageComponent);
    if (this.settingsPageComponent != null && this.componentService.components.find(current => current == this.settingsPageComponent)) {
      this.componentService.removeComponent(this.settingsPageComponent);
      this.logger.debug("remove setting", this.settingsPageComponent);
    }
    this.settingsPageComponent = null;
  }

  openSettings(opened: boolean) {

    if (!opened) {
      this.closeSettings();
      return;
    }

    if (opened == true && this.settingsPageComponent != null) {
      return;
    }

    this.componentService.componentInfoAdded.subscribe((component) => {
      this.settingsPageComponent = component;
    });

    this.componentService.createComponent(SettingsContainerComponent, this.resolver);

    let len = this.kendoTabStripInstance.tabs.toArray().length;
    setTimeout(() => {
      this.kendoTabStripInstance.selectTab(len); //
    }, 1);
  }
}
