import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {catchError, filter, of} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HttpHelperService {

  private readonly http = inject(HttpClient);

  public get<T>(url: string, params: string | number = "") {
    return this.http.get<T>(this.generateUrl(url,params))
    .pipe(
      filter((data): data is T => !!data),
      catchError(error => {
        console.error(error);
        return of();
      })
    );
  }

  public post<T>(url: string, body: T) {
    return this.http.post<T>(url, body)
      .pipe(
        filter((data): data is T => !!data),
        catchError(error => {
          console.error(error);
          return of();
        })
    );
  }

  public put<T>(url: string, body: T, params: string | number = "") {
    return this.http.put<T>(this.generateUrl(url,params), body)
      .pipe(
        filter((data): data is T => !!data),
        catchError(error => {
          console.error(error);
          return of();
        })
    );
  }

  public delete(url: string, params: string | number = "") {
    return this.http.delete<boolean>(this.generateUrl(url,params))
    .pipe(
      filter((data): data is boolean => data),
      catchError(error => {
        console.error(error);
        return of();
      })
    );
  }

  private generateUrl(url: string, params: string | number = "") : string{
    url = `${url}`
    url += params == ""? "" : `/${params}`;
    return url;
  }
}
