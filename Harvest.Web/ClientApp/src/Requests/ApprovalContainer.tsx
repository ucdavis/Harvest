import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { Button, Input, Label, Row } from "reactstrap";

import { Project, ProjectWithQuote } from "../types";

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

  return <div>hi</div>;
};
