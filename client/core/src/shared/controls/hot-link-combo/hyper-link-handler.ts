import { TbHotlinkComboComponent } from './tb-hot-link-combo.component';

export class TbHotlinkComboHyperLinkHandler {
    static Attach(hlb: TbHotlinkComboComponent): TbHotlinkComboHyperLinkHandler {
        return new TbHotlinkComboHyperLinkHandler(hlb);
    }

    private getHotLinkElement: () => HTMLElement;
    private constructor (hlb: any) {
        this.getHotLinkElement = () => {
            let searchBar = (hlb.vcr.element.nativeElement.parentNode.getElementsByClassName('k-searchbar') as HTMLCollection).item(0) as HTMLElement;
            if(searchBar) return (searchBar.getElementsByClassName('k-input') as HTMLCollection).item(0) as HTMLElement;
            return undefined; 
          }
          hlb.hyperLinkService.start(
            () => this.getHotLinkElement(),
            null, 
            { name: hlb.hotLinkInfo.name,
              cmpId: hlb.documentService.mainCmpId, 
              enableAddOnFly: hlb.hotLinkInfo.enableAddOnFly, 
              mustExistData: hlb.hotLinkInfo.mustExistData,
              model: hlb.modelComponent.model 
            }, 
            hlb.slice$, hlb.afterNoAddOnFly, hlb.afterAddOnFly);
    }
}