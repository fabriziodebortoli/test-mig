import { WarmWebPage } from './app.po';

describe('warm-web App', function() {
  let page: WarmWebPage;

  beforeEach(() => {
    page = new WarmWebPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
