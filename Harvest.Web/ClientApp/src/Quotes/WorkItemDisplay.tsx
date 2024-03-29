import { WorkItem } from "../types";
import { ActivityRateTypes } from "../constants";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  adjustment: number;
  workItems: WorkItem[];
}

export const WorkItemDisplay = (props: Props) => {
  // TODO: should we break out individual display renderer by labor/equip/other?

  return (
    <div>
      {ActivityRateTypes.map((type) => (
        <table key={`type-${type}`} className="table activity-table">
          <thead>
            <tr>
              <th>{type}</th>
              <th>Quantity</th>
              <th>Rate</th>
              <th>Total</th>
            </tr>
          </thead>
          <tbody>
            {props.workItems
              .filter((w) => w.type === type)
              .map((workItem) => (
                <tr key={`item-${workItem.id}`}>
                  <td>{workItem.description}</td>
                  <td>{workItem.quantity}</td>
                  <td>
                    {workItem.rate}
                    {props.adjustment > 0 && (
                      <span className="primary-color">
                        {" "}
                        + $
                        {formatCurrency(
                          workItem.rate * (props.adjustment / 100)
                        )}
                      </span>
                    )}
                  </td>
                  <td>${formatCurrency(workItem.total)}</td>
                </tr>
              ))}
          </tbody>
        </table>
      ))}
    </div>
  );
};
