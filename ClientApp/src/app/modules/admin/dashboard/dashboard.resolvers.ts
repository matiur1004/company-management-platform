import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { DashboardService } from './dasboard.service';

@Injectable({
    providedIn: 'root'
})
export class DashboardResolver implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(private _dashboardService: DashboardService)
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Resolver
     *
     * @param route
     * @param state
     */
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return this._dashboardService.getStats();
    }
}