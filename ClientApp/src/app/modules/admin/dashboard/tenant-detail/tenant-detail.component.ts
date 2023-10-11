import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { DashboardService } from '../dasboard.service';
import { ApexAxisChartSeries, ApexChart, ApexDataLabels, ApexFill, ApexLegend, ApexPlotOptions, ApexStroke, ApexTitleSubtitle, ApexTooltip, ApexXAxis, ApexYAxis, ChartComponent } from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  title: ApexTitleSubtitle;
  plotOptions: any;
  dataLabels: ApexDataLabels;
  stroke: ApexStroke;
  yaxis: ApexYAxis;
  fill: ApexFill;
  tooltip: ApexTooltip;
  legend: ApexLegend;
  colors: any
};

export type LineChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  dataLabels: ApexDataLabels;
  plotOptions: ApexPlotOptions;
  stroke: ApexStroke;
  xaxis: ApexXAxis;
  yaxis: ApexYAxis;
  colors: string[];
  fill: ApexFill;
  legend: ApexLegend;
  tooltip: ApexTooltip;
  title: any;
  grid: any;
  markers: any;
};

export type TreemapChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  dataLabels: ApexDataLabels;
  title: ApexTitleSubtitle;
  plotOptions: ApexPlotOptions;
  legend: ApexLegend;
  colors: string[];
}

@Component({
  selector: 'app-tenant-detail',
  templateUrl: './tenant-detail.component.html',
  styleUrls: ['./tenant-detail.component.scss']
})
export class TenantDetailComponent implements OnInit {

  @Input() buildingId: number;
  @Input() tenantId: number;
  @Input() tenantName: string;

  dataSource: any;
  billingSummaryDataSource: any;
  tenantDetailDashboard: any;
  lastPeriodName: string;
  lastPeriodBillings: any[] = [];
  billingTotal: number;
  shopListItems: any[] = [{value: 0, label: 'All'}];
  allAvailableImages: number;
  groupList: any[] = [];
  periodList: any[] = [];
  billingPeriodList: any[] = [];

  monthNameList = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
  yearList = [];
  availableGroupColors: any;
  groupColors = ['#008E0E', '#452AEB', '#2FAFB7', '#C23BC4', '#6E6E6E', '#46a34a', '#C24F19', '#C8166C', '#84cc16', '#06b6d4', '#8b5cf6', '#f59e0b', '#6b21a8', '#9f1239', '#d946ef', '#a855f7'];
  
  selectedMonth;

  selectedShop: number = 0;
  includeVacant: boolean = false;
  public treeMapOptions: Partial<TreemapChartOptions>;

  billingElectricityChartType: string = 'Bar';
  billingUsageElectricityChartType: string = 'Bar';
  billingWaterChartType: string = 'Bar';
  billingUsageWaterChartType: string = 'Bar';
  billingSewerChartType: string = 'Bar';
  billingUsageSewerChartType: string = 'Bar';

  billingElectricitySeries = [];
  billingUsageElectricitySeries = [];
  billingWaterSeries = [];
  billingUsageWaterSeries = [];
  billingSewerageSeries = [];
  billingUsageSewerageSeries = [];

  public commonBarChartOptions: Partial<ChartOptions>;
  public commonUsageBarChartOptions: Partial<ChartOptions>;
  public commonLineChartOptions: Partial<LineChartOptions>;

  private _unsubscribeAll: Subject<any> = new Subject<any>();
  
