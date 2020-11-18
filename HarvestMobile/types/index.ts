export type RootStackParamList = {
  Root: undefined;
  NotFound: undefined;
};

export type DrawerParamList = {
  SignIn: undefined;
  Home: undefined;
  TimeSheets: undefined;
};

export type HomeParamList = {
  HomeScreen: undefined;
};

export type ActionType  = {
  id: number;
  name: string;
};

export type MeasurementUnit = {
  id: number;
  name: string;
};

export type Action = {
  id: number;
  type: ActionType;
  name: string;
  unit: MeasurementUnit
};

export type WorkItem = {
  id: string;
  action: Action;
  quantity: number;
};

export type Location = {
  id: number;
  name: string;
  unit: MeasurementUnit; // just in case an area is measured in something other than acres
  size: number;
};

export type Project = {
  id: number;
  name: string;
  description: string;
  locations: Location[];
};

export type TimeSheet = {
  id: string;
  location: Location;
  project: Project;
  workItems: WorkItem[];
};