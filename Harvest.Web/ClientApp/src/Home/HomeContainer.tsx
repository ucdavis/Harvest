import React, { useContext } from "react";

import AppContext from "../Shared/AppContext";
import { WorkerHome } from "./WorkerHome";
import { FieldManagerHome } from "./FieldManagerHome";
import { SupervisorHome } from "./SupervisorHome";
import { PIHome } from "./PIHome";
import { RoleName } from "../types";

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

  return (
    <div className="row mt-3">
      <div className="col">
        <h1>Welcome to Harvest, {userInfo.user.detail.name}</h1>
        <p>
          You have the following roles: {userInfo.user.roles.join(", ")}. Lorem
          ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
          tempor incididunt ut labore et dolore magna aliqua.
        </p>
        <hr />
        {ShowCustomActions(userInfo.user.roles)}
      </div>
      <div className="col text-center">
        <img
          className="img-fluid"
          src="/media/studentfarmer.svg"
          alt="Pencil Sketch of farmer holding produce"
        ></img>
      </div>
    </div>
  );
};
