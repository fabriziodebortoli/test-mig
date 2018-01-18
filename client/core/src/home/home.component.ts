import { TbComponentService } from './../core/services/tbcomponent.service';
import { EventManagerService } from './../core/services/event-manager.service';
import { ThemeService } from './../core/services/theme.service';
import { SettingsContainerComponent, SettingsContainerFactoryComponent } from './../settings/settings-container/settings-container.component';
import { BoolEditComponent } from './../shared/controls/bool-edit/bool-edit.component';
import { SettingsService } from './../core/services/settings.service';
import { Component, OnInit, Output, EventEmitter, ViewChild, OnDestroy, HostListener, ElementRef, AfterContentInit, ViewEncapsulation, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

import { Subscription } from '../rxjs.imports';

import { MessageDlgArgs } from './../shared/models/message-dialog.model';
import { ComponentInfo } from './../shared/models/component-info.model';

import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";
import { TabStripTabComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip-tab.component";
import { MessageDialogComponent } from './../shared/containers/message-dialog/message-dialog.component';

import { InfoService } from './../core/services/info.service';
import { UtilsService } from './../core/services/utils.service';
import { ComponentInfoService } from './../core/services/component-info.service';
import { EnumsService } from './../core/services/enums.service';
import { FormattersService } from './../core/services/formatters.service';
import { TabberService } from './../core/services/tabber.service';
import { LayoutService } from './../core/services/layout.service';
import { ComponentService } from './../core/services/component.service';
import { TaskBuilderService } from './../core/services/taskbuilder.service';
import { SidenavService } from './../core/services/sidenav.service';
import { LoadingService } from './../core/services/loading.service';

import { MenuService } from './../menu/services/menu.service';
import { TbComponent } from './../shared/components/tb.component';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers: [ComponentInfoService]
})
export class HomeComponent extends TbComponent implements OnDestroy, AfterContentInit, OnInit {

  @ViewChild('sidenavleft') sidenavleft;
  @ViewChild('sidenavright') sidenavright;
  subscriptions: Subscription[] = [];

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('tabberContainer') tabberContainer: ElementRef;
  @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;

  @ViewChild('dashBoardTabStrip') dashBoardTabStrip: TabStripTabComponent;
  @ViewChild('menuTabStrip') menuTabStrip: TabStripTabComponent;
  @ViewChild('settingsTabStrip') settingsTabStrip: TabStripTabComponent;


  viewHeight: number;

  isDesktop: boolean;
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
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
    this.initialize();
  }

  initialize() {

    this.loadingService.setLoading(true, this._TB('connecting...'));

    this.isDesktop = this.infoService.isDesktop;

    this.subscriptions.push(this.settingsService.settingsPageOpenedEvent.subscribe((opened) => { this.openSettings(opened); }));

    this.subscriptions.push(this.sidenavService.sidenavOpenedLeft$.subscribe(() => this.sidenavleft.toggle()));
    this.subscriptions.push(this.sidenavService.sidenavOpenedRight$.subscribe(() => this.sidenavright.toggle()));

    this.subscriptions.push(this.componentService.componentInfoCreated.subscribe(arg => {
      if (arg.activate) {
        this.kendoTabStripInstance.selectTab(arg.index + 2);
      }
    }));

    this.subscriptions.push(this.eventManagerService.loggingOff.subscribe(() => {
      //sulla logout, se serve, chiudo i settings
      console.log("loggedOff");
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
      this.kendoTabStripInstance.selectTab(0);
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
    this.formattersService.loadFormattersTable();
    //in modalità web non aspetto che tbloader venga su
    if (!this.isDesktop) {
      this.loadingService.setLoading(false);
    }
  }

  ngOnInit() {
    super.ngOnInit();
    let sub = this.taskbuilderService.openTbConnection().subscribe(res => {
      sub.unsubscribe();
      if (this.isDesktop) {
        this.loadingService.setLoading(false);
      }
    });
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
    console.log("screen.height", screen.height);
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
    console.log("closeSettings", this.settingsPageComponent);
    if (this.settingsPageComponent != null && this.componentService.components.find(current => current == this.settingsPageComponent)) {
      this.componentService.removeComponent(this.settingsPageComponent);
      console.log("remove setting", this.settingsPageComponent);
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
