import React, { useContext } from "react";

import AppContext from "./AppContext";
import { ShowFor } from "../Shared/ShowFor";

import { Project } from "../types";

interface Props {
  children: any;
  project: Project;
  condition?: boolean | (() => boolean);
}

export const ShowForPiOnly = (props: Props) => {
  const user = useContext(AppContext).user;
  return (
    <ShowFor
      roles={["PI"]}
      condition={
        props.condition
          ? props.condition &&
            props.project.principalInvestigator.iam === user.detail.iam
          : props.project.principalInvestigator.iam === user.detail.iam
      }
    >
      {props.children}
    </ShowFor>
  );
};
