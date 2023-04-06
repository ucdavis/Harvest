export type CropType = "Row" | "Tree" | "Other";
export interface Crop {
  id: number;
  type: CropType;
  name: string;
}

export type RateType =
  | "Acreage"
  | "Equipment"
  | "Labor"
  | "Other"
  | "Adjustment";
export type RoleName =
  | "System"
  | "FieldManager"
  | "Supervisor"
  | "Worker"
  | "Finance"
  | "PI";

export type ProjectStatus =
  | "Requested"
  | "PendingApproval"
  | "QuoteRejected"
  | "Active"
  | "ChangeRequested"
  | "Completed"
  | "AwaitingCloseout"
  | "PendingCloseoutApproval"
  | "FinalInvoicePending";

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
  status: ProjectStatus;
  currentAccountVersion: number;
  isActive: boolean;
  accounts: ProjectAccount[];
  quotes: null;
  attachments: BlobFile[];
  acreageRate: Rate;
  acres: number;
  team: Team;
}

export interface BlobFile {
  identifier: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  uploaded: boolean;
  sasLink?: string;
}

export interface Invoice {
  id: number;
  projectId: number;
  total: number;
  createdOn: Date;
  notes: string;
  status: string;
  expenses: Expense[];
  transfers: Transfer[];
}

export interface Team {
    id: number;
    name: string;
    slug: string;
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
  isPassthrough: boolean;
}

export interface Expense {
  id: number;
  activity: string;
  description: string;
  type: RateType;
  quantity: number;
  markup: boolean;
  rate: Rate | null;
  rateId: number;
  price: number;
  total: number;
  createdOn?: Date;
  createdBy?: User;
}

export interface AdhocProjectModel {
  project?: Project;
  expenses: Expense[];
  accounts: ProjectAccount[];
  quote: QuoteContent;
}

export enum ExpenseQueryParams {
  ReturnOnSubmit = "returnOnSubmit",
}

export interface Transfer {
  id: number;
  type: string;
  account: string;
  total: number;
  isProjectAccount: boolean;
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
  markup = false;
  total = 0;
  isPassthrough = false;

  constructor(activityId: number, id: number, type: RateType) {
    this.activityId = activityId;
    this.id = id;
    this.type = type;
    this.rate = 0;
    this.quantity = 0;
    this.description = "";
    this.unit = "";
    this.isPassthrough = false;
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
  markup: boolean;
  total: number;
  isPassthrough: boolean;
}

// the dynamic content which will be stored in Quote.text
export class QuoteContentImpl implements QuoteContent {
  projectName = ""; // TODO: might be worth removing when feasible
  acres = 0;
  acreageRate = 360;
  acreageRateId = null;
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

  activities = [
    {
      id: 1,
      name: "Activity",
      total: 0,
      workItems: [
        new WorkItemImpl(1, 1, "Labor"),
        new WorkItemImpl(1, 2, "Equipment"),
        new WorkItemImpl(1, 3, "Other"),
      ],
      year: 1, // default new activity to no adjustment
      adjustment: 0,
    },
  ] as Activity[];
}

export interface QuoteContent {
  projectName: string; // TODO: might be worth removing when feasible
  acres: number;
  acreageRate: number;
  acreageRateId: number | null;
  acreageRateDescription: string;
  activities: Activity[];
  years: number;
  acreageTotal: number;
  activitiesTotal: number;
  laborTotal: number;
  equipmentTotal: number;
  otherTotal: number;
  grandTotal: number;
  fields: Field[];
  approvedBy?: User;
  approvedOn?: Date;
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

export interface TicketInput {
  name: string;
  requirements: string;
  attachments: BlobFile[];
  dueDate?: Date;
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
  newAttachments: BlobFile[];
  completed: boolean;
}
export interface TicketAttachment {
  id: number;
  fileName: string;
  createdOn: Date;
  createdBy: User;
  identifier: string;
  sasLink?: string;
}
export interface TicketMessage {
  id: number;
  message: string;
  createdBy: User;
  createdOn: Date;
}

export interface RequestInput {
  id: number;
  start: Date;
  end: Date;
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
  antiForgeryToken: string;
  user: {
    detail: User;
    roles: RoleName[];
  };
  usecoa: boolean;
}

export interface PromiseStatus {
  pending: boolean;
  success: boolean;
}

export interface Result<T> {
  value: T;
  isError: boolean;
  message: string;
}
