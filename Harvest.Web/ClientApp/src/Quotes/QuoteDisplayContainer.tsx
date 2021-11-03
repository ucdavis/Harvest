import React, { Suspense, useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";

import { ProjectWithQuote } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { QuoteDisplay } from "./QuoteDisplay";
import { formatCurrency } from "../Util/NumberFormatting";

import { useIsMounted } from "../Shared/UseIsMounted";

// Lazy load quote pdf link since it's a large JS file and causes a console warning
const QuotePDFLink = React.lazy(() => import("../Pdf/QuotePDFLink"));

interface RouteParams {
  projectId?: string;
}

export const QuoteDisplayContainer = () => {
  const history = useHistory();
  const { projectId } = useParams<RouteParams>();
  const [projectAndQuote, setProjectAndQuote] = useState<ProjectWithQuote>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await fetch(`/api/Quote/GetApproved/${projectId}`);

      if (quoteResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();

        getIsMounted() && setProjectAndQuote(projectWithQuote);
      }
    };

    cb();
  }, [history, projectId, getIsMounted]);

  if (projectAndQuote === undefined) {
    return <div>Loading ...</div>;
  }

  if (
    projectAndQuote.project === undefined ||
    projectAndQuote.quote === undefined ||
    projectAndQuote.quote === null
  ) {
    return <div>No quote found</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={projectAndQuote.project}
        title={"Field Request #" + (projectAndQuote.project.id || "")}
      ></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <QuoteDisplay quote={projectAndQuote.quote} />
          <div className="row mt-4 justify-content-center">
            <div className="col-md-6 text-center">
              <h2 className="primary-font bold-font">
                Quote Total: ${formatCurrency(projectAndQuote.quote.grandTotal)}
              </h2>
              <Suspense fallback={<div>Generating PDF...</div>}>
                <QuotePDFLink
                  quote={projectAndQuote.quote}
                  fileName="Quote.pdf"
                ></QuotePDFLink>
              </Suspense>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
