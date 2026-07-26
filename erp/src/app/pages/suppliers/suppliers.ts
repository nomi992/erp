import { Component } from '@angular/core';
import { BusinessPartnerList } from '../../shared/business-partners/business-partner-list';
import { BusinessPartnerListConfig } from '../../shared/business-partners/business-partner-list.models';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-suppliers',
  imports: [BusinessPartnerList],
  template: `<app-business-partner-list [config]="config" />`,
})
export class Suppliers {
  readonly config: BusinessPartnerListConfig = {
    listPartnerType: 'Supplier',
    allowedPartnerTypes: ['Supplier', 'Both'],
    title: 'Suppliers',
    createRight: RightCode.SuppliersCreate,
    editRight: RightCode.SuppliersEdit,
  };
}
