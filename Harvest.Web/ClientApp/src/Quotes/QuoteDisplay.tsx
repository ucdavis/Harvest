import React from "react";

import { QuoteContent } from "../types";
import { WorkItemDisplay } from "./WorkItemDisplay";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  quote: QuoteContent;
}

export const QuoteDisplay = (props: Props) => {
  const { quote } = props;
  return (
    <div>
      <h2>Quote</h2>
      {quote.acreageRateDescription}: {quote.acres} @ {formatCurrency(quote.acreageRate)} = ${formatCurrency(quote.acreageTotal)}
      <hr/>
      {quote.activities.map((activity) => (
        <div key={`${activity.name}-${activity.id}`}>
          <h2>
            {activity.name} -- Activity Total: ${formatCurrency(activity.total)}
          </h2>
          <WorkItemDisplay workItems={activity.workItems}></WorkItemDisplay>
        </div>
      ))}
      DEBUG: {JSON.stringify(quote)}
    </div>
  );
};
