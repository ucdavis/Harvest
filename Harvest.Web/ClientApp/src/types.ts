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
  name: string;
}

export interface Quote {}

export interface ProjectWithQuotes {
  project: Project;
  quotes: Quote[];
}

export interface ActionType {
  id: number;
  name: string;
};

export interface MeasurementUnit {
  id: number;
  name: string;
};

export interface Action {
  id: number;
  type: ActionType;
  name: string;
  unit: MeasurementUnit;
};

export interface WorkItem {
  id?: string;
  type: string;
  rate: number;
  quantity: number;
};

export interface Activity{
  workItems: WorkItem[];
};