  constructor(
    private service: DashboardService,
    private _cdr: ChangeDetectorRef
  ) { 
    this.treeMapOptions = {
      series: [
      ],
      legend: {
        show: false
      },
      chart: {
        type: "treemap",
        toolbar: {
          show: false
        }
      },
      title: {
        text: "",
        align: "center"
      },
      colors: [
      ],
      plotOptions: {
        treemap: {
          distributed: true,
          enableShades: false
        }
      }
    };
    this.commonBarChartOptions = {
      series: [],
      chart: {
        type: "bar",
        height: 350,
        toolbar: {
          show: false
        }
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "55%",
          endingShape: "rounded"
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        show: true,
        width: 2,
        colors: ["transparent"]
      },
      xaxis: {
        categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
      },
      yaxis: {
        labels: {
          formatter: function(val) {
            return 'R ' + val;
          } 
        }
      },
      fill: {
        opacity: 1,
        colors: []
      },
      tooltip: {
        y: {
          formatter: function(val) {
            return 'R ' + val;
          }
        }
      }
    };
    this.commonUsageBarChartOptions = {
      series: [],
      chart: {
        type: "bar",
        height: 350,
        toolbar: {
          show: false
        }
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "55%",
          endingShape: "rounded"
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        show: true,
        width: 2,
        colors: ["transparent"]
      },
      xaxis: {
        categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
      },
      yaxis: {
        labels: {
          formatter: function(val) {
            return '' + val;
          } 
        }
      },
      fill: {
        opacity: 1,
        colors: []
      },
      tooltip: {
        y: {
          formatter: function(val) {
            return '' + val;
          }
        }
      }
    };
    this.commonLineChartOptions = {
      series: [
      ],
      chart: {
        height: 400,
        type: "line",
        toolbar: {
          show: false
        }
      },
      dataLabels: {
        enabled: true
      },
      stroke: {
        curve: "smooth"
      },
      title: {
        text: "",
        align: "left"
      },
      grid: {
        borderColor: "#e7e7e7",
        row: {
          colors: ["#f3f3f3", "transparent"], // takes an array which will be repeated on columns
          opacity: 0.5
        }
      },
      markers: {
        size: 4
      },
      xaxis: {
        categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
      },
      yaxis: {
        labels: {
          formatter: function(val) {
            return 'R ' + val;
          } 
        }
      },
      tooltip: {
        y: {
          formatter: function(val) {
            return 'R ' + Math.round(Number(val) * 100) / 100;
          }
        }
      }
    };
  }

  ngOnInit(): void {
    this.service.tenantDetail$
      .pipe(takeUntil(this._unsubscribeAll))
      .subscribe((res) => {
        if(res) {
          this.tenantDetailDashboard = res;
          if(this.tenantDetailDashboard['BillingData'].length > 0) {
            this.lastPeriodName = this.tenantDetailDashboard['BillingData'][this.tenantDetailDashboard['BillingData'].length - 1]['PeriodName'];
            this.lastPeriodBillings = this.tenantDetailDashboard['BillingData'].filter(item => item['PeriodName'] == this.lastPeriodName);
            this.billingTotal = this.lastPeriodBillings.reduce((prev, cur) => prev + cur.Amount, 0);
            this.allAvailableImages = this.tenantDetailDashboard.ReadingsInfo.reduce((prev, cur) => prev + cur.HasImages, 0);
            this.tenantDetailDashboard.Shops.forEach(shop => {
              let result = {value: shop.ShopID, item: shop};
              this.shopListItems.push(result);
            })
            this.groupList = this.tenantDetailDashboard.BillingData.map(billing => billing.GroupName.trim()).filter(this.onlyUnique);
            this.periodList = this.tenantDetailDashboard.BillingData.map(billing => billing.PeriodName).filter(this.onlyUnique);
            this.yearList = this.tenantDetailDashboard.BillingData.map(billing => billing.PeriodName.split(' ')[1]).filter(this.onlyUnique);
            this.billingPeriodList = this.periodList.map(period => {
              return {name:period, value: period}
            }).reverse();
            this.selectedMonth = this.billingPeriodList[0]['value'];
            this.setBillingSummary();

            this.setSeriesForBillingChart();

            this._cdr.detectChanges();
          }
        }
      });
  }

