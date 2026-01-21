import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HttpHelperService {

  private readonly http = inject(HttpClient);

  public get<T>(url: string, params: string | number = "") {
    return this.http.get<T>(this.generateUrl(url,params))
    .pipe(
      catchError(error => {
        console.error(error);
        return of(null);
      })
    );
  }

  public post<T>(url: string, body: T) {
    return this.http.post<T>(url, body)
    .pipe(
      catchError(error => {
        console.error(error);
        return of(null);
      })
    );
  }

  public put<T>(url: string, body: T, params: string | number = "") {
    return this.http.put<T>(this.generateUrl(url,params), body)
    .pipe(
      catchError(error => {
        console.error(error);
        return of(null);
      })
    );
  }

  public delete(url: string, params: string | number = "") {
    return this.http.delete(this.generateUrl(url,params))
    .pipe(
      catchError(error => {
        console.error(error);
        return of(null);
      })
    );
  }

  private generateUrl(url: string, params: string | number = "") : string{
    url = `${url}`
    url += params == ""? "" : `/${params}`;
    return url;
  }
}
