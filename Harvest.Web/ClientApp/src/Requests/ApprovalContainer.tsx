import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { ProjectAccount, ProjectWithQuote } from "../types";
import { AccountsInput } from "./AccountsInput";
import { RequestHeader } from "./RequestHeader";
import { QuoteDisplay } from "../Quotes/QuoteDisplay";
import { formatCurrency } from "../Util/NumberFormatting";

interface RouteParams {
  projectId?: string;
}

export const ApprovalContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [projectAndQuote, setProjectAndQuote] = useState<ProjectWithQuote>();
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]); // TODO: better to have part of project obj?

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

  const approve = async () => {
    const model = { accounts };
    // TODO: validation, loading spinner
    const response = await fetch(`/Request/Approve/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(model),
    });

    if (response.ok) {
      alert("created!");
    } else {
      alert("didn't work");
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
      <RequestHeader project={projectAndQuote.project}></RequestHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <QuoteDisplay quote={projectAndQuote.quote}></QuoteDisplay>
          <hr />
          Quote Total: ${formatCurrency(projectAndQuote.quote.grandTotal)}
          <AccountsInput
            accounts={accounts}
            setAccounts={setAccounts}
          ></AccountsInput>
          <hr />
          <button onClick={approve}>Approve Quote</button>
          <button>Reject Quote Somehow</button>
        </div>
      </div>
    </div>
  );
};
