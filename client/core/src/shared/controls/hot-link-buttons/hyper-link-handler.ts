import { TbHotlinkButtonsComponent } from './tb-hot-link-buttons.component';

export class TbHotlinkButtonsHyperLinkHandler {
    static Attach(hlb: TbHotlinkButtonsComponent): TbHotlinkButtonsHyperLinkHandler {
        return new TbHotlinkButtonsHyperLinkHandler(hlb);
    }

    private getHotLinkElement: () => HTMLElement;
    private shouldAddOnFly: (focusedElem: HTMLElement) => boolean;

    private constructor (hlb: any) {
        this.getHotLinkElement = () => (hlb.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0) as HTMLElement;
        this.shouldAddOnFly = (focusedElem: HTMLElement) => !hlb.hotLinkButtonTemplate.nativeElement.contains(focusedElem);
        // fix for themes css conflict in form.scss style 
        if (hlb.modelComponent) {
            hlb.mediator.storage.options.componentInfo.cmpId = hlb.modelComponent.cmpId;
            hlb.hyperLinkService.start(
                this.getHotLinkElement,
                hlb.hotLinkButtonTemplate.nativeElement,
                {
                    name: hlb.hotLinkInfo.name,
                    cmpId: hlb.documentService.mainCmpId,
                    enableAddOnFly: hlb.hotLinkInfo.enableAddOnFly,
                    mustExistData: hlb.hotLinkInfo.mustExistData,
                    model: hlb.modelComponent.model
                },
                hlb.slice$,
                hlb.afterNoAddOnFly,
                hlb.afterAddOnFly,
                this.shouldAddOnFly);
        }
    }
}