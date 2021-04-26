import { WorkItem } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  workItems: WorkItem[];
}

export const WorkItemDisplay = (props: Props) => {
  // TODO: should we break out individual display renderer by labor/equip/other?
  const types = ["labor", "equipment", "other"];
  return (
    <div>
      {types.map((type) => (
        <table key={`type-${type}`} className="table">
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
                  <td>{workItem.rate}</td>
                  <td>${formatCurrency(workItem.total)}</td>
                </tr>
              ))}
          </tbody>
        </table>
      ))}
    </div>
  );
};
