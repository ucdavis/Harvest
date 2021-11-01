import React, { useContext } from "react";

import AppContext from "./AppContext";

import { isBoolean, isFunction } from "../Util/TypeChecks";
import { Project } from "../types";

interface Props {
  children: any;
  condition?: boolean | (() => boolean);
  project: Project;
}

export const ShowForPiOnly = (props: Props) => {
  const user = useContext(AppContext).user;
  const { children } = props;
  const userRoles = useContext(AppContext).user.roles;
  const conditionSatisfied = isBoolean(props.condition)
    ? props.condition
    : isFunction(props.condition)
    ? props.condition()
    : true;

  if (
    conditionSatisfied &&
    user.detail.iam === props.project.principalInvestigator.iam &&
    (userRoles.includes("System") || userRoles.includes("PI"))
  ) {
    return <>{children}</>;
  }

  return null;
};
