import { ModuleWithProviders } from '@angular/core';
export * from './services/bo.service';
export * from './services/bohelper.service';
export * from './services/component.service';
export * from './services/data.service';
export * from './services/document.service';
export * from './services/enums.service';
export * from './services/eventdata.service';
export * from './services/explorer.service';
export * from './services/http.service';
export * from './services/info.service';
export * from './services/layout.service';
export * from './services/logger.service';
export * from './services/login-session.service';
export * from './services/sidenav.service';
export * from './services/tabber.service';
export * from './services/utils.service';
export * from './services/websocket.service';
export * from './guards/core.guard';
export * from './containers/frame/frame.component';
export * from './containers/frame/frame-content/frame-content.component';
export * from './containers/view/view.component';
export * from './containers/view/view-container/view-container.component';
export * from './containers/dockpane/dockpane.component';
export * from './containers/dockpane/dockpane-container/dockpane-container.component';
export * from './containers/tiles/tile-manager/tile-manager.component';
export * from './containers/tiles/tile-group/tile-group.component';
export * from './containers/tiles/tile/tile.component';
export * from './containers/tiles/tile-panel/tile-panel.component';
export * from './containers/tiles/layout-container/layout-container.component';
export * from './containers/message-dialog/message-dialog.component';
export * from './components/bo.component';
export * from './components/document.component';
export * from './components/dynamic-cmp.component';
export * from './components/page-not-found.component';
export * from './components/tb.component';
export * from './components/topbar/topbar.component';
export * from './components/topbar/topbar-menu/topbar-menu.component';
export * from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
export * from './components/topbar/topbar-menu/topbar-menu-elements/topbar-menu-elements.component';
export * from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
export * from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
export * from './components/context-menu/context-menu.component';
export * from './components/explorer/save/save.component';
export * from './components/explorer/open/open.component';
export * from './components/accordion/accordion.component';
export * from './components/header-strip/header-strip.component';
export * from './components/toolbar/toolbar-top/toolbar-top.component';
export * from './components/toolbar/toolbar-top/toolbar-separator.component';
export * from './components/toolbar/toolbar-top/toolbar-top-button/toolbar-top-button.component';
export * from './components/toolbar/toolbar-bottom/toolbar-bottom.component';
export * from './components/toolbar/toolbar-bottom/toolbar-bottom-button/toolbar-bottom-button.component';
export * from './controls/body-edit/body-edit.component';
export * from './controls/bool-edit/bool-edit.component';
export * from './controls/button/button.component';
export * from './controls/caption/caption.component';
export * from './controls/charts/linear-gauge/linear-gauge.component';
export * from './controls/checkbox/checkbox.component';
export * from './controls/color-picker/color-picker.component';
export * from './controls/combo/combo.component';
export * from './controls/combo-simple/combo-simple.component';
export * from './controls/date-input/date-input.component';
export * from './controls/email/email.component';
export * from './controls/enum-combo/enum-combo.component';
export * from './controls/file/file.component';
export * from './controls/grid/grid.component';
export * from './controls/image/image.component';
export * from './controls/label-static/label-static.component';
export * from './controls/link/link.component';
export * from './controls/masked-text-box/masked-text-box.component';
export * from './controls/numeric-text-box/numeric-text-box.component';
export * from './controls/password/password.component';
export * from './controls/phone/phone.component';
export * from './controls/placeholder/placeholder.component';
export * from './controls/radio/radio.component';
export * from './controls/section-title/section-title.component';
export * from './controls/state-button/state-button.component';
export * from './controls/tb-card/tb-card.component';
export * from './controls/tb-card/tb-card-content/tb-card-content.component';
export * from './controls/tb-card/tb-card-title/tb-card-title.component';
export * from './controls/text/text.component';
export * from './controls/textarea/textarea.component';
export * from './controls/time-input/time-input.component';
export * from './controls/unknown/unknown.component';
export declare class TbCoreModule {
    static forRoot(): ModuleWithProviders;
    constructor(parentModule: TbCoreModule);
}
