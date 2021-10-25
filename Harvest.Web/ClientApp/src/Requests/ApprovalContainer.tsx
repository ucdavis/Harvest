import React, { Suspense, useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";

import { Project, ProjectAccount, ProjectWithQuote } from "../types";
import { AccountsInput } from "./AccountsInput";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { QuoteDisplay } from "../Quotes/QuoteDisplay";
import { RejectQuote } from "../Quotes/RejectQuote";
import { formatCurrency } from "../Util/NumberFormatting";

import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";

// Lazy load quote pdf link since it's a large JS file and causes a console warning
const QuotePDFLink = React.lazy(() => import("../Pdf/QuotePDFLink"));

interface RouteParams {
  projectId?: string;
}

export const ApprovalContainer = () => {
  const history = useHistory();
  const { projectId } = useParams<RouteParams>();
  const [projectAndQuote, setProjectAndQuote] = useState<ProjectWithQuote>();
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]); // TODO: better to have part of project obj?
  const [disabled, setDisabled] = useState<boolean>(true);

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await fetch(`/api/Quote/Get/${projectId}`);

      if (quoteResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();

        getIsMounted() && setProjectAndQuote(projectWithQuote);

        // can only approve pendingApproval projects
        if (projectWithQuote.project.status !== "PendingApproval") {
          history.push(`/Project/Details/${projectWithQuote.project.id}`);
        }
      }
    };

    cb();
  }, [history, projectId, getIsMounted]);

  const approve = async () => {
    const model = { accounts };

    const request = fetch(`/api/Request/Approve/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(model),
    });

    setNotification(request, "Saving Approval", "Project Approved");

    const response = await request;

    if (response.ok) {
      const proj: Project = await response.json();
      history.replace(`/Project/Details/${proj.id}`);
    }
  };

  if (projectAndQuote === undefined) {
    return <div>Loading ...</div>;
  }

  // TODO: make sure both project and quote are in the correct statuses in order for an approval
  if (
    projectAndQuote.project === undefined ||
    projectAndQuote.quote === undefined ||
    projectAndQuote.quote === null
  ) {
    return <div>No project or open quote found</div>;
  }

  // we have a project with a quote, time for the approval step
  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={projectAndQuote.project}
        title={"Field Request #" + (projectAndQuote.project.id || "")}
      ></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <QuoteDisplay quote={projectAndQuote.quote} />
          <div className="row mt-4">
            <div className="col-md-6">
              <h2 className="primary-font bold-font">
                Quote Total: ${formatCurrency(projectAndQuote.quote.grandTotal)}
              </h2>
              <Suspense fallback={<div>Generating PDF...</div>}>
                <QuotePDFLink
                  quote={projectAndQuote.quote}
                  fileName="Quote.pdf"
                ></QuotePDFLink>
              </Suspense>
              <AccountsInput
                accounts={accounts}
                setAccounts={setAccounts}
                setDisabled={setDisabled}
              />
            </div>
            <div className="col-md-6">
              <h4>
                <b>Terms and Conditions</b>
              </h4>
              <ol>
                <li>
                  This estimate is approximate based on the information provided
                  by the client. A change order is required if substantial
                  elements of the project are altered. Estimate valid for 30
                  days.
                </li>
                <li>
                  Payment for initial materials/supplies due prior to work
                  performed.
                </li>
                <li>
                  Payment for work performed due immediately upon completion of
                  job.
                </li>
              </ol>
              <div className="text-right mt-5">
                <RejectQuote project={projectAndQuote.project}></RejectQuote>
                <button
                  className="btn btn-primary"
                  disabled={disabled || notification.pending}
                  onClick={approve}
                >
                  Approve Quote
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
