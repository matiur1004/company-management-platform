import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, catchError, of, tap, throwError } from "rxjs";
import { IBuildingRecoveryParams, IDXReport, IShopCostVarianceParams, IShopUsageVarianceParams, IUmfaBuilding, IUmfaPartner, IUmfaPeriod, IUtilityReportParams } from "../../core/models";
import { BuildingService } from "./building.service";
import { CONFIG } from "@core/helpers";
import { HttpClient } from "@angular/common/http";

@Injectable({ providedIn: 'root' })
export class DXReportService {
  private _shopUsageVariance: BehaviorSubject<any> = new BehaviorSubject([]);
  private _shopCostVariance: BehaviorSubject<any> = new BehaviorSubject([]);
  private _utilityRecoveryExpense: BehaviorSubject<any> = new BehaviorSubject(null);
  
  private _reportChanged: BehaviorSubject<boolean> = new BehaviorSubject(false);

  constructor(
    private buildingService: BuildingService,
    private http: HttpClient
  ) {
    this.ErrorSubject = new BehaviorSubject<string>(null);
    this.Error$ = this.ErrorSubject.asObservable();
    this.bsBuildings = new BehaviorSubject<IUmfaBuilding[]>(null);
    this.obsBuildings = this.bsBuildings.asObservable();
    this.bsPartners = new BehaviorSubject<IUmfaPartner[]>(null);
    this.obsPartners = this.bsPartners.asObservable();
    this.bsPeriods = new BehaviorSubject<IUmfaPeriod[]>(null);
    this.obsPeriods = this.bsPeriods.asObservable();
    this.bsEndPeriods = new BehaviorSubject<IUmfaPeriod[]>(null);
    this.obsEndPeriods = this.bsEndPeriods.asObservable();
    this.bsLoadReport = new BehaviorSubject<IDXReport>(null);
    this.obsLoadReport = this.bsLoadReport.asObservable();
    this.bsShopLoadReport = new BehaviorSubject<IDXReport>(null);
    this.obsShopLoadReport = this.bsShopLoadReport.asObservable();
  }

  get shopUsageVariance$(): Observable<any> {
    return this._shopUsageVariance.asObservable();
  }

  get shopCostVariance$(): Observable<any> {
    return this._shopCostVariance.asObservable();
  }

  get utilityRecoveryExpense$(): Observable<any> {
    return this._utilityRecoveryExpense.asObservable();
  }

  get reportChanged$(): Observable<boolean> {
    return this._reportChanged.asObservable();
  }
  
  private ErrorSubject: BehaviorSubject<string>;
  public Error$: Observable<string>;

  //reports
  private dxReportList: IDXReport[] = [
    { Id: 1, Name: 'Building Recovery', Description: 'Building Recovery Report', DXReportName: 'BuildingRecovery' },
    { Id: 2, Name: 'Shop Usage Variance', Description: 'Shop Usage Variance Report', DXReportName: 'ShopUsageVariance' },
    { Id: 3, Name: 'Shop Costs Variances', Description: 'Shop Costs Variances Report', DXReportName: 'ShopCostsVariances' },
    { Id: 4, Name: 'Utility Recovery and Expense', Description: 'Utility Recovery and Expense Report', DXReportName: 'UtilityRecoveryExpense' },
  ];
  public dxReportList$ = of(this.dxReportList);

  private tenantOptions: any[] = [
    {Id: 1, Name: 'Show Latest'},
    {Id: 2, Name: 'Show All'},
    {Id: 3, Name: 'Show Each'},
  ];
  public tenantOptions$ = of(this.tenantOptions);

  private selectedReport: IDXReport;

  //Buildings
  public buildings: IUmfaBuilding[];
  private bsBuildings: BehaviorSubject<IUmfaBuilding[]>;
  public obsBuildings: Observable<IUmfaBuilding[]>;
  
  public loadBuildings(userId: number): void {
    this.buildingService.getBuildingsForUser(userId).subscribe({
      next: bldgs => {
        this.bsBuildings.next(bldgs);
        this.buildings = bldgs;
        // this.bsPartners.next(<IUmfaPartner[]>[]);
        // this.bsPeriods.next(<IUmfaPeriod[]>[]);
        // this.bsEndPeriods.next(<IUmfaPeriod[]>[]);
      },
      error: err => { this.catchErrors(err); },
      complete: () => { }
    });
  }

  //partners
  private partners: IUmfaPartner[];
  private bsPartners: BehaviorSubject<IUmfaPartner[]>;
  public obsPartners: Observable<IUmfaPartner[]>;

