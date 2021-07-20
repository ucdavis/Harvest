import { AppContextShape, ProjectWithQuote, Rate } from "../types";

const fakeUser = {
  id: 1,
  firstName: "Bob",
  lastName: "Dobalina",
  email: "bdobalina@ucdavis.edu",
  iam: "1000037182",
  kerberos: "bdobalina",
  name: "Mr Mr Mr Bob Dobalina",
};

export const fakeAppContext: AppContextShape = {
  user: {
    detail: {
      ...fakeUser,
    },
    roles: ["System"],
  },
};

export const fakeProject: ProjectWithQuote = {
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
  },
  quote: null,
};

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
