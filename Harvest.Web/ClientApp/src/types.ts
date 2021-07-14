export type CropType = "Row" | "Tree";
export type RateType = "Acreage" | "Equipment" | "Labor" | "Other";
export type RoleName =
  | "Admin"
  | "FieldManager"
  | "Supervisor"
  | "System"
  | "Worker";

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
  attachments: BlobFile[];
}

export interface BlobFile {
  identifier: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  uploaded: boolean;
}

export interface Invoice {
  id: number;
  total: number;
  createdOn: Date;
  notes: string;
  status: string;
  expenses: Expense[];
  transfers: Transfer[];
}

export interface User {
  id: number;
  firstName?: string;
  lastName: string;
  email: string;
  iam: string;
  kerberos: string;
  name: string;
}

export interface Rate {
  price: number;
  unit: string;
  type: RateType;
  description: string;
  id: number;
}

export interface Expense {
  id: number;
  activity: string;
  description: string;
  type: RateType;
  quantity: number;
  rate: Rate | null;
  rateId: number;
  price: number;
  total: number;
  createdOn?: Date;
  createdBy?: User;
}

export interface Transfer {
  id: number;
  type: string;
  account: string;
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
  years = 1;
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
  years: number;
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
  geometry: GeoJSON.Polygon;
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
  unit = "hr";
  quantity;
  total = 0;

  constructor(activityId: number, id: number, type: RateType) {
    this.activityId = activityId;
    this.id = id;
    this.type = type;
    this.rate = 0;
    this.quantity = 0;
    this.description = "";
    this.unit = "";
  }
}
export interface WorkItem {
  id: number;
  activityId: number;
  type: RateType;
  rateId: number;
  rate: number;
  description: string;
  quantity: number;
  unit: string;
  total: number;
}

export interface Activity {
  total: number;
  id: number;
  name: string;
  year: number;
  adjustment: number;
  workItems: WorkItem[];
}

export interface ProjectAccount {
  id: number;
  projectId: number;
  number: string;
  name: string;
  accountManagerName: string;
  accountManagerEmail: string;
  percentage: number;
}

//Rename to TicketCreate? (Attachments is a BlobFile[] which doesn't work when I'm pulling the attachments from the DB see TicketDetails)
export interface Ticket {
    id: number;
    projectId: number;
    name: string;
    requirements: string;
    dueDate?: Date;
    updatedOn?: Date;
    attachments: BlobFile[];
    status: string;
    createdOn: Date;
}
export interface TicketDetails {
    id: number;
    projectId: number;
    name: string;
    requirements: string;
    dueDate?: Date;
    updatedOn?: Date;
    attachments: TicketAttachment[];
    messages: TicketMessage[];
    status: string;
    createdOn: Date;
    createdBy: User;
    workNotes: string;
    updatedBy?: User;
}
export interface TicketAttachment {
    id: number;
    fileName: string;
    createdOn: Date;
    createdBy: User;
}
export interface TicketMessage {
    id: number;
    message: string;
    createdBy: User;
    createdOn: Date;
}

export interface RequestInput {
  id: number;
  start: string;
  end: string;
  crop: string;
  cropType: string;
  requirements?: string;
  principalInvestigator: User;
  files: BlobFile[];
}

export interface ProjectWithInvoice {
  project: Project;
  invoice: Invoice;
}

export interface AppContextShape {
  user: {
    detail: User;
    roles: RoleName[];
  };
}
