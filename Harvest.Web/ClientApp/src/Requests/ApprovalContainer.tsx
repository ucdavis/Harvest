import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import {
  Button,
  Card,
  CardBody,
  CardTitle,
  Input,
  Label,
  Row,
} from "reactstrap";

import { Project, ProjectWithQuote } from "../types";
import { AccountsInput } from "./AccountsInput";
import { RequestHeader } from "./RequestHeader";

interface RouteParams {
  projectId?: string;
}

export const ApprovalContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [projectAndQuote, setProjectAndQuote] = useState<ProjectWithQuote>();

  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await fetch(`/Quote/Get/${projectId}`);

      if (quoteResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();

        setProjectAndQuote(projectWithQuote);
      }
    };

    cb();
  }, [projectId]);

  if (projectAndQuote === undefined) {
    return <div>Loading ...</div>;
  }

  // TODO: make sure both project and quote are in the correct statuses in order for an approval
  if (
    projectAndQuote.project === undefined ||
    projectAndQuote.quote === undefined
  ) {
    return <div>No project or open quote found</div>;
  }

  // we have a project with a quote, time for the approval step
  return (
    <div className="card-wrapper">
      <RequestHeader project={projectAndQuote.project}></RequestHeader>
      <div className="card-green-bg">
        <div className="card-content">
          Quote Details go here
          <hr />
          Quote Total: $123.45 (TODO: store on quote)

          <AccountsInput></AccountsInput>

          <hr/>
          <button>Approve Quote TODO</button>
          <button>Reject Quote Somehow</button>
        </div>
      </div>
    </div>
  );
};
