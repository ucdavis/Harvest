export interface Project {
  id: number;
  start: Date;
  end: Date;
  crop: string;
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

// TODO: should this be a different name or is it ok?  Do we even need an interface?

// the dynamic content which will be stored in Quote.text
export class QuoteContentImpl implements QuoteContent {
  projectName = ""; // TODO: might be worth removing when feasible
  acres = 0;
  acreageRate = 360;
  get acreageTotal(): number {
    return this.acres * this.acreageRate;
  }
  activities = [];
}

export interface QuoteContent {
  projectName: string; // TODO: might be worth removing when feasible
  acres: number;
  acreageRate: number;
  activities: Activity[];
  readonly acreageTotal: number;
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

export interface ProjectWithQuotes {
  project: Project;
  quotes: Quote[];
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
  type;
  rate;
  quantity;

  get total(): number {
    return this.rate * this.quantity;
  }

  constructor(activityId: number, id: number, type: string) {
    this.activityId = activityId;
    this.id = id;
    this.type = type;
    this.rate = 0;
    this.quantity = 0;
  }
}
export interface WorkItem {
  id: number;
  activityId: number;
  type: string;
  rate: number;
  quantity: number;
  readonly total: number;
}

export interface Activity {
  id: number;
  name: string;
  workItems: WorkItem[];
}

export enum CropType {
  Row = "Row",
  Tree = "Tree"
}

export interface Crop {
  id: number;
  name: string;
}

export interface Request {
  id: number;
  projectId: number;
  project: Project | null;
  requirements: string;
  start: Date;
  end: Date;
  initatedById: number;
  initiatedBy: User | null;
  cropType: CropType;
  crops: string;
  principalInvestigatorId: number;
  principalInvestigator: User | null;

  approvedById: number | null;
  approvedBy: User | null;
  approvedOn: string | null;

  createdDate: Date;
  status: string;
}

