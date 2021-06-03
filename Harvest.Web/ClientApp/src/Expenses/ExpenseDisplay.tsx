import { Expense, RateType } from "../types";
import { ActivityRateTypes } from "../constants";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  expenses: Expense[];
}

export const ExpenseDisplay = (props: Props) => {
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
            {props.expenses
              .filter((w) => w.type === type)
              .map((expense) => (
                <tr key={`item-${expense.id}`}>
                  <td>{expense.description}</td>
                  <td>{expense.quantity}</td>
                  <td>{expense.rate && `(${expense.rate.unit}) $${formatCurrency(expense.rate.price)}`} </td>
                  <td>${formatCurrency(expense.total)}</td>
                </tr>
              ))}
          </tbody>
        </table>
      ))}
    </div>
  );
};