  public loadPartners(userId: number): void {
    this.buildingService.getPartnersForUser(userId).subscribe({
      next: ps => {
        this.bsPartners.next(ps);
        this.partners = ps;
      },
      error: err => { this.catchErrors(err); },
      complete: () => { }
    });
  }

  public selectPartner(partnerId: number) {    
    const filteredBuildings = this.buildings.filter(bld => bld.PartnerId == partnerId);
    this.bsBuildings.next(filteredBuildings);
  }

  //Periods
  private periods: IUmfaPeriod[];
  private bsPeriods: BehaviorSubject<IUmfaPeriod[]>;
  public obsPeriods: Observable<IUmfaPeriod[]>;
  private bsEndPeriods: BehaviorSubject<IUmfaPeriod[]>;
  public obsEndPeriods: Observable<IUmfaPeriod[]>;

  public loadPeriods(umfaBuildingId: number) {
    if(umfaBuildingId == 0 || umfaBuildingId == null) { this.periods = []; this.bsPeriods.next([]);}
    else {
      this.buildingService.getPeriodsForBuilding(umfaBuildingId).subscribe({
        next: bps => {
          this.periods = bps;
          this.bsPeriods.next(bps);
        },
        error: err => { this.catchErrors(err); },
        complete: () => { }
      });
    }
    
  }

  public selectStartPeriod(periodId: number) {
    if (periodId == 0) {
      this.bsEndPeriods.next(<IUmfaPeriod[]>{});
    } else {
      const endPeriods = this.periods.filter(p => p.PeriodId >= periodId);
      this.bsEndPeriods.next(endPeriods);
    }
  }

  //forms
  private frmSelectValid: boolean = false;
  private frmCriteriaValid: boolean = false;
  private showCrit = false;
  private showResults = false;
  private bsLoadReport: BehaviorSubject<IDXReport>;
  public obsLoadReport: Observable<IDXReport>;

  private bsShopLoadReport: BehaviorSubject<IDXReport>;
  public obsShopLoadReport: Observable<IDXReport>;

  public IsFrmsValid(): boolean {
    return this.frmCriteriaValid && this.frmSelectValid;
  }

  public ShowCrit(): boolean {
    return this.showCrit;
  }

  public setFrmValid(frm: number, valid: boolean) {
    switch (frm) {
      case 1: {
        this.frmSelectValid = valid;
        this.showCrit = valid;
        break;
      }
      case 2: {
        this.frmCriteriaValid = valid;
        break;
      }
      default: {
        break;
      }
    }
  }

  public ShowResults(value: boolean) {
    if (value) this.bsLoadReport.next(this.selectedReport);
    this.showResults = value;
  }

  public ShowShopResults(value: boolean) {
    if (value) this.bsShopLoadReport.next(this.selectedReport);
  }

  public ShowResultsPage(): boolean {
    if (this.frmSelectValid && this.frmCriteriaValid && this.BRParams && this.showResults) return true;
    else return false;
  }

  //Report Params
  private BRParams: IBuildingRecoveryParams;
  private SUVParams: IShopUsageVarianceParams;
  private SCVParams: IShopCostVarianceParams;
  private UREParams: IUtilityReportParams;

  get BuildingRecoveryParams(): IBuildingRecoveryParams {
    return this.BRParams;
  }

  set BuildingRecoveryParams(value: IBuildingRecoveryParams) {
    this.BRParams = value;
   if (this.selectedReport && this.selectedReport.Id != 0) {
     switch (this.selectedReport.Id) {
       case 1: {
         if (this.BRParams)
           this.selectedReport.DXReportName = `BuildingRecovery?${this.BRParams.StartPeriodId},${this.BRParams.EndPeriodId}`;
         break;
       }
       default: {
         break;
       }
     }
   }
  }

  get ShopUsageVarianceParams(): IShopUsageVarianceParams {
    return this.SUVParams;
  }

  set ShopUsageVarianceParams(value: IShopUsageVarianceParams) {
    this.SUVParams = value;
   if (this.selectedReport && this.selectedReport.Id != 0) {
     switch (this.selectedReport.Id) {
       case 2: {
         if (this.SUVParams)
           this.selectedReport.DXReportName = `ShopUsageVariance?${this.SUVParams.BuildingId},${this.SUVParams.FromPeriodId},${this.SUVParams.ToPeriodId},${this.SUVParams.AllTenants}`;
         break;
       }
       default: {
         break;
       }
     }
   }
  }

