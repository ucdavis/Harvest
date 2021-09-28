import * as yup from "yup";
import { SchemaOf } from "yup";
import { BlobFile, User, TicketInput, WorkItem, Activity } from "./types";
import { ErrorMessages } from "./errorMessages";
import { addDays } from "./Util/Calculations";

// @ts-ignore - override correct yup type
export const requiredDateSchema: yup.SchemaOf<Date> = yup.date().required();

export const investigatorSchema: SchemaOf<User> = yup.object().shape({
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
  sasLink: yup.string(),
});

export const requestSchema = yup.object().shape({
  id: yup.number().required(),
  start: requiredDateSchema,
  end: requiredDateSchema.when("start", (start, yup) =>
    yup.min(addDays(start, 1), ErrorMessages.EndDateAfterStartDate)
  ),
  crop: yup.string().required(),
  cropType: yup.string().required(),
  requirements: yup.string().required(),
  principalInvestigator: yup.lazy((value) =>
    // investigatorSchema.required() didn't work, but this seems to do the trick
    value ? investigatorSchema : yup.object().required(ErrorMessages.PIRequired)
  ),
  files: yup.array().of(fileSchema),
});

export const ticketSchema: SchemaOf<TicketInput> = yup.object().shape({
  name: yup
    .string()
    .required(ErrorMessages.TicketSubjectRequired)
    .max(50, ErrorMessages.TicketSubjectMaxLength),
  requirements: yup.string().required(ErrorMessages.TicketDetailsRequired),
  attachments: yup.array().of(fileSchema),
  dueDate: yup.date().optional(),
});

export const workItemSchema: SchemaOf<WorkItem> = yup.object().shape({
  id: yup.number().required().default(0),
  activityId: yup.number().required().integer(),
  type: yup
    .mixed()
    .required()
    .oneOf(["Acreage", "Equipment", "Other", "Labor"]),
  rateId: yup.number().required().default(0),
  rate: yup
    .number()
    .required()
    .typeError(ErrorMessages.WorkItemRate)
    .positive(ErrorMessages.WorkItemRate),
  description: yup.string().defined(),
  quantity: yup
    .number()
    .required()
    .when(
      // only validate when user has selected a labor/equipment/other option
      "rateId",
      (rateId, yup) =>
        rateId &&
        yup
          .typeError(ErrorMessages.WorkItemUnit)
          .positive(ErrorMessages.WorkItemUnit)
          .test(
            "maxDigitsAfterDecimal",
            ErrorMessages.WorkItemQuantityDecimalPlaces,
            (number: number) => Number.isInteger((number || 0) * 10 ** 2)
          )
    ),
  unit: yup.string().defined(),
  markup: yup.boolean().defined(),
  isPassthrough: yup.boolean().defined(),
  total: yup
    .number()
    .required()
    .typeError(ErrorMessages.WorkItemTotal)
    .positive(ErrorMessages.WorkItemTotal),
});

export const fieldSchema /*: SchemaOf<Field>*/ = yup.object().shape({
  id: yup.number().required(),
  name: yup.string().required(),
  crop: yup.string().required(),
  details: yup.string(),
  //geometry: ? //not sure of a clean way to validate GeoJSON.Polygon
});

export const activitySchema: SchemaOf<Activity> = yup.object().shape({
  total: yup.number().required(ErrorMessages.ActivityTotalRequired),
  id: yup.number().required(),
  name: yup.string().required(ErrorMessages.ActivityNameRequired),
  year: yup.number().required(ErrorMessages.ActivityYearRequired),
  adjustment: yup.number().required(ErrorMessages.ActivityAdjustmentRequired),
  workItems: yup.array().of(workItemSchema).required(),
});

export const quoteContentSchema /*: SchemaOf<QuoteContent>*/ = yup
  .object()
  .shape({
    projectName: yup.string().required(ErrorMessages.ProjectNameRequired),
    acres: yup
      .number()
      .typeError(ErrorMessages.NumberAcresType)
      .min(0, ErrorMessages.NumberAcresNegative)
      .required(ErrorMessages.NumberAcresRequired),
    acreageRate: yup.number().required(ErrorMessages.AcreageRateRequired),
    acreageRateId: yup.number().nullable(),
    acreageRateDescription: yup.string().defined(),
    activities: yup.array().of(activitySchema).required(),
    years: yup
      .number()
      .integer()
      .min(0, ErrorMessages.YearsNegative)
      .required(),
    acreageTotal: yup.number().required(),
    activitiesTotal: yup.number().required(),
    laborTotal: yup.number().required(),
    equipmentTotal: yup.number().required(),
    otherTotal: yup.number().required(),
    grandTotal: yup.number().required(),
    fields: yup.array().of(fieldSchema).required(),
  });

export const finalAcreageExpenseSchema = yup.object().shape({
  amount: yup
    .number()
    .required()
    .default(0)
    .typeError("Acreage Expense must be a number")
    .positive("Acreage Expense must be positive"),
});
