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
      <h3>Quote</h3>



      {quote.approvedBy && (

        <p><b>Approved By:</b> {quote.approvedBy.firstName} {quote.approvedBy.lastName}</p>

      )}
      {quote.approvedOn && (

        <p><b>Approved On:</b> {new Date(quote.approvedOn).toDateString()}</p>


      )}

      <p>
        {quote.acreageTotal ? (
          <b>
            {quote.acreageRateDescription}: {quote.acres} @{" "}
            {formatCurrency(quote.acreageRate)} * {quote.years}{" "}
            {quote.years > 1 ? "years" : "year"} = $
            {formatCurrency(quote.acreageTotal)}
          </b>
        ) : (
          <b>No acreage fees</b>
        )}
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
            <WorkItemDisplay
              adjustment={activity.adjustment}
              workItems={activity.workItems}
            ></WorkItemDisplay>
          </div>
        </div>
      ))}
      <QuoteTotals quote={props.quote}></QuoteTotals>
    </div>
  );
};
