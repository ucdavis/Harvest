import React, { useContext } from "react";

import AppContext from "./AppContext";
import { ShowFor } from "../Shared/ShowFor";
import { isBoolean, isFunction } from "../Util/TypeChecks";

import { Project } from "../types";

interface Props {
  children: any;
  project: Project;
  condition?: boolean | (() => boolean);
}

export const ShowForPiOnly = (props: Props) => {
  const user = useContext(AppContext).user;
  const conditionSatisfied = isBoolean(props.condition)
    ? props.condition
    : isFunction(props.condition)
    ? props.condition()
    : true;

  if (conditionSatisfied) {
    return (
      <ShowFor
        roles={["PI"]}
        condition={props.project.principalInvestigator.iam === user.detail.iam}
      >
        {props.children}
      </ShowFor>
    );
  }

  return null;
};
