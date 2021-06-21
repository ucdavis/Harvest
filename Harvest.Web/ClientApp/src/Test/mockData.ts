import { ProjectWithQuote, Rate } from "../types";

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
      id: 1,
      firstName: "Scott",
      lastName: "Kirkland",
      email: "srkirkland@ucdavis.edu",
      iam: "1000029584",
      kerberos: "postit",
      name: "Scott Kirkland",
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
      id: 1,
      firstName: "Scott",
      lastName: "Kirkland",
      email: "srkirkland@ucdavis.edu",
      iam: "1000029584",
      kerberos: "postit",
      name: "Scott Kirkland",
    },
    accounts: null,
    quotes: null,
    files: []
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
