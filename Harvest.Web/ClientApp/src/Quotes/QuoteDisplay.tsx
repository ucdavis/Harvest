import React from "react";

import { QuoteContent } from "../types";
import { QuoteTotals } from "./QuoteTotals";
import { WorkItemDisplay } from "./WorkItemDisplay";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  quote: QuoteContent;
}

export const QuoteDisplay = (props: Props) => {
  const { quote } = props;
  return (
    <div>
      <h1>Quote</h1>
      <p>
        <b>
          {quote.acreageRateDescription}: {quote.acres} @{" "}
          {formatCurrency(quote.acreageRate)} = $
          {formatCurrency(quote.acreageTotal)}
        </b>
      </p>
      {quote.activities.map((activity) => (
        <div
          className="quote-actvitiy-item card-wrapper mb-4 gray-top"
          key={`${activity.name}-${activity.id}`}
        >
          <div className="card-header">
            <h4>
              <span className="primary-color bold-font">{activity.name}</span> •
              Activity Total: ${formatCurrency(activity.total)}
            </h4>
          </div>
          <div className="card-content">
            <WorkItemDisplay workItems={activity.workItems}></WorkItemDisplay>
          </div>
        </div>
      ))}
      <QuoteTotals {...props.quote}></QuoteTotals>
      DEBUG: {JSON.stringify(quote)}
    </div>
  );
};
