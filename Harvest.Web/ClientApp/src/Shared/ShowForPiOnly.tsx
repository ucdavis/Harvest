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
        condition={
          props.project.principalInvestigator.iam === user.detail.iam ||
          props.project.projectPermissions?.some(
            (pp) =>
              pp.user.iam === user.detail.iam &&
              pp.permission === "ProjectEditor"
          )
        }
      >
        {props.children}
      </ShowFor>
    );
  }

  return null;
};

// Can be used as either a hook or a component. Exporting under
// seperate name as a reminder that rules of hooks still apply.
export const useForPiOnly = ShowForPiOnly;
