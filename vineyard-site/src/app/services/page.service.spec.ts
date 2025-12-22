import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PageService, PageData } from './page.service';

describe('PageService', () => {
  let service: PageService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(PageService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('requests page data by route', () => {
    const mockResponse: PageData = { blocks: [{ type: 'p', content: 'hi' }] };

    service.getPage('about').subscribe((data) => {
      expect(data).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('/api/pages/about');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});
