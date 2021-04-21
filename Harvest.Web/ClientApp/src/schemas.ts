import * as yup from "yup";
import { SchemaOf } from "yup";
import { PrincipalInvestigator, RequestInput } from "./types";

export const investigatorSchema: SchemaOf<PrincipalInvestigator> = yup
  .object()
  .shape({
    id: yup.number().required(),
    firstName: yup.string().required(),
    lastName: yup.string().required(),
    email: yup.string().required(),
    iam: yup.string().required(),
    kerberos: yup.string().required(),
    name: yup.string().required(),
    nameAndEmail: yup.string().required(),
  });

export const requestSchema: SchemaOf<RequestInput> = yup.object().shape({
  id: yup.number().required(),
  start: yup.string().required(),
  end: yup.string().required(),
  crop: yup.string().required(),
  principalInvestigator: investigatorSchema,
});