  get ShopCostVarianceParams(): IShopCostVarianceParams {
    return this.SCVParams;
  }

  set ShopCostVarianceParams(value: IShopCostVarianceParams) {
    this.SCVParams = value;
   if (this.selectedReport && this.selectedReport.Id != 0) {
     switch (this.selectedReport.Id) {
       case 2: {
         if (this.SCVParams)
           this.selectedReport.DXReportName = `ShopUsageVariance?${this.SUVParams.BuildingId},${this.SUVParams.FromPeriodId},${this.SUVParams.ToPeriodId},${this.SUVParams.AllTenants}`;
         break;
       }
       default: {
         break;
       }
     }
   }
  }

  get UtilityReportParams(): IUtilityReportParams {
    return this.UREParams;
  }

  set UtilityReportParams(value: IUtilityReportParams) {
    this.UREParams = value;
   if (this.selectedReport && this.selectedReport.Id != 0) {
     switch (this.selectedReport.Id) {
       case 3: {
         if (this.UREParams)
           this.selectedReport.DXReportName = `ShopUsageVariance?${this.SUVParams.BuildingId},${this.SUVParams.FromPeriodId},${this.SUVParams.ToPeriodId},${this.SUVParams.AllTenants}`;
         break;
       }
       default: {
         break;
       }
     }
   }
  }

  get SelectedReportId(): number {
    return this.selectedReport ? this.selectedReport.Id : null;
  }

  set SelectedReportId(id: number) {
    this.selectedReport = this.dxReportList.find(r => r.Id == id);
  }

  public sendError(msg: string) {
    this.ErrorSubject.next(msg);
  }

  resetAll(): void {
    this.ErrorSubject.next(null);
    this.bsBuildings.next([]);
    this.bsPartners.next(null);
    this.bsPeriods.next(null);
    this.bsEndPeriods.next(null);
    this.bsLoadReport.next(null);
    this.BRParams = null;
    this.showCrit = false;
    this.showCrit = false;
  }

  ReportSelectionChanged() {
    this.showCrit = false;
    this.frmCriteriaValid = false;
    this.frmSelectValid = false;
    this.SelectedReportId = 0;
    this.showResults = false;
    this.BRParams = null;
    this._reportChanged.next(true);
  }

  getReportDataForShop() {
    // this.SUVParams = {
    //   "BuildingId": 2983,
    //   "StartPeriodId": 172047,
    //   "ToPeriodId": 172047,
    //   "AllTenants": 1
    // }
    const url = `${CONFIG.apiURL}/ReportShopUsageVariance/getReportDataNoPivotWithAverageAndVariance`;
    return this.http.post<any>(url, this.SUVParams, { withCredentials: true })
      .pipe(
          catchError(err => this.catchErrors(err)),
          tap(m => {
            this._shopUsageVariance.next(m);
          //console.log(`getMetersForUser observable returned ${m}`);
          }),
      );
  }

  getReportDataForShopCosts() {
    const url = `${CONFIG.apiURL}/ReportShopCostsVariance/getReportData`;
    return this.http.post<any>(url, this.SCVParams, { withCredentials: true })
      .pipe(
          catchError(err => this.catchErrors(err)),
          tap(m => {
            this._shopCostVariance.next(m);
          }),
      );
  }

  getReportDataForUtility() {
    const url = `${CONFIG.apiURL}/Reports/UtilityRecoveryReport`;
    const queryParams =
      '?' +
      Object.keys(this.UREParams)
        .map(key => {
          return `${key}=${encodeURIComponent(this.UREParams[key])}`;
        })
        .join('&');
    return this.http.get<any>(url + queryParams, { withCredentials: true })
      .pipe(
          catchError(err => this.catchErrors(err)),
          tap(m => {
            this._utilityRecoveryExpense.next(m);
          }),
      );
  }

  setShopUsageVariance(data) {
    this._shopUsageVariance.next(data);
  }
  
  setShopCostVariance(data) {
    this._shopCostVariance.next(data);
  }

  catchErrors(error: { error: { message: any; }; message: any; }): Observable<Response> {
    if (error && error.error && error.error.message) { //clientside error
      console.log(`Client side error: ${error.error.message}`);
      this.ErrorSubject.next(error.error.message);
    } else if (error && error.message) { //server side error
      console.log(`Server side error: ${error.message}`);
      this.ErrorSubject.next(error.message);
    } else {
      console.log(`Error occurred: ${JSON.stringify(error)}`);
      this.ErrorSubject.next(JSON.stringify(error));
    }
    return throwError(error);
  }

}
