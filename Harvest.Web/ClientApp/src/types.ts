export type CropType = "Row" | "Tree"

export interface Project {
  id: number;
  start: Date;
  end: Date;
  crop: string;
  cropType: CropType;
  requirements: string;
  name: string;
  principalInvestigator: User;
  location: null;
  locationCode: null;
  quoteId: number;
  quote: null;
  quoteTotal: number;
  chargedTotal: number;
  createdBy: User;
  createdOn: Date;
  status: string;
  currentAccountVersion: number;
  isActive: boolean;
  accounts: null;
  quotes: null;
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
}

export interface Rate {
  price: number;
  unit: string;
  type: string;
  description: string;
  id: number;
}

export interface Expense {
  id: number;
  description: string;
  type: string;
  quantity: number;
  rate: Rate;
  total: number;
}

// TODO: should this be a different name or is it ok?  Do we even need an interface?

// the dynamic content which will be stored in Quote.text
export class QuoteContentImpl implements QuoteContent {
  projectName = ""; // TODO: might be worth removing when feasible
  acres = 0;
  acreageRate = 360;
  acreageRateId = 0;
  acreageRateDescription = "";
  total = 0;
  acreageTotal = 0;
  activitiesTotal = 0;
  laborTotal = 0;
  equipmentTotal = 0;
  otherTotal = 0;
  grandTotal = 0;
  fields = [];

  activities = [] as Activity[];
}

export interface QuoteContent {
  projectName: string; // TODO: might be worth removing when feasible
  acres: number;
  acreageRate: number;
  acreageRateId: number;
  acreageRateDescription: string;
  activities: Activity[];
  total: number;
  acreageTotal: number;
  activitiesTotal: number;
  laborTotal: number;
  equipmentTotal: number;
  otherTotal: number;
  grandTotal: number;
  fields: Field[];
}

export interface Field {
  id: number;
  name: string;
  crop: string;
  details: string;
  geometry: GeoJSON.FeatureCollection;
}

export interface Quote {
  id: number;
  projectId: number;
  text: string;
  total: number;
  initatedById: number;
  currentDocumentId: number | null;
  approvedById: number | null;
  approvedOn: string | null;
  createdDate: string;
  status: string;
  project: Project;
  initiatedBy: User;
  approvedBy: User;
  documents: Document[];
  currentDocument: Document;
}

export interface ProjectWithQuote {
  project: Project;
  quote: QuoteContent | null;
}

export interface ActionType {
  id: number;
  name: string;
}

export interface MeasurementUnit {
  id: number;
  name: string;
}

export interface Action {
  id: number;
  type: ActionType;
  name: string;
  unit: MeasurementUnit;
}

export class WorkItemImpl implements WorkItem {
  id;
  activityId;
  description;
  type;
  rate;
  rateId = 0;
  quantity;
  total = 0;

  constructor(activityId: number, id: number, type: string) {
    this.activityId = activityId;
    this.id = id;
    this.type = type;
    this.rate = 0;
    this.quantity = 0;
    this.description = "";
  }
}
export interface WorkItem {
  id: number;
  activityId: number;
  type: string;
  rateId: number;
  rate: number;
  description: string;
  quantity: number;
  total: number;
}

export interface Activity {
  total: number;
  id: number;
  name: string;
  workItems: WorkItem[];
}

export interface ProjectAccount {
  id: number;
  projectId: number;
  number: string;
  name: string;
  percentage: number;
}

export interface RequestInput {
  id: number;
  start: string;
  end: string;
  crop: string;
  cropType: string;
  requirements?: string;
  principalInvestigator: PrincipalInvestigator;
}

export interface PrincipalInvestigator {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
  nameAndEmail: string;
}