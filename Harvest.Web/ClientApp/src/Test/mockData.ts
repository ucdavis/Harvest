import {
  AppContextShape,
  BlobFile,
  Crop,
  Invoice,
  Project,
  ProjectWithQuote,
  Rate,
  Team,
  Ticket,
  History,
} from "../types";

const fakeUser = {
  id: 1,
  firstName: "Bob",
  lastName: "Dobalina",
  email: "bdobalina@ucdavis.edu",
  iam: "1000037182",
  kerberos: "bdobalina",
  name: "Mr Mr Mr Bob Dobalina",
};

const fakeAttachments: BlobFile[] = [
  {
    identifier: "1",
    fileName: "file1.pdf",
    fileSize: 22,
    contentType: "application/pdf",
    uploaded: true,
    sasLink: "link1",
  },
  {
    identifier: "1",
    fileName: "file2.pdf",
    fileSize: 22,
    contentType: "application/pdf",
    uploaded: true,
    sasLink: "link2",
  },
];

export const fakeAppContext: AppContextShape = {
  antiForgeryToken: "fakeAntiForgeryToken",
  user: {
    detail: {
      ...fakeUser,
    },
    roles: ["System", "FieldManager", "PI"],
  },
  usecoa: false,
};

export const fakeTeam: Team = {
  id: 1,
  name: "Team 1",
  slug: "team1",
  description: "Team 1",
};

export const fakeProject: Project = {
  id: 3,
  start: new Date("2021-03-15T00:00:00"),
  end: new Date("2021-03-29T00:00:00"),
  crop: "Tomato",
  cropType: "Row",
  requirements: "Grow me some tomatoes",
  name: "Tomato",
  principalInvestigator: {
    ...fakeUser,
  },
  location: null,
  locationCode: null,
  quoteId: 0,
  quote: null,
  quoteTotal: 0.0,
  chargedTotal: 0.0,
  createdOn: new Date("2021-03-15T00:00:00"),
  lastStatusUpdatedOn: new Date("2021-03-15T00:00:00"),
  status: "Active",
  currentAccountVersion: 0,
  isActive: false,
  createdBy: {
    ...fakeUser,
  },
  accounts: [],
  quotes: null,
  attachments: fakeAttachments,
  acres: 1,
  acreageRate: {
    id: 1,
    price: 0,
    unit: "",
    type: "Acreage",
    description: "",
    isPassthrough: false,
  },
  team: {
    id: 1,
    name: "Team 1",
    slug: "team-1",
    description: "Team 1",
  },
  shareId: "1234567890",
  projectPermissions: [
    {
      id: 1,
      user: fakeUser,
      permission: "View",
      projectId: 3,
    },
    {
      id: 2,
      user: fakeUser,
      permission: "Edit",
      projectId: 3,
    },
  ],
};

export const fakeProjectWithQuote: ProjectWithQuote = {
  project: {
    id: 3,
    start: new Date("2021-03-15T00:00:00"),
    end: new Date("2021-03-29T00:00:00"),
    crop: "Tomato",
    cropType: "Row",
    requirements: "Grow me some tomatoes",
    name: "Tomato",
    principalInvestigator: {
      ...fakeUser,
    },
    location: null,
    locationCode: null,
    quoteId: 0,
    quote: null,
    quoteTotal: 0.0,
    chargedTotal: 0.0,
    createdOn: new Date("2021-03-15T00:00:00"),
    lastStatusUpdatedOn: new Date("2021-03-15T00:00:00"),
    status: "Requested",
    currentAccountVersion: 0,
    isActive: false,
    createdBy: {
      ...fakeUser,
    },
    accounts: [],
    quotes: null,
    attachments: [],
    acres: 1,
    acreageRate: {
      id: 1,
      price: 0,
      unit: "",
      type: "Acreage",
      description: "",
      isPassthrough: false,
    },
    team: {
      id: 1,
      name: "Team 1",
      slug: "team-1",
      description: "Team 1",
    },
    shareId: "1234567890",
    projectPermissions: [
      {
        id: 1,
        user: fakeUser,
        permission: "View",
        projectId: 3,
      },
      {
        id: 2,
        user: fakeUser,
        permission: "Edit",
        projectId: 3,
      },
    ],
  },
  quote: null,
};

