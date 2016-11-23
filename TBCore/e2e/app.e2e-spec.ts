import { TBCorePage } from './app.po';

describe('tbcore App', function() {
  let page: TBCorePage;

  beforeEach(() => {
    page = new TBCorePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('tb works!');
  });
});