  setBillingSummary() {
    
    let billingSummaryData = [];
    this.billingSummaryDataSource = [];
    
    this.groupList.forEach(groupName => {
      let groupData = [];
      let groupUsageData = [];
      groupData.push(this.tenantDetailDashboard.BillingData
                            .filter(period => period.PeriodName == this.selectedMonth && period.GroupName.trim() == groupName)
                            .reduce((prev, cur) => prev + cur.Amount, 0));
      groupUsageData.push(this.tenantDetailDashboard.BillingData
        .filter(period => period.PeriodName == this.selectedMonth && period.GroupName.trim() == groupName)
        .reduce((prev, cur) => prev + cur.Usage, 0));

      let totalByGroup = groupData.reduce((prev, cur) => prev + cur, 0);
      let totalUsageByGroup = groupUsageData.reduce((prev, cur) => prev + cur, 0);

      billingSummaryData.push({x: groupName, y: totalByGroup});
      this.billingSummaryDataSource.push({name: groupName, amount: totalByGroup, usage: totalUsageByGroup});
    })
    
    this.treeMapOptions.colors = this.groupColors.slice(0, this.groupList.length);
    this.treeMapOptions.series = [];
    this.treeMapOptions.series.push({'data': billingSummaryData});

  }

  onBillingMonthChange(event) {
    this.setBillingSummary();
  }

  setSeriesForBillingChart() {
    let billingElectricitySeries = []; 
    let billingUsageElectricitySeries = [];
    let billingWaterSeries = [];
    let billingUsageWaterSeries = [];
    let billingSewerageSeries = [];
    let billingUsageSewerageSeries = [];

    this.yearList.forEach(year => {
      billingElectricitySeries.push({name: year, data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]});
      billingUsageElectricitySeries.push({name: year, data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]});
      billingWaterSeries.push({name: year, data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]});
      billingUsageWaterSeries.push({name: year, data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]});
      billingSewerageSeries.push({name: year, data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]});
      billingUsageSewerageSeries.push({name: year, data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]});
    });

    this.tenantDetailDashboard.BillingData.forEach(billing => {
      let year = billing['PeriodName'].split(' ')[1];
      let month = billing['PeriodName'].split(' ')[0];
      let idx = billingElectricitySeries.findIndex(obj => obj['name'] == year);
      let monthIdx = this.monthNameList.indexOf(month);
      if(billing['Utility'] == 'Electricity') {        
        billingElectricitySeries[idx]['data'][monthIdx] += billing['Amount'];
        billingUsageElectricitySeries[idx]['data'][monthIdx] += billing['Usage'];
      }
      if(billing['Utility'] == 'Water') {
        billingWaterSeries[idx]['data'][monthIdx] += billing['Amount'];
        billingUsageWaterSeries[idx]['data'][monthIdx] += billing['Usage'];
      }
      if(billing['Utility'] == 'Sewerage') {
        billingSewerageSeries[idx]['data'][monthIdx] += billing['Amount'];
        billingUsageSewerageSeries[idx]['data'][monthIdx] += billing['Usage'];
      }
    });
    this.billingElectricitySeries = billingElectricitySeries;
    this.billingUsageElectricitySeries = billingUsageElectricitySeries;
    this.billingWaterSeries = billingWaterSeries;
    this.billingUsageWaterSeries = billingUsageWaterSeries;
    this.billingSewerageSeries = billingSewerageSeries;
    this.billingUsageSewerageSeries = billingUsageSewerageSeries;

  }

  onChangeShop(event) {
    this.service.getTenantDashboardDetail(this.buildingId, this.tenantId, event.value, this.includeVacant).subscribe();
  }

  onIncludeVacantChange(event) {
    this.service.getTenantDashboardDetail(this.buildingId, this.tenantId, this.selectedShop, event.checked).subscribe();
  }

  onlyUnique(value, index, array) {
    return array.indexOf(value) === index;
  }

  /**
     * On destroy
     */
  ngOnDestroy(): void
  {
    // Unsubscribe from all subscriptions
    this._unsubscribeAll.next(null);
    this._unsubscribeAll.complete();
    this.service.destroyTenantDetail();
  }
}