export const fakeHistories: History[] = [
  {
    id: 1,
    description: "Project created",
    actionDate: new Date("2021-03-15T00:00:00"),
    actor: fakeUser,
  },
  {
    id: 2,
    description: "Project updated",
    actionDate: new Date("2021-03-16T00:00:00"),
    actor: fakeUser,
  },
];

export const fakeInvoices: Invoice[] = [
  {
    id: 1,
    projectId: 1,
    total: 100,
    createdOn: new Date("2021-03-15T00:00:00"),
    notes: "hello",
    status: "Requested",
    expenses: [],
    transfers: [],
  },
  {
    id: 2,
    projectId: 1,
    total: 200,
    createdOn: new Date("2021-03-15T00:00:00"),
    notes: "hello",
    status: "Requested",
    expenses: [],
    transfers: [],
  },
  {
    id: 3,
    projectId: 1,
    total: 300,
    createdOn: new Date("2021-03-15T00:00:00"),
    notes: "hello",
    status: "Requested",
    expenses: [],
    transfers: [],
  },
];

export const fakeTickets: Ticket[] = [
  {
    id: 1,
    projectId: 3,
    name: "Ticket 1",
    requirements: "none",
    dueDate: new Date("2021-03-15T00:00:00"),
    updatedOn: new Date("2021-04-15T00:00:00"),
    attachments: [],
    status: "Requested",
    createdOn: new Date("2021-01-15T00:00:00"),
    project: fakeProject,
  },
  {
    id: 2,
    projectId: 3,
    name: "Ticket 2",
    requirements: "none",
    dueDate: new Date("2021-03-15T00:00:00"),
    updatedOn: new Date("2021-04-15T00:00:00"),
    attachments: [],
    status: "Requested",
    createdOn: new Date("2021-01-15T00:00:00"),
    project: fakeProject,
  },
  {
    id: 3,
    projectId: 3,
    name: "Ticket 3",
    requirements: "none",
    dueDate: new Date("2021-03-15T00:00:00"),
    updatedOn: new Date("2021-04-15T00:00:00"),
    attachments: [],
    status: "Requested",
    createdOn: new Date("2021-01-15T00:00:00"),
    project: fakeProject,
  },
  {
    id: 4,
    projectId: 3,
    name: "Ticket 4",
    requirements: "none",
    dueDate: new Date("2021-03-15T00:00:00"),
    updatedOn: new Date("2021-04-15T00:00:00"),
    attachments: [],
    status: "Requested",
    createdOn: new Date("2021-01-15T00:00:00"),
    project: fakeProject,
  },
];

export const sampleRates: Rate[] = [
  {
    price: 1234.0,
    unit: "Per Acre per Year",
    type: "Acreage",
    description: "Russell Ranch Acreage",
    id: 1,
    isPassthrough: false,
  },
  {
    price: 60.0,
    unit: "Hourly",
    type: "Labor",
    description: "Skilled Labor",
    id: 3,
    isPassthrough: false,
  },
  {
    price: 66.67,
    unit: "Per acre",
    type: "Other",
    description: "Crop Destruction",
    id: 5,
    isPassthrough: false,
  },
  {
    price: 34.91,
    unit: "Per Acre",
    type: "Equipment",
    description: "Backhoe",
    id: 8,
    isPassthrough: false,
  },
];

export const fakeCrops = [
  {
    id: 1,
    type: "Row",
    name: "Corn",
  },
  {
    id: 2,
    type: "Row",
    name: "Cabbage",
  },
  {
    id: 3,
    type: "Row",
    name: "Celery",
  },
  {
    id: 4,
    type: "Row",
    name: "Potato",
  },
  {
    id: 5,
    type: "Tree",
    name: "Almond",
  },
  {
    id: 6,
    type: "Tree",
    name: "Orange",
  },
  {
    id: 7,
    type: "Tree",
    name: "Lemon",
  },
] as Crop[];
