import {
  AppContextShape,
  BlobFile,
  Invoice,
  Project,
  ProjectWithQuote,
  Rate,
  Ticket,
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
    sasLink: "link1"
  },
  {
    identifier: "1",
    fileName: "file2.pdf",
    fileSize: 22,
    contentType: "application/pdf",
    uploaded: true,
    sasLink: "link2"
  },
]

export const fakeAppContext: AppContextShape = {
  user: {
    detail: {
      ...fakeUser,
    },
    roles: ["System", "FieldManager", "PI"],
  },
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
  status: "Requested",
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
  }
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
    }
  },
  quote: null,
};

export const fakeInvoices: Invoice[] = [
  {
    id: 1,
    total: 100,
    createdOn: new Date("2021-03-15T00:00:00"),
    notes: "hello",
    status: "Requested",
    expenses: [],
    transfers: [],
  },
  {
    id: 2,
    total: 200,
    createdOn: new Date("2021-03-15T00:00:00"),
    notes: "hello",
    status: "Requested",
    expenses: [],
    transfers: [],
  },
  {
    id: 3,
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
    createdOn: new Date("2021-01-15T00:00:00")
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
    createdOn: new Date("2021-01-15T00:00:00")
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
    createdOn: new Date("2021-01-15T00:00:00")
  }
];

export const sampleRates: Rate[] = [
  {
    price: 1234.0,
    unit: "Per Acre per Year",
    type: "Acreage",
    description: "Russell Ranch Acreage",
    id: 1,
  },
  {
    price: 60.0,
    unit: "Hourly",
    type: "Labor",
    description: "Skilled Labor",
    id: 3,
  },
  {
    price: 66.67,
    unit: "Per acre",
    type: "Other",
    description: "Crop Destruction",
    id: 5,
  },
  {
    price: 34.91,
    unit: "Per Acre",
    type: "Equipment",
    description: "Backhoe",
    id: 8,
  },
];
