import React, { useContext } from "react";

import AppContext from "./AppContext";
import { ShowFor } from "../Shared/ShowFor";

import { Project } from "../types";

interface Props {
  children: any;
  condition?: boolean | (() => boolean);
  project: Project;
}

export const ShowForPiOnly = (props: Props) => {
  const user = useContext(AppContext).user;
  return (
    <ShowFor
      roles={["PI"]}
      condition={
        props.project.principalInvestigator.iam === user.detail.iam &&
        props.project.status === "Active"
      }
    >
      {props.children}
    </ShowFor>
  );
};
