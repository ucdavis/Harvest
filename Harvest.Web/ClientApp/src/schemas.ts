import * as yup from "yup";
import { SchemaOf } from "yup";
import { RequestInput, User, WorkItem, QuoteContent, Field, FieldWithoutGeometry, Activity } from "./types";

export const investigatorSchema: SchemaOf<User> = yup
  .object()
  .shape({
    id: yup.number().required(),
    firstName: yup.string().required(),
    lastName: yup.string().required(),
    email: yup.string().required(),
    iam: yup.string().required(),
    kerberos: yup.string().required(),
    name: yup.string().required(),
    nameAndEmail: yup.string(),
  });

export const requestSchema: SchemaOf<RequestInput> = yup.object().shape({
  id: yup.number().required(),
  start: yup.string().required(),
  end: yup.string().required(),
  crop: yup.string().required(),
  cropType: yup.string().required(),
  requirements: yup.string(),
  principalInvestigator: investigatorSchema,
});

export const workItemSchema: SchemaOf<WorkItem> = yup.object().shape({
  id: yup.number().required().default(0),
  activityId: yup.number().required().integer(),
  type: yup.mixed().required().oneOf(["Acreage", "Equipment", "Other", "Labor"]),
  rateId: yup.number().required().default(0),
  rate: yup.number().required().positive(),
  description: yup.string().defined(),
  quantity: yup.number().required().positive(),
  unit: yup.string().nullable(),
  total: yup.number().required().positive()
});

export const fieldSchema/*: SchemaOf<FieldWithoutGeometry>*/ = yup.object().shape({
  id: yup.number().required(),
  name: yup.string().required(),
  crop: yup.string().required(),
  details: yup.string().required(),
  //geometry: ? //not sure of a clean way to validate GeoJSON.Polygon
});

export const activitySchema: SchemaOf<Activity> = yup.object().shape({
  total: yup.number().required(),
  id: yup.number().required(),
  name: yup.string().required(),
  workItems: yup.array().of(workItemSchema).required()
});

export const quoteContentSchema/*: SchemaOf<QuoteContent>*/ = yup.object().shape({
  projectName: yup.string().required(),
  acres: yup.number().min(0).required(),
  acreageRate: yup.number().required(),
  acreageRateId: yup.number().required(),
  acreageRateDescription: yup.string().required(),
  activities: yup.array().of(activitySchema).required(),
  total: yup.number().required(),
  acreageTotal: yup.number().required(),
  activitiesTotal: yup.number().required(),
  laborTotal: yup.number().required(),
  equipmentTotal: yup.number().required(),
  otherTotal: yup.number().required(),
  grandTotal: yup.number().required(),
  fields: yup.array().of(fieldSchema).required()
});