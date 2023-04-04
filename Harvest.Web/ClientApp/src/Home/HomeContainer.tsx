import React, { useContext } from "react";

import AppContext from "../Shared/AppContext";
import { WorkerHome } from "./WorkerHome";
import { FieldManagerHome } from "./FieldManagerHome";
import { SupervisorHome } from "./SupervisorHome";
import { PIHome } from "./PIHome";
import { RoleName } from "../types";
import { useParams } from "react-router";
import { Redirect } from "react-router-dom";

interface RouteParams {
  team?: string;
}

const ShowCustomActions = (roles: RoleName[]) => {
  if (roles.includes("FieldManager")) {
    return <FieldManagerHome />;
  } else if (roles.includes("Supervisor")) {
    return <SupervisorHome />;
  } else if (roles.includes("Worker")) {
    return <WorkerHome />;
  } else {
    // basic view for PI or person without role
    return <PIHome />;
  }
};

export const HomeContainer = () => {
  const userInfo = useContext(AppContext);

  const { team } = useParams<RouteParams>();

  // if team is null but we have a non-PI role, go to team picker
  const nonPIRoles = userInfo.user.roles.filter((role) => role !== "PI");
  if (!team && nonPIRoles.length > 0) {
    return <Redirect to="/team" />;
  }

  return (
    <div className="row mt-3">
      <div className="col-12 col-md-6">
        <h1>Welcome to Harvest, {userInfo.user.detail.name}</h1>
        {userInfo.user.roles.length > 0 && (
          <p>You have the following roles: {userInfo.user.roles.join(", ")}.</p>
        )}
        <hr />
        <div className="quick-actions-wrapper">
          {ShowCustomActions(userInfo.user.roles)}
        </div>
      </div>
      <div className="col-12 col-md-6 text-center">
        <img
          className="img-fluid"
          src="/media/studentfarmer.svg"
          alt="Pencil Sketch of farmer holding produce"
        ></img>
      </div>
    </div>
  );
};
