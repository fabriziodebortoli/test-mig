import { M4Page } from './app.po';

describe('m4 App', () => {
  let page: M4Page;

  beforeEach(() => {
    page = new M4Page();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
