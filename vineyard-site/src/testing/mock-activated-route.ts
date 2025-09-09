import { convertToParamMap } from '@angular/router';

export function mockActivatedRoute(
  params: Record<string, any> = {},
  data: Record<string, any> = {}
) {
  return {
    snapshot: {
      paramMap: convertToParamMap(params),
      data
    }
  };
}
