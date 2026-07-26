import { Component } from '@angular/core';
import { BusinessPartnerList } from '../../shared/business-partners/business-partner-list';
import { BusinessPartnerListConfig } from '../../shared/business-partners/business-partner-list.models';
import { RightCode } from '../../core/auth/right-code';

@Component({
  selector: 'app-customers',
  imports: [BusinessPartnerList],
  template: `<app-business-partner-list [config]="config" />`,
})
export class Customers {
  readonly config: BusinessPartnerListConfig = {
    listPartnerType: 'Customer',
    allowedPartnerTypes: ['Customer', 'Both'],
    title: 'Customers',
    createRight: RightCode.CustomersCreate,
    editRight: RightCode.CustomersEdit,
  };
}
