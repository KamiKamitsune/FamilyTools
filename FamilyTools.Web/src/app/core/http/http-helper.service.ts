import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { filter } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HttpHelperService {

  private readonly http = inject(HttpClient);

  public get<T>(url: string, params: string | number = "") {
    return this.http.get<T>(this.generateUrl(url, params))
      .pipe(filter((data): data is T => !!data));
  }

  public post<T>(url: string, body: unknown) {
    return this.http.post<T>(url, body)
      .pipe(filter((data): data is T => !!data));
  }

  public put<T>(url: string, body: unknown, params: string | number = "") {
    return this.http.put<T>(this.generateUrl(url, params), body)
      .pipe(filter((data): data is T => !!data));
  }

  public patch<T>(url: string, body: unknown, params: string | number = "") {
    return this.http.patch<T>(this.generateUrl(url, params), body)
      .pipe(filter((data): data is T => !!data));
  }

  public delete(url: string, params: string | number = "") {
    return this.http.delete<boolean>(this.generateUrl(url, params));
  }

  private generateUrl(url: string, params: string | number = "") : string{
    url = `${url}`
    url += params == ""? "" : `/${params}`;
    return url;
  }
}
