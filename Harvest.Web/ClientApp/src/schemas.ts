import * as yup from "yup";
import { SchemaOf } from "yup";
import { BlobFile, RequestInput, User, TicketInput, WorkItem, Activity } from "./types";

export const investigatorSchema: SchemaOf<User> = yup
  .object()
  .shape({
    id: yup.number().required(),
    firstName: yup.string(),
    lastName: yup.string().required(),
    email: yup.string().required(),
    iam: yup.string().required(),
    kerberos: yup.string().required(),
    name: yup.string().required(),
    nameAndEmail: yup.string(),
  });

export const fileSchema: SchemaOf<BlobFile> = yup.object().shape({
  identifier: yup.string().required(),
  fileName: yup.string().required(),
  fileSize: yup.number().required(),
  contentType: yup.string().required(),
  uploaded: yup.boolean().required().isTrue(), // files are only valid if they are done uploading
  sasLink: yup.string()
});

export const requestSchema: SchemaOf<RequestInput> = yup.object().shape({
  id: yup.number().required(),
  start: yup.string().required(),
  end: yup.string().required(),
  crop: yup.string().required(),
  cropType: yup.string().required(),
  requirements: yup.string(),
  principalInvestigator: investigatorSchema,
  files: yup.array().of(fileSchema)
});

export const ticketSchema: SchemaOf<TicketInput> = yup.object().shape({
  name: yup.string().required('Subject is required'),
  requirements: yup.string().required('Ticket details are required'),
  attachments: yup.array().of(fileSchema)
}); 

export const workItemSchema: SchemaOf<WorkItem> = yup.object().shape({
  id: yup.number().required().default(0),
  activityId: yup.number().required().integer(),
  type: yup.mixed().required().oneOf(["Acreage", "Equipment", "Other", "Labor"]),
  rateId: yup.number().required().default(0),
  rate: yup.number().required().positive("Work item rate must be a positive number"),
  description: yup.string().defined(),
  quantity: yup.number().required().positive("Work item time/unit must be a positive nmber"),
  unit: yup.string().defined(),
  total: yup.number().required().positive("Work item total must be positive")
});

export const fieldSchema/*: SchemaOf<Field>*/ = yup.object().shape({
  id: yup.number().required(),
  name: yup.string().required(),
  crop: yup.string().required(),
  details: yup.string(),
  //geometry: ? //not sure of a clean way to validate GeoJSON.Polygon
});

export const activitySchema: SchemaOf<Activity> = yup.object().shape({
  total: yup.number().required("Activity total is required"),
  id: yup.number().required(),
  name: yup.string().required("Activity name is required"),
  year: yup.number().required("Activity year is required"),
  adjustment: yup.number().required("Activity adjustment is required"),
  workItems: yup.array().of(workItemSchema).required()
});

export const quoteContentSchema /*: SchemaOf<QuoteContent>*/ = yup.object().shape({
  projectName: yup.string().required("Project name is required"),
  acres: yup.number().min(0, "Number of acres cannot be negative").required("Number of acres is required"),
  acreageRate: yup.number().required("Acreage rate is required"),
  acreageRateId: yup.number().required(),
  acreageRateDescription: yup.string().defined(),
  activities: yup.array().of(activitySchema).required(),
  years: yup.number().integer().min(0, "Years cannot be negative").required(),
  acreageTotal: yup.number().required(),
  activitiesTotal: yup.number().required(),
  laborTotal: yup.number().required(),
  equipmentTotal: yup.number().required(),
  otherTotal: yup.number().required(),
  grandTotal: yup.number().required(),
  fields: yup.array().of(fieldSchema).required()
});