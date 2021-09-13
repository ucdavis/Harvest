import React from "react";
import { Project } from "../types";
import { Progress } from "reactstrap";

import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  project: Project;
}

export const ProjectProgress = (props: Props) => {
  const { project } = props;
  return (
    <div className="row justify-content-between">
      <div className="col">
        <h2>Project Progress</h2>
        <div className="row justify-content-between">
          <div className="col">
            <p className="mb-1">
              ${formatCurrency(project.chargedTotal)} Billed
            </p>
          </div>
          <div className="col text-right">
            <p className="mb-1">
              ${formatCurrency(project.quoteTotal - project.chargedTotal)}{" "}
              Remaining
            </p>
          </div>
        </div>
        <Progress
          style={{ width: "100%", height: "20px" }}
          value={(project.chargedTotal / project.quoteTotal) * 100}
        />
      </div>
    </div>
  );
};
