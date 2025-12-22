import { convertToParamMap } from '@angular/router';

export function mockActivatedRoute(
  params: Record<string, unknown> = {},
  data: Record<string, unknown> = {}
) {
  return {
    snapshot: {
      paramMap: convertToParamMap(params),
      data
    }
  };
}
